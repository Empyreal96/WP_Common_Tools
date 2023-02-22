using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class RegKeys : PkgPlugin
	{
		public override void ConvertEntries(XElement ToWm, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromPkg)
		{
			XElement xElement = new XElement(ToWm.Name.Namespace + "regKeys");
			string attributeValue = PkgBldrHelpers.GetAttributeValue(FromPkg, "buildFilter");
			if (attributeValue != null)
			{
				string value = Helpers.ConvertBuildFilter(attributeValue);
				xElement.Add(new XAttribute("buildFilter", value));
			}
			base.ConvertEntries(xElement, plugins, enviorn, FromPkg);
			if (xElement.HasElements)
			{
				string text = Helpers.GenerateWmBuildFilter(FromPkg, enviorn.Logger);
				if (text != null)
				{
					xElement.Add(new XAttribute("buildFilter", text));
				}
				ToWm.Add(xElement);
			}
		}
	}
}
