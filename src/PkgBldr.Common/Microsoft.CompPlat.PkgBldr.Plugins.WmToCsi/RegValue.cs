using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.WmToCsi
{
	[Export(typeof(IPkgPlugin))]
	internal class RegValue : PkgPlugin
	{
		public override void ConvertEntries(XElement ToCsi, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromWm)
		{
			XElement xElement = new XElement(ToCsi.Name.Namespace + "registryValue");
			string text = null;
			string text2 = null;
			string text3 = null;
			foreach (XAttribute item in FromWm.Attributes())
			{
				switch (item.Name.LocalName)
				{
				case "name":
					text = item.Value;
					xElement.Add(new XAttribute("name", text));
					break;
				case "value":
					text2 = enviorn.Macros.Resolve(item.Value);
					xElement.Add(new XAttribute("value", text2));
					break;
				case "type":
					text3 = item.Value;
					xElement.Add(new XAttribute("valueType", text3));
					break;
				case "buildFilter":
					xElement.Add(new XAttribute("buildFilter", item.Value));
					break;
				case "operationHint":
					xElement.Add(new XAttribute("operationHint", item.Value));
					break;
				}
			}
			ToCsi.Add(xElement);
		}
	}
}
