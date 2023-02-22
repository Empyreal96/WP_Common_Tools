using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins
{
	[Export(typeof(IPkgPlugin))]
	internal class Capability : Capabilities
	{
		protected static string adminCapabilitiesKey = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\SecurityManager\\AdminCapabilities";

		public override void ConvertEntries(XElement parent, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement component)
		{
			MacroResolver macros = enviorn.Macros;
			base.ConvertEntries(parent, plugins, enviorn, component);
			if (macros.Resolve(GetAttributeValue(component, "adminOnMultiSession")) == "Yes")
			{
				string attributeValue = GetAttributeValue(component, "id");
				XElement xElement = new XElement(parent.Name.Namespace + "registryKeys");
				XElement xElement2 = new XElement(parent.Name.Namespace + "registryKey");
				xElement2.Add(new XAttribute("keyName", adminCapabilitiesKey));
				XElement xElement3 = new XElement(parent.Name.Namespace + "registryValue");
				xElement3.Add(new XAttribute("name", attributeValue));
				xElement3.Add(new XAttribute("value", 1));
				xElement3.Add(new XAttribute("valueType", "REG_DWORD"));
				xElement2.Add(xElement3);
				xElement.Add(xElement2);
				parent.Add(xElement);
			}
		}
	}
}
