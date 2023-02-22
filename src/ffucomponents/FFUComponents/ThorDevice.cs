using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Windows.Flashing.Platform;

namespace FFUComponents
{
	public class ThorDevice : IFFUDeviceInternal, IFFUDevice, IDisposable
	{
		private class Progress : GenericProgress
		{
			private ThorDevice Device;

			private long FfuFileSize;

			public Progress(ThorDevice device, long ffuFileSize)
			{
				Device = device;
				FfuFileSize = ffuFileSize;
			}

			public override void RegisterProgress(uint progress)
			{
				ProgressEventArgs args = new ProgressEventArgs(Device, progress * FfuFileSize / 100, FfuFileSize);
				Task.Factory.StartNew(delegate
				{
					Device.ProgressEvent(Device, args);
				});
			}
		}

		private FlashingDevice flashingDevice;

		private FlashingTelemetryLogger telemetryLogger;

		private ConnectionType connectionType;

		public string DeviceFriendlyName => flashingDevice.GetDeviceFriendlyName();

		public Guid DeviceUniqueID => flashingDevice.GetDeviceUniqueID();

		public Guid SerialNumber => flashingDevice.GetDeviceSerialNumber();

		public string UsbDevicePath { get; set; }

		public string DeviceType => "UFPDevice";

		public event EventHandler<ProgressEventArgs> ProgressEvent;

		public ThorDevice(FlashingDevice device, string devicePath)
		{
			flashingDevice = device;
			UsbDevicePath = devicePath;
			try
			{
				USBSpeedChecker uSBSpeedChecker = new USBSpeedChecker(devicePath);
				connectionType = uSBSpeedChecker.GetConnectionSpeed();
			}
			catch
			{
				connectionType = ConnectionType.Unknown;
			}
			telemetryLogger = FlashingTelemetryLogger.Instance;
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
			}
		}

		public void FlashFFUFile(string ffuFilePath)
		{
			FlashFFUFile(ffuFilePath, false);
		}

		public void FlashFFUFile(string ffuFilePath, bool optimizeHint)
		{
			Guid sessionId = Guid.NewGuid();
			try
			{
				telemetryLogger.LogFlashingInitialized(sessionId, this, optimizeHint, ffuFilePath);
				telemetryLogger.LogThorDeviceUSBConnectionType(sessionId, connectionType);
				long length = new FileInfo(ffuFilePath).Length;
				Progress progress = new Progress(this, length);
				HandleRef handleRef = default(HandleRef);
				telemetryLogger.LogFlashingStarted(sessionId);
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start();
				flashingDevice.FlashFFUFile(ffuFilePath, (FlashFlags)0, (GenericProgress)(object)progress, handleRef);
				stopwatch.Stop();
				telemetryLogger.LogFlashingEnded(sessionId, stopwatch, ffuFilePath, this);
			}
			catch (Exception e)
			{
				telemetryLogger.LogFlashingException(sessionId, e);
				throw;
			}
			Reboot();
		}

		public bool WriteWim(string wimPath)
		{
			long length = new FileInfo(wimPath).Length;
			Progress progress = new Progress(this, length);
			flashingDevice.WriteWim(wimPath, (GenericProgress)(object)progress);
			return true;
		}

		public bool EndTransfer()
		{
			return false;
		}

		public bool SkipTransfer()
		{
			flashingDevice.SkipTransfer();
			return true;
		}

		public bool Reboot()
		{
			flashingDevice.Reboot();
			return true;
		}

		public bool EnterMassStorage()
		{
			flashingDevice.EnterMassStorageMode();
			return true;
		}

		public bool ClearIdOverride()
		{
			throw new NotImplementedException();
		}

		public bool GetDiskInfo(out uint blockSize, out ulong lastBlock)
		{
			throw new NotImplementedException();
		}

		public void ReadDisk(ulong diskOffset, byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		public void WriteDisk(ulong diskOffset, byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		public uint SetBootMode(uint bootMode, string profileName)
		{
			flashingDevice.SetBootMode(bootMode, profileName);
			Reboot();
			return 0u;
		}

		public string GetServicingLogs(string logFolderPath)
		{
			return flashingDevice.GetLogs((DeviceLogType)2, logFolderPath);
		}

		public string GetFlashingLogs(string logFolderPath)
		{
			return flashingDevice.GetLogs((DeviceLogType)1, logFolderPath);
		}

		public void QueryDeviceUnlockId(out byte[] unlockId, out byte[] oemId, out byte[] platformId)
		{
			UNLOCK_ID deviceUnlockID = flashingDevice.GetDeviceUnlockID();
			unlockId = new byte[deviceUnlockID.UnlockId.Length];
			oemId = new byte[deviceUnlockID.OemId.Length];
			platformId = new byte[deviceUnlockID.PlatformId.Length];
			Array.Copy(deviceUnlockID.UnlockId, unlockId, deviceUnlockID.UnlockId.Length);
			Array.Copy(deviceUnlockID.OemId, oemId, deviceUnlockID.OemId.Length);
			Array.Copy(deviceUnlockID.PlatformId, platformId, deviceUnlockID.PlatformId.Length);
		}

		public void RelockDeviceUnlockId()
		{
			flashingDevice.RelockDevice();
		}

		public uint[] QueryUnlockTokenFiles()
		{
			UNLOCK_TOKEN_FILES obj = flashingDevice.QueryUnlockTokenFiles();
			List<uint> list = new List<uint>();
			BitArray bitArray = new BitArray(obj.TokenIdBitmask);
			for (uint num = 0u; num < bitArray.Count; num++)
			{
				if (bitArray.Get(Convert.ToInt32(num)))
				{
					list.Add(num);
				}
			}
			return list.ToArray();
		}

		public void WriteUnlockTokenFile(uint unlockTokenId, byte[] fileData)
		{
			throw new NotImplementedException();
		}

		public bool QueryBitlockerState()
		{
			return flashingDevice.GetBitlockerState() != 0;
		}

		public void Unlock(uint tokenId, string tokenFilePath, string pin)
		{
			flashingDevice.UnlockDevice(tokenId, tokenFilePath, pin);
		}

		public byte[] GetDeviceProperties()
		{
			return flashingDevice.GetDeviceProperties();
		}

		public bool NeedsTimer()
		{
			return false;
		}
	}
}
