using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.WmToCsi
{
	[Export(typeof(IPkgPlugin))]
	internal class Instrumentation : PkgPlugin
	{
		public override string XmlSchemaPath => "PkgBldr.Shared.Xsd\\SharedTypes.xsd";

		public override void ConvertEntries(XElement ToCsi, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromWm)
		{
			XElement parent = new XElement(FromWm);
			PkgBldrHelpers.ReplaceDefaultNameSpace(ref parent, FromWm.Name.Namespace, ToCsi.Name.Namespace);
			ToCsi.Add(parent);
		}
	}
}
