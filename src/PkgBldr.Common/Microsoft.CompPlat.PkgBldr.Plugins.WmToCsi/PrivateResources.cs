using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Base.Security;
using Microsoft.CompPlat.PkgBldr.Base.Security.SecurityPolicy;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.WmToCsi
{
	[Export(typeof(IPkgPlugin))]
	internal class PrivateResources : PkgPlugin
	{
		public override void ConvertEntries(XElement ToCsi, Dictionary<string, IPkgPlugin> plugins, Config environ, XElement FromWm)
		{
			GlobalSecurity globalSecurity = environ.GlobalSecurity;
			MacroResolver macros = environ.Macros;
			string text = GetAttributeValue(FromWm.Parent, "name");
			PrivateResourceClaimerType resourceClaimerType;
			if (text != null)
			{
				resourceClaimerType = PrivateResourceClaimerType.Service;
			}
			else
			{
				text = FromWm.Parent.Element(FromWm.Parent.Name.Namespace + "PackageId").Value;
				if (text == null)
				{
					throw new PkgGenException("Private resources can only be specified for services and tasks");
				}
				resourceClaimerType = PrivateResourceClaimerType.Task;
			}
			foreach (XElement item in FromWm.Elements())
			{
				bool readOnly = macros.Resolve(GetAttributeValue(item, "readOnly")) == "Yes";
				bool protectToUser = false;
				switch (item.Name.LocalName)
				{
				case "regKey":
				{
					string name = macros.Resolve(GetAttributeValue(item, "path"));
					if (name.StartsWith("HKEY_CURRENT_USER", StringComparison.OrdinalIgnoreCase))
					{
						protectToUser = true;
					}
					globalSecurity.AddPrivateResource(name, ResourceType.Registry, text, resourceClaimerType, readOnly, protectToUser);
					break;
				}
				case "file":
				{
					string name = macros.Resolve(GetAttributeValue(item, "path"));
					if (name.StartsWith("$(runtime.userProfile)", StringComparison.OrdinalIgnoreCase))
					{
						protectToUser = true;
					}
					globalSecurity.AddPrivateResource(name, ResourceType.File, text, resourceClaimerType, readOnly, protectToUser);
					break;
				}
				case "directory":
				{
					string name = macros.Resolve(GetAttributeValue(item, "path"));
					if (name.StartsWith("$(runtime.userProfile)", StringComparison.OrdinalIgnoreCase))
					{
						protectToUser = true;
					}
					globalSecurity.AddPrivateResource(name, ResourceType.Directory, text, resourceClaimerType, readOnly, protectToUser);
					break;
				}
				case "serviceAccess":
				{
					string attributeValue4 = macros.Resolve(GetAttributeValue(item, "name"));
					globalSecurity.AddPrivateResource(attributeValue4, ResourceType.ServiceAccess, text, resourceClaimerType, readOnly);
					break;
				}
				case "wnf":
				{
					string attributeValue4 = GetAttributeValue(item, "name");
					string attributeValue5 = GetAttributeValue(item, "tag");
					string attributeValue6 = GetAttributeValue(item, "scope");
					string attributeValue7 = GetAttributeValue(item, "sequence");
					WnfValue wnfValue = new WnfValue(attributeValue4, attributeValue5, attributeValue6, attributeValue7);
					globalSecurity.AddPrivateResource(wnfValue, text, resourceClaimerType, readOnly);
					break;
				}
				case "transientObject":
				{
					string name = GetAttributeValue(item, "path");
					string qualifyingType = macros.Resolve(GetAttributeValue(item, "type"));
					protectToUser = macros.Resolve(GetAttributeValue(item, "protectToUser")) == "Yes";
					SdRegValue sdRegValue5 = new SdRegValue(SdRegType.TransientObject, name, qualifyingType, protectToUser);
					globalSecurity.AddPrivateResource(sdRegValue5, ResourceType.TransientObject, text, resourceClaimerType, readOnly, protectToUser);
					break;
				}
				case "etwProvider":
				{
					string attributeValue3 = GetAttributeValue(item, "guid");
					SdRegValue sdRegValue4 = new SdRegValue(SdRegType.EtwProvider, attributeValue3);
					globalSecurity.AddPrivateResource(sdRegValue4, ResourceType.EtwProvider, text, resourceClaimerType, readOnly);
					break;
				}
				case "sdRegValue":
				{
					string name = macros.Resolve(GetAttributeValue(item, "path"));
					bool isString = macros.Resolve(GetAttributeValue(item, "saveAsString")) == "Yes";
					SdRegValue sdRegValue3 = new SdRegValue(SdRegType.Generic, name, null, isString);
					globalSecurity.AddPrivateResource(sdRegValue3, ResourceType.SdReg, text, resourceClaimerType, readOnly);
					break;
				}
				case "com":
				{
					string attributeValue2 = GetAttributeValue(item, "appId");
					protectToUser = macros.Resolve(GetAttributeValue(item, "protectToUser")) == "Yes";
					SdRegValue sdRegValue2 = new SdRegValue(SdRegType.Com, attributeValue2);
					globalSecurity.AddPrivateResource(sdRegValue2, ResourceType.ComAccess, text, resourceClaimerType, readOnly, protectToUser);
					globalSecurity.AddPrivateResource(sdRegValue2, ResourceType.ComLaunch, text, resourceClaimerType, readOnly, protectToUser);
					break;
				}
				case "winRT":
				{
					string attributeValue = GetAttributeValue(item, "serverName");
					protectToUser = macros.Resolve(GetAttributeValue(item, "protectToUser")) == "Yes";
					SdRegValue sdRegValue = new SdRegValue(SdRegType.WinRt, attributeValue);
					globalSecurity.AddPrivateResource(sdRegValue, ResourceType.WinRt, text, resourceClaimerType, readOnly, protectToUser);
					break;
				}
				}
			}
		}

		private string GetAttributeValue(XElement element, string attributeName)
		{
			return element.Attribute(attributeName)?.Value;
		}
	}
}
