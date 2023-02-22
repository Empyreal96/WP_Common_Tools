using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.WmToCsi
{
	[Export(typeof(IPkgPlugin))]
	internal class Language : Identity
	{
		public override string XmlSchemaPath => "PkgBldr.WM.Xsd\\Common.xsd";

		public override void ConvertEntries(XElement toCsi, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement fromWm)
		{
			XElement xElement = toCsi.Element(toCsi.Name.Namespace + "assemblyIdentity");
			if (enviorn.build.satellite.Type == SatelliteType.Neutral)
			{
				XElement xElement2 = new XElement(xElement);
				xElement2.Attribute("name").Value += ".Resources";
				xElement2.Attribute("language").Value = "*";
				XElement xElement3 = new XElement(toCsi.Name.Namespace + "dependentAssembly");
				xElement3.Add(new XAttribute("dependencyType", "prerequisite"));
				xElement3.Add(xElement2);
				XElement xElement4 = new XElement(toCsi.Name.Namespace + "dependency");
				xElement4.Add(new XAttribute("discoverable", "false"));
				xElement4.Add(new XAttribute("optional", "false"));
				xElement4.Add(new XAttribute("resourceType", "Resources"));
				xElement4.Add(xElement3);
				toCsi.Add(xElement4);
				string attributeValue = PkgBldrHelpers.GetAttributeValue(fromWm, "buildFilter");
				if (attributeValue != null)
				{
					xElement4.Add(new XAttribute("buildFilter", attributeValue));
				}
			}
			else
			{
				if (enviorn.build.satellite.Type != SatelliteType.Language)
				{
					return;
				}
				XElement xElement5 = new XElement(xElement);
				xElement5.Attribute("name").Value += ".Resources";
				xElement5.Attribute("language").Value = enviorn.Bld.Lang;
				xElement.Remove();
				toCsi.Add(xElement5);
				string attributeValue2 = PkgBldrHelpers.GetAttributeValue(fromWm, "multilingual");
				if (attributeValue2 != null && attributeValue2.Equals("true", StringComparison.OrdinalIgnoreCase))
				{
					switch (enviorn.build.wow)
					{
					case Build.WowType.host:
						enviorn.Output = Identity.UpdateOutputPath(enviorn.Output, ManifestType.HostMultiLang);
						break;
					case Build.WowType.guest:
						enviorn.Output = Identity.UpdateOutputPath(enviorn.Output, ManifestType.GuestMultiLang);
						break;
					}
				}
				ProcessDesendents(toCsi, plugins, enviorn, fromWm);
			}
		}
	}
}
