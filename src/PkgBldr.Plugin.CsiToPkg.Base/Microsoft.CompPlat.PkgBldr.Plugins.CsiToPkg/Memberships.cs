using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.CsiToPkg
{
	[Export(typeof(IPkgPlugin))]
	internal class Memberships : PkgPlugin
	{
		public override void ConvertEntries(XElement ToPkg, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromCsi)
		{
			XElement regKeys = ((MyContainter)enviorn.arg).RegKeys;
			foreach (XElement item in FromCsi.Descendants(FromCsi.Name.Namespace + "serviceData"))
			{
				string attributeValue = PkgBldrHelpers.GetAttributeValue(item, "name");
				if (attributeValue == null)
				{
					continue;
				}
				IEnumerable<XAttribute> enumerable = item.Attributes();
				XElement xElement = RegHelpers.PkgRegKey("$(hklm.services)\\" + attributeValue);
				foreach (XAttribute item2 in enumerable)
				{
					if (item2.Value == null)
					{
						continue;
					}
					switch (item2.Name.LocalName)
					{
					case "requiredPrivileges":
					{
						XElement content = RegHelpers.PkgRegValue("RequiredPrivileges", "REG_MULTI_SZ", item2.Value);
						xElement.Add(content);
						break;
					}
					case "imagePath":
					{
						XElement content = RegHelpers.PkgRegValue("ImagePath", "REG_EXPAND_SZ", item2.Value);
						xElement.Add(content);
						break;
					}
					case "sidType":
					{
						switch (item2.Value.ToLowerInvariant())
						{
						case "none":
							item2.Value = "00000000";
							break;
						case "Restricted":
							item2.Value = "00000003";
							break;
						case "Unrestricted":
							item2.Value = "00000001";
							break;
						default:
							Console.WriteLine("warning: unknown sidType");
							goto end_IL_0097;
						}
						XElement content = RegHelpers.PkgRegValue("ServiceSidType", "REG_DWORD", item2.Value);
						xElement.Add(content);
						break;
					}
					case "dependOnService":
					{
						XElement content = RegHelpers.PkgRegValue("DependOnService", "REG_MULTI_SZ", item2.Value);
						xElement.Add(content);
						break;
					}
					case "description":
					{
						XElement content = RegHelpers.PkgRegValue("Description", "REG_SZ", item2.Value);
						xElement.Add(content);
						break;
					}
					case "displayName":
					{
						XElement content = RegHelpers.PkgRegValue("DisplayName", "REG_SZ", item2.Value);
						xElement.Add(content);
						break;
					}
					case "group":
					{
						XElement content = RegHelpers.PkgRegValue("Group", "REG_SZ", item2.Value);
						xElement.Add(content);
						break;
					}
					case "objectName":
					{
						XElement content = RegHelpers.PkgRegValue("ObjectName", "REG_SZ", item2.Value);
						xElement.Add(content);
						break;
					}
					case "dependOnGroup":
					{
						XElement content = RegHelpers.PkgRegValue("DependOnGroup", "REG_SZ", item2.Value);
						xElement.Add(content);
						break;
					}
					case "errorControl":
					{
						switch (item2.Value.ToLowerInvariant())
						{
						case "ignore":
							item2.Value = "00000000";
							break;
						case "normal":
							item2.Value = "00000001";
							break;
						case "critical":
							item2.Value = "00000003";
							break;
						default:
							Console.WriteLine("warning: unknown service errorContorl type {0}", item2.Value);
							goto end_IL_0097;
						}
						XElement content = RegHelpers.PkgRegValue("ErrorControl", "REG_DWORD", item2.Value);
						xElement.Add(content);
						break;
					}
					case "start":
					{
						switch (item2.Value.ToLowerInvariant())
						{
						case "auto":
							item2.Value = "00000002";
							break;
						case "boot":
							item2.Value = "00000000";
							break;
						case "delayedAuto":
							item2.Value = "00000002";
							break;
						case "demand":
							item2.Value = "00000003";
							break;
						case "disabled":
							item2.Value = "00000004";
							break;
						case "system":
							item2.Value = "00000001";
							break;
						default:
							Console.WriteLine("warning: unknown service start type {0}", item2.Value);
							goto end_IL_0097;
						}
						XElement content = RegHelpers.PkgRegValue("Start", "REG_DWORD", item2.Value);
						xElement.Add(content);
						break;
					}
					case "type":
					{
						switch (item2.Value.ToLowerInvariant())
						{
						case "win32shareprocess":
							item2.Value = "00000020";
							break;
						case "win32ownprocess":
							item2.Value = "00000010";
							break;
						case "kerneldriver":
							item2.Value = "00000001";
							break;
						case "filesystemdriver":
							item2.Value = "00000002";
							break;
						default:
							Console.WriteLine("warning: unknown service type {0}", item2.Value);
							goto end_IL_0097;
						}
						XElement content = RegHelpers.PkgRegValue("Type", "REG_DWORD", item2.Value);
						xElement.Add(content);
						break;
					}
					case "tag":
					{
						XElement content = RegHelpers.PkgRegValue("Tag", "REG_DWORD", item2.Value);
						xElement.Add(content);
						break;
					}
					default:
						Console.WriteLine("warning: unknown service attribute {0}", item2.Name.LocalName);
						break;
					case "name":
						break;
						end_IL_0097:
						break;
					}
				}
				base.ConvertEntries(xElement, plugins, enviorn, item);
				regKeys.Add(xElement);
			}
		}
	}
}
