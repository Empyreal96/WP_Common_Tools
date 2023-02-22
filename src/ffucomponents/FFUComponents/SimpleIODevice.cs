using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace FFUComponents
{
	public class SimpleIODevice : IFFUDeviceInternal, IFFUDevice, IDisposable
	{
		private enum SioOpcode : byte
		{
			SioId = 1,
			SioFlash = 2,
			SioAck = 3,
			SioNack = 4,
			SioLog = 5,
			SioErr = 6,
			SioSkip = 7,
			SioReset = 8,
			SioFile = 9,
			SioReboot = 10,
			SioMassStorage = 11,
			SioGetDiskInfo = 12,
			SioReadDisk = 13,
			SioWriteDisk = 14,
			SioClearIdOverride = 15,
			SioWim = 16,
			SioSerialNumber = 17,
			SioExternalWim = 18,
			SioSetBootMode = 19,
			SioFastFlash = 20,
			SioDeviceParams = 21,
			SioDeviceVersion = 22,
			SioQueryForCmd = 23,
			SioGetUpdateLogs = 24,
			SioQueryDeviceUnlockId = 25,
			SioRelockDeviceUnlockId = 26,
			SioQueryUnlockTokenFiles = 27,
			SioWriteUnlockTokenFile = 28,
			SioQueryBitlockerState = 29,
			SioLast = 29
		}

		private const int DefaultUSBTransactionSize = 16376;

		private const int DefaultWIMTransactionSize = 1048576;

		private const int MaxResets = 3;

		private const int ManufacturingProfileNameSizeInBytes = 128;

		private const int COMPATFLASH_MagicSequence = 1000;

		private const byte INDEX_SUPPORTCOMPATFLASH = 15;

		private const byte INDEX_SUPPORTV2CMDS = 14;

		private const byte INDEX_SUPPORTFASTFLASH = 0;

		private volatile bool fConnected;

		private volatile bool fOperationStarted;

		private DTSFUsbStream usbStream;

		private MemoryStream memStm;

		private AutoResetEvent connectEvent;

		private PacketConstructor packets;

		private FlashingHostLogger hostLogger;

		private FlashingDeviceLogger deviceLogger;

		private long curPosition;

		private Mutex syncMutex;

		private string usbDevicePath;

		private object pathSync;

		private int errId;

		private string errInfo;

		private int resetCount;

		private int diskTransferSize;

		private uint diskBlockSize;

		private ulong diskLastBlock;

		private long lastProgress;

		private bool forceClearOnReconnect;

		private Guid serialNumber;

		private bool serialNumberChecked;

		private int usbTransactionSize;

		private bool supportsFastFlash;

		private bool supportsCompatFastFlash;

		private bool hasCheckedForV2;

		private int clientVersion;

		private FlashingTelemetryLogger telemetryLogger;

		private ManualResetEvent errorEvent;

		private AutoResetEvent writeEvent;

		public string DeviceFriendlyName { get; private set; }

		public Guid DeviceUniqueID { get; private set; }

		public Guid SerialNumber
		{
			get
			{
				if (!serialNumberChecked)
				{
					serialNumberChecked = true;
					serialNumber = GetSerialNumberFromDevice();
				}
				return serialNumber;
			}
		}

		public string UsbDevicePath
		{
			get
			{
				return usbDevicePath;
			}
			private set
			{
				lock (pathSync)
				{
					if (syncMutex != null)
					{
						syncMutex.Close();
						syncMutex = null;
					}
					string text = GetPnPIdFromDevicePath(value).Replace('\\', '_');
					syncMutex = new Mutex(false, "Global\\FFU_Mutex_" + text);
					usbDevicePath = value;
				}
			}
		}

		public string DeviceType => "SimpleIODevice";

		public event EventHandler<ProgressEventArgs> ProgressEvent;

		public SimpleIODevice(string devicePath)
		{
			fConnected = false;
			fOperationStarted = false;
			forceClearOnReconnect = true;
			usbStream = null;
			memStm = new MemoryStream();
			connectEvent = new AutoResetEvent(false);
			pathSync = new object();
			UsbDevicePath = devicePath;
			hostLogger = FFUManager.HostLogger;
			deviceLogger = FFUManager.DeviceLogger;
			packets = new PacketConstructor();
			DeviceUniqueID = Guid.Empty;
			DeviceFriendlyName = string.Empty;
			resetCount = 0;
			diskTransferSize = 0;
			diskBlockSize = 0u;
			diskLastBlock = 0uL;
			serialNumber = Guid.Empty;
			serialNumberChecked = false;
			usbTransactionSize = 16376;
			supportsFastFlash = false;
			supportsCompatFastFlash = false;
			hasCheckedForV2 = false;
			clientVersion = 1;
			telemetryLogger = FlashingTelemetryLogger.Instance;
			errorEvent = new ManualResetEvent(false);
			writeEvent = new AutoResetEvent(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool fDisposing)
		{
			if (fDisposing)
			{
				FFUManager.DisconnectDevice(this);
				if (usbStream != null)
				{
					usbStream.Dispose();
					usbStream = null;
					fConnected = false;
				}
				if (memStm != null)
				{
					memStm.Dispose();
					memStm = null;
				}
				if (syncMutex != null)
				{
					syncMutex.Close();
					syncMutex = null;
				}
				if (packets != null)
				{
					packets.Dispose();
					packets = null;
				}
				if (connectEvent != null)
				{
					connectEvent.Close();
					connectEvent = null;
				}
			}
		}

		public void FlashFFUFile(string ffuFilePath)
		{
			FlashFFUFile(ffuFilePath, false);
		}

		public void FlashFFUFile(string ffuFilePath, bool optimizeHint)
		{
			bool useOptimize = false;
			if (curPosition != 0L)
			{
				throw new FFUException(DeviceFriendlyName, DeviceUniqueID, Resources.ERROR_ALREADY_RECEIVED_DATA);
			}
			lastProgress = 0L;
			fConnected = true;
			fOperationStarted = true;
			Guid sessionId = Guid.NewGuid();
			try
			{
				Stream stream = (packets.DataStream = GetBufferedFileStream(ffuFilePath));
				using (stream)
				{
					InitFlashingStream(optimizeHint, out useOptimize);
					telemetryLogger.LogFlashingInitialized(sessionId, this, optimizeHint, ffuFilePath);
					hostLogger.EventWriteDeviceFlashParameters(usbTransactionSize, (int)packets.PacketDataSize);
					object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyVersionAttribute), false);
					if (customAttributes.Length != 0)
					{
						AssemblyVersionAttribute assemblyVersionAttribute = (AssemblyVersionAttribute)customAttributes[0];
						hostLogger.EventWriteFlash_Start(DeviceUniqueID, DeviceFriendlyName, string.Format(CultureInfo.CurrentCulture, Resources.MODULE_VERSION, new object[1] { assemblyVersionAttribute.ToString() }));
					}
					telemetryLogger.LogFlashingStarted(sessionId);
					Stopwatch stopwatch = new Stopwatch();
					stopwatch.Start();
					if (useOptimize)
					{
						byte[] array = new byte[usbTransactionSize];
						usbStream.BeginRead(array, 0, array.Length, ErrorCallback, errorEvent);
					}
					TransferPackets(useOptimize);
					WaitForEndResponse(useOptimize);
					stopwatch.Stop();
					telemetryLogger.LogFlashingEnded(sessionId, stopwatch, ffuFilePath, this);
					hostLogger.EventWriteFlash_Stop(DeviceUniqueID, DeviceFriendlyName);
				}
			}
			catch (Exception e)
			{
				telemetryLogger.LogFlashingException(sessionId, e);
				throw;
			}
			finally
			{
				if (useOptimize)
				{
					usbTransactionSize = 16376;
					packets.PacketDataSize = PacketConstructor.DefaultPacketDataSize;
				}
				fConnected = false;
				FFUManager.DisconnectDevice(DeviceUniqueID);
			}
		}

		public bool WriteWim(string wimPath)
		{
			bool result = false;
			lock (pathSync)
			{
				if (fConnected || !AcquirePathMutex())
				{
					return false;
				}
				fOperationStarted = true;
				try
				{
					using (Stream wimStream = GetBufferedFileStream(wimPath))
					{
						using (Stream sdiStream = new MemoryStream(Resources.bootsdi))
						{
							using (usbStream = new DTSFUsbStream(UsbDevicePath, TimeSpan.FromSeconds(1.0)))
							{
								usbStream.SetShortPacketTerminate();
								try
								{
									WriteWim(sdiStream, wimStream);
								}
								catch (Win32Exception ex)
								{
									hostLogger.EventWriteWimWin32Exception(DeviceUniqueID, DeviceFriendlyName, ex.NativeErrorCode);
								}
								usbStream.SetTransferTimeout(TimeSpan.FromSeconds(15.0));
								result = ReadStatus();
								return result;
							}
						}
					}
				}
				catch (IOException)
				{
					hostLogger.EventWriteWimIOException(DeviceUniqueID, DeviceFriendlyName);
					return result;
				}
				catch (Win32Exception ex3)
				{
					hostLogger.EventWriteWimWin32Exception(DeviceUniqueID, DeviceFriendlyName, ex3.NativeErrorCode);
					return result;
				}
				finally
				{
					ReleasePathMutex();
				}
			}
		}

		public bool EndTransfer()
		{
			bool result = false;
			if (curPosition == 0L)
			{
				return true;
			}
			lock (pathSync)
			{
				if (fConnected || !AcquirePathMutex())
				{
					return false;
				}
				try
				{
					using (DTSFUsbStream dTSFUsbStream = new DTSFUsbStream(UsbDevicePath, TimeSpan.FromSeconds(5.0)))
					{
						dTSFUsbStream.WriteByte(8);
						byte[] array = new byte[usbTransactionSize];
						do
						{
							dTSFUsbStream.Read(array, 0, array.Length);
						}
						while (array[0] == 5);
						if (array[0] == 6)
						{
							ReadBootmeFromStream(dTSFUsbStream);
							if (curPosition == 0L)
							{
								result = true;
								return result;
							}
							return result;
						}
						return result;
					}
				}
				catch (IOException)
				{
					return result;
				}
				catch (Win32Exception)
				{
					return result;
				}
				finally
				{
					ReleasePathMutex();
				}
			}
		}

		public bool SkipTransfer()
		{
			bool result = false;
			lock (pathSync)
			{
				if (curPosition != 0L || fConnected || !AcquirePathMutex())
				{
					return false;
				}
				fOperationStarted = true;
				try
				{
					using (DTSFUsbStream skipStream = new DTSFUsbStream(UsbDevicePath, TimeSpan.FromSeconds(5.0)))
					{
						result = WriteSkip(skipStream);
						return result;
					}
				}
				catch (IOException)
				{
					hostLogger.EventWriteSkipIOException(DeviceUniqueID, DeviceFriendlyName);
					return result;
				}
				catch (Win32Exception ex2)
				{
					hostLogger.EventWriteSkipWin32Exception(DeviceUniqueID, DeviceFriendlyName, ex2.NativeErrorCode);
					return result;
				}
				finally
				{
					ReleasePathMutex();
				}
			}
		}

		public bool Reboot()
		{
			bool result = false;
			lock (pathSync)
			{
				if (fConnected || !AcquirePathMutex())
				{
					return false;
				}
				try
				{
					using (DTSFUsbStream dTSFUsbStream = new DTSFUsbStream(UsbDevicePath, TimeSpan.FromSeconds(5.0)))
					{
						dTSFUsbStream.WriteByte(10);
						result = true;
						return result;
					}
				}
				catch (IOException)
				{
					hostLogger.EventWriteRebootIOException(DeviceUniqueID, DeviceFriendlyName);
					return result;
				}
				catch (Win32Exception ex2)
				{
					hostLogger.EventWriteRebootWin32Exception(DeviceUniqueID, DeviceFriendlyName, ex2.NativeErrorCode);
					return result;
				}
				finally
				{
					ReleasePathMutex();
				}
			}
		}

		public bool EnterMassStorage()
		{
			bool result = false;
			lock (pathSync)
			{
				if (fConnected || !AcquirePathMutex())
				{
					return false;
				}
				try
				{
					using (DTSFUsbStream dTSFUsbStream = new DTSFUsbStream(UsbDevicePath, TimeSpan.FromSeconds(5.0)))
					{
						dTSFUsbStream.WriteByte(11);
						if (dTSFUsbStream.ReadByte() == 3)
						{
							result = true;
							return result;
						}
						return result;
					}
				}
				catch (IOException)
				{
					hostLogger.EventWriteMassStorageIOException(DeviceUniqueID, DeviceFriendlyName);
					return result;
				}
				catch (Win32Exception ex2)
				{
					hostLogger.EventWriteMassStorageWin32Exception(DeviceUniqueID, DeviceFriendlyName, ex2.NativeErrorCode);
					return result;
				}
				finally
				{
					ReleasePathMutex();
				}
			}
		}

		public bool ClearIdOverride()
		{
			bool result = false;
			lock (pathSync)
			{
				if (fConnected || !AcquirePathMutex())
				{
					return false;
				}
				try
				{
					using (DTSFUsbStream dTSFUsbStream = new DTSFUsbStream(UsbDevicePath, TimeSpan.FromSeconds(1.0)))
					{
						dTSFUsbStream.WriteByte(15);
						if (dTSFUsbStream.ReadByte() == 3)
						{
							result = true;
							ReadBootmeFromStream(dTSFUsbStream);
							return result;
						}
						return result;
					}
				}
				catch (IOException)
				{
					hostLogger.EventWriteClearIdIOException(DeviceUniqueID, DeviceFriendlyName);
					return result;
				}
				catch (Win32Exception ex2)
				{
					hostLogger.EventWriteClearIdWin32Exception(DeviceUniqueID, DeviceFriendlyName, ex2.NativeErrorCode);
					return result;
				}
				finally
				{
					ReleasePathMutex();
				}
			}
		}

		public bool GetDiskInfo(out uint blockSize, out ulong lastBlock)
		{
			bool result = false;
			blockSize = 0u;
			lastBlock = 0uL;
			lock (pathSync)
			{
				if (fConnected || !AcquirePathMutex())
				{
					return false;
				}
				try
				{
					using (usbStream = new DTSFUsbStream(UsbDevicePath, TimeSpan.FromSeconds(1.0)))
					{
						ReadDiskInfo(out diskTransferSize, out diskBlockSize, out diskLastBlock);
						result = true;
					}
				}
				catch (IOException)
				{
				}
				catch (Win32Exception)
				{
				}
				finally
				{
					ReleasePathMutex();
				}
			}
			blockSize = diskBlockSize;
			lastBlock = diskLastBlock;
			return result;
		}

		public void ReadDisk(ulong diskOffset, byte[] buffer, int offset, int count)
		{
			lock (pathSync)
			{
				if (diskTransferSize <= 0 || fConnected || !AcquirePathMutex())
				{
					throw new FFUDeviceNotReadyException(this);
				}
				ulong num = (diskLastBlock + 1) * diskBlockSize;
				if (count <= 0 || diskOffset >= num || (long)(num - diskOffset) < (long)count)
				{
					throw new FFUDeviceDiskReadException(this, Resources.ERROR_UNABLE_TO_READ_REGION, null);
				}
				try
				{
					using (usbStream = new DTSFUsbStream(UsbDevicePath, TimeSpan.FromSeconds(1.0)))
					{
						ReadDataToBuffer(diskOffset, buffer, offset, count);
					}
				}
				catch (IOException)
				{
					throw new FFUDeviceNotReadyException(this);
				}
				catch (Win32Exception e)
				{
					throw new FFUDeviceDiskReadException(this, Resources.ERROR_USB_TRANSFER, e);
				}
				finally
				{
					ReleasePathMutex();
				}
			}
		}

		public void WriteDisk(ulong diskOffset, byte[] buffer, int offset, int count)
		{
			lock (pathSync)
			{
				if (diskTransferSize <= 0 || fConnected || !AcquirePathMutex())
				{
					throw new FFUDeviceNotReadyException(this);
				}
				ulong num = (diskLastBlock + 1) * diskBlockSize;
				if (count <= 0 || diskOffset >= num || (long)(num - diskOffset) < (long)count)
				{
					throw new FFUDeviceDiskReadException(this, Resources.ERROR_UNABLE_TO_READ_REGION, null);
				}
				try
				{
					using (usbStream = new DTSFUsbStream(UsbDevicePath, TimeSpan.FromSeconds(1.0)))
					{
						WriteDataFromBuffer(diskOffset, buffer, offset, count);
					}
				}
				catch (IOException)
				{
					throw new FFUDeviceNotReadyException(this);
				}
				catch (Win32Exception e)
				{
					throw new FFUDeviceDiskWriteException(this, Resources.ERROR_USB_TRANSFER, e);
				}
				finally
				{
					ReleasePathMutex();
				}
			}
		}

		public uint SetBootMode(uint bootMode, string profileName)
		{
			uint result = 2147483669u;
			lock (pathSync)
			{
				if (fConnected || !AcquirePathMutex())
				{
					return result;
				}
				try
				{
					using (DTSFUsbStream dTSFUsbStream = new DTSFUsbStream(UsbDevicePath, TimeSpan.FromSeconds(5.0)))
					{
						if (Encoding.Unicode.GetByteCount(profileName) >= 128)
						{
							result = 2147483650u;
							throw new Win32Exception(87);
						}
						byte[] array = new byte[132];
						Array.Clear(array, 0, array.Length);
						BitConverter.GetBytes(bootMode).CopyTo(array, 0);
						Encoding.Unicode.GetBytes(profileName).CopyTo(array, 4);
						dTSFUsbStream.WriteByte(19);
						dTSFUsbStream.Write(array, 0, array.Length);
						byte[] array2 = new byte[4];
						dTSFUsbStream.Read(array2, 0, array2.Length);
						result = BitConverter.ToUInt32(array2, 0);
						return result;
					}
				}
				catch (IOException)
				{
					hostLogger.EventWriteBootModeIOException(DeviceUniqueID, DeviceFriendlyName);
					return result;
				}
				catch (Win32Exception ex2)
				{
					hostLogger.EventWriteBootModeWin32Exception(DeviceUniqueID, DeviceFriendlyName, ex2.NativeErrorCode);
					return result;
				}
				finally
				{
					ReleasePathMutex();
				}
			}
		}

		public string GetServicingLogs(string logFolderPath)
		{
			string text = null;
			lock (pathSync)
			{
				if (fConnected || !AcquirePathMutex())
				{
					throw new FFUDeviceNotReadyException(this);
				}
				try
				{
					using (usbStream = new DTSFUsbStream(UsbDevicePath, TimeSpan.FromMinutes(1.0)))
					{
						if (!QueryForCommandAvailable(usbStream, SioOpcode.SioGetUpdateLogs))
						{
							throw new FFUDeviceCommandNotAvailableException(this);
						}
						if (string.IsNullOrEmpty(logFolderPath))
						{
							throw new ArgumentNullException("logFolderPath");
						}
						usbStream.WriteByte(24);
						byte[] array = new byte[262144];
						int num = 0;
						int num2 = 0;
						byte[] array2 = new byte[4];
						num = usbStream.Read(array2, 0, array2.Length);
						int num3 = BitConverter.ToInt32(array2, 0);
						string fullPath = LongPath.GetFullPath(logFolderPath);
						LongPathDirectory.CreateDirectory(fullPath);
						fullPath = Path.Combine(fullPath, Path.GetRandomFileName() + ".cab");
						using (FileStream fileStream = LongPathFile.Open(fullPath, FileMode.Create, FileAccess.Write))
						{
							do
							{
								Array.Clear(array, 0, array.Length);
								num = 0;
								num = usbStream.Read(array, 0, array.Length);
								num2 += num;
								fileStream.Write(array, 0, array.Length);
							}
							while (num2 != num3);
							return fullPath;
						}
					}
				}
				finally
				{
					ReleasePathMutex();
				}
			}
		}

		public string GetFlashingLogs(string logFolderPath)
		{
			throw new NotImplementedException();
		}

		public void QueryDeviceUnlockId(out byte[] unlockId, out byte[] oemId, out byte[] platformId)
		{
			unlockId = new byte[32];
			oemId = new byte[16];
			platformId = new byte[16];
			lock (pathSync)
			{
				if (fConnected || !AcquirePathMutex())
				{
					throw new FFUDeviceNotReadyException(this);
				}
				try
				{
					using (DTSFUsbStream dTSFUsbStream = new DTSFUsbStream(UsbDevicePath, TimeSpan.FromSeconds(5.0)))
					{
						BinaryWriter binaryWriter = new BinaryWriter(dTSFUsbStream);
						BinaryReader binaryReader = new BinaryReader(dTSFUsbStream);
						if (!QueryForCommandAvailable(dTSFUsbStream, SioOpcode.SioQueryDeviceUnlockId))
						{
							throw new FFUDeviceCommandNotAvailableException(this);
						}
						binaryWriter.Write((byte)25);
						int num = binaryReader.ReadInt32();
						binaryReader.ReadUInt32();
						unlockId = binaryReader.ReadBytes(32);
						oemId = binaryReader.ReadBytes(16);
						platformId = binaryReader.ReadBytes(16);
						if (num != 0)
						{
							throw new FFUDeviceRetailUnlockException(this, num);
						}
					}
				}
				catch (IOException)
				{
					throw new FFUDeviceNotReadyException(this);
				}
				catch (Win32Exception e)
				{
					throw new FFUDeviceRetailUnlockException(this, Resources.ERROR_USB_TRANSFER, e);
				}
				finally
				{
					ReleasePathMutex();
				}
			}
		}

		public void RelockDeviceUnlockId()
		{
			lock (pathSync)
			{
				if (fConnected || !AcquirePathMutex())
				{
					throw new FFUDeviceNotReadyException(this);
				}
				try
				{
					using (DTSFUsbStream dTSFUsbStream = new DTSFUsbStream(UsbDevicePath, TimeSpan.FromSeconds(5.0)))
					{
						BinaryWriter binaryWriter = new BinaryWriter(dTSFUsbStream);
						BinaryReader binaryReader = new BinaryReader(dTSFUsbStream);
						if (!QueryForCommandAvailable(dTSFUsbStream, SioOpcode.SioRelockDeviceUnlockId))
						{
							throw new FFUDeviceCommandNotAvailableException(this);
						}
						binaryWriter.Write((byte)26);
						int num = binaryReader.ReadInt32();
						if (num != 0)
						{
							throw new FFUDeviceRetailUnlockException(this, num);
						}
					}
				}
				catch (IOException)
				{
					throw new FFUDeviceNotReadyException(this);
				}
				catch (Win32Exception e)
				{
					throw new FFUDeviceRetailUnlockException(this, Resources.ERROR_USB_TRANSFER, e);
				}
				finally
				{
					ReleasePathMutex();
				}
			}
		}

		public uint[] QueryUnlockTokenFiles()
		{
			byte[] array = new byte[16];
			List<uint> list = new List<uint>();
			lock (pathSync)
			{
				if (fConnected || !AcquirePathMutex())
				{
					throw new FFUDeviceNotReadyException(this);
				}
				try
				{
					using (DTSFUsbStream dTSFUsbStream = new DTSFUsbStream(UsbDevicePath, TimeSpan.FromSeconds(5.0)))
					{
						BinaryWriter binaryWriter = new BinaryWriter(dTSFUsbStream);
						BinaryReader binaryReader = new BinaryReader(dTSFUsbStream);
						if (!QueryForCommandAvailable(dTSFUsbStream, SioOpcode.SioQueryUnlockTokenFiles))
						{
							throw new FFUDeviceCommandNotAvailableException(this);
						}
						binaryWriter.Write((byte)27);
						int num = binaryReader.ReadInt32();
						binaryReader.ReadUInt32();
						BitArray bitArray = new BitArray(binaryReader.ReadBytes(16));
						for (uint num2 = 0u; num2 < bitArray.Count; num2++)
						{
							if (bitArray.Get(Convert.ToInt32(num2)))
							{
								list.Add(num2);
							}
						}
						if (num != 0)
						{
							throw new FFUDeviceRetailUnlockException(this, num);
						}
					}
				}
				catch (IOException)
				{
					throw new FFUDeviceNotReadyException(this);
				}
				catch (Win32Exception e)
				{
					throw new FFUDeviceRetailUnlockException(this, Resources.ERROR_USB_TRANSFER, e);
				}
				finally
				{
					ReleasePathMutex();
				}
			}
			return list.ToArray();
		}

		public void WriteUnlockTokenFile(uint unlockTokenId, byte[] fileData)
		{
			uint value = 0u;
			uint value2 = (uint)fileData.Length;
			if (1048576 < fileData.Length)
			{
				throw new ArgumentException("fileData");
			}
			if (127 < unlockTokenId)
			{
				throw new ArgumentException("unlockTokenId");
			}
			lock (pathSync)
			{
				if (fConnected || !AcquirePathMutex())
				{
					throw new FFUDeviceNotReadyException(this);
				}
				try
				{
					using (DTSFUsbStream dTSFUsbStream = new DTSFUsbStream(UsbDevicePath, TimeSpan.FromSeconds(5.0)))
					{
						BinaryWriter binaryWriter = new BinaryWriter(dTSFUsbStream);
						BinaryReader binaryReader = new BinaryReader(dTSFUsbStream);
						if (!QueryForCommandAvailable(dTSFUsbStream, SioOpcode.SioWriteUnlockTokenFile))
						{
							throw new FFUDeviceCommandNotAvailableException(this);
						}
						binaryWriter.Write((byte)28);
						binaryWriter.Write(value);
						binaryWriter.Write(value2);
						binaryWriter.Write(unlockTokenId);
						binaryWriter.Write(fileData);
						int num = binaryReader.ReadInt32();
						if (num != 0)
						{
							throw new FFUDeviceRetailUnlockException(this, num);
						}
					}
				}
				catch (IOException)
				{
					throw new FFUDeviceNotReadyException(this);
				}
				catch (Win32Exception e)
				{
					throw new FFUDeviceRetailUnlockException(this, Resources.ERROR_USB_TRANSFER, e);
				}
				finally
				{
					ReleasePathMutex();
				}
			}
		}

		public bool QueryBitlockerState()
		{
			bool flag = false;
			lock (pathSync)
			{
				if (fConnected || !AcquirePathMutex())
				{
					throw new FFUDeviceNotReadyException(this);
				}
				try
				{
					using (DTSFUsbStream dTSFUsbStream = new DTSFUsbStream(UsbDevicePath, TimeSpan.FromSeconds(5.0)))
					{
						BinaryWriter binaryWriter = new BinaryWriter(dTSFUsbStream);
						BinaryReader binaryReader = new BinaryReader(dTSFUsbStream);
						if (!QueryForCommandAvailable(dTSFUsbStream, SioOpcode.SioQueryBitlockerState))
						{
							throw new FFUDeviceCommandNotAvailableException(this);
						}
						binaryWriter.Write((byte)29);
						int num = binaryReader.ReadInt32();
						flag = ((binaryReader.ReadByte() != 0) ? true : false);
						if (num != 0)
						{
							throw new FFUDeviceRetailUnlockException(this, num);
						}
						return flag;
					}
				}
				catch (IOException)
				{
					throw new FFUDeviceNotReadyException(this);
				}
				catch (Win32Exception e)
				{
					throw new FFUDeviceRetailUnlockException(this, Resources.ERROR_USB_TRANSFER, e);
				}
				finally
				{
					ReleasePathMutex();
				}
			}
		}

		public void Unlock(uint tokenId, string tokenFilePath, string pin)
		{
			throw new NotImplementedException();
		}

		public byte[] GetDeviceProperties()
		{
			throw new NotImplementedException();
		}

		public bool OnConnect(SimpleIODevice device)
		{
			if (device != null && device.UsbDevicePath != UsbDevicePath)
			{
				UsbDevicePath = device.UsbDevicePath;
			}
			if (fConnected)
			{
				connectEvent.Set();
				return true;
			}
			if (ReadBootme())
			{
				return true;
			}
			return false;
		}

		public bool IsConnected()
		{
			if (!fConnected)
			{
				return ReadBootme();
			}
			return true;
		}

		public bool NeedsTimer()
		{
			return !fOperationStarted;
		}

		public bool OnDisconnect()
		{
			return false;
		}

		private bool AcquirePathMutex()
		{
			TimeoutHelper timeoutHelper = new TimeoutHelper(TimeSpan.FromMinutes(2.0));
			TimeSpan remaining = timeoutHelper.Remaining;
			if (remaining <= TimeSpan.Zero)
			{
				hostLogger.EventWriteMutexTimeout(DeviceUniqueID, DeviceFriendlyName);
				return false;
			}
			try
			{
				if (!syncMutex.WaitOne(remaining, false))
				{
					hostLogger.EventWriteMutexTimeout(DeviceUniqueID, DeviceFriendlyName);
					return false;
				}
				return true;
			}
			catch (AbandonedMutexException)
			{
				hostLogger.EventWriteWaitAbandoned(DeviceUniqueID, DeviceFriendlyName);
				return true;
			}
		}

		private void ReleasePathMutex()
		{
			syncMutex.ReleaseMutex();
		}

		private void InitFlashingStream()
		{
			bool useOptimize = false;
			InitFlashingStream(false, out useOptimize);
		}

		private void InitFlashingStream(bool optimizeHint, out bool useOptimize)
		{
			bool flag = false;
			bool flag2 = false;
			useOptimize = false;
			lock (pathSync)
			{
				if (!AcquirePathMutex())
				{
					throw new FFUException(DeviceFriendlyName, DeviceUniqueID, Resources.ERROR_ACQUIRE_MUTEX);
				}
				try
				{
					if (usbStream != null)
					{
						usbStream.Dispose();
					}
					usbStream = new DTSFUsbStream(UsbDevicePath, TimeSpan.FromMinutes(1.0));
					if (optimizeHint)
					{
						int num = 0;
						do
						{
							ReadBootmeFromStream(usbStream);
							num++;
						}
						while (!supportsFastFlash && !supportsCompatFastFlash && num < 1000);
						flag2 = supportsFastFlash || supportsCompatFastFlash;
					}
					if (!flag2)
					{
						usbStream.WriteByte(2);
					}
					else if (supportsFastFlash)
					{
						usbStream.WriteByte(20);
						InitFastFlash();
						useOptimize = true;
					}
					else if (supportsCompatFastFlash)
					{
						usbStream.WriteByte(20);
						usbTransactionSize = 8388600;
						packets.PacketDataSize = 8388608L;
						useOptimize = true;
					}
				}
				catch (IOException)
				{
					flag = true;
				}
				catch (Win32Exception ex2)
				{
					flag = true;
					if (ex2.NativeErrorCode == 31)
					{
						forceClearOnReconnect = false;
					}
				}
				finally
				{
					ReleasePathMutex();
				}
			}
			if (flag)
			{
				WaitForReconnect();
			}
		}

		private void InitFastFlash()
		{
			byte[] array = new byte[usbTransactionSize];
			usbStream.Read(array, 0, array.Length);
			switch ((SioOpcode)array[0])
			{
			case SioOpcode.SioErr:
				hostLogger.EventWriteFlash_Error(DeviceUniqueID, DeviceFriendlyName);
				throw new FFUFlashException(DeviceFriendlyName, DeviceUniqueID, (FFUFlashException.ErrorCode)errId, string.Format(CultureInfo.CurrentCulture, Resources.ERROR_FLASH, new object[1] { errInfo }));
			default:
				throw new FFUFlashException();
			case SioOpcode.SioDeviceParams:
			{
				BinaryReader binaryReader = new BinaryReader(new MemoryStream(array));
				binaryReader.ReadByte();
				if (binaryReader.ReadUInt32() != 13)
				{
					throw new FFUFlashException(Resources.ERROR_INVALID_DEVICE_PARAMS);
				}
				uint num = binaryReader.ReadUInt32();
				if (num < 16376)
				{
					throw new FFUFlashException(Resources.ERROR_INVALID_DEVICE_PARAMS);
				}
				uint num2 = binaryReader.ReadUInt32();
				if (num2 < PacketConstructor.DefaultPacketDataSize || num2 > PacketConstructor.MaxPacketDataSize || (long)num2 % PacketConstructor.DefaultPacketDataSize != 0L)
				{
					throw new FFUFlashException(Resources.ERROR_INVALID_DEVICE_PARAMS);
				}
				usbTransactionSize = (int)num;
				packets.PacketDataSize = num2;
				break;
			}
			}
		}

		private Stream GetBufferedFileStream(string path)
		{
			return new BufferedStream(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read), 5242880);
		}

		private Stream GetStringStream(string src)
		{
			MemoryStream memoryStream = new MemoryStream();
			new BinaryWriter(memoryStream, Encoding.BigEndianUnicode).Write(src);
			memoryStream.Seek(0L, SeekOrigin.Begin);
			return memoryStream;
		}

		private void WriteCallback(IAsyncResult ar)
		{
			(ar.AsyncState as AutoResetEvent).Set();
		}

		private void SendPacket(byte[] packet, bool optimize)
		{
			bool flag = false;
			WaitHandle[] waitHandles = new WaitHandle[2] { writeEvent, errorEvent };
			while (!flag)
			{
				try
				{
					for (int i = 0; i < packet.Length; i += usbTransactionSize)
					{
						if (optimize)
						{
							usbStream.BeginWrite(packet, i, Math.Min(usbTransactionSize, packet.Length - i), WriteCallback, writeEvent);
							if (WaitHandle.WaitAny(waitHandles) == 1)
							{
								if (usbStream != null)
								{
									usbStream.Dispose();
									usbStream = null;
									fConnected = false;
								}
								hostLogger.EventWriteFlash_Error(DeviceUniqueID, DeviceFriendlyName);
								throw new FFUFlashException(DeviceFriendlyName, DeviceUniqueID, (FFUFlashException.ErrorCode)errId, string.Format(CultureInfo.CurrentCulture, Resources.ERROR_FLASH, new object[1] { errInfo }));
							}
						}
						else
						{
							usbStream.Write(packet, i, Math.Min(usbTransactionSize, packet.Length - i));
						}
					}
					flag = optimize || WaitForAck();
				}
				catch (Win32Exception ex)
				{
					hostLogger.EventWriteTransferException(DeviceUniqueID, DeviceFriendlyName, ex.NativeErrorCode);
					long position = packets.Position;
					WaitForReconnect();
					if (position != packets.Position)
					{
						break;
					}
				}
			}
		}

		private bool WaitForAck()
		{
			while (true)
			{
				byte[] array = new byte[usbTransactionSize];
				usbStream.Read(array, 0, array.Length);
				switch ((SioOpcode)array[0])
				{
				case SioOpcode.SioAck:
					return true;
				case SioOpcode.SioLog:
				{
					BinaryReader binaryReader = new BinaryReader(new MemoryStream(array));
					binaryReader.ReadByte();
					errId = binaryReader.ReadInt16();
					deviceLogger.LogDeviceEvent(array, DeviceUniqueID, DeviceFriendlyName, out errInfo);
					if (string.IsNullOrEmpty(errInfo))
					{
						errId = 0;
					}
					break;
				}
				case SioOpcode.SioErr:
					usbStream.Dispose();
					usbStream = null;
					fConnected = false;
					hostLogger.EventWriteFlash_Error(DeviceUniqueID, DeviceFriendlyName);
					throw new FFUFlashException(DeviceFriendlyName, DeviceUniqueID, (FFUFlashException.ErrorCode)errId, string.Format(CultureInfo.CurrentCulture, Resources.ERROR_FLASH, new object[1] { errInfo }));
				default:
					return false;
				}
			}
		}

		private bool WaitForEndResponse(bool optimize)
		{
			if (!optimize)
			{
				return WaitForAck();
			}
			return true;
		}

		private bool WriteSkip(DTSFUsbStream skipStream)
		{
			skipStream.WriteByte(7);
			int num = skipStream.ReadByte();
			if (num == 3)
			{
				return true;
			}
			hostLogger.EventWriteWriteSkipFailed(DeviceUniqueID, DeviceFriendlyName, num);
			return false;
		}

		private void WaitForReconnect()
		{
			hostLogger.EventWriteDevice_Detach(DeviceUniqueID, DeviceFriendlyName);
			if (!DoWaitForDevice())
			{
				hostLogger.EventWriteFlash_Timeout(DeviceUniqueID, DeviceFriendlyName);
				throw new FFUException(DeviceFriendlyName, DeviceUniqueID, Resources.ERROR_RECONNECT_TIMEOUT);
			}
			if (curPosition == 0L && resetCount < 3)
			{
				packets.Reset();
				resetCount++;
			}
			if ((ulong)(packets.Position - curPosition) > (ulong)packets.PacketDataSize)
			{
				throw new FFUException(DeviceFriendlyName, DeviceUniqueID, string.Format(CultureInfo.CurrentCulture, Resources.ERROR_RESUME_UNEXPECTED_POSITION, new object[2] { packets.Position, curPosition }));
			}
			usbStream.WriteByte(2);
			hostLogger.EventWriteDevice_Attach(DeviceUniqueID, DeviceFriendlyName);
		}

		private bool DoWaitForDevice()
		{
			bool result = false;
			if (usbStream != null)
			{
				usbStream.Dispose();
				usbStream = null;
			}
			connectEvent.WaitOne(30000, false);
			lock (pathSync)
			{
				if (!AcquirePathMutex())
				{
					throw new FFUException(DeviceFriendlyName, DeviceUniqueID, Resources.ERROR_ACQUIRE_MUTEX);
				}
				try
				{
					bool num = forceClearOnReconnect;
					forceClearOnReconnect = true;
					if (num)
					{
						using (DTSFUsbStream clearStream = new DTSFUsbStream(UsbDevicePath, TimeSpan.FromMilliseconds(100.0)))
						{
							ClearJunkDataFromStream(clearStream);
						}
					}
					usbStream = new DTSFUsbStream(UsbDevicePath, TimeSpan.FromMinutes(1.0));
					ReadBootmeFromStream(usbStream);
					result = true;
					return result;
				}
				catch (IOException)
				{
					hostLogger.EventWriteReconnectIOException(DeviceUniqueID, DeviceFriendlyName);
					return result;
				}
				catch (Win32Exception ex2)
				{
					hostLogger.EventWriteReconnectWin32Exception(DeviceUniqueID, DeviceFriendlyName, ex2.NativeErrorCode);
					return result;
				}
				finally
				{
					ReleasePathMutex();
				}
			}
		}

		private void ClearJunkDataFromStream(DTSFUsbStream clearStream)
		{
			hostLogger.EventWriteStreamClearStart(DeviceUniqueID, DeviceFriendlyName);
			try
			{
				clearStream.PipeReset();
				for (int i = 0; i < 3; i++)
				{
					byte[] array = new byte[usbTransactionSize];
					for (int j = 0; j < 17; j++)
					{
						try
						{
							clearStream.Write(array, 0, array.Length);
						}
						catch (Win32Exception ex)
						{
							hostLogger.EventWriteStreamClearPushWin32Exception(DeviceUniqueID, DeviceFriendlyName, ex.ErrorCode);
						}
					}
					for (int k = 0; k < 5; k++)
					{
						try
						{
							clearStream.Read(array, 0, array.Length);
						}
						catch (Win32Exception ex2)
						{
							hostLogger.EventWriteStreamClearPullWin32Exception(DeviceUniqueID, DeviceFriendlyName, ex2.ErrorCode);
						}
					}
				}
				clearStream.PipeReset();
			}
			catch (IOException)
			{
				hostLogger.EventWriteStreamClearIOException(DeviceUniqueID, DeviceFriendlyName);
				connectEvent.WaitOne(5000, false);
			}
			Thread.Sleep(TimeSpan.FromSeconds(1.0));
			hostLogger.EventWriteStreamClearStop(DeviceUniqueID, DeviceFriendlyName);
		}

		private string GetPnPIdFromDevicePath(string path)
		{
			string text = path.Replace('#', '\\').Substring(4);
			return text.Remove(text.IndexOf('\\', 22));
		}

		public void ErrorCallback(IAsyncResult ar)
		{
			usbStream.EndRead(ar);
			DTSFUsbStreamReadAsyncResult dTSFUsbStreamReadAsyncResult = (DTSFUsbStreamReadAsyncResult)ar;
			BinaryReader binaryReader = new BinaryReader(new MemoryStream(dTSFUsbStreamReadAsyncResult.Buffer));
			binaryReader.ReadByte();
			errId = binaryReader.ReadInt16();
			deviceLogger.LogDeviceEvent(dTSFUsbStreamReadAsyncResult.Buffer, DeviceUniqueID, DeviceFriendlyName, out errInfo);
			if (string.IsNullOrEmpty(errInfo))
			{
				errId = 0;
			}
			(ar.AsyncState as ManualResetEvent).Set();
		}

		private void TransferPackets(bool optimize)
		{
			while (packets.RemainingData > 0)
			{
				hostLogger.EventWriteFileRead_Start(DeviceUniqueID, DeviceFriendlyName);
				byte[] nextPacket = packets.GetNextPacket(optimize);
				hostLogger.EventWriteFileRead_Stop(DeviceUniqueID, DeviceFriendlyName);
				SendPacket(nextPacket, optimize);
				if (this.ProgressEvent != null && (packets.Position - lastProgress > 1048576 || packets.Position == packets.Length))
				{
					lastProgress = packets.Position;
					ProgressEventArgs args = new ProgressEventArgs(this, packets.Position, packets.Length);
					Task.Factory.StartNew(delegate
					{
						this.ProgressEvent(this, args);
					});
				}
			}
			if (packets.Length % packets.PacketDataSize == 0L)
			{
				byte[] nextPacket = packets.GetZeroLengthPacket();
				SendPacket(nextPacket, optimize);
			}
		}

		private bool HasWimHeader(Stream wimStream)
		{
			byte[] obj = new byte[8] { 77, 83, 87, 73, 77, 0, 0, 0 };
			byte[] array = new byte[obj.Length];
			long position = wimStream.Position;
			wimStream.Read(array, 0, array.Length);
			wimStream.Position = position;
			return obj.SequenceEqual(array);
		}

		private void WriteWim(Stream sdiStream, Stream wimStream)
		{
			int num = 1048576;
			if (DeviceFriendlyName.Contains("Nokia.MSM8960.P4301"))
			{
				num = 16376;
			}
			bool num2 = HasWimHeader(wimStream);
			byte[] array = new byte[12];
			uint value = 0u;
			if (num2)
			{
				value = (uint)sdiStream.Length;
			}
			BitConverter.GetBytes(value).CopyTo(array, 0);
			BitConverter.GetBytes((uint)wimStream.Length).CopyTo(array, 4);
			BitConverter.GetBytes(num).CopyTo(array, 8);
			byte[] buffer = new byte[num];
			Stream[] array2 = ((!num2) ? new Stream[1] { wimStream } : new Stream[2] { sdiStream, wimStream });
			usbStream.WriteByte(16);
			usbStream.Write(array, 0, array.Length);
			Stream[] array3 = array2;
			foreach (Stream stream in array3)
			{
				hostLogger.EventWriteWimTransferStart(DeviceUniqueID, DeviceFriendlyName);
				while (stream.Position < stream.Length)
				{
					int num3 = stream.Read(buffer, 0, num);
					hostLogger.EventWriteWimPacketStart(DeviceUniqueID, DeviceFriendlyName, num3);
					usbStream.Write(buffer, 0, num3);
					hostLogger.EventWriteWimPacketStop(DeviceUniqueID, DeviceFriendlyName, 0);
				}
				hostLogger.EventWriteWimTransferStop(DeviceUniqueID, DeviceFriendlyName);
			}
		}

		private bool ReadStatus()
		{
			hostLogger.EventWriteWimGetStatus(DeviceUniqueID, DeviceFriendlyName);
			byte[] array = new byte[4];
			usbStream.Read(array, 0, array.Length);
			int num = BitConverter.ToInt32(array, 0);
			bool flag = num >= 0;
			if (flag)
			{
				hostLogger.EventWriteWimSuccess(DeviceUniqueID, DeviceFriendlyName, num);
				return flag;
			}
			hostLogger.EventWriteWimError(DeviceUniqueID, DeviceFriendlyName, num);
			throw new FFUException(DeviceFriendlyName, DeviceUniqueID, string.Format(CultureInfo.CurrentCulture, Resources.ERROR_WIMBOOT, new object[1] { num }));
		}

		private void ReadDiskInfo(out int transferSize, out uint blockSize, out ulong lastBlock)
		{
			usbStream.WriteByte(12);
			byte[] array = new byte[16];
			usbStream.Read(array, 0, array.Length);
			int num = 0;
			transferSize = BitConverter.ToInt32(array, num);
			num += 4;
			blockSize = BitConverter.ToUInt32(array, num);
			num += 4;
			lastBlock = BitConverter.ToUInt64(array, num);
			num += 8;
		}

		private bool NeedsToHandleZLP()
		{
			string[] array = new string[1] { ".*\\.MSM8960\\.*" };
			foreach (string pattern in array)
			{
				if (Regex.IsMatch(DeviceFriendlyName, pattern))
				{
					return true;
				}
			}
			return false;
		}

		private void ReadDataToBuffer(ulong diskOffset, byte[] buffer, int offset, int count)
		{
			long value = count;
			usbStream.WriteByte(13);
			byte[] array = new byte[16];
			BitConverter.GetBytes(diskOffset).CopyTo(array, 0);
			BitConverter.GetBytes((ulong)value).CopyTo(array, 8);
			usbStream.Write(array, 0, array.Length);
			int i = offset;
			int num2;
			for (int num = offset + count; i < num; i += num2)
			{
				num2 = diskTransferSize;
				if (num2 > num - i)
				{
					num2 = num - i;
				}
				usbStream.Read(buffer, i, num2);
				if (num2 % 512 == 0 && NeedsToHandleZLP())
				{
					usbStream.ReadByte();
				}
			}
		}

		private void WriteDataFromBuffer(ulong diskOffset, byte[] buffer, int offset, int count)
		{
			long value = count;
			usbStream.WriteByte(14);
			byte[] array = new byte[16];
			BitConverter.GetBytes(diskOffset).CopyTo(array, 0);
			BitConverter.GetBytes((ulong)value).CopyTo(array, 8);
			usbStream.Write(array, 0, array.Length);
			int i = offset;
			int num2;
			for (int num = offset + count; i < num; i += num2)
			{
				num2 = diskTransferSize;
				if (num2 > num - i)
				{
					num2 = num - i;
				}
				usbStream.Write(buffer, i, num2);
				if (num2 % 512 == 0)
				{
					byte[] array2 = new byte[0];
					usbStream.Write(array2, 0, array2.Length);
				}
			}
			byte[] array3 = new byte[8];
			usbStream.Read(array3, 0, array3.Length);
			if ((ulong)count != BitConverter.ToUInt64(array3, 0))
			{
				throw new FFUDeviceDiskWriteException(this, Resources.ERROR_UNABLE_TO_COMPLETE_WRITE, null);
			}
		}

		private bool QueryForCommandAvailable(DTSFUsbStream idStream, SioOpcode Cmd)
		{
			if (!hasCheckedForV2)
			{
				int num = 0;
				do
				{
					ReadBootmeFromStream(idStream);
					num++;
				}
				while (!supportsFastFlash && !supportsCompatFastFlash && clientVersion < 2 && num < 1000);
				hasCheckedForV2 = true;
			}
			if (clientVersion < 2)
			{
				if ((int)Cmd < 20)
				{
					return true;
				}
				if (supportsFastFlash)
				{
					return true;
				}
				return false;
			}
			idStream.WriteByte(23);
			idStream.WriteByte((byte)Cmd);
			if (idStream.ReadByte() == 0)
			{
				return false;
			}
			return true;
		}

		private unsafe void ReadBootmeFromStream(DTSFUsbStream idStream)
		{
			idStream.WriteByte(1);
			BinaryReader binaryReader = new BinaryReader(idStream);
			curPosition = binaryReader.ReadInt64();
			Guid guid = new Guid(binaryReader.ReadBytes(sizeof(Guid)));
			byte* ptr = (byte*)(&guid);
			if (*ptr >= 1)
			{
				supportsFastFlash = true;
			}
			else if (ptr[15] == 1)
			{
				supportsCompatFastFlash = true;
			}
			if (ptr[14] >= 1)
			{
				clientVersion = ptr[14] + 1;
			}
			DeviceUniqueID = new Guid(binaryReader.ReadBytes(sizeof(Guid)));
			DeviceFriendlyName = binaryReader.ReadString();
		}

		private bool ReadBootme()
		{
			bool result = false;
			for (int i = 0; i < 3; i++)
			{
				lock (pathSync)
				{
					if (syncMutex == null || !AcquirePathMutex())
					{
						return false;
					}
					try
					{
						if (i > 0)
						{
							using (DTSFUsbStream clearStream = new DTSFUsbStream(UsbDevicePath, TimeSpan.FromMilliseconds(100.0)))
							{
								ClearJunkDataFromStream(clearStream);
							}
						}
						using (DTSFUsbStream idStream = new DTSFUsbStream(UsbDevicePath, TimeSpan.FromSeconds(2.0)))
						{
							ReadBootmeFromStream(idStream);
							result = true;
							return result;
						}
					}
					catch (IOException)
					{
						hostLogger.EventWriteReadBootmeIOException(DeviceUniqueID, DeviceFriendlyName);
					}
					catch (Win32Exception ex2)
					{
						hostLogger.EventWriteReadBootmeWin32Exception(DeviceUniqueID, DeviceFriendlyName, ex2.NativeErrorCode);
					}
					finally
					{
						ReleasePathMutex();
					}
				}
			}
			return result;
		}

		private Guid GetSerialNumberFromDevice()
		{
			Guid result = Guid.Empty;
			lock (pathSync)
			{
				if (syncMutex == null || !AcquirePathMutex())
				{
					return result;
				}
				try
				{
					using (DTSFUsbStream dTSFUsbStream = new DTSFUsbStream(UsbDevicePath, TimeSpan.FromSeconds(1.0)))
					{
						byte[] array = new byte[16];
						dTSFUsbStream.WriteByte(17);
						dTSFUsbStream.Read(array, 0, array.Length);
						result = new Guid(array);
						return result;
					}
				}
				catch (IOException)
				{
					return result;
				}
				catch (Win32Exception)
				{
					return result;
				}
				finally
				{
					ReleasePathMutex();
				}
			}
		}
	}
}
