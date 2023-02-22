using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class Macros : PkgPlugin
	{
		public override bool Pass(BuildPass pass)
		{
			return pass == BuildPass.MACRO_PASS;
		}

		public override void ConvertEntries(XElement ToWm, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromPkg)
		{
			XElement xElement = new XElement(ToWm.Name.Namespace + "macros");
			base.ConvertEntries(xElement, plugins, enviorn, FromPkg);
			if (xElement.HasElements)
			{
				ToWm.Add(xElement);
			}
		}
	}
}
