using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.WmToCsi
{
	[Export(typeof(IPkgPlugin))]
	internal class ServiceGroup : PkgPlugin
	{
		public override void ConvertEntries(XElement toCsi, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement fromWm)
		{
			string attributeValue = PkgBldrHelpers.GetAttributeValue(fromWm, "serviceName");
			string attributeValue2 = PkgBldrHelpers.GetAttributeValue(fromWm, "groupName");
			string attributeValue3 = PkgBldrHelpers.GetAttributeValue(fromWm, "buildFilter");
			string attributeValue4 = PkgBldrHelpers.GetAttributeValue(fromWm, "position");
			XElement xElement = Membership.Add(toCsi, attributeValue3, attributeValue2, "Microsoft.Windows.Categories", "1.0.0.0", "365143bb27e7ac8b", "SvcHost");
			XElement xElement2 = new XElement(toCsi.Name.Namespace + "serviceGroup");
			xElement.Add(xElement2);
			xElement2.Add(new XAttribute("serviceName", attributeValue));
			if (attributeValue4 != null)
			{
				xElement2.Add(new XAttribute("position", attributeValue4));
			}
		}
	}
}
