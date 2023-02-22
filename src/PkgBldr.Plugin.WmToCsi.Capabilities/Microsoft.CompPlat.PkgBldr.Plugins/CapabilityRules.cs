using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Base.Security;
using Microsoft.CompPlat.PkgBldr.Base.Security.SecurityPolicy;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins
{
	[Export(typeof(IPkgPlugin))]
	internal class CapabilityRules : Capabilities
	{
		public override void ConvertEntries(XElement parent, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement component)
		{
			GlobalSecurity globalSecurity = enviorn.GlobalSecurity;
			MacroResolver macros = enviorn.Macros;
			string attributeValue = GetAttributeValue(component.Parent, "id");
			bool adminOnMultiSession = macros.Resolve(GetAttributeValue(component.Parent, "adminOnMultiSession")) == "Yes";
			foreach (XElement item in component.Elements())
			{
				bool protectToUser = false;
				switch (item.Name.LocalName)
				{
				case "regKey":
				{
					string attributeValue3 = macros.Resolve(GetAttributeValue(item, "path"));
					if (attributeValue3.StartsWith("HKEY_CURRENT_USER", StringComparison.OrdinalIgnoreCase))
					{
						protectToUser = true;
					}
					string rights = macros.Resolve(GetAttributeValue(item, "rights"));
					globalSecurity.AddCapability(attributeValue, attributeValue3, ResourceType.Registry, rights, adminOnMultiSession, protectToUser, null);
					break;
				}
				case "file":
				{
					string attributeValue3 = macros.Resolve(GetAttributeValue(item, "path"));
					if (attributeValue3.StartsWith("$(runtime.userProfile)", StringComparison.OrdinalIgnoreCase))
					{
						protectToUser = true;
					}
					string rights = macros.Resolve(GetAttributeValue(item, "rights"));
					globalSecurity.AddCapability(attributeValue, attributeValue3, ResourceType.File, rights, adminOnMultiSession, protectToUser, null);
					break;
				}
				case "directory":
				{
					string attributeValue3 = macros.Resolve(GetAttributeValue(item, "path"));
					if (attributeValue3.StartsWith("$(runtime.userProfile)", StringComparison.OrdinalIgnoreCase))
					{
						protectToUser = true;
					}
					string rights = macros.Resolve(GetAttributeValue(item, "rights"));
					globalSecurity.AddCapability(attributeValue, attributeValue3, ResourceType.Directory, rights, adminOnMultiSession, protectToUser, null);
					break;
				}
				case "serviceAccess":
				{
					string attributeValue4 = macros.Resolve(GetAttributeValue(item, "name"));
					string rights = macros.Resolve(GetAttributeValue(item, "rights"));
					globalSecurity.AddCapability(attributeValue, attributeValue4, ResourceType.ServiceAccess, rights, adminOnMultiSession);
					break;
				}
				case "wnf":
				{
					string attributeValue4 = GetAttributeValue(item, "name");
					string attributeValue5 = GetAttributeValue(item, "tag");
					string attributeValue6 = GetAttributeValue(item, "scope");
					string attributeValue7 = GetAttributeValue(item, "sequence");
					string rights = macros.Resolve(GetAttributeValue(item, "rights"));
					WnfValue wnfValue = new WnfValue(attributeValue4, attributeValue5, attributeValue6, attributeValue7);
					globalSecurity.AddCapability(attributeValue, wnfValue, rights, adminOnMultiSession);
					break;
				}
				case "transientObject":
				{
					string attributeValue3 = GetAttributeValue(item, "path");
					string qualifyingType = macros.Resolve(GetAttributeValue(item, "type"));
					string rights = macros.Resolve(GetAttributeValue(item, "rights"));
					protectToUser = macros.Resolve(GetAttributeValue(item, "protectToUser")) == "Yes";
					SdRegValue sdRegValue2 = new SdRegValue(SdRegType.TransientObject, attributeValue3, qualifyingType, protectToUser);
					globalSecurity.AddCapability(attributeValue, sdRegValue2, ResourceType.TransientObject, rights, adminOnMultiSession, protectToUser);
					break;
				}
				case "etwProvider":
				{
					string attributeValue9 = GetAttributeValue(item, "guid");
					string rights = macros.Resolve(GetAttributeValue(item, "rights"));
					SdRegValue sdRegValue5 = new SdRegValue(SdRegType.EtwProvider, attributeValue9);
					globalSecurity.AddCapability(attributeValue, sdRegValue5, ResourceType.EtwProvider, rights, adminOnMultiSession);
					break;
				}
				case "sdRegValue":
				{
					string attributeValue3 = macros.Resolve(GetAttributeValue(item, "path"));
					bool isString = macros.Resolve(GetAttributeValue(item, "saveAsString")) == "Yes";
					string rights = macros.Resolve(GetAttributeValue(item, "rights"));
					SdRegValue sdRegValue4 = new SdRegValue(SdRegType.Generic, attributeValue3, null, isString);
					globalSecurity.AddCapability(attributeValue, sdRegValue4, ResourceType.SdReg, rights, adminOnMultiSession);
					break;
				}
				case "com":
				{
					string attributeValue8 = GetAttributeValue(item, "appId");
					string text2 = macros.Resolve(GetAttributeValue(item, "accessPermission"));
					string text = macros.Resolve(GetAttributeValue(item, "launchPermission"));
					protectToUser = macros.Resolve(GetAttributeValue(item, "protectToUser")) == "Yes";
					SdRegValue sdRegValue3 = new SdRegValue(SdRegType.Com, attributeValue8);
					if (text2 != null)
					{
						globalSecurity.AddCapability(attributeValue, sdRegValue3, ResourceType.ComAccess, text2, adminOnMultiSession, protectToUser);
					}
					if (text != null)
					{
						globalSecurity.AddCapability(attributeValue, sdRegValue3, ResourceType.ComLaunch, text, adminOnMultiSession, protectToUser);
					}
					break;
				}
				case "winRT":
				{
					string attributeValue2 = GetAttributeValue(item, "serverName");
					string text = macros.Resolve(GetAttributeValue(item, "launchPermission"));
					string text2 = macros.Resolve(GetAttributeValue(item, "accessPermission"));
					protectToUser = macros.Resolve(GetAttributeValue(item, "protectToUser")) == "Yes";
					SdRegValue sdRegValue = new SdRegValue(SdRegType.WinRt, attributeValue2);
					if (text != null)
					{
						globalSecurity.AddCapability(attributeValue, sdRegValue, ResourceType.WinRt, text, adminOnMultiSession, protectToUser);
					}
					else if (text2 != null)
					{
						globalSecurity.AddCapability(attributeValue, sdRegValue, ResourceType.WinRt, text2, adminOnMultiSession, protectToUser);
					}
					break;
				}
				}
			}
		}
	}
}
