using System;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class CapabilityRulesSDRegValue : BaseRule
	{
		private string inputAttributeName = string.Empty;

		private string inputAttribute;

		private string path = "Not Calculated";

		private string sdRegValueType = "Not Calculated";

		private string saveAsString = "Not Calculated";

		[XmlAttribute(AttributeName = "Path")]
		public string Path
		{
			get
			{
				return path;
			}
			set
			{
				path = value;
			}
		}

		[XmlAttribute(AttributeName = "Type")]
		public string SDRegValueType
		{
			get
			{
				return sdRegValueType;
			}
			set
			{
				sdRegValueType = value;
			}
		}

		[XmlAttribute(AttributeName = "SaveAsString")]
		public string SaveAsString
		{
			get
			{
				return saveAsString;
			}
			set
			{
				saveAsString = value;
			}
		}

		public bool ShouldSerializeSaveAsString()
		{
			return saveAsString != "Not Calculated";
		}

		public CapabilityRulesSDRegValue()
		{
		}

		public CapabilityRulesSDRegValue(string value, CapabilityOwnerType type)
		{
			base.RuleType = value;
			base.OwnerType = type;
		}

		public override void Add(IXPathNavigable BasicRuleXmlElement, string appCapSID, string svcCapSID)
		{
			AddAttributes((XmlElement)BasicRuleXmlElement);
			CompileAttributes(appCapSID, svcCapSID);
		}

		protected override void AddAttributes(XmlElement BasicRuleXmlElement)
		{
			base.AddAttributes(BasicRuleXmlElement);
			switch (base.RuleType)
			{
			case "ServiceAccess":
				inputAttributeName = "Name";
				SetServiceAccessRights(BasicRuleXmlElement.GetAttribute(inputAttributeName), BasicRuleXmlElement.GetAttribute("Rights"));
				base.Flags |= 518u;
				break;
			case "COMAccessPermission":
				SetCOMRights(BasicRuleXmlElement.GetAttribute("AccessPermission"));
				inputAttributeName = "AppId";
				base.Flags |= 262660u;
				break;
			case "COMLaunchPermission":
				SetCOMRights(BasicRuleXmlElement.GetAttribute("LaunchPermission"));
				inputAttributeName = "AppId";
				base.Flags |= 262660u;
				break;
			case "WinRTAccessPermission":
				SetWinRTRights(BasicRuleXmlElement.GetAttribute("AccessPermission"));
				inputAttributeName = "ServerName";
				base.Flags |= 262660u;
				break;
			case "WinRTLaunchPermission":
				SetWinRTRights(BasicRuleXmlElement.GetAttribute("LaunchPermission"));
				inputAttributeName = "ServerName";
				base.Flags |= 262660u;
				break;
			case "DeviceSetupClass":
				inputAttributeName = "Guid";
				base.Flags = (base.Flags & 0xFFFF00FFu) | 0x80000000u | 4u;
				break;
			case "ETWProvider":
				inputAttributeName = "Guid";
				base.Flags |= 516u;
				break;
			case "SDRegValue":
			{
				inputAttributeName = "Path";
				base.Flags |= 4u;
				SaveAsString = BasicRuleXmlElement.GetAttribute("SaveAsString");
				string attribute = BasicRuleXmlElement.GetAttribute("SetOwner");
				if (string.IsNullOrEmpty(attribute) || !attribute.Equals("No", GlobalVariables.GlobalStringComparison))
				{
					base.Flags |= 512u;
				}
				break;
			}
			case "WNF":
				inputAttribute = GenerateWnfId(BasicRuleXmlElement.GetAttribute("Scope"), BasicRuleXmlElement.GetAttribute("Tag"), BasicRuleXmlElement.GetAttribute("Sequence"), BasicRuleXmlElement.GetAttribute("DataPermanent"));
				base.Flags |= 4u;
				return;
			default:
				throw new PolicyCompilerInternalException("Undefined rule type: " + base.RuleType);
			}
			inputAttribute = BasicRuleXmlElement.GetAttribute(inputAttributeName);
		}

		private void SetCOMRights(string value)
		{
			if (base.OwnerType == CapabilityOwnerType.Application || base.OwnerType == CapabilityOwnerType.Service)
			{
				if (base.RuleType == "COMAccessPermission")
				{
					base.Rights = "$(COM_LOCAL_ACCESS)";
				}
				else
				{
					base.Rights = "$(COM_LOCAL_LAUNCH)";
				}
			}
			else
			{
				base.Rights = value;
			}
		}

		private void SetWinRTRights(string value)
		{
			if (base.OwnerType == CapabilityOwnerType.Application || base.OwnerType == CapabilityOwnerType.Service)
			{
				if (base.RuleType == "WinRTAccessPermission")
				{
					base.Rights = "$(COM_LOCAL_ACCESS)";
				}
				else
				{
					base.Rights = "$(COM_LOCAL_LAUNCH)";
				}
			}
			else
			{
				base.Rights = value;
			}
		}

		private void SetServiceAccessRights(string serviceName, string value)
		{
			if (base.OwnerType == CapabilityOwnerType.Application || base.OwnerType == CapabilityOwnerType.Service)
			{
				if (base.ReadOnlyRights)
				{
					base.Rights = "$(GENERIC_READ)";
				}
				else
				{
					base.Rights = "$(SERVICE_PRIVATE_RESOURCE_ACCESS)";
				}
			}
			else
			{
				base.Rights = value;
			}
		}

		private static string GenerateWnfId(string scope, string tag, string sequence, string dataPermanent)
		{
			UTF32Encoding uTF32Encoding = new UTF32Encoding();
			uint num = 0u;
			uint num2 = 0u;
			uint num3 = 0u;
			uint num4 = 0u;
			uint num5 = 0u;
			switch (scope)
			{
			case "System":
				num = 0u;
				break;
			case "Session":
				num = 1u;
				break;
			case "User":
				num = 2u;
				break;
			case "Process":
				num = 3u;
				break;
			default:
				throw new PolicyCompilerInternalException("Undefined WNF scope value: " + scope);
			}
			byte[] array = new byte[8];
			for (int num6 = tag.Length - 1; num6 >= 0; num6--)
			{
				Array.Clear(array, 0, 8);
				uTF32Encoding.GetBytes(tag, num6, 1, array, 0);
				num5 = (num5 << 8) + (uint)BitConverter.ToInt32(array, 0);
			}
			num5 ^= 0x41C64E6Du;
			if (!string.IsNullOrEmpty(dataPermanent))
			{
				num3 = (uint)Convert.ToInt32(dataPermanent, GlobalVariables.Culture) & 1u;
			}
			num2 = (uint)Convert.ToInt32(sequence, GlobalVariables.Culture) & 0xFFu;
			num4 = 1u | (num << 6) | (num3 << 10) | (num2 << 11);
			num4 ^= 0xA3BC0074u;
			return string.Format(GlobalVariables.Culture, "{0:X8}{1:X8}", new object[2] { num5, num4 });
		}

		protected override void CompileAttributes(string appCapSID, string svcCapSID)
		{
			base.CompileAttributes(appCapSID, svcCapSID);
			CalculatePathAndType();
			base.ElementId = HashCalculator.CalculateSha256Hash("SDRegValue" + Path, false);
			if (base.RuleType == "ServiceAccess")
			{
				uint num = 2u;
				if ((AccessRightHelper.MergeAccessRight(base.ResolvedRights, inputAttribute, base.RuleType) & num) != 0)
				{
					throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "It is not allowed to grant SERVICE_CHANGE_CONFIG on service '{0}'.", new object[1] { inputAttribute }));
				}
			}
		}

		private void CalculatePathAndType()
		{
			switch (base.RuleType)
			{
			case "ServiceAccess":
				Path = "HKEY_LOCAL_MACHINE\\SYSTEM\\ControlSet001\\services\\" + inputAttribute + "\\Security\\Security";
				SDRegValueType = "ServiceAccess";
				break;
			case "COMAccessPermission":
				Path = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\AppID\\" + inputAttribute + "\\AccessPermission";
				SDRegValueType = "COM";
				break;
			case "COMLaunchPermission":
				Path = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\AppID\\" + inputAttribute + "\\LaunchPermission";
				SDRegValueType = "COM";
				break;
			case "WinRTAccessPermission":
				Path = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\WindowsRuntime\\Server\\" + inputAttribute + "\\Permissions";
				SDRegValueType = "WinRT";
				break;
			case "WinRTLaunchPermission":
				Path = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\WindowsRuntime\\Server\\" + inputAttribute + "\\Permissions";
				SDRegValueType = "WinRT";
				break;
			case "DeviceSetupClass":
				Path = "HKEY_LOCAL_MACHINE\\SYSTEM\\ControlSet001\\Control\\Class\\" + inputAttribute + "\\Properties\\Security";
				SDRegValueType = "DeviceSetupClass";
				break;
			case "ETWProvider":
				Path = "HKEY_LOCAL_MACHINE\\SYSTEM\\ControlSet001\\Control\\WMI\\Security\\" + inputAttribute;
				SDRegValueType = "ETWProvider";
				break;
			case "SDRegValue":
				Path = ResolveMacro(inputAttribute, "Path");
				SDRegValueType = "SDRegValue";
				break;
			case "WNF":
				Path = "HKEY_LOCAL_MACHINE\\SYSTEM\\ControlSet001\\Control\\Notifications\\" + inputAttribute;
				SDRegValueType = "WNF";
				break;
			}
			Path = NormalizedString.Get(Path);
		}

		public override string GetAttributesString()
		{
			if (SaveAsString != "Not Calculated")
			{
				return base.GetAttributesString() + Path + SaveAsString;
			}
			return base.GetAttributesString() + Path;
		}

		public override void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel4, base.RuleType + "Rule");
			base.Print();
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel5, "Path", Path);
		}
	}
}
