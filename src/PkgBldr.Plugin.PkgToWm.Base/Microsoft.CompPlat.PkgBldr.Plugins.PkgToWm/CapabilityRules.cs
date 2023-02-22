using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;
using Microsoft.CompPlat.PkgBldr.Tools;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class CapabilityRules : PkgPlugin
	{
		public override void ConvertEntries(XElement ToWm, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromPkg)
		{
			XElement xElement = new XElement(ToWm.Name.Namespace + "capabilityRules");
			foreach (XElement item in FromPkg.Elements())
			{
				XElement xElement2 = ConvertCapRule(xElement.Name.Namespace, item, enviorn.Macros, ref enviorn.ExitStatus, enviorn.Logger);
				if (xElement2 != null)
				{
					xElement.Add(xElement2);
				}
			}
			if (xElement.HasElements)
			{
				ToWm.Add(xElement);
			}
		}

		private XElement ConvertCapRule(XNamespace wmNamespace, XElement PkgCapRule, MacroResolver macros, ref ExitStatus exitStatus, IDeploymentLogger logger)
		{
			string localName = PkgCapRule.Name.LocalName;
			switch (PkgCapRule.Name.LocalName)
			{
			case "File":
				localName = "file";
				break;
			case "Directory":
				localName = "directory";
				break;
			case "RegKey":
				localName = "regKey";
				break;
			case "WNF":
				localName = "wnf";
				break;
			case "ETWProvider":
				localName = "etwProvider";
				break;
			case "COM":
				localName = "com";
				break;
			case "SDRegValue":
				localName = "sdRegValue";
				break;
			case "ServiceAccess":
				localName = "serviceAccess";
				break;
			case "TransientObject":
				localName = "transientObject";
				break;
			default:
				logger.LogWarning("<CapabilityRule> {0} not converted", PkgCapRule.Name.LocalName);
				return null;
			}
			XElement xElement = new XElement(wmNamespace + localName);
			foreach (XAttribute item in PkgCapRule.Attributes())
			{
				if (item.Name.LocalName.Equals("Path"))
				{
					item.Value = macros.Resolve(item.Value);
				}
				string localName2 = item.Name.LocalName;
				XAttribute content = new XAttribute(Helpers.lowerCamel(item.Name.LocalName), item.Value);
				xElement.Add(content);
			}
			return xElement;
		}
	}
}
