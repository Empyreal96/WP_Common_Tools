using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class SvcHostGroup : PkgPlugin
	{
		[Flags]
		private enum AuthenticationCapabitities
		{
			None = 0,
			MutualAuth = 1,
			StaticCloaking = 0x20,
			DynamicCloaking = 0x40,
			AnyAuthority = 0x80,
			MakeFullSIC = 0x100,
			Default = 0x800,
			SecureRefs = 2,
			AccessControl = 4,
			AppId = 8,
			Dynamic = 0x10,
			RequireFullSIC = 0x200,
			AutoImpersonate = 0x400,
			NoCustomMarshal = 0x2000,
			DisableAAA = 0x1000
		}

		public override void ConvertEntries(XElement ToWm, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromPkg)
		{
			XElement xElement = PkgBldrHelpers.AddIfNotFound(ToWm, "regKeys");
			XElement xElement2 = new XElement(xElement.Name.Namespace + "regKey");
			string attributeValue = PkgBldrHelpers.GetAttributeValue(FromPkg, "Name");
			attributeValue = "HKEY_LOCAL_MACHINE\\Software\\Microsoft\\Windows NT\\CurrentVersion\\SvcHost\\" + attributeValue;
			xElement2.Add(new XAttribute("keyName", attributeValue));
			xElement.Add(xElement2);
			attributeValue = PkgBldrHelpers.GetAttributeValue(FromPkg, "CoInitializeSecurityParam");
			if (!string.IsNullOrEmpty(attributeValue))
			{
				XElement xElement3 = new XElement(xElement.Name.Namespace + "regValue");
				string text = int.Parse(attributeValue).ToString("x");
				text = text.PadLeft(8, '0');
				text = "0x" + text;
				xElement3.Add(new XAttribute("name", "CoInitializeSecurityParam"));
				xElement3.Add(new XAttribute("type", "REG_DWORD"));
				xElement3.Add(new XAttribute("value", text));
				xElement2.Add(xElement3);
			}
			attributeValue = PkgBldrHelpers.GetAttributeValue(FromPkg, "CoInitializeSecurityAllowLowBox");
			if (!string.IsNullOrEmpty(attributeValue))
			{
				XElement xElement4 = new XElement(xElement.Name.Namespace + "regValue");
				string text2 = int.Parse(attributeValue).ToString("x");
				text2 = text2.PadLeft(8, '0');
				text2 = "0x" + text2;
				xElement4.Add(new XAttribute("name", "CoInitializeSecurityAllowLowBox"));
				xElement4.Add(new XAttribute("type", "REG_DWORD"));
				xElement4.Add(new XAttribute("value", text2));
				xElement2.Add(xElement4);
			}
			attributeValue = PkgBldrHelpers.GetAttributeValue(FromPkg, "ImpersonationLevel");
			if (!string.IsNullOrEmpty(attributeValue))
			{
				if (attributeValue != "Identify")
				{
					enviorn.Logger.LogWarning("Skipping SvcHostGroup entry because ImpersonationLevel != Identify");
				}
				else
				{
					XElement xElement5 = new XElement(xElement.Name.Namespace + "regValue");
					xElement5.Add(new XAttribute("name", "ImpersonationLevel"));
					xElement5.Add(new XAttribute("type", "REG_DWORD"));
					xElement5.Add(new XAttribute("value", "0x00000002"));
					xElement2.Add(xElement5);
				}
			}
			attributeValue = PkgBldrHelpers.GetAttributeValue(FromPkg, "AuthenticationCapabilities");
			if (string.IsNullOrEmpty(attributeValue))
			{
				return;
			}
			XElement xElement6 = new XElement(xElement.Name.Namespace + "regValue");
			xElement6.Add(new XAttribute("name", "AuthenticationCapabilities"));
			xElement6.Add(new XAttribute("type", "REG_DWORD"));
			string[] array = attributeValue.Split(' ');
			AuthenticationCapabitities authenticationCapabitities = AuthenticationCapabitities.None;
			string[] array2 = array;
			foreach (string text3 in array2)
			{
				switch (text3)
				{
				case "None":
					authenticationCapabitities |= AuthenticationCapabitities.None;
					break;
				case "MutualAuth":
					authenticationCapabitities |= AuthenticationCapabitities.MutualAuth;
					break;
				case "StaticCloaking":
					authenticationCapabitities |= AuthenticationCapabitities.StaticCloaking;
					break;
				case "DynamicCloaking":
					authenticationCapabitities |= AuthenticationCapabitities.DynamicCloaking;
					break;
				case "AnyAuthority":
					authenticationCapabitities |= AuthenticationCapabitities.AnyAuthority;
					break;
				case "MakeFullSIC":
					authenticationCapabitities |= AuthenticationCapabitities.MakeFullSIC;
					break;
				case "Default":
					authenticationCapabitities |= AuthenticationCapabitities.Default;
					break;
				case "SecureRefs":
					authenticationCapabitities |= AuthenticationCapabitities.SecureRefs;
					break;
				case "AccessControl":
					authenticationCapabitities |= AuthenticationCapabitities.AccessControl;
					break;
				case "AppId":
					authenticationCapabitities |= AuthenticationCapabitities.AppId;
					break;
				case "Dynamic":
					authenticationCapabitities |= AuthenticationCapabitities.Dynamic;
					break;
				case "RequireFullSIC":
					authenticationCapabitities |= AuthenticationCapabitities.RequireFullSIC;
					break;
				case "AutoImpersonate":
					authenticationCapabitities |= AuthenticationCapabitities.AutoImpersonate;
					break;
				case "NoCustomMarshal":
					authenticationCapabitities |= AuthenticationCapabitities.NoCustomMarshal;
					break;
				case "DisableAAA":
					authenticationCapabitities |= AuthenticationCapabitities.DisableAAA;
					break;
				default:
					enviorn.Logger.LogWarning($"Unknown Authentication Capability {text3}");
					break;
				}
			}
			attributeValue = $"0x{(int)authenticationCapabitities:X8}";
			xElement6.Add(new XAttribute("value", attributeValue));
			xElement2.Add(xElement6);
		}
	}
}
