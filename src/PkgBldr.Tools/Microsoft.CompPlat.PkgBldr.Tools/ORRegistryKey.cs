using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Security.AccessControl;
using System.Text;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public class ORRegistryKey : IDisposable
	{
		private IntPtr m_handle = IntPtr.Zero;

		private string m_name = string.Empty;

		private bool m_isRoot;

		private ORRegistryKey m_parent;

		private const string STR_ROOT = "\\";

		private const string STR_NULLCHAR = "\0";

		private readonly char[] BSLASH_DELIMITER = new char[1] { '\\' };

		private Dictionary<ORRegistryKey, bool> m_children = new Dictionary<ORRegistryKey, bool>();

		public ORRegistryKey Parent => m_parent;

		[SuppressMessage("Microsoft.Performance", "CA1819")]
		public string[] SubKeys => OfflineRegUtils.GetSubKeys(m_handle);

		public string FullName => m_name;

		public string Class => OfflineRegUtils.GetClass(m_handle);

		public ReadOnlyCollection<string> ValueNames => new ReadOnlyCollection<string>(OfflineRegUtils.GetValueNames(m_handle));

		public List<KeyValuePair<string, RegistryValueType>> ValueNameAndTypes => OfflineRegUtils.GetValueNamesAndTypes(m_handle);

		public RegistrySecurity RegistrySecurity => OfflineRegUtils.GetRegistrySecurity(m_handle);

		private ORRegistryKey(string name, IntPtr handle, bool isRoot, ORRegistryKey parent)
		{
			m_name = name;
			m_handle = handle;
			m_isRoot = isRoot;
			m_parent = parent;
			if (m_parent != null)
			{
				m_parent.m_children[this] = true;
			}
		}

		~ORRegistryKey()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}
			foreach (ORRegistryKey key in m_children.Keys)
			{
				key.Close();
			}
			m_children.Clear();
			if (m_parent != null)
			{
				m_parent.m_children.Remove(this);
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1026")]
		public static ORRegistryKey OpenHive(string hivefile, string prefix = null)
		{
			if (prefix == null)
			{
				prefix = "\\";
			}
			return new ORRegistryKey(prefix, OfflineRegUtils.OpenHive(hivefile), true, null);
		}

		[SuppressMessage("Microsoft.Design", "CA1026")]
		public static ORRegistryKey CreateEmptyHive(string prefix = null)
		{
			return new ORRegistryKey(string.IsNullOrEmpty(prefix) ? "\\" : prefix, OfflineRegUtils.CreateHive(), true, null);
		}

		public ORRegistryKey OpenSubKey(string subkeyname)
		{
			if (subkeyname == null)
			{
				throw new ArgumentNullException("subkeyname");
			}
			ORRegistryKey oRRegistryKey = null;
			int num = subkeyname.IndexOf("\\", StringComparison.OrdinalIgnoreCase);
			if (-1 < num)
			{
				string[] array = subkeyname.Split(BSLASH_DELIMITER);
				ORRegistryKey oRRegistryKey2 = this;
				ORRegistryKey oRRegistryKey3 = null;
				string[] array2 = array;
				foreach (string subkeyname2 in array2)
				{
					oRRegistryKey3 = oRRegistryKey2.OpenSubKey(subkeyname2);
					oRRegistryKey2 = oRRegistryKey3;
				}
				return oRRegistryKey3;
			}
			IntPtr handle = OfflineRegUtils.OpenKey(m_handle, subkeyname);
			return new ORRegistryKey(CombineSubKeys(m_name, subkeyname), handle, false, this);
		}

		public RegistryValueType GetValueKind(string valueName)
		{
			return OfflineRegUtils.GetValueType(m_handle, valueName);
		}

		public byte[] GetByteValue(string valueName)
		{
			return OfflineRegUtils.GetValue(m_handle, valueName);
		}

		public uint GetDwordValue(string valueName)
		{
			byte[] byteValue = GetByteValue(valueName);
			if (byteValue.Length != 0)
			{
				return BitConverter.ToUInt32(byteValue, 0);
			}
			return 0u;
		}

		public ulong GetQwordValue(string valueName)
		{
			byte[] byteValue = GetByteValue(valueName);
			if (byteValue.Length != 0)
			{
				return BitConverter.ToUInt64(byteValue, 0);
			}
			return 0uL;
		}

		public string GetStringValue(string valueName)
		{
			byte[] byteValue = GetByteValue(valueName);
			string empty = string.Empty;
			if (byteValue.Length > 1)
			{
				if (byteValue[byteValue.Length - 1] == 0)
				{
					if (byteValue[byteValue.Length - 2] == 0)
					{
						return Encoding.Unicode.GetString(byteValue, 0, byteValue.Length - 2);
					}
				}
			}
			return Encoding.Unicode.GetString(byteValue);
		}

		public string[] GetMultiStringValue(string valueName)
		{
			byte[] byteValue = GetByteValue(valueName);
			return Encoding.Unicode.GetString(byteValue).Split(new char[1], StringSplitOptions.RemoveEmptyEntries);
		}

		public object GetValue(string valueName)
		{
			RegistryValueType valueKind = GetValueKind(valueName);
			object result = null;
			switch (valueKind)
			{
			case RegistryValueType.Binary:
				result = GetByteValue(valueName);
				break;
			case RegistryValueType.DWord:
				result = GetDwordValue(valueName);
				break;
			case RegistryValueType.ExpandString:
				result = GetStringValue(valueName);
				break;
			case RegistryValueType.MultiString:
				result = GetMultiStringValue(valueName);
				break;
			case RegistryValueType.QWord:
				result = GetQwordValue(valueName);
				break;
			case RegistryValueType.String:
				result = GetStringValue(valueName);
				break;
			case RegistryValueType.DWordBigEndian:
				result = GetByteValue(valueName);
				break;
			case RegistryValueType.Link:
				result = GetByteValue(valueName);
				break;
			case RegistryValueType.RegFullResourceDescriptor:
				result = GetByteValue(valueName);
				break;
			case RegistryValueType.RegResourceList:
				result = GetByteValue(valueName);
				break;
			case RegistryValueType.RegResourceRequirementsList:
				result = GetByteValue(valueName);
				break;
			case RegistryValueType.None:
				result = GetByteValue(valueName);
				break;
			}
			return result;
		}

		public void SaveHive(string path)
		{
			if (!m_isRoot)
			{
				throw new IUException("Invalid operation - This registry key does not represent hive root");
			}
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException("path");
			}
			OfflineRegUtils.SaveHive(m_handle, path, 6, 3);
		}

		public ORRegistryKey CreateSubKey(string subkeyName)
		{
			if (subkeyName == null)
			{
				throw new ArgumentNullException("subkeyName");
			}
			ORRegistryKey oRRegistryKey = null;
			int num = subkeyName.IndexOf("\\", StringComparison.OrdinalIgnoreCase);
			if (-1 != num)
			{
				string[] array = subkeyName.Split(BSLASH_DELIMITER, StringSplitOptions.RemoveEmptyEntries);
				ORRegistryKey oRRegistryKey2 = this;
				ORRegistryKey oRRegistryKey3 = null;
				string[] array2 = array;
				foreach (string subkeyName2 in array2)
				{
					oRRegistryKey3 = oRRegistryKey2.CreateSubKey(subkeyName2);
					oRRegistryKey2 = oRRegistryKey3;
				}
				return oRRegistryKey3;
			}
			IntPtr handle = OfflineRegUtils.CreateKey(m_handle, subkeyName);
			return new ORRegistryKey(CombineSubKeys(m_name, subkeyName), handle, false, this);
		}

		public void SetValue(string valueName, byte[] value)
		{
			OfflineRegUtils.SetValue(m_handle, valueName, RegistryValueType.Binary, value);
		}

		public void SetValue(string valueName, string value)
		{
			byte[] bytes = Encoding.Unicode.GetBytes(value);
			OfflineRegUtils.SetValue(m_handle, valueName, RegistryValueType.String, bytes);
		}

		public void SetValue(string valueName, string[] values)
		{
			if (values == null)
			{
				throw new ArgumentNullException("values");
			}
			StringBuilder stringBuilder = new StringBuilder(1024);
			foreach (string arg in values)
			{
				stringBuilder.AppendFormat("{0}{1}", arg, "\0");
			}
			stringBuilder.Append("\0");
			byte[] bytes = Encoding.Unicode.GetBytes(stringBuilder.ToString());
			OfflineRegUtils.SetValue(m_handle, valueName, RegistryValueType.MultiString, bytes);
		}

		public void SetValue(string valueName, int value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			OfflineRegUtils.SetValue(m_handle, valueName, RegistryValueType.DWord, bytes);
		}

		public void SetValue(string valueName, long value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			OfflineRegUtils.SetValue(m_handle, valueName, RegistryValueType.QWord, bytes);
		}

		public void DeleteValue(string valueName)
		{
			OfflineRegUtils.DeleteValue(m_handle, valueName);
		}

		public void DeleteKey(string keyName)
		{
			OfflineRegUtils.DeleteKey(m_handle, keyName);
		}

		private string CombineSubKeys(string path1, string path2)
		{
			if (path1 == null)
			{
				throw new ArgumentNullException("path1");
			}
			if (path2 == null)
			{
				throw new ArgumentNullException("path2");
			}
			if (-1 < path2.IndexOf("\\", StringComparison.OrdinalIgnoreCase) || path1.Length == 0)
			{
				return path2;
			}
			if (path2.Length == 0)
			{
				return path1;
			}
			if (path1.Length == path1.LastIndexOfAny(BSLASH_DELIMITER) + 1)
			{
				return path1 + path2;
			}
			return path1 + BSLASH_DELIMITER[0] + path2;
		}

		private void Close()
		{
			if (m_handle != IntPtr.Zero)
			{
				if (m_isRoot)
				{
					OfflineRegUtils.CloseHive(m_handle);
				}
				else
				{
					OfflineRegUtils.CloseKey(m_handle);
				}
				m_handle = IntPtr.Zero;
			}
		}
	}
}
