using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class Action : PkgPlugin
	{
		public override void ConvertEntries(XElement ToWm, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromPkg)
		{
			XElement xElement = new XElement(ToWm.Name.Namespace + "action");
			foreach (XAttribute item in FromPkg.Attributes())
			{
				item.Value = enviorn.Macros.Resolve(item.Value);
				string localName = item.Name.LocalName;
				if (localName == "Type" || localName == "Delay")
				{
					string text = Helpers.lowerCamel(item.Name.LocalName);
					string value = Helpers.lowerCamel(item.Value);
					xElement.Add(new XAttribute(text, value));
				}
			}
			ToWm.Add(xElement);
		}
	}
}
