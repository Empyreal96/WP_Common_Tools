using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.WindowsPhone.MCSF.Offline
{
	public class PolicySettingDestination
	{
		public const string RegistryIntegerType = "REG_DWORD";

		public const string RegistryStringType = "REG_SZ";

		public const string RegistryExpandType = "REG_EXPAND_SZ";

		public const string RegistryMultistring = "REG_MULTI_SZ";

		public const string RegistryBinary = "REG_BINARY";

		public const string CspIntegerType = "CFG_DATATYPE_INTEGER";

		public const string CspStringType = "CFG_DATATYPE_STRING";

		public const string CspMultistringType = "CFG_DATATYPE_MULTIPLE_STRING";

		public const string CspBooleanType = "CFG_DATATYPE_BOOLEAN";

		public const string CspBinaryType = "CFG_DATATYPE_BINARY";

		public const string CspUnknownType = "CFG_DATATYPE_UNKNOWN";

		private const string RegistrySourceName = "RegistrySource";

		private const string DestinationType = "Type";

		private const string DestinationPath = "Path";

		private const string McsfNode = "MCSF";

		public PolicySettingDestinationType Destination { get; private set; }

		public string Type { get; private set; }

		public string Path { get; private set; }

		public string RegistryKey { get; private set; }

		public string RegistryValueName { get; private set; }

		public IEnumerable<string> ProvisioningPath { get; private set; }

		public PolicySettingDestination(XElement destinationElement, PolicySetting parent, PolicyGroup grandparent)
		{
			Destination = (string.Equals("RegistrySource", destinationElement.Name.LocalName, StringComparison.OrdinalIgnoreCase) ? PolicySettingDestinationType.Registry : PolicySettingDestinationType.CSP);
			Type = (string)destinationElement.LocalAttribute("Type");
			Path = (string)destinationElement.LocalAttribute("Path");
			InitProvisionPath(parent.Name, grandparent);
		}

		public PolicySettingDestination(PolicySetting parent, PolicyGroup grandParent)
		{
			Destination = PolicySettingDestinationType.None;
			Type = null;
			Path = null;
			InitProvisionPath(parent.Name, grandParent);
		}

		public PolicySettingDestination(string leafNode, PolicyGroup grandParent)
		{
			Destination = PolicySettingDestinationType.None;
			Type = null;
			Path = null;
			InitProvisionPath(leafNode, grandParent);
		}

		public string GetResolvedRegistryKey(PolicyMacroTable macroTable)
		{
			return macroTable.ReplaceMacros(RegistryKey);
		}

		public string GetResolvedRegistryValueName(PolicyMacroTable macroTable)
		{
			return macroTable.ReplaceMacros(RegistryValueName);
		}

		public IEnumerable<string> GetResolvedProvisioningPath(PolicyMacroTable macroTable)
		{
			foreach (string item in ProvisioningPath)
			{
				yield return PolicyMacroTable.MacroTildeToDollar(macroTable.ReplaceMacros(item));
			}
		}

		private void SetRegistryPath(string registryPath)
		{
			int num = registryPath.LastIndexOf('\\');
			if (num < 0)
			{
				throw new ArgumentException("registryPath doesn't appear to be a valid registry path. Could not find separator character.");
			}
			RegistryKey = registryPath.Substring(0, num);
			RegistryValueName = registryPath.Substring(num + 1);
		}

		private void InitProvisionPath(string leafNode, PolicyGroup grandParent)
		{
			if (Destination == PolicySettingDestinationType.CSP)
			{
				ProvisioningPath = Path.Split('/').AsEnumerable();
				return;
			}
			List<string> list = new List<string>();
			list.Add("MCSF");
			if (grandParent != null)
			{
				list.Add(grandParent.Path);
			}
			if (leafNode != null)
			{
				list.Add(leafNode);
			}
			ProvisioningPath = list;
			if (Destination == PolicySettingDestinationType.Registry)
			{
				SetRegistryPath(Path);
			}
		}
	}
}
