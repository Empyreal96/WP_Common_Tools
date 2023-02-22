using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins
{
	[Export(typeof(IPkgPlugin))]
	internal class Authorization : PkgPlugin
	{
		private static string securityComponentSchemaPath = "PkgBldr.WM.Xsd\\SecurityPlugin.xsd";

		protected static string authorizationRegKeyRoot = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\SecurityManager\\";

		public override string XmlSchemaPath => securityComponentSchemaPath;

		public override string XmlElementName => "authorization";

		public override void ConvertEntries(XElement root, Dictionary<string, IPkgPlugin> plugins, Config environ, XElement component)
		{
			XElement xElement = new XElement(root.Name.Namespace + "registryKeys");
			base.ConvertEntries(xElement, plugins, environ, component);
			root.Add(xElement);
		}
	}
}
