using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Base.Security.SecurityPolicy;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins
{
	[Export(typeof(IPkgPlugin))]
	internal class CapabilityClass : Authorization
	{
		private static string capabilityClassRegKeyName = "CapabilityClasses\\";

		public override string XmlElementName => "capabilityClass";

		public override void ConvertEntries(XElement authorizationRegKeys, Dictionary<string, IPkgPlugin> plugins, Config environ, XElement component)
		{
			if (environ.Bld.Product.Equals("mobilecore", StringComparison.OrdinalIgnoreCase))
			{
				ConvertEntriesForMobileCore(authorizationRegKeys, plugins, environ, component);
			}
			else
			{
				ConvertEntriesForOneCore(authorizationRegKeys, plugins, environ, component);
			}
		}

		private void ConvertEntriesForOneCore(XElement authorizationRegKeys, Dictionary<string, IPkgPlugin> plugins, Config environ, XElement component)
		{
			MacroResolver macros = environ.Macros;
			string value = component.Attribute("name").Value;
			XElement xElement = new XElement(authorizationRegKeys.Name.Namespace + "registryKey");
			xElement.Add(new XAttribute("keyName", Authorization.authorizationRegKeyRoot + capabilityClassRegKeyName + value));
			string text = "\"";
			string text2 = null;
			bool flag = true;
			string text3 = null;
			bool flag2 = true;
			foreach (XElement item in component.Descendants())
			{
				string localName = item.Name.LocalName;
				if (!(localName == "memberCapability"))
				{
					if (localName == "memberCapabilityClass")
					{
						text3 += (flag2 ? null : ",");
						text3 = text3 + text + item.Attribute("name").Value + text;
						flag2 = false;
					}
				}
				else
				{
					text2 += (flag ? null : ",");
					string text4 = macros.Resolve(item.Attribute("id").Value);
					text2 = text2 + text + text4 + text;
					flag = false;
				}
			}
			if (text2 != null)
			{
				XElement xElement2 = new XElement(authorizationRegKeys.Name.Namespace + "registryValue");
				xElement2.Add(new XAttribute("name", "MemberCapability"));
				xElement2.Add(new XAttribute("value", text2));
				xElement2.Add(new XAttribute("valueType", "REG_MULTI_SZ"));
				xElement.Add(xElement2);
			}
			if (text3 != null)
			{
				XElement xElement3 = new XElement(authorizationRegKeys.Name.Namespace + "registryValue");
				xElement3.Add(new XAttribute("name", "MemberCapabilityClass"));
				xElement3.Add(new XAttribute("value", text3));
				xElement3.Add(new XAttribute("valueType", "REG_MULTI_SZ"));
				xElement.Add(xElement3);
			}
			authorizationRegKeys.Add(xElement);
		}

		private void ConvertEntriesForMobileCore(XElement authorizationRegKeys, Dictionary<string, IPkgPlugin> plugins, Config environ, XElement component)
		{
			MacroResolver macros = environ.Macros;
			string value = component.Attribute("name").Value;
			string value2 = "CAPABILITY_CLASS_" + value.Substring(value.IndexOf('_') + 1).ToUpperInvariant();
			XElement xElement = new XElement(authorizationRegKeys.Name.Namespace + "registryKey");
			xElement.Add(new XAttribute("keyName", Authorization.authorizationRegKeyRoot + capabilityClassRegKeyName));
			foreach (XElement item in component.Descendants())
			{
				string localName = item.Name.LocalName;
				if (localName == "memberCapability")
				{
					XElement xElement2 = new XElement(authorizationRegKeys.Name.Namespace + "registryValue");
					string value3 = SidBuilder.BuildApplicationCapabilitySidString(macros.Resolve(item.Attribute("id").Value));
					xElement2.Add(new XAttribute("name", value3));
					xElement2.Add(new XAttribute("value", value2));
					xElement2.Add(new XAttribute("valueType", "REG_MULTI_SZ"));
					xElement.Add(xElement2);
				}
			}
			authorizationRegKeys.Add(xElement);
		}
	}
}
