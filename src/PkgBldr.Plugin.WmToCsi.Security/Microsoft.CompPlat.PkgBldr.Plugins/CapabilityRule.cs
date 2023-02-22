using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins
{
	[Export(typeof(IPkgPlugin))]
	internal class CapabilityRule : Authorization
	{
		private static string capabilityClassRegKeyName = "AuthorizationRules\\Capability\\";

		public override string XmlElementName => "capabilityRule";

		public override void ConvertEntries(XElement authorizationRegKeys, Dictionary<string, IPkgPlugin> plugins, Config environ, XElement component)
		{
			XElement xElement = new XElement(authorizationRegKeys.Name.Namespace + "registryKey");
			xElement.Add(new XAttribute("keyName", Authorization.authorizationRegKeyRoot + capabilityClassRegKeyName + component.Attribute("name").Value));
			XElement xElement2 = new XElement(authorizationRegKeys.Name.Namespace + "registryValue");
			XElement xElement3 = new XElement(authorizationRegKeys.Name.Namespace + "registryValue");
			xElement2.Add(new XAttribute("name", "CapabilityClass"));
			xElement2.Add(new XAttribute("value", component.Attribute("capabilityClass").Value));
			xElement2.Add(new XAttribute("valueType", "REG_SZ"));
			xElement3.Add(new XAttribute("name", "PrincipalClass"));
			xElement3.Add(new XAttribute("value", component.Attribute("principalClass").Value));
			xElement3.Add(new XAttribute("valueType", "REG_SZ"));
			xElement.Add(xElement2);
			xElement.Add(xElement3);
			XElement xElement4 = environ.Bld.CSI.Root.Element(environ.Bld.CSI.Root.Name.Namespace + "registryKeys");
			if (xElement4 == null)
			{
				xElement4 = new XElement(environ.Bld.CSI.Root.Name.Namespace + "registryKeys");
				environ.Bld.CSI.Root.Add(xElement4);
			}
			xElement4.Add(xElement);
		}
	}
}
