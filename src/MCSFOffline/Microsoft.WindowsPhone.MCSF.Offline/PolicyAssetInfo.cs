using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.WindowsPhone.MCSF.Offline
{
	public class PolicyAssetInfo
	{
		private List<string> fileTypes;

		public string DefinedIn;

		private List<string> _oemMacros;

		public IEnumerable<string> FileTypes => fileTypes;

		public string Name { get; private set; }

		public string Description { get; private set; }

		public string TargetDir { get; private set; }

		public string TargetPackage { get; private set; }

		public List<PolicyEnum> Presets { get; private set; }

		public Dictionary<string, string> PresetsAltDir { get; private set; }

		public string OemRegKey { get; private set; }

		public string OemRegValue { get; private set; }

		public bool FileNameOnly { get; private set; }

		public string MORegKey { get; private set; }

		public bool GenerateAssetProvXML
		{
			get
			{
				if (OemRegKey == null && MORegKey == null)
				{
					return false;
				}
				return true;
			}
		}

		public List<string> OEMMacros
		{
			get
			{
				if (_oemMacros == null)
				{
					_oemMacros = PolicyMacroTable.OEMMacroList(Name);
				}
				return _oemMacros;
			}
		}

		public bool HasOEMMacros => OEMMacros.Any();

		public PolicyAssetInfo(XElement assetElement)
			: this(assetElement, null)
		{
		}

		public PolicyAssetInfo(XElement assetElement, string definedIn)
		{
			string text = (string)assetElement.LocalAttribute("Type");
			fileTypes = (from x in text.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries)
				select x.Trim()).ToList();
			TargetDir = (string)assetElement.LocalAttribute("Path");
			Name = (string)assetElement.LocalAttribute("Name");
			Description = (string)assetElement.LocalAttribute("Description");
			TargetPackage = (string)assetElement.LocalAttribute("TargetPackage");
			if (string.IsNullOrEmpty(TargetPackage) || TargetPackage.Equals("Default"))
			{
				TargetPackage = "";
			}
			XElement xElement = assetElement.LocalElement("ValueList") ?? assetElement.LocalElement("MultiStringList");
			if (xElement == null)
			{
				OemRegKey = (OemRegValue = (MORegKey = null));
			}
			else
			{
				OemRegKey = (string)(xElement.LocalAttribute("OEMKey") ?? xElement.LocalAttribute("Key"));
				OemRegValue = (string)xElement.LocalAttribute("Value");
				MORegKey = (string)xElement.LocalAttribute("MOKey");
				FileNameOnly = string.Equals((string)xElement.LocalAttribute("FileNamesOnly"), "YES", StringComparison.OrdinalIgnoreCase);
			}
			Presets = new List<PolicyEnum>();
			PresetsAltDir = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			IEnumerable<XElement> enumerable = assetElement.LocalElements("Preset");
			if (enumerable != null)
			{
				foreach (XElement item in enumerable)
				{
					string text4 = (string)item.LocalAttribute("TargetFileName");
					Presets.Add(new PolicyEnum((string)item.LocalAttribute("DisplayName"), text4));
					string value = (string)item.LocalAttribute("AlternatePath");
					if (!string.IsNullOrEmpty(value))
					{
						PresetsAltDir.Add(text4, value);
					}
				}
			}
			DefinedIn = definedIn;
		}

		public bool IsValidFileType(string filename)
		{
			return FileTypes.Any((string type) => filename.EndsWith(type, StringComparison.OrdinalIgnoreCase));
		}

		public bool IsMatch(string value)
		{
			return PolicyMacroTable.IsMatch(Name, value, StringComparison.OrdinalIgnoreCase);
		}
	}
}
