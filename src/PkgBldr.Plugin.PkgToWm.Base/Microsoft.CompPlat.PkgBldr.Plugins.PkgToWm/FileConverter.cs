using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	public class FileConverter
	{
		private MacroResolver m_macros;

		private Config m_config;

		public bool IsLangFile;

		public FileConverter(Config env)
		{
			if (env == null)
			{
				throw new ArgumentNullException("env");
			}
			m_config = env;
			m_macros = env.Macros;
		}

		public XElement WmFile(XNamespace ns, XElement pkgElement)
		{
			if (pkgElement == null)
			{
				throw new ArgumentNullException("pkgElement");
			}
			XElement xElement = new XElement(ns + "file");
			string attributeValue = PkgBldrHelpers.GetAttributeValue(pkgElement, "DestinationDir");
			if (attributeValue == null)
			{
				attributeValue = "$(runtime.system32)";
			}
			else
			{
				attributeValue = m_macros.Resolve(attributeValue);
				if (attributeValue.StartsWith("\\", StringComparison.OrdinalIgnoreCase))
				{
					attributeValue = "$(runtime.systemDrive)" + attributeValue;
				}
			}
			xElement.Add(new XAttribute("destinationDir", attributeValue));
			foreach (XAttribute item in pkgElement.Attributes())
			{
				if (item.Name.LocalName == "DestinationDir")
				{
					continue;
				}
				if (item.Name.LocalName != "Source")
				{
					item.Value = m_macros.Resolve(item.Value);
				}
				switch (item.Name.LocalName)
				{
				case "Attributes":
				{
					string input = item.Value.ToLowerInvariant();
					input = Regex.Replace(input, "readonly", "readOnly");
					xElement.Add(new XAttribute("attributes", input));
					break;
				}
				case "Name":
					xElement.Add(new XAttribute("name", item.Value));
					break;
				case "Source":
				{
					string text = item.Value.TrimEnd('\\');
					if (!text.Contains("\\"))
					{
						text = "$(RETAIL_BINARY_PATH)\\" + text;
					}
					text = m_config.Macros.Resolve(text);
					xElement.Add(new XAttribute("source", text));
					break;
				}
				case "buildFilter":
				{
					string value = Helpers.ConvertBuildFilter(item.Value);
					xElement.Add(new XAttribute("buildFilter", value));
					break;
				}
				default:
					m_config.Logger.LogWarning("<File> attribute {0} not converted", item.Name.LocalName);
					break;
				}
			}
			return xElement;
		}
	}
}
