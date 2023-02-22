using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class Executable : PkgPlugin
	{
		public override void ConvertEntries(XElement toWm, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement fromPkg)
		{
			XElement xElement = new XElement(fromPkg);
			XAttribute xAttribute = xElement.Attribute("ImagePath");
			XAttribute xAttribute2 = xElement.Attribute("BinaryInOneCorePkg");
			if (xAttribute2 != null && (xAttribute2.Value.Equals("1") || xAttribute2.Value.Equals("true")))
			{
				enviorn.Logger.LogWarning("<Executable> Not converted because binary is in OneCore");
				return;
			}
			string attributeValue = PkgBldrHelpers.GetAttributeValue(toWm, "imagePath");
			if (attributeValue != null)
			{
				enviorn.Logger.LogWarning($"<Executable> Not converted because imagePath is already defined by SvcHostGroupName as {attributeValue}");
				return;
			}
			string text = null;
			if (xAttribute == null)
			{
				string attributeValue2 = PkgBldrHelpers.GetAttributeValue(xElement, "Name");
				string attributeValue3 = PkgBldrHelpers.GetAttributeValue(xElement, "Source");
				if (attributeValue2 != null)
				{
					text = attributeValue2;
				}
				else if (attributeValue3 != null)
				{
					string[] array = attributeValue3.Split('\\');
					if (array.Length == 0)
					{
						text = attributeValue3;
					}
					else
					{
						text = array[array.Length - 1];
					}
				}
				text = "$(runtime.system32)\\" + text;
			}
			else
			{
				text = enviorn.Macros.Resolve(xAttribute.Value);
				xAttribute.Remove();
			}
			if (text.StartsWith("$(runtime.system32)", StringComparison.InvariantCulture))
			{
				text = text.Replace("$(runtime.system32)", "%SystemRoot%\\System32");
			}
			if (!text.StartsWith("%SystemRoot%\\System32", StringComparison.InvariantCultureIgnoreCase))
			{
				enviorn.Logger.LogWarning($"<Executable> Not converted because ImagePath does not start with %SystemRoot%\\System32");
				return;
			}
			toWm.Add(new XAttribute("imagePath", text));
			XElement xElement2 = new XElement(toWm.Name.Namespace + "files");
			XElement content = new FileConverter(enviorn).WmFile(toWm.Name.Namespace, xElement);
			xElement2.Add(content);
			toWm.Add(xElement2);
		}
	}
}
