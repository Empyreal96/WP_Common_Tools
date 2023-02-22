using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins
{
	[Export(typeof(IPkgPlugin))]
	internal class Capabilities : PkgPlugin
	{
		public override string XmlSchemaPath => "PkgBldr.WM.Xsd\\SecurityPlugin.xsd";

		public override void ConvertEntries(XElement parent, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement component)
		{
			base.ConvertEntries(parent, plugins, enviorn, component);
		}

		protected string GetAttributeValue(XElement element, string attributeName)
		{
			return element.Attribute(attributeName)?.Value;
		}
	}
}
