using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.WmToCsi
{
	[Export(typeof(IPkgPlugin))]
	internal class Task : PkgPlugin
	{
		public override string XmlSchemaPath => "PkgBldr.WM.Xsd\\Task.xsd";

		public override string XmlSchemaNameSpace => "urn:Microsoft.CompPlat/ManifestSchema.v1.00";

		public override void ConvertEntries(XElement ToCsi, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromWm)
		{
			XElement parent = new XElement(FromWm);
			XNamespace xNamespace = "http://schemas.microsoft.com/windows/2004/02/mit/task";
			PkgBldrHelpers.ReplaceDefaultNameSpace(ref parent, FromWm.Name.Namespace, xNamespace);
			base.ConvertEntries(ToCsi, plugins, enviorn, FromWm);
			foreach (XElement item in parent.Descendants(xNamespace + "privateResources").ToList())
			{
				item.Remove();
			}
			ToCsi.Add(parent);
		}
	}
}
