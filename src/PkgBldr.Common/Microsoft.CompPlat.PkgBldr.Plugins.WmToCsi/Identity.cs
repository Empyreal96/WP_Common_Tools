using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;
using Microsoft.CompPlat.PkgBldr.Tools;

namespace Microsoft.CompPlat.PkgBldr.Plugins.WmToCsi
{
	[Export(typeof(IPkgPlugin))]
	public class Identity : PkgPlugin
	{
		protected enum ManifestType
		{
			HostNeutral,
			GuestNeutral,
			HostLang,
			GuestLang,
			HostMultiLang,
			GuestMultiLang
		}

		protected static Dictionary<ManifestType, string> _table = new Dictionary<ManifestType, string>
		{
			{
				ManifestType.GuestLang,
				"GL_"
			},
			{
				ManifestType.GuestMultiLang,
				"GM_"
			},
			{
				ManifestType.GuestNeutral,
				"GN_"
			},
			{
				ManifestType.HostLang,
				"HL_"
			},
			{
				ManifestType.HostMultiLang,
				"HM_"
			},
			{
				ManifestType.HostNeutral,
				"HN_"
			}
		};

		public override string XmlSchemaPath => "PkgBldr.WM.Xsd\\Common.xsd";

		public static string WmIdentityNameToCsiAssemblyName(string owner, string nameSpace, string name, string legacyName)
		{
			string text = null;
			if (legacyName == null)
			{
				text = owner;
				if (nameSpace != null)
				{
					text = text + "-" + nameSpace;
				}
				return text + "-" + name;
			}
			return legacyName;
		}

		protected static string UpdateOutputPath(string outputPath, ManifestType manType)
		{
			string directoryName = LongPath.GetDirectoryName(outputPath);
			string text = LongPath.GetFileName(outputPath);
			if (text[2].Equals('_'))
			{
				text = text.Remove(0, 3);
			}
			text = _table[manType] + text;
			switch (manType)
			{
			case ManifestType.HostLang:
			case ManifestType.GuestLang:
			case ManifestType.HostMultiLang:
			case ManifestType.GuestMultiLang:
				if (!text.EndsWith(".Resources.man", StringComparison.OrdinalIgnoreCase))
				{
					text = text.Replace(".man", ".Resources.man");
				}
				break;
			}
			return LongPath.Combine(directoryName, text);
		}

		public override void ConvertEntries(XElement ToCsi, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromWm)
		{
			enviorn.ExitStatus = ExitStatus.SUCCESS;
			ConvertWmXmlBuildFiltersToCsiFormat(FromWm);
			enviorn.Macros = new MacroResolver();
			enviorn.Macros.Load(XmlReader.Create(PkgGenResources.GetResourceStream("Macros_WmToCsi.xml")));
			if (enviorn.Bld.BuildMacros != null)
			{
				Dictionary<string, Microsoft.CompPlat.PkgBldr.Base.Macro> macroTable = enviorn.Bld.BuildMacros.GetMacroTable();
				enviorn.Macros.Register(macroTable, true);
			}
			if (enviorn.Bld.Lang != "neutral")
			{
				if (enviorn.Bld.Lang == "*")
				{
					enviorn.Bld.Lang = "en-us";
				}
				if (FromWm.Element(FromWm.Name.Namespace + "language") == null)
				{
					enviorn.ExitStatus = ExitStatus.SKIPPED;
					return;
				}
			}
			if (enviorn.Bld.Resolution != null)
			{
				enviorn.ExitStatus = ExitStatus.SKIPPED;
				return;
			}
			if (enviorn.build.wow == Build.WowType.guest)
			{
				string attributeValue = PkgBldrHelpers.GetAttributeValue(FromWm, "buildWow");
				if (attributeValue == null)
				{
					enviorn.ExitStatus = ExitStatus.SKIPPED;
					return;
				}
				if (!attributeValue.Equals("true", StringComparison.OrdinalIgnoreCase))
				{
					enviorn.ExitStatus = ExitStatus.SKIPPED;
					return;
				}
			}
			XNamespace xNamespace = "urn:schemas-microsoft-com:asm.v3";
			ToCsi.Name = xNamespace + ToCsi.Name.LocalName;
			ToCsi.Add(new XAttribute("xmlns", "urn:schemas-microsoft-com:asm.v3"));
			ToCsi.Add(new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"));
			ToCsi.Add(new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"));
			ToCsi.Add(new XAttribute("manifestVersion", "1.0"));
			enviorn.Bld.CSI.Root = ToCsi;
			enviorn.Bld.WM.Root = FromWm;
			XElement xElement = new XElement(ToCsi.Name.Namespace + "assemblyIdentity");
			string value = "$(build.buildType)";
			string value2 = "$(build.arch)";
			string value3 = "$(build.WindowsPublicKeyToken)";
			string value4 = "$(build.version)";
			string attributeValue2 = PkgBldrHelpers.GetAttributeValue(FromWm, "owner");
			string attributeValue3 = PkgBldrHelpers.GetAttributeValue(FromWm, "namespace");
			string attributeValue4 = PkgBldrHelpers.GetAttributeValue(FromWm, "name");
			string attributeValue5 = PkgBldrHelpers.GetAttributeValue(FromWm, "legacyName");
			string text = WmIdentityNameToCsiAssemblyName(attributeValue2, attributeValue3, attributeValue4, attributeValue5);
			enviorn.Bld.CSI.Name = text;
			xElement.Add(new XAttribute("name", text));
			xElement.Add(new XAttribute("language", "neutral"));
			xElement.Add(new XAttribute("buildType", value));
			xElement.Add(new XAttribute("processorArchitecture", value2));
			xElement.Add(new XAttribute("publicKeyToken", value3));
			xElement.Add(new XAttribute("version", value4));
			xElement.Add(new XAttribute("versionScope", "nonSxS"));
			if (enviorn.AutoGenerateOutput)
			{
				enviorn.Output = LongPath.Combine(enviorn.Output, text + ".man");
			}
			switch (enviorn.build.wow)
			{
			case Build.WowType.host:
				switch (enviorn.build.satellite.Type)
				{
				case SatelliteType.Neutral:
					enviorn.Output = UpdateOutputPath(enviorn.Output, ManifestType.HostNeutral);
					break;
				case SatelliteType.Language:
					enviorn.Output = UpdateOutputPath(enviorn.Output, ManifestType.HostLang);
					break;
				}
				break;
			case Build.WowType.guest:
				switch (enviorn.build.satellite.Type)
				{
				case SatelliteType.Neutral:
					enviorn.Output = UpdateOutputPath(enviorn.Output, ManifestType.GuestNeutral);
					break;
				case SatelliteType.Language:
					enviorn.Output = UpdateOutputPath(enviorn.Output, ManifestType.GuestLang);
					break;
				}
				break;
			}
			if (enviorn.Bld.Product != "windows")
			{
				xElement.Add(new XAttribute("product", "$(build.product)"));
			}
			ToCsi.Add(xElement);
			if (enviorn.build.satellite.Type == SatelliteType.Language)
			{
				RemoveNeutralContent(enviorn.Bld.WM.Root);
			}
			BuildPass[] array = (BuildPass[])Enum.GetValues(typeof(BuildPass));
			foreach (BuildPass pass in array)
			{
				enviorn.Pass = pass;
				ProcessDesendents(ToCsi, plugins, enviorn, FromWm);
			}
		}

		protected void ProcessDesendents(XElement toCsi, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement fromWm)
		{
			base.ConvertEntries(toCsi, plugins, enviorn, fromWm);
		}

		private void RemoveNeutralContent(XElement wmRoot)
		{
			List<XElement> list = new List<XElement>();
			foreach (XElement item in wmRoot.Elements())
			{
				string localName = item.Name.LocalName;
				if (!(localName == "macros") && !(localName == "language"))
				{
					list.Add(item);
				}
			}
			foreach (XElement item2 in list)
			{
				item2.Remove();
			}
		}

		private void ConvertWmXmlBuildFiltersToCsiFormat(XElement windowsManifest)
		{
			foreach (XElement item in from el in windowsManifest.Descendants()
				where el.Attribute("buildFilter") != null
				select el)
			{
				string value = item.Attribute("buildFilter").Value;
				item.Attribute("buildFilter").Value = ConvertBuildFilterToCSI(value);
			}
		}

		private static string ConvertBuildFilterToCSI(string wmBuildFilter)
		{
			string text = wmBuildFilter;
			foreach (Match item in new Regex("\\([^\\)]+\\)", RegexOptions.IgnoreCase).Matches(text))
			{
				if (!item.Value.ToLowerInvariant().Contains(" or ") && !item.Value.ToLowerInvariant().Contains(" and "))
				{
					string newValue = item.Value.Trim("()".ToCharArray());
					text = text.Replace(item.Value, newValue);
				}
			}
			return text;
		}
	}
}
