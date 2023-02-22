using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace Microsoft.WindowsPhone.Imaging
{
	public sealed class OfflineRegistryHandle : SafeHandle
	{
		private readonly IntPtr _registryHandle;

		private string _name;

		private string _path;

		private bool _disposed;

		private bool _hive;

		public IntPtr UnsafeHandle => _registryHandle;

		public override bool IsInvalid => _disposed;

		public string Name => _name;

		public string Path
		{
			get
			{
				if (_hive)
				{
					return "[" + Name + "]";
				}
				return _path + "\\" + Name;
			}
		}

		public OfflineRegistryHandle(string hivePath)
			: base(IntPtr.Zero, true)
		{
			_registryHandle = Win32Exports.OfflineRegistryOpenHive(hivePath);
			_hive = true;
			_name = hivePath;
			_path = "";
		}

		private OfflineRegistryHandle(IntPtr subKeyHandle, string name, string path)
			: base(IntPtr.Zero, true)
		{
			_registryHandle = subKeyHandle;
			_hive = false;
			_name = name;
			_path = path;
		}

		public void SaveHive(string path)
		{
			if (_hive)
			{
				Win32Exports.OfflineRegistrySaveHive(_registryHandle, path);
				return;
			}
			throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: This function can only be called on a hive handle.");
		}

		public static implicit operator IntPtr(OfflineRegistryHandle offlineRegistryHandle)
		{
			return offlineRegistryHandle._registryHandle;
		}

		protected override bool ReleaseHandle()
		{
			if (!_disposed)
			{
				_disposed = true;
				GC.SuppressFinalize(this);
				if (_registryHandle != IntPtr.Zero)
				{
					if (_hive)
					{
						Win32Exports.OfflineRegistryCloseHive(_registryHandle);
					}
					else
					{
						Win32Exports.OfflineRegistryCloseSubKey(_registryHandle);
					}
				}
			}
			return true;
		}

		public string[] GetSubKeyNames()
		{
			List<string> list = new List<string>();
			string text = null;
			uint num = 0u;
			do
			{
				text = Win32Exports.OfflineRegistryEnumKey(_registryHandle, num++);
				if (text != null)
				{
					list.Add(text);
				}
			}
			while (text != null);
			return list.ToArray();
		}

		public string[] GetValueNames()
		{
			List<string> list = new List<string>();
			string text = null;
			uint num = 0u;
			do
			{
				text = Win32Exports.OfflineRegistryEnumValue(_registryHandle, num++);
				if (text != null)
				{
					list.Add(text);
				}
			}
			while (text != null);
			return list.ToArray();
		}

		public OfflineRegistryHandle OpenSubKey(string keyName)
		{
			IntPtr intPtr = Win32Exports.OfflineRegistryOpenSubKey(_registryHandle, keyName);
			if (intPtr == IntPtr.Zero)
			{
				return null;
			}
			return new OfflineRegistryHandle(intPtr, keyName, Path);
		}

		public RegistryValueKind GetValueKind(string valueName)
		{
			return GetValueKind(Win32Exports.OfflineRegistryGetValueKind(_registryHandle, valueName));
		}

		[CLSCompliant(false)]
		public uint GetValueSize(string valueName)
		{
			return Win32Exports.OfflineRegistryGetValueSize(_registryHandle, valueName);
		}

		public object GetValue(string valueName)
		{
			return Win32Exports.OfflineRegistryGetValue(_registryHandle, valueName);
		}

		public object GetValue(string valueName, object defaultValue)
		{
			object result = defaultValue;
			try
			{
				result = Win32Exports.OfflineRegistryGetValue(_registryHandle, valueName);
				return result;
			}
			catch (ImageStorageException)
			{
				return result;
			}
		}

		public void SetValue(string valueName, byte[] binaryData)
		{
			Win32Exports.OfflineRegistrySetValue(_registryHandle, valueName, Win32Exports.OfflineRegistryGetValueKind(_registryHandle, valueName), binaryData);
		}

		[CLSCompliant(false)]
		public void SetValue(string valueName, byte[] binaryData, uint valueType)
		{
			Win32Exports.OfflineRegistrySetValue(_registryHandle, valueName, valueType, binaryData);
		}

		public void SetValue(string valueName, string stringData)
		{
			byte[] bytes = Encoding.Unicode.GetBytes(stringData);
			SetValue(valueName, bytes, 1u);
		}

		public void SetValue(string valueName, List<string> values)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string value in values)
			{
				stringBuilder.Append(value);
				stringBuilder.Append('\0');
			}
			byte[] bytes = Encoding.Unicode.GetBytes(stringBuilder.ToString());
			SetValue(valueName, bytes, 7u);
		}

		[CLSCompliant(false)]
		public void SetValue(string valueName, uint value)
		{
			byte[] array = new byte[4];
			array[3] = (byte)((value >> 24) & 0xFFu);
			array[2] = (byte)((value >> 16) & 0xFFu);
			array[1] = (byte)((value >> 8) & 0xFFu);
			array[0] = (byte)(value & 0xFFu);
			SetValue(valueName, array, 4u);
		}

		public OfflineRegistryHandle CreateSubKey(string subKey)
		{
			return new OfflineRegistryHandle(Win32Exports.OfflineRegistryCreateKey(this, subKey), subKey, Path);
		}

		public override string ToString()
		{
			return Path;
		}

		private static RegistryValueKind GetValueKind(uint valueType)
		{
			RegistryValueKind result = RegistryValueKind.Unknown;
			switch (valueType)
			{
			case 0u:
				result = RegistryValueKind.None;
				break;
			case 1u:
				result = RegistryValueKind.String;
				break;
			case 2u:
				result = RegistryValueKind.ExpandString;
				break;
			case 3u:
				result = RegistryValueKind.Binary;
				break;
			case 4u:
				result = RegistryValueKind.DWord;
				break;
			case 5u:
				result = RegistryValueKind.DWord;
				break;
			case 6u:
				result = RegistryValueKind.String;
				break;
			case 7u:
				result = RegistryValueKind.MultiString;
				break;
			case 8u:
				result = RegistryValueKind.MultiString;
				break;
			case 9u:
				result = RegistryValueKind.String;
				break;
			case 10u:
				result = RegistryValueKind.Binary;
				break;
			case 11u:
				result = RegistryValueKind.QWord;
				break;
			}
			return result;
		}
	}
}
