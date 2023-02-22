using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.WmToCsi
{
	[Export(typeof(IPkgPlugin))]
	internal class Macros : PkgPlugin
	{
		public override bool Pass(BuildPass pass)
		{
			return pass == BuildPass.MACRO_PASS;
		}

		public override void ConvertEntries(XElement parent, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement component)
		{
			base.ConvertEntries(parent, plugins, enviorn, component);
		}
	}
}
