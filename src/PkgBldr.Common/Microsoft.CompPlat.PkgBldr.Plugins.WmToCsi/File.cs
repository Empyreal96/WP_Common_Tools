using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;
using Microsoft.CompPlat.PkgBldr.Tools;

namespace Microsoft.CompPlat.PkgBldr.Plugins.WmToCsi
{
	[Export(typeof(IPkgPlugin))]
	internal class File : PkgPlugin
	{
		public override void ConvertEntries(XElement ToCsi, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromWm)
		{
			XElement xElement = new XElement(ToCsi.Name.Namespace + "file");
			string text = null;
			string text2 = null;
			string text3 = null;
			foreach (XAttribute item in FromWm.Attributes())
			{
				switch (item.Name.LocalName)
				{
				case "source":
				{
					try
					{
						text = enviorn.Bld.BuildMacros.Resolve(item.Value).TrimEnd('\\');
					}
					catch
					{
					}
					text = ((text != null) ? Environment.ExpandEnvironmentVariables(text) : enviorn.Macros.Resolve(item.Value).TrimEnd('\\'));
					string directoryName = LongPath.GetDirectoryName(text);
					if (string.IsNullOrEmpty(directoryName) || directoryName.StartsWith(".", StringComparison.OrdinalIgnoreCase))
					{
						directoryName = LongPath.GetDirectoryName(enviorn.Input);
						directoryName = LongPath.Combine(directoryName, text);
						text = directoryName;
					}
					break;
				}
				case "destinationDir":
					text2 = enviorn.Macros.Resolve(item.Value).TrimEnd('\\');
					if (text2.StartsWith("\\", StringComparison.OrdinalIgnoreCase))
					{
						text2 = "$(runtime.systemDrive)" + text2;
					}
					xElement.Add(new XAttribute("destinationPath", text2));
					break;
				case "name":
					text3 = item.Value;
					break;
				case "securityDescriptor":
					item.Value = enviorn.Macros.Resolve(item.Value);
					xElement.Add(new XElement(ToCsi.Name.Namespace + "securityDescriptor", new XAttribute("name", item.Value)));
					break;
				case "attributes":
					item.Value = enviorn.Macros.Resolve(item.Value);
					xElement.Add(new XAttribute("attributes", item.Value));
					break;
				case "buildFilter":
					xElement.Add(new XAttribute("buildFilter", item.Value));
					break;
				case "writeableType":
					xElement.Add(new XAttribute("writeableType", item.Value));
					break;
				}
			}
			LongPath.GetFileName(text);
			string directoryName2 = LongPath.GetDirectoryName(text);
			string fileName = LongPath.GetFileName(text);
			string value = ".\\";
			xElement.Add(new XAttribute("importPath", directoryName2));
			xElement.Add(new XAttribute("sourceName", fileName));
			xElement.Add(new XAttribute("sourcePath", value));
			string text4 = null;
			text4 = ((text3 != null) ? text3 : fileName);
			xElement.Add(new XAttribute("name", text4));
			string attributeValue = PkgBldrHelpers.GetAttributeValue(FromWm.Parent, "buildFilter");
			if (attributeValue != null)
			{
				string attributeValue2 = PkgBldrHelpers.GetAttributeValue(xElement, "buildFilter");
				if (attributeValue2 == null)
				{
					xElement.Add(new XAttribute("buildFilter", attributeValue));
				}
				else
				{
					enviorn.Logger.LogWarning("ambiguous build filter '{0}' on file element", attributeValue2);
					xElement.Attribute("buildFilter").Value = attributeValue;
				}
			}
			foreach (XElement item2 in FromWm.Elements())
			{
				PkgBldrHelpers.SetDefaultNameSpace(item2, ToCsi.Name.Namespace);
				xElement.Add(item2);
			}
			ToCsi.Add(xElement);
		}
	}
}
