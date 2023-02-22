using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.WmToCsi
{
	[Export(typeof(IPkgPlugin))]
	internal class RegKey : PkgPlugin
	{
		public override void ConvertEntries(XElement toCsi, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement fromWm)
		{
			string attributeValue = PkgBldrHelpers.GetAttributeValue(fromWm.Parent, "buildFilter");
			XElement xElement = new XElement(toCsi.Name.Namespace + "registryKey");
			if (attributeValue != null)
			{
				xElement.Add(new XAttribute("buildFilter", attributeValue));
			}
			XElement xElement2 = null;
			foreach (XAttribute item in fromWm.Attributes())
			{
				switch (item.Name.LocalName)
				{
				case "keyName":
					xElement.Add(new XAttribute("keyName", enviorn.Macros.Resolve(item.Value)));
					break;
				case "buildFilter":
					if (attributeValue != null)
					{
						enviorn.Logger.LogWarning("TBD: Can't aggregate build filters from:");
						enviorn.Logger.LogWarning("   RegKeys = {0}", attributeValue);
						enviorn.Logger.LogWarning("   RegKey  = {0}", item.Value);
						enviorn.Logger.LogWarning("Using {0}", attributeValue);
					}
					else
					{
						xElement.Add(new XAttribute("buildFilter", item.Value));
					}
					break;
				case "securityDescriptor":
					xElement2 = new XElement(toCsi.Name.Namespace + "securityDescriptor");
					xElement2.Add(new XAttribute("name", enviorn.Macros.Resolve(item.Value)));
					break;
				case "owner":
					xElement.Add(new XAttribute("owner", item.Value));
					break;
				default:
					throw new PkgGenException("Attribute not supported {0}", item.Name.LocalName);
				}
			}
			base.ConvertEntries(xElement, plugins, enviorn, fromWm);
			if (xElement2 != null)
			{
				xElement.Add(xElement2);
			}
			toCsi.Add(xElement);
		}
	}
}
