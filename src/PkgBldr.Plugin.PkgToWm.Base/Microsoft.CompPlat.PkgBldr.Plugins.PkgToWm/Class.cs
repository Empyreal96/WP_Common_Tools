using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class Class : PkgPlugin
	{
		public override void ConvertEntries(XElement toWm, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement fromPkg)
		{
			ComData obj = (ComData)enviorn.arg;
			XElement xElement = new XElement(toWm.Name.Namespace + "classDefinition");
			obj.InProcServerClasses.Add(xElement);
			bool flag = true;
			foreach (XAttribute item in fromPkg.Attributes())
			{
				item.Value = enviorn.Macros.Resolve(item.Value);
				switch (item.Name.LocalName)
				{
				case "AppId":
					xElement.Add(new XAttribute("appId", item.Value));
					break;
				case "ProgId":
					xElement.Add(new XAttribute("progId", item.Value));
					break;
				case "VersionIndependentProgId":
					xElement.Add(new XAttribute("versionIndependentProgId", item.Value));
					break;
				case "DefaultIcon":
					xElement.Add(new XAttribute("defaultIcon", item.Value));
					break;
				case "Id":
					xElement.Add(new XAttribute("id", item.Value));
					break;
				case "TypeLib":
					xElement.Add(new XAttribute("typeLib", item.Value));
					break;
				case "Version":
				{
					string text2 = item.Value;
					if (!text2.Contains('.'))
					{
						text2 += ".0";
					}
					xElement.Add(new XAttribute("version", text2));
					break;
				}
				case "ThreadingModel":
				{
					string text = Helpers.ComConvertThreading(item.Value, enviorn.Logger);
					if (text != null)
					{
						xElement.Add(new XAttribute("threading", text));
						flag = false;
					}
					break;
				}
				case "Description":
					xElement.Add(new XAttribute("name", item.Value));
					break;
				}
			}
			if (flag)
			{
				enviorn.Logger.LogWarning("<Class> COM threading not specified, setting it to \"Both\"");
				xElement.Add(new XAttribute("threading", "Both"));
			}
		}
	}
}
