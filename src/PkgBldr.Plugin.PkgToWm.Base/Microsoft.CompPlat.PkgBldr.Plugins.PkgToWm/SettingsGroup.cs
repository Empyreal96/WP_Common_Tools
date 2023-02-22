using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class SettingsGroup : PkgPlugin
	{
		public override void ConvertEntries(XElement toWindowsManifest, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement settingsGroup)
		{
			toWindowsManifest.Add(settingsGroup);
		}
	}
}
