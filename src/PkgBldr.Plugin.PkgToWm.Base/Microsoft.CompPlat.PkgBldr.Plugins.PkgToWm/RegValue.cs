using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;
using Microsoft.CompPlat.PkgBldr.Tools;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class RegValue : PkgPlugin
	{
		public override void ConvertEntries(XElement ToWm, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromPkg)
		{
			XElement xElement = new XElement(ToWm.Name.Namespace + "regValue");
			string attributeValue = PkgBldrHelpers.GetAttributeValue(FromPkg, "buildFilter");
			if (attributeValue != null)
			{
				string value = Helpers.ConvertBuildFilter(attributeValue);
				xElement.Add(new XAttribute("buildFilter", value));
			}
			string pkgValue = null;
			string text = null;
			foreach (XAttribute item in FromPkg.Attributes())
			{
				item.Value = enviorn.Macros.Resolve(item.Value);
				switch (item.Name.LocalName)
				{
				case "Name":
					if (!item.Value.Equals("@"))
					{
						xElement.Add(new XAttribute("name", item.Value));
					}
					break;
				case "Type":
					text = item.Value;
					xElement.Add(new XAttribute("type", text));
					break;
				case "Value":
					pkgValue = item.Value;
					break;
				}
			}
			pkgValue = ConvertValue(pkgValue, text, enviorn.Logger);
			if (pkgValue != null)
			{
				xElement.Add(new XAttribute("value", pkgValue));
				ToWm.Add(xElement);
			}
		}

		private string ConvertValue(string pkgValue, string wmType, IDeploymentLogger logger)
		{
			string text = null;
			if (string.IsNullOrEmpty(pkgValue))
			{
				return null;
			}
			switch (wmType)
			{
			case "REG_BINARY":
			{
				string[] array = pkgValue.Split(',');
				foreach (string text2 in array)
				{
					text = ((text2.Length != 1) ? (text + text2) : (text + string.Format(CultureInfo.InvariantCulture, "0{0}", new object[1] { text2 })));
				}
				if (text == null)
				{
					text = pkgValue;
				}
				text = text.ToUpperInvariant();
				break;
			}
			case "REG_DWORD":
				text = pkgValue.TrimStart("0x".ToCharArray()).PadLeft(8, '0');
				text = "0x" + text;
				break;
			case "REG_QWORD":
				text = pkgValue.TrimStart("0x".ToCharArray()).PadLeft(16, '0');
				break;
			case "REG_MULTI_SZ":
				text = Helpers.ConvertMulitSz(pkgValue);
				break;
			case "REG_HEX":
				logger.LogWarning("not coverting pkg.xml REG_HEX value");
				break;
			default:
				text = pkgValue;
				break;
			}
			return text;
		}
	}
}
