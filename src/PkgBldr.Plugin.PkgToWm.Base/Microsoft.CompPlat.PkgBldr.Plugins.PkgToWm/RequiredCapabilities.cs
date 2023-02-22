using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class RequiredCapabilities : PkgPlugin
	{
		public override void ConvertEntries(XElement ToWm, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromPkg)
		{
			XElement xElement = new XElement(ToWm.Name.Namespace + "requiredCapabilities");
			foreach (XElement item in FromPkg.Elements())
			{
				XElement xElement2 = new XElement(ToWm.Name.Namespace + "requiredCapability");
				string attributeValue = PkgBldrHelpers.GetAttributeValue(item, "CapId");
				xElement2.Add(new XAttribute("id", attributeValue));
				xElement.Add(xElement2);
			}
			if (xElement.HasElements)
			{
				ToWm.Add(xElement);
			}
		}
	}
}
