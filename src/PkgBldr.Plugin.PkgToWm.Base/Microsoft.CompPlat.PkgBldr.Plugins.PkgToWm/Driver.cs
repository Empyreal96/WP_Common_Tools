using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class Driver : PkgPlugin
	{
		private Config _env;

		public override void ConvertEntries(XElement toWm, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement fromPkg)
		{
			_env = enviorn;
			string value = fromPkg.Attribute("InfSource").Value;
			value = enviorn.Macros.Resolve(value);
			XElement xElement = new XElement(toWm.Name.Namespace + "driver");
			xElement.Add(new XElement(toWm.Name.Namespace + "inf", new XAttribute("source", value)));
			XElement xElement2 = new XElement(toWm.Name.Namespace + "files");
			foreach (XElement item in fromPkg.Elements(fromPkg.Name.Namespace + "Files"))
			{
				base.ConvertEntries(xElement2, plugins, enviorn, item);
			}
			foreach (XElement item2 in fromPkg.Elements(fromPkg.Name.Namespace + "Reference"))
			{
				XElement xElement3 = MergeDriverFile(xElement2, item2);
				if (xElement3 != null)
				{
					xElement2.Add(xElement3);
				}
			}
			if (xElement2.HasElements)
			{
				xElement.Add(xElement2);
			}
			XElement xElement4 = fromPkg.Element(fromPkg.Name.Namespace + "Security");
			if (xElement4 != null)
			{
				List<XElement> list = new List<XElement>();
				foreach (XElement item3 in xElement4.Elements())
				{
					string localName = item3.Name.LocalName;
					if (localName == "AccessedByCapability")
					{
						list.Add(new XElement(toWm.Name.Namespace + "accessedByCapability", new XAttribute("id", item3.Attribute("Id").Value), new XAttribute("rights", item3.Attribute("Rights").Value)));
					}
					else
					{
						enviorn.Logger.LogWarning(string.Format(CultureInfo.InvariantCulture, "<Package> <{0}> not converted", new object[1] { item3.Name.LocalName }));
					}
				}
				if (list.Any())
				{
					xElement.Add(new XElement(toWm.Name.Namespace + "security", new XAttribute("infSectionName", xElement4.Attribute("InfSectionName").Value), list));
				}
			}
			PkgBldrHelpers.AddIfNotFound(toWm, "drivers").Add(xElement);
		}

		private XElement MergeDriverFile(XElement wmDriverFiles, XElement pkgDriverReference)
		{
			string value = pkgDriverReference.Attribute("Source").Value;
			value = _env.Macros.Resolve(value);
			string value2 = value.ToLowerInvariant();
			foreach (XElement item in wmDriverFiles.Elements())
			{
				if (item.Attribute("source").Value.ToLowerInvariant().Equals(value2))
				{
					return null;
				}
			}
			XElement xElement = new XElement(wmDriverFiles.Name.Namespace + "file");
			xElement.Add(new XAttribute("source", value));
			xElement.Add(new XAttribute("destinationDir", "$(runtime.drivers)"));
			return xElement;
		}
	}
}
