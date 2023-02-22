using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.WindowsPhone.MCSF.Offline
{
	public class PolicyGroup
	{
		private List<PolicySetting> settingsList;

		private Dictionary<string, PolicyAssetInfo> assetInfoList = new Dictionary<string, PolicyAssetInfo>(StringComparer.OrdinalIgnoreCase);

		private List<string> _oemMacros;

		public string DefinedIn;

		public string Path { get; private set; }

		public bool Atomic { get; private set; }

		public bool ImageTimeOnly { get; private set; }

		public bool FirstVariationOnly { get; private set; }

		public bool CriticalSettings { get; private set; }

		public List<string> OEMMacros
		{
			get
			{
				if (_oemMacros == null)
				{
					_oemMacros = PolicyMacroTable.OEMMacroList(Path);
				}
				return _oemMacros;
			}
		}

		public bool HasOEMMacros => OEMMacros.Count() > 0;

		public IEnumerable<PolicySetting> Settings => settingsList;

		public IEnumerable<PolicyAssetInfo> Assets => assetInfoList.Values;

		public PolicySetting SettingByName(string name)
		{
			IEnumerable<PolicySetting> source = settingsList.Where((PolicySetting x) => PolicyMacroTable.IsMatch(x.Name, name, StringComparison.OrdinalIgnoreCase));
			if (source.Count() > 1)
			{
				source = source.Where((PolicySetting x) => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
			}
			if (source.Count() == 0)
			{
				return null;
			}
			return source.First();
		}

		public PolicyAssetInfo AssetByName(string name)
		{
			if (!assetInfoList.ContainsKey(name))
			{
				if (assetInfoList.Values.Any((PolicyAssetInfo asstInfo) => asstInfo.IsMatch(name)))
				{
					return assetInfoList.Values.First((PolicyAssetInfo asstInfo) => asstInfo.IsMatch(name));
				}
				return null;
			}
			return assetInfoList[name];
		}

		public PolicyMacroTable GetMacroTable(string path)
		{
			return new PolicyMacroTable(Path, path);
		}

		public PolicyGroup(XElement policyGroupElement)
			: this(policyGroupElement, null, null)
		{
		}

		public PolicyGroup(XElement policyGroupElement, string definedIn)
			: this(policyGroupElement, definedIn, null)
		{
		}

		public PolicyGroup(XElement policyGroupElement, string definedIn, string partition)
		{
			settingsList = new List<PolicySetting>();
			Path = (string)policyGroupElement.LocalAttribute("Path");
			CriticalSettings = policyGroupElement.LocalAttribute("CriticalSettings") != null && policyGroupElement.LocalAttribute("CriticalSettings").Value.Equals("Yes");
			DefinedIn = definedIn;
			XElement xElement = policyGroupElement.LocalElement("Constraints");
			if (xElement != null)
			{
				ImageTimeOnly = xElement.LocalAttribute("ImageTimeOnly") != null && xElement.LocalAttribute("ImageTimeOnly").Value.Equals("Yes");
				FirstVariationOnly = xElement.LocalAttribute("FirstVariationOnly") != null && xElement.LocalAttribute("FirstVariationOnly").Value.Equals("Yes");
				Atomic = xElement.LocalAttribute("Atomic") != null && xElement.LocalAttribute("Atomic").Value.Equals("Yes");
			}
			foreach (XElement item2 in policyGroupElement.LocalElements("Asset"))
			{
				PolicyAssetInfo policyAssetInfo = new PolicyAssetInfo(item2, definedIn);
				assetInfoList.Add(policyAssetInfo.Name, policyAssetInfo);
			}
			foreach (XElement item3 in policyGroupElement.LocalElements("Setting"))
			{
				PolicySetting item = new PolicySetting(item3, this, definedIn, partition);
				settingsList.Add(item);
			}
		}

		public override string ToString()
		{
			return Path;
		}
	}
}
