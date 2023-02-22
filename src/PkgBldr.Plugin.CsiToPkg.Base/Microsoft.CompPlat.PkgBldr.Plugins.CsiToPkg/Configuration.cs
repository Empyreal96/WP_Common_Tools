using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.CsiToPkg
{
	[Export(typeof(IPkgPlugin))]
	internal class Configuration : PkgPlugin
	{
		public override void ConvertEntries(XElement ToPkg, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromCsi)
		{
			PkgBldrHelpers.SetDefaultNameSpace(FromCsi, FromCsi.Name.Namespace);
			IEnumerable<XElement> enumerable = FromCsi.Descendants(FromCsi.Name.Namespace + "element");
			MyContainter myContainter = (MyContainter)enviorn.arg;
			foreach (XElement item in enumerable)
			{
				IEnumerable<XAttribute> enumerable2 = item.Attributes();
				string text = null;
				string value = null;
				string text2 = null;
				string text3 = null;
				bool flag = false;
				foreach (XAttribute item2 in enumerable2)
				{
					switch (item2.Name.LocalName)
					{
					case "default":
						text = item2.Value;
						break;
					case "name":
						value = item2.Value;
						break;
					case "type":
						text2 = item2.Value;
						break;
					case "handler":
						text3 = item2.Value;
						break;
					}
				}
				if (text3 == null || !text3.StartsWith("regkey", StringComparison.OrdinalIgnoreCase) || text == null)
				{
					continue;
				}
				flag = false;
				text3 = text3.Split('\'')[1];
				switch (text2)
				{
				case "xsd:boolean":
					text2 = "REG_DWORD";
					text = ((!text.ToLowerInvariant().Equals("false")) ? "00000001" : "00000000");
					flag = true;
					break;
				case "xsd:string":
					text2 = "REG_SZ";
					flag = true;
					break;
				case "xsd:unsignedInt":
					text2 = "REG_DWORD";
					text = Convert.ToUInt32(text, CultureInfo.InvariantCulture).ToString("X8", CultureInfo.InvariantCulture);
					flag = true;
					break;
				default:
					Console.WriteLine("");
					break;
				}
				if (flag)
				{
					text3 = RegHelpers.RegKeyNameToMacro(text3);
					if (!string.IsNullOrEmpty(text3))
					{
						XElement xElement = new XElement(myContainter.RegKeys.Name.Namespace + "RegKey");
						xElement.Add(new XAttribute("KeyName", text3));
						XElement xElement2 = new XElement(myContainter.RegKeys.Name.Namespace + "RegValue");
						xElement2.Add(new XAttribute("Name", value));
						xElement2.Add(new XAttribute("Type", text2));
						xElement2.Add(new XAttribute("Value", text));
						xElement.Add(xElement2);
						Share.MergeNewPkgRegKey(myContainter.RegKeys, xElement);
					}
				}
			}
		}
	}
}
