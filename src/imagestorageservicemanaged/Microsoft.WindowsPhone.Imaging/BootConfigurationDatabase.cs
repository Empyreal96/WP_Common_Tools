using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BootConfigurationDatabase : IDisposable
	{
		private string _filePath;

		private OfflineRegistryHandle _bcdKey;

		private IULogger _logger = new IULogger();

		public static readonly string SubKeyName = "BootConfigurationKey";

		private bool _alreadyDisposed;

		public List<BcdObject> Objects { get; }

		~BootConfigurationDatabase()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool isDisposing)
		{
			if (!_alreadyDisposed)
			{
				if (isDisposing)
				{
					_filePath = null;
					_logger = null;
				}
				if (_bcdKey != null)
				{
					_bcdKey.Close();
					_bcdKey = null;
				}
				_alreadyDisposed = true;
			}
		}

		public BootConfigurationDatabase(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The filePath is empty or null.");
			}
			if (!File.Exists(filePath))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The file ({filePath}) does not exist or is inaccessible.");
			}
			_filePath = filePath;
			Objects = new List<BcdObject>();
		}

		[CLSCompliant(false)]
		public BootConfigurationDatabase(string filePath, IULogger logger)
			: this(filePath)
		{
			_logger = logger;
		}

		public void Mount()
		{
			_bcdKey = new OfflineRegistryHandle(_filePath);
			try
			{
				OfflineRegistryHandle offlineRegistryHandle = null;
				try
				{
					offlineRegistryHandle = _bcdKey.OpenSubKey("Objects");
				}
				catch (Exception innerException)
				{
					throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The BCD hive is invalid.  Unable to open the 'Objects' key.", innerException);
				}
				try
				{
					string[] subKeyNames = offlineRegistryHandle.GetSubKeyNames();
					if (subKeyNames == null || subKeyNames.Length == 0)
					{
						throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The BCD hive is invalid. There are no keys under 'Objects'.");
					}
					string[] array = subKeyNames;
					foreach (string text in array)
					{
						BcdObject bcdObject = new BcdObject(text);
						OfflineRegistryHandle offlineRegistryHandle2 = offlineRegistryHandle.OpenSubKey(text);
						try
						{
							bcdObject.ReadFromRegistry(offlineRegistryHandle2);
						}
						finally
						{
							if (offlineRegistryHandle2 != null)
							{
								offlineRegistryHandle2.Close();
								offlineRegistryHandle2 = null;
							}
						}
						Objects.Add(bcdObject);
					}
				}
				finally
				{
					if (offlineRegistryHandle != null)
					{
						offlineRegistryHandle.Close();
						offlineRegistryHandle = null;
					}
				}
			}
			catch (ImageStorageException)
			{
				throw;
			}
			catch (Exception innerException2)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Unable to mount and parse the BCD key.", innerException2);
			}
		}

		public void DismountHive(bool save)
		{
			try
			{
				string tempFileName = Path.GetTempFileName();
				File.Delete(tempFileName);
				_bcdKey.SaveHive(tempFileName);
				_bcdKey.Close();
				_bcdKey = null;
				FileAttributes attributes = File.GetAttributes(_filePath);
				if ((attributes & FileAttributes.ReadOnly) != 0)
				{
					File.SetAttributes(_filePath, attributes & ~FileAttributes.ReadOnly);
				}
				File.Delete(_filePath);
				File.Move(tempFileName, _filePath);
				File.SetAttributes(_filePath, attributes);
			}
			catch (Exception innerException)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Unable to save the hive.", innerException);
			}
		}

		public BcdObject GetObject(Guid objectId)
		{
			foreach (BcdObject @object in Objects)
			{
				if (@object.Id == objectId)
				{
					return @object;
				}
			}
			return null;
		}

		public void AddObject(BcdObject bcdObject)
		{
			foreach (BcdObject @object in Objects)
			{
				if (@object.Id == bcdObject.Id)
				{
					throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The object already exists in the BCD.");
				}
			}
			Objects.Add(bcdObject);
		}

		public void LogInfo(int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			_logger.LogInfo(text + "Boot Configuration Database");
			foreach (BcdObject @object in Objects)
			{
				@object.LogInfo(_logger, checked(indentLevel + 2));
				_logger.LogInfo("");
			}
		}

		public void SaveObject(BcdObject bcdObject)
		{
			OfflineRegistryHandle offlineRegistryHandle = null;
			OfflineRegistryHandle offlineRegistryHandle2 = null;
			OfflineRegistryHandle offlineRegistryHandle3 = null;
			OfflineRegistryHandle offlineRegistryHandle4 = null;
			try
			{
				offlineRegistryHandle = _bcdKey.OpenSubKey("objects");
				offlineRegistryHandle2 = offlineRegistryHandle.CreateSubKey(bcdObject.Name);
				offlineRegistryHandle3 = offlineRegistryHandle2.CreateSubKey("Elements");
				offlineRegistryHandle4 = offlineRegistryHandle2.CreateSubKey("Description");
				offlineRegistryHandle4.SetValue("Type", bcdObject.Type);
			}
			finally
			{
				if (offlineRegistryHandle2 != null)
				{
					offlineRegistryHandle2.Close();
					offlineRegistryHandle2 = null;
				}
				if (offlineRegistryHandle != null)
				{
					offlineRegistryHandle.Close();
					offlineRegistryHandle = null;
				}
				if (offlineRegistryHandle3 != null)
				{
					offlineRegistryHandle3.Close();
					offlineRegistryHandle3 = null;
				}
				if (offlineRegistryHandle4 != null)
				{
					offlineRegistryHandle4.Close();
					offlineRegistryHandle4 = null;
				}
			}
		}

		private void SaveBinaryDeviceElement(OfflineRegistryHandle elementKey, byte[] binaryData)
		{
			elementKey.SetValue("Element", binaryData, 3u);
		}

		[Conditional("DEBUG")]
		private void ValidateDeviceElement(BcdObject bcdObject, BcdElement bcdElement, OfflineRegistryHandle elementKey)
		{
			BcdElementDevice bcdElementDevice = bcdElement as BcdElementDevice;
			byte[] array = new byte[bcdElementDevice.BinarySize];
			MemoryStream memoryStream = null;
			try
			{
				memoryStream = new MemoryStream(array);
				bcdElementDevice.WriteToStream(memoryStream);
				byte[] binaryData = bcdElement.GetBinaryData();
				if (binaryData.Length != array.Length)
				{
					throw new ImageStorageException("The binary data length is wrong.");
				}
				for (int i = 0; i < binaryData.Length; i++)
				{
					if (array[i] != binaryData[i])
					{
						throw new ImageStorageException("The binary data is wrong.");
					}
				}
			}
			finally
			{
				if (memoryStream != null)
				{
					memoryStream.Close();
					memoryStream = null;
				}
				bcdElementDevice = null;
				array = null;
			}
		}

		private void SaveStringDeviceElement(OfflineRegistryHandle elementKey, string value)
		{
			elementKey.SetValue("Element", value);
		}

		private void SaveMultiStringDeviceElement(OfflineRegistryHandle elementKey, List<string> values)
		{
			elementKey.SetValue("Element", values);
		}

		public void SaveElementValue(BcdObject bcdObject, BcdElement bcdElement)
		{
			OfflineRegistryHandle offlineRegistryHandle = null;
			OfflineRegistryHandle offlineRegistryHandle2 = null;
			OfflineRegistryHandle offlineRegistryHandle3 = null;
			OfflineRegistryHandle offlineRegistryHandle4 = null;
			string text = bcdElement.DataType.ToString();
			try
			{
				offlineRegistryHandle = _bcdKey.OpenSubKey("Objects");
				offlineRegistryHandle2 = offlineRegistryHandle.OpenSubKey(bcdObject.Name);
				offlineRegistryHandle3 = offlineRegistryHandle2.OpenSubKey("Elements");
				offlineRegistryHandle4 = offlineRegistryHandle3.OpenSubKey(text);
				if (offlineRegistryHandle4 == null)
				{
					offlineRegistryHandle4 = offlineRegistryHandle3.CreateSubKey(text);
				}
				switch (bcdElement.DataType.Format)
				{
				case ElementFormat.Boolean:
					SaveBinaryDeviceElement(offlineRegistryHandle4, bcdElement.GetBinaryData());
					break;
				case ElementFormat.Device:
					SaveBinaryDeviceElement(offlineRegistryHandle4, bcdElement.GetBinaryData());
					break;
				case ElementFormat.Integer:
					SaveBinaryDeviceElement(offlineRegistryHandle4, bcdElement.GetBinaryData());
					break;
				case ElementFormat.IntegerList:
					SaveBinaryDeviceElement(offlineRegistryHandle4, bcdElement.GetBinaryData());
					break;
				case ElementFormat.Object:
					SaveStringDeviceElement(offlineRegistryHandle4, bcdElement.StringData);
					break;
				case ElementFormat.ObjectList:
					SaveMultiStringDeviceElement(offlineRegistryHandle4, bcdElement.MultiStringData);
					break;
				case ElementFormat.String:
					SaveStringDeviceElement(offlineRegistryHandle4, bcdElement.StringData);
					break;
				default:
					throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Unknown element format: {bcdElement.DataType.RawValue}.");
				}
			}
			finally
			{
				if (offlineRegistryHandle4 != null)
				{
					offlineRegistryHandle4.Close();
					offlineRegistryHandle4 = null;
				}
				if (offlineRegistryHandle3 != null)
				{
					offlineRegistryHandle3.Close();
					offlineRegistryHandle3 = null;
				}
				if (offlineRegistryHandle2 != null)
				{
					offlineRegistryHandle2.Close();
					offlineRegistryHandle2 = null;
				}
				if (offlineRegistryHandle != null)
				{
					offlineRegistryHandle.Close();
					offlineRegistryHandle = null;
				}
			}
		}
	}
}
