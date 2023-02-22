using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class FailureActions : PkgPlugin
	{
		public override void ConvertEntries(XElement ToWm, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromPkg)
		{
			XElement xElement = new XElement(ToWm.Name.Namespace + "failureActions");
			foreach (XAttribute item in FromPkg.Attributes())
			{
				item.Value = enviorn.Macros.Resolve(item.Value);
				switch (item.Name.LocalName)
				{
				case "Command":
				case "ResetPeriod":
				case "RebootMessage":
				{
					string text = Helpers.lowerCamel(item.Name.LocalName);
					xElement.Add(new XAttribute(text, item.Value));
					break;
				}
				}
			}
			XElement xElement2 = new XElement(ToWm.Name.Namespace + "actions");
			xElement.Add(xElement2);
			base.ConvertEntries(xElement2, plugins, enviorn, FromPkg);
			ToWm.Add(xElement);
		}
	}
}
