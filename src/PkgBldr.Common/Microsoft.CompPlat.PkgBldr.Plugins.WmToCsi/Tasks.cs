using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.WmToCsi
{
	[Export(typeof(IPkgPlugin))]
	internal class Tasks : PkgPlugin
	{
		public override string XmlSchemaPath => "PkgBldr.WM.Xsd\\Task.xsd";

		public override void ConvertEntries(XElement ToCsi, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromWm)
		{
			XElement xElement = PkgBldrHelpers.AddIfNotFound(ToCsi, "taskScheduler");
			foreach (XElement item in FromWm.Descendants())
			{
				string localName = item.Name.LocalName;
				XName name = (localName.Equals("uri", StringComparison.OrdinalIgnoreCase) ? (item.GetDefaultNamespace() + "URI") : (item.GetDefaultNamespace() + string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[2]
				{
					char.ToUpperInvariant(localName[0]),
					localName.Substring(1)
				})));
				item.Name = name;
				if (item.Attribute("context") != null)
				{
					List<XAttribute> list = item.Attributes().ToList();
					XAttribute xAttribute = list.Where((XAttribute x) => x.Name.LocalName.Equals("context")).First();
					list.Add(new XAttribute("Context", xAttribute.Value));
					list.Remove(xAttribute);
					item.ReplaceAttributes(list);
				}
			}
			foreach (XAttribute item2 in FromWm.Attributes())
			{
				XAttribute content = new XAttribute(item2);
				xElement.Add(content);
			}
			base.ConvertEntries(xElement, plugins, enviorn, FromWm);
		}
	}
}
