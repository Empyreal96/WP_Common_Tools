using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.WmToCsi
{
	[Export(typeof(IPkgPlugin))]
	internal class FirewallRule : PkgPlugin
	{
		public override string XmlSchemaPath => "PkgBldr.Shared.Xsd\\SharedTypes.xsd";

		public override string XmlSchemaNameSpace => "urn:Microsoft.CompPlat/ManifestSchema.v1.00";

		public override void ConvertEntries(XElement ToCsi, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromWm)
		{
			XElement xElement = new XElement(FromWm);
			PkgBldrHelpers.SetDefaultNameSpace(xElement, ToCsi.Name.Namespace);
			ToCsi.Add(xElement);
		}
	}
}
