using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.WmToCsi
{
	[Export(typeof(IPkgPlugin))]
	internal class BootCritical : PkgPlugin
	{
		public override void ConvertEntries(XElement toCsi, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement fromWm)
		{
			string attributeValue = PkgBldrHelpers.GetAttributeValue(fromWm, "buildFilter");
			Membership.Add(toCsi, attributeValue, null, "Microsoft.Windows.Categories", "1.0.0.0", "365143bb27e7ac8b", "BootCritical").Remove();
		}
	}
}
