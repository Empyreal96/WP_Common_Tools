using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class Capability : PkgPlugin
	{
		public override void ConvertEntries(XElement ToWm, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromPkg)
		{
			XElement xElement = new XElement(ToWm.Name.Namespace + "capability");
			foreach (XAttribute item in FromPkg.Attributes())
			{
				switch (item.Name.LocalName)
				{
				case "Id":
					xElement.Add(new XAttribute("id", item.Value));
					break;
				case "FriendlyName":
					xElement.Add(new XAttribute("friendlyName", item.Value));
					break;
				case "Visibility":
					xElement.Add(new XAttribute("visibility", item.Value));
					break;
				default:
					enviorn.Logger.LogWarning("<Capability> attribute not handled {0}", item.Name.LocalName);
					break;
				}
			}
			ToWm.Add(xElement);
			base.ConvertEntries(xElement, plugins, enviorn, FromPkg);
		}
	}
}
