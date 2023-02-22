using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class CapabilityRules
	{
		private string appCapSID = string.Empty;

		private string svcCapSID = string.Empty;

		private CapabilityOwnerType ownerType = CapabilityOwnerType.Unknown;

		private List<RulePolicyElement> allRules = new List<RulePolicyElement>();

		[XmlArrayItem(typeof(CapabilityRulesFile), ElementName = "File")]
		[XmlArrayItem(typeof(CapabilityRulesDirectory), ElementName = "Directory")]
		[XmlArrayItem(typeof(CapabilityRulesRegKey), ElementName = "RegKey")]
		[XmlArrayItem(typeof(CapabilityRulesSDRegValue), ElementName = "SDRegValue")]
		[XmlArrayItem(typeof(CapabilityRulesTransientObject), ElementName = "TransientObject")]
		[XmlArrayItem(typeof(CapabilityRulesPrivilege), ElementName = "Privilege")]
		[XmlArrayItem(typeof(CapabilityRulesWindows), ElementName = "WindowsCapability")]
		public RulePolicyElement[] Rules
		{
			get
			{
				return allRules.ToArray();
			}
			set
			{
				allRules.Clear();
				allRules.AddRange(value);
			}
		}

		public CapabilityRules()
		{
		}

		public CapabilityRules(CapabilityOwnerType value)
		{
			ownerType = value;
		}

		public void Add(IXPathNavigable capabilityRulesXmlElement, string inAppCapSID, string inSvcCapSID)
		{
			appCapSID = inAppCapSID;
			svcCapSID = inSvcCapSID;
			AddElements((XmlElement)capabilityRulesXmlElement);
		}

		private void AddElements(XmlElement capabilityRulesXmlElement)
		{
			foreach (XmlNode childNode in capabilityRulesXmlElement.ChildNodes)
			{
				if (childNode.NodeType == XmlNodeType.Element)
				{
					XmlElement xmlElement = (XmlElement)childNode;
					string localName = xmlElement.LocalName;
					RulePolicyElement rulePolicyElement = null;
					switch (localName)
					{
					case "File":
						rulePolicyElement = new CapabilityRulesFile(ownerType);
						break;
					case "Directory":
						rulePolicyElement = new CapabilityRulesDirectory(ownerType);
						break;
					case "InstallationFolder":
						rulePolicyElement = new CapabilityRulesInstallationFolder(ownerType);
						break;
					case "ChamberProfileDataDefaultFolder":
						rulePolicyElement = new CapabilityRulesChamberProfileDefaultDataFolder(ownerType);
						break;
					case "ChamberProfileDataShellContentFolder":
						rulePolicyElement = new CapabilityRulesChamberProfileShellContentFolder(ownerType);
						break;
					case "ChamberProfileDataMediaFolder":
						rulePolicyElement = new CapabilityRulesChamberProfileMediaFolder(ownerType);
						break;
					case "ChamberProfileDataPlatformDataFolder":
						rulePolicyElement = new CapabilityRulesChamberProfilePlatformDataFolder(ownerType);
						break;
					case "ChamberProfileDataLiveTilesFolder":
						rulePolicyElement = new CapabilityRulesChamberProfileLiveTilesFolder(ownerType);
						break;
					case "RegKey":
						rulePolicyElement = new CapabilityRulesRegKey(ownerType);
						break;
					case "ServiceAccess":
					case "DeviceSetupClass":
					case "ETWProvider":
					case "WNF":
					case "SDRegValue":
						AddSDRegValueRule(xmlElement, localName);
						break;
					case "COM":
						AddSDRegValueCOMRule(xmlElement);
						break;
					case "WinRT":
						AddSDRegValueWinRTRule(xmlElement);
						break;
					case "Privilege":
						rulePolicyElement = new CapabilityRulesPrivilege(ownerType);
						break;
					case "WindowsCapability":
						rulePolicyElement = new CapabilityRulesWindows(ownerType);
						break;
					case "TransientObject":
						rulePolicyElement = new CapabilityRulesTransientObject(ownerType);
						break;
					default:
						throw new PolicyCompilerInternalException("Internal Error: Capability Rule Element has been an invalid type: " + localName);
					}
					if (rulePolicyElement != null)
					{
						rulePolicyElement.Add(xmlElement, appCapSID, svcCapSID);
						allRules.Add(rulePolicyElement);
					}
				}
			}
		}

		private void AddSDRegValueRule(XmlElement ruleXmlElement, string sdRegValueRuleType)
		{
			CapabilityRulesSDRegValue capabilityRulesSDRegValue = new CapabilityRulesSDRegValue(sdRegValueRuleType, ownerType);
			capabilityRulesSDRegValue.Add(ruleXmlElement, appCapSID, svcCapSID);
			allRules.Add(capabilityRulesSDRegValue);
		}

		private void AddSDRegValueCOMRule(XmlElement ruleXmlElement)
		{
			bool flag = true;
			if (!string.IsNullOrEmpty(ruleXmlElement.GetAttribute("LaunchPermission")))
			{
				AddSDRegValueRule(ruleXmlElement, "COMLaunchPermission");
				flag = false;
			}
			if (!string.IsNullOrEmpty(ruleXmlElement.GetAttribute("AccessPermission")))
			{
				AddSDRegValueRule(ruleXmlElement, "COMAccessPermission");
				flag = false;
			}
			if (flag)
			{
				if (ownerType != CapabilityOwnerType.Application && ownerType != CapabilityOwnerType.Service)
				{
					throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "Capability COM doesn't contain {0} or {1} attributes", new object[2] { "LaunchPermission", "AccessPermission" }));
				}
				AddSDRegValueRule(ruleXmlElement, "COMLaunchPermission");
				AddSDRegValueRule(ruleXmlElement, "COMAccessPermission");
			}
		}

		private void AddSDRegValueWinRTRule(XmlElement ruleXmlElement)
		{
			bool flag = true;
			string empty = string.Empty;
			if (!string.IsNullOrEmpty(ruleXmlElement.GetAttribute("LaunchPermission")))
			{
				AddSDRegValueRule(ruleXmlElement, "WinRTLaunchPermission");
				flag = false;
			}
			if (flag && !string.IsNullOrEmpty(ruleXmlElement.GetAttribute("AccessPermission")))
			{
				AddSDRegValueRule(ruleXmlElement, "WinRTAccessPermission");
				flag = false;
			}
			if (flag)
			{
				if (ownerType != CapabilityOwnerType.Application && ownerType != CapabilityOwnerType.Service)
				{
					throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "Capability WinRT doesn't contain {0} or {1} attributes", new object[2] { "LaunchPermission", "AccessPermission" }));
				}
				AddSDRegValueRule(ruleXmlElement, "WinRTLaunchPermission");
			}
		}

		public string GetAllAttributesString()
		{
			string text = string.Empty;
			if (allRules != null)
			{
				foreach (RulePolicyElement allRule in allRules)
				{
					text += allRule.GetAttributesString();
				}
				return text;
			}
			return text;
		}

		public void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel3, "CapabilityRules");
			instance.DebugLine(string.Empty);
			if (allRules == null)
			{
				return;
			}
			foreach (RulePolicyElement allRule in allRules)
			{
				allRule.Print();
			}
		}
	}
}
