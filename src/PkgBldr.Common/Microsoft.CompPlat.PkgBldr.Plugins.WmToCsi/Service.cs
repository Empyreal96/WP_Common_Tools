using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.WmToCsi
{
	[Export(typeof(IPkgPlugin))]
	internal class Service : PkgPlugin
	{
		public override void ConvertEntries(XElement ToCsi, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromWm)
		{
			string attributeValue = PkgBldrHelpers.GetAttributeValue(FromWm, "buildFilter");
			string attributeValue2 = PkgBldrHelpers.GetAttributeValue(FromWm, "subCategory");
			XElement xElement = Membership.Add(ToCsi, attributeValue, attributeValue2, "Microsoft.Windows.Categories.Services", "$(build.version)", "$(build.WindowsPublicKeyToken)", "Service");
			XElement xElement2 = new XElement(ToCsi.Name.Namespace + "serviceData");
			xElement.Add(xElement2);
			string text = null;
			foreach (XAttribute item in FromWm.Attributes())
			{
				switch (item.Name.LocalName)
				{
				case "securityDescriptor":
					text = enviorn.Macros.Resolve(item.Value);
					break;
				default:
					xElement2.Add(new XAttribute(item.Name.LocalName, item.Value));
					break;
				case "subCategory":
				case "buildFilter":
					break;
				}
			}
			if (text != null)
			{
				XElement xElement3 = new XElement(ToCsi.Name.Namespace + "securityDescriptor");
				xElement3.Add(new XAttribute("name", text));
				xElement2.Add(xElement3);
			}
			base.ConvertEntries(xElement2, plugins, enviorn, FromWm);
		}
	}
}
