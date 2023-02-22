using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public class OfflineRegUtils
	{
		private static readonly char[] BSLASH_DELIMITER = new char[1] { '\\' };

		public static IntPtr CreateHive()
		{
			IntPtr handle = IntPtr.Zero;
			int num = OffRegNativeMethods.ORCreateHive(ref handle);
			if (num != 0)
			{
				throw new Win32Exception(num);
			}
			return handle;
		}

		public static IntPtr CreateKey(IntPtr handle, string keyName)
		{
			if (handle == IntPtr.Zero)
			{
				throw new ArgumentNullException("handle");
			}
			if (string.IsNullOrEmpty("keyName"))
			{
				throw new ArgumentNullException("keyName");
			}
			IntPtr keyHandle = IntPtr.Zero;
			uint dwDisposition = 0u;
			string[] array = keyName.Split(BSLASH_DELIMITER);
			foreach (string subKeyName in array)
			{
				int num = OffRegNativeMethods.ORCreateKey(handle, subKeyName, null, 0u, null, ref keyHandle, ref dwDisposition);
				if (num != 0)
				{
					throw new Win32Exception(num);
				}
				handle = keyHandle;
			}
			return keyHandle;
		}

		public static void SetValue(IntPtr handle, string valueName, RegistryValueType type, byte[] value)
		{
			if (handle == IntPtr.Zero)
			{
				throw new ArgumentNullException("handle");
			}
			if (valueName == null)
			{
				valueName = string.Empty;
			}
			int num = OffRegNativeMethods.ORSetValue(handle, valueName, (uint)type, value, (uint)value.Length);
			if (num != 0)
			{
				throw new Win32Exception(num);
			}
		}

		public static void DeleteValue(IntPtr handle, string valueName)
		{
			if (handle == IntPtr.Zero)
			{
				throw new ArgumentNullException("handle");
			}
			if (valueName == null)
			{
				valueName = string.Empty;
			}
			int num = OffRegNativeMethods.ORDeleteValue(handle, valueName);
			if (num != 0)
			{
				throw new Win32Exception(num);
			}
		}

		public static void DeleteKey(IntPtr handle, string keyName)
		{
			if (handle == IntPtr.Zero)
			{
				throw new ArgumentNullException("handle");
			}
			if (keyName == null)
			{
				throw new ArgumentNullException("keyName");
			}
			int num = OffRegNativeMethods.ORDeleteKey(handle, keyName);
			if (num != 0)
			{
				throw new Win32Exception(num);
			}
		}

		public static IntPtr OpenHive(string hivefile)
		{
			if (string.IsNullOrEmpty(hivefile))
			{
				throw new ArgumentNullException("hivefile");
			}
			IntPtr handle = IntPtr.Zero;
			int num = OffRegNativeMethods.OROpenHive(hivefile, ref handle);
			if (num != 0)
			{
				throw new Win32Exception(num);
			}
			return handle;
		}

		public static void SaveHive(IntPtr handle, string path, int osMajor, int osMinor)
		{
			if (handle == IntPtr.Zero)
			{
				throw new ArgumentNullException("handle");
			}
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException("path");
			}
			if (LongPathFile.Exists(path))
			{
				FileUtils.DeleteFile(path);
			}
			int num = OffRegNativeMethods.ORSaveHive(handle, path, osMajor, osMinor);
			if (num != 0)
			{
				throw new Win32Exception(num);
			}
		}

		public static void CloseHive(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
			{
				throw new ArgumentNullException("handle");
			}
			int num = OffRegNativeMethods.ORCloseHive(handle);
			if (num != 0)
			{
				throw new Win32Exception(num);
			}
		}

		public static IntPtr OpenKey(IntPtr handle, string subKeyName)
		{
			if (handle == IntPtr.Zero)
			{
				throw new ArgumentNullException("handle");
			}
			if (string.IsNullOrEmpty("subKeyName"))
			{
				throw new ArgumentNullException("subKeyName");
			}
			IntPtr subkeyHandle = IntPtr.Zero;
			int num = OffRegNativeMethods.OROpenKey(handle, subKeyName, ref subkeyHandle);
			if (num != 0)
			{
				throw new Win32Exception(num);
			}
			return subkeyHandle;
		}

		public static void CloseKey(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
			{
				throw new ArgumentNullException("handle");
			}
			int num = OffRegNativeMethods.ORCloseKey(handle);
			if (num != 0)
			{
				throw new Win32Exception(num);
			}
		}

		public static void ConvertHiveToReg(string inputHiveFile, string outputRegFile)
		{
			new HiveToRegConverter(inputHiveFile).ConvertToReg(outputRegFile);
		}

		public static void ConvertHiveToReg(string inputHiveFile, string outputRegFile, string keyPrefix)
		{
			new HiveToRegConverter(inputHiveFile, keyPrefix).ConvertToReg(outputRegFile);
		}

		public static void ConvertHiveToReg(string inputHiveFile, string outputRegFile, string keyPrefix, bool appendExisting)
		{
			new HiveToRegConverter(inputHiveFile, keyPrefix).ConvertToReg(outputRegFile, null, appendExisting);
		}

		public static string ConvertByteArrayToRegStrings(byte[] data)
		{
			return ConvertByteArrayToRegStrings(data, 40);
		}

		public static string ConvertByteArrayToRegStrings(byte[] data, int maxOnALine)
		{
			string empty = string.Empty;
			if (-1 == maxOnALine)
			{
				return BitConverter.ToString(data).Replace('-', ',');
			}
			int num = 0;
			int num2 = data.Length;
			StringBuilder stringBuilder = new StringBuilder();
			while (num2 > 0)
			{
				int num3 = ((num2 > maxOnALine) ? maxOnALine : num2);
				string text = BitConverter.ToString(data, num, num3);
				num += num3;
				num2 -= num3;
				text = text.Replace('-', ',');
				stringBuilder.Append(text);
				if (num2 > 0)
				{
					stringBuilder.Append(",\\");
					stringBuilder.AppendLine();
				}
			}
			return stringBuilder.ToString();
		}

		public static RegistryValueType GetValueType(IntPtr handle, string valueName)
		{
			uint pdwType = 0u;
			uint pcbData = 0u;
			int num = OffRegNativeMethods.ORGetValue(handle, null, valueName, out pdwType, null, ref pcbData);
			if (num != 0)
			{
				throw new Win32Exception(num);
			}
			return (RegistryValueType)pdwType;
		}

		public static List<KeyValuePair<string, RegistryValueType>> GetValueNamesAndTypes(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
			{
				throw new ArgumentNullException("handle");
			}
			uint num = 0u;
			int num2 = 0;
			StringBuilder stringBuilder = new StringBuilder(1024);
			List<KeyValuePair<string, RegistryValueType>> list = new List<KeyValuePair<string, RegistryValueType>>();
			do
			{
				uint lpcValueName = (uint)stringBuilder.Capacity;
				uint lpType = 0u;
				num2 = OffRegNativeMethods.OREnumValue(handle, num, stringBuilder, ref lpcValueName, out lpType, IntPtr.Zero, IntPtr.Zero);
				switch (num2)
				{
				case 0:
				{
					string key = stringBuilder.ToString();
					RegistryValueType value = (RegistryValueType)lpType;
					list.Add(new KeyValuePair<string, RegistryValueType>(key, value));
					num++;
					break;
				}
				default:
					throw new Win32Exception(num2);
				case 259:
					break;
				}
			}
			while (num2 != 259);
			return list;
		}

		public static string[] GetValueNames(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
			{
				throw new ArgumentNullException("handle");
			}
			return (from a in GetValueNamesAndTypes(handle)
				select a.Key).ToArray();
		}

		public static string GetClass(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
			{
				throw new ArgumentNullException("handle");
			}
			StringBuilder stringBuilder = new StringBuilder(128);
			uint lpcClass = (uint)stringBuilder.Capacity;
			uint[] array = new uint[8];
			IntPtr zero = IntPtr.Zero;
			int num = OffRegNativeMethods.ORQueryInfoKey(handle, stringBuilder, ref lpcClass, out array[0], out array[1], out array[3], out array[4], out array[5], out array[6], out array[7], zero);
			if (num == 234)
			{
				lpcClass = (uint)(stringBuilder.Capacity = (int)(lpcClass + 1));
				num = OffRegNativeMethods.ORQueryInfoKey(handle, stringBuilder, ref lpcClass, out array[0], out array[1], out array[3], out array[4], out array[5], out array[6], out array[7], zero);
			}
			if (num != 0)
			{
				throw new Win32Exception(num);
			}
			return stringBuilder.ToString();
		}

		public static byte[] GetValue(IntPtr handle, string valueName)
		{
			if (handle == IntPtr.Zero)
			{
				throw new ArgumentNullException("handle");
			}
			uint pdwType = 0u;
			uint pcbData = 0u;
			int num = OffRegNativeMethods.ORGetValue(handle, null, valueName, out pdwType, null, ref pcbData);
			if (num != 0)
			{
				throw new Win32Exception(num);
			}
			byte[] array = new byte[pcbData];
			num = OffRegNativeMethods.ORGetValue(handle, null, valueName, out pdwType, array, ref pcbData);
			if (num != 0)
			{
				throw new Win32Exception(num);
			}
			return array;
		}

		public static string[] GetSubKeys(IntPtr registryKey)
		{
			if (registryKey == IntPtr.Zero)
			{
				throw new ArgumentNullException("registryKey");
			}
			uint num = 0u;
			int num2 = 0;
			StringBuilder stringBuilder = new StringBuilder(1024);
			List<string> list = new List<string>();
			do
			{
				uint classnamecount = 0u;
				IntPtr filetimeptr = IntPtr.Zero;
				uint count = (uint)stringBuilder.Capacity;
				num2 = OffRegNativeMethods.OREnumKey(registryKey, num, stringBuilder, ref count, null, ref classnamecount, ref filetimeptr);
				switch (num2)
				{
				case 0:
					list.Add(stringBuilder.ToString());
					num++;
					break;
				default:
					throw new Win32Exception(num2);
				case 259:
					break;
				}
			}
			while (num2 != 259);
			return list.ToArray();
		}

		public static byte[] GetRawRegistrySecurity(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
			{
				throw new ArgumentNullException("handle");
			}
			uint size = 0u;
			int num = OffRegNativeMethods.ORGetKeySecurity(handle, (SecurityInformationFlags)28u, null, ref size);
			if (234 != num)
			{
				throw new Win32Exception(num);
			}
			byte[] array = new byte[size];
			num = OffRegNativeMethods.ORGetKeySecurity(handle, (SecurityInformationFlags)28u, array, ref size);
			if (num != 0)
			{
				throw new Win32Exception(num);
			}
			return array;
		}

		public static void SetRawRegistrySecurity(IntPtr handle, byte[] buf)
		{
			if (handle == IntPtr.Zero)
			{
				throw new ArgumentNullException("handle");
			}
			int num = OffRegNativeMethods.ORSetKeySecurity(handle, (SecurityInformationFlags)28u, buf);
			if (num != 0)
			{
				throw new Win32Exception(num);
			}
		}

		public static RegistrySecurity GetRegistrySecurity(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
			{
				throw new ArgumentNullException("handle");
			}
			byte[] rawRegistrySecurity = GetRawRegistrySecurity(handle);
			SecurityUtils.ConvertSDToStringSD(rawRegistrySecurity, (SecurityInformationFlags)24u);
			RegistrySecurity registrySecurity = new RegistrySecurity();
			registrySecurity.SetSecurityDescriptorBinaryForm(rawRegistrySecurity);
			return registrySecurity;
		}

		[SuppressMessage("Microsoft.Usage", "CA1806")]
		public static int GetVirtualFlags(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
			{
				throw new ArgumentNullException("handle");
			}
			int pbFlags = 0;
			OffRegNativeMethods.ORGetVirtualFlags(handle, ref pbFlags);
			return pbFlags;
		}

		public static int ExtractFromHive(string hivePath, RegistryValueType type, string targetPath)
		{
			if (string.IsNullOrEmpty("hivePath"))
			{
				throw new ArgumentNullException("hivePath");
			}
			if (string.IsNullOrEmpty("targetPath"))
			{
				throw new ArgumentNullException("targetPath");
			}
			if (!LongPathFile.Exists(hivePath))
			{
				throw new FileNotFoundException("Hive file {0} does not exist", hivePath);
			}
			int result = 0;
			bool flag = false;
			using (ORRegistryKey oRRegistryKey = ORRegistryKey.OpenHive(hivePath))
			{
				using (ORRegistryKey oRRegistryKey2 = ORRegistryKey.CreateEmptyHive())
				{
					flag = 0 < (result = ExtractFromHiveRecursive(oRRegistryKey, type, oRRegistryKey2));
					if (flag)
					{
						oRRegistryKey2.SaveHive(targetPath);
					}
				}
				if (flag)
				{
					oRRegistryKey.SaveHive(hivePath);
					return result;
				}
				return result;
			}
		}

		private static int ExtractFromHiveRecursive(ORRegistryKey srcHiveRoot, RegistryValueType type, ORRegistryKey dstHiveRoot)
		{
			int num = 0;
			string fullName = srcHiveRoot.FullName;
			foreach (string item in from p in srcHiveRoot.ValueNameAndTypes
				where p.Value == RegistryValueType.MultiString
				select p into q
				select q.Key)
			{
				string valueName = (string.IsNullOrEmpty(item) ? null : item);
				string[] multiStringValue = srcHiveRoot.GetMultiStringValue(valueName);
				using (ORRegistryKey oRRegistryKey = dstHiveRoot.CreateSubKey(fullName))
				{
					oRRegistryKey.SetValue(valueName, multiStringValue);
					num++;
				}
				srcHiveRoot.DeleteValue(valueName);
			}
			string[] subKeys = srcHiveRoot.SubKeys;
			foreach (string subkeyname in subKeys)
			{
				using (ORRegistryKey srcHiveRoot2 = srcHiveRoot.OpenSubKey(subkeyname))
				{
					num += ExtractFromHiveRecursive(srcHiveRoot2, type, dstHiveRoot);
				}
			}
			return num;
		}
	}
}
