using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.WmToCsi
{
	[Export(typeof(IPkgPlugin))]
	internal class Directory : PkgPlugin
	{
		public override void ConvertEntries(XElement ToCsi, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromWm)
		{
			XElement xElement = new XElement(ToCsi.Name.Namespace + "directory");
			string text = null;
			foreach (XAttribute item in FromWm.Attributes())
			{
				switch (item.Name.LocalName)
				{
				case "path":
				{
					string value2 = enviorn.Macros.Resolve(item.Value).TrimEnd('\\');
					xElement.Add(new XAttribute("destinationPath", value2));
					break;
				}
				case "securityDescriptor":
					text = enviorn.Macros.Resolve(item.Value);
					break;
				case "attributes":
				{
					string value = enviorn.Macros.Resolve(item.Value);
					xElement.Add(new XAttribute("attributes", value));
					break;
				}
				case "owner":
					xElement.Add(new XAttribute("owner", item.Value));
					break;
				default:
					throw new PkgGenException("Unknow directory attribute {0}", item.Name.LocalName);
				}
			}
			if (text != null)
			{
				XElement xElement2 = new XElement(ToCsi.Name.Namespace + "securityDescriptor");
				xElement2.Add(new XAttribute("name", text));
				xElement.Add(xElement2);
			}
			ToCsi.Add(xElement);
		}
	}
}
