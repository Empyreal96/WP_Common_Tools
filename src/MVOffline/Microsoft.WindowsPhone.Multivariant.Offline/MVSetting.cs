using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.WindowsPhone.Multivariant.Offline
{
	public class MVSetting
	{
		private static readonly Dictionary<string, string> DatatypeMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{ "REG_DWORD", "integer" },
			{ "REG_SZ", "string" },
			{ "REG_EXPAND_SZ", "expandstring" },
			{ "REG_MULTI_SZ", "multiplestring" },
			{ "REG_BINARY", "binary" },
			{ "CFG_DATATYPE_INTEGER", "integer" },
			{ "CFG_DATATYPE_STRING", "string" },
			{ "CFG_DATATYPE_MULTIPLE_STRING", "multiplestring" },
			{ "CFG_DATATYPE_BOOLEAN", "boolean" },
			{ "CFG_DATATYPE_BINARY", "binary" },
			{ "CFG_DATATYPE_UNKNOWN", null }
		};

		public string RegistryKey { get; private set; }

		public string RegistryValue { get; private set; }

		public string RegType { get; private set; }

		public IEnumerable<string> ProvisioningPath { get; private set; }

		public MVSettingProvisioning ProvisioningTime { get; set; }

		public string Value { get; set; }

		public string Partition { get; set; }

		public string DataType { get; set; }

		public MVSetting(IEnumerable<string> provisioningPath)
			: this(provisioningPath, null, null, null, null)
		{
		}

		public MVSetting(IEnumerable<string> provisioningPath, string registryKey, string registryValue, string regType)
			: this(provisioningPath, registryKey, registryValue, regType, null)
		{
		}

		public MVSetting(IEnumerable<string> provisioningPath, string registryKey, string registryValue, string regType, string partition)
		{
			if (provisioningPath == null)
			{
				throw new ArgumentNullException("provisioningPath");
			}
			if (provisioningPath.Count() == 0)
			{
				throw new ArgumentException("provisioningPath cannot be empty");
			}
			ProvisioningPath = provisioningPath;
			RegistryKey = registryKey;
			RegistryValue = registryValue;
			ProvisioningTime = MVSettingProvisioning.General;
			RegType = regType;
			Partition = partition;
			if (RegType == null)
			{
				throw new ArgumentException("RegType can not be null");
			}
			if (!DatatypeMapping.ContainsKey(RegType))
			{
				throw new ArgumentException("Unknown 'RegType': {0}", RegType);
			}
			DataType = DatatypeMapping[RegType];
		}
	}
}
