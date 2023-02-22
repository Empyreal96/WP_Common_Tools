using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class Service : PkgPlugin
	{
		public override void ConvertEntries(XElement ToWm, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromPkg)
		{
			XElement xElement = new XElement(ToWm.Name.Namespace + "service");
			foreach (XAttribute item in FromPkg.Attributes())
			{
				item.Value = enviorn.Macros.Resolve(item.Value);
				switch (item.Name.LocalName)
				{
				case "Name":
					xElement.Add(new XAttribute("name", item.Value));
					break;
				case "Start":
				{
					string value2 = Helpers.lowerCamel(item.Value);
					xElement.Add(new XAttribute("start", value2));
					break;
				}
				case "SvcHostGroupName":
				{
					string value4 = "%SystemRoot%\\System32\\svchost.exe -k " + item.Value;
					xElement.Add(new XAttribute("imagePath", value4));
					break;
				}
				case "DisplayName":
					xElement.Add(new XAttribute("displayName", item.Value));
					break;
				case "Description":
					xElement.Add(new XAttribute("description", item.Value));
					break;
				case "Group":
					xElement.Add(new XAttribute("group", item.Value));
					break;
				case "Type":
				{
					string value3 = Helpers.lowerCamel(item.Value);
					xElement.Add(new XAttribute("type", value3));
					break;
				}
				case "DependOnGroup":
					xElement.Add(new XAttribute("dependOnGroup", item.Value));
					break;
				case "DependOnService":
					xElement.Add(new XAttribute("dependOnService", item.Value));
					break;
				case "ErrorControl":
				{
					string text = null;
					switch (item.Value)
					{
					case "Normal":
						text = "normal";
						break;
					case "Critical":
						text = "critical";
						break;
					case "Ignore":
						text = "ignore";
						break;
					case "Severe":
						text = "critical";
						enviorn.Logger.LogWarning("Windows manifest does not support service error type severe, setting to critical");
						break;
					default:
						enviorn.Logger.LogWarning("Windows manifest does not support service error type {0}, setting to normal", item.Value);
						text = "normal";
						break;
					}
					xElement.Add(new XAttribute("errorControl", text));
					break;
				}
				case "IsTCB":
					if (item.Value.Equals("Yes", StringComparison.InvariantCultureIgnoreCase))
					{
						xElement.Add(new XAttribute("objectName", "LocalSystem"));
					}
					break;
				case "buildFilter":
				{
					string value = Helpers.ConvertBuildFilter(item.Value);
					xElement.Add(new XAttribute("buildFilter", value));
					break;
				}
				default:
					enviorn.Logger.LogWarning("Unknown service type {0}", item.Name.LocalName);
					break;
				}
			}
			base.ConvertEntries(xElement, plugins, enviorn, FromPkg);
			foreach (XElement item2 in xElement.Elements(xElement.Name.Namespace + "regKeys"))
			{
				ToWm.Add(item2);
				item2.Remove();
			}
			IEnumerable<XElement> enumerable = xElement.Elements(xElement.Name.Namespace + "files");
			foreach (XElement item3 in enumerable)
			{
				XElement xElement2 = item3;
				ToWm.Add(enumerable);
				enumerable.Remove();
			}
			ToWm.Add(xElement);
		}
	}
}
