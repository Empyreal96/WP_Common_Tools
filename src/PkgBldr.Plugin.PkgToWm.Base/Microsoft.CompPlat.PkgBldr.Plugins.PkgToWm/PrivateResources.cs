using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class PrivateResources : PkgPlugin
	{
		public override void ConvertEntries(XElement ToWm, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromPkg)
		{
			XElement xElement = new XElement(ToWm.Name.Namespace + "privateResources");
			foreach (XElement item in FromPkg.Elements())
			{
				XElement xElement2 = new XElement(ToWm.Name.Namespace + Helpers.lowerCamel(item.Name.LocalName));
				foreach (XAttribute item2 in item.Attributes())
				{
					if (item2.Name.LocalName == "Path")
					{
						item2.Value = enviorn.Macros.Resolve(item2.Value);
					}
					xElement2.Add(new XAttribute(Helpers.lowerCamel(item2.Name.LocalName), item2.Value));
				}
				xElement.Add(xElement2);
			}
			if (xElement.HasElements)
			{
				ToWm.Add(xElement);
			}
		}
	}
}
