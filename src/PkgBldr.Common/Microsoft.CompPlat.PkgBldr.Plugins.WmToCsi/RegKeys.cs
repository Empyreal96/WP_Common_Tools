using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.WmToCsi
{
	[Export(typeof(IPkgPlugin))]
	internal class RegKeys : PkgPlugin
	{
		public override void ConvertEntries(XElement toCsi, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement fromWm)
		{
			XElement parent = PkgBldrHelpers.AddIfNotFound(toCsi, "registryKeys");
			base.ConvertEntries(parent, plugins, enviorn, fromWm);
		}
	}
}
