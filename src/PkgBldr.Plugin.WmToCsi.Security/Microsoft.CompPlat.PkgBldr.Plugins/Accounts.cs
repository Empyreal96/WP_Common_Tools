using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins
{
	[Export(typeof(IPkgPlugin))]
	internal class Accounts : PkgPlugin
	{
		private static string securityComponentSchemaPath = "PkgBldr.WM.Xsd\\SecurityPlugin.xsd";

		public override string XmlSchemaPath => securityComponentSchemaPath;

		public override string XmlElementName => "accounts";

		public override void ConvertEntries(XElement root, Dictionary<string, IPkgPlugin> plugins, Config environ, XElement component)
		{
			XElement xElement = new XElement("trustInfo");
			XElement xElement2 = new XElement("security");
			XElement xElement3 = new XElement("accessControl");
			XElement xElement4 = new XElement("trustees");
			base.ConvertEntries(xElement4, plugins, environ, component);
			xElement3.Add(xElement4);
			xElement2.Add(xElement3);
			xElement.Add(xElement2);
			PkgBldrHelpers.SetDefaultNameSpace(xElement, root.Name.Namespace);
			environ.Bld.CSI.Root.Add(xElement);
		}
	}
}
