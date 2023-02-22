using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins
{
	[Export(typeof(IPkgPlugin))]
	internal class Account : PkgPlugin
	{
		private static string securityComponentSchemaPath = "PkgBldr.WM.Xsd\\SecurityPlugin.xsd";

		public override string XmlSchemaPath => securityComponentSchemaPath;

		public override string XmlElementName => "account";

		public override void ConvertEntries(XElement trustees, Dictionary<string, IPkgPlugin> plugins, Config environ, XElement component)
		{
			XElement xElement = new XElement("groupTrustee");
			xElement.Add(new XAttribute("name", component.Attribute("name").Value));
			xElement.Add(new XAttribute("description", component.Attribute("description").Value));
			xElement.Add(new XAttribute("type", "VirtualAccount"));
			base.ConvertEntries(xElement, plugins, environ, component);
			trustees.Add(xElement);
		}
	}
}
