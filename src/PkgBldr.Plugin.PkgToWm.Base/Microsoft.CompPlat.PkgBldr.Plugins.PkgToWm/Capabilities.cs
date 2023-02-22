using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class Capabilities : PkgPlugin
	{
		public override void ConvertEntries(XElement ToWm, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromPkg)
		{
			XElement xElement = PkgBldrHelpers.AddIfNotFound(ToWm, "capabilities");
			base.ConvertEntries(xElement, plugins, enviorn, FromPkg);
			if (!xElement.HasElements)
			{
				xElement.Remove();
			}
		}
	}
}
