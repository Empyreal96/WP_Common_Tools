using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.WmToCsi
{
	[Export(typeof(IPkgPlugin))]
	internal class Drivers : PkgPlugin
	{
		public override void ConvertEntries(XElement toCsi, Dictionary<string, IPkgPlugin> plugins, Config environ, XElement fromWm)
		{
			if (environ.build.wow != Build.WowType.guest && environ.build.satellite.Type == SatelliteType.Neutral)
			{
				if (environ.Convert.Equals(ConversionType.pkg2csi))
				{
					environ.Logger.LogInfo("Cannot convert drivers directly from pkg.xml to csi.");
				}
				else
				{
					base.ConvertEntries(toCsi, plugins, environ, fromWm);
				}
			}
		}
	}
}
