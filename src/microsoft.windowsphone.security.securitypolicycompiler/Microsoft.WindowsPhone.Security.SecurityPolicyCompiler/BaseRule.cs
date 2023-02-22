using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public abstract class BaseRule : RulePolicyElement
	{
		private CapabilityOwnerType ownerType = CapabilityOwnerType.Unknown;

		private string ruleType;

		private string resolvedRights;

		private bool readOnlyRights;

		private string inheritance = string.Empty;

		private string inRights;

		private string elementId = "Not Calculated";

		private string dacl = "Not Calculated";

		protected const uint ProtectedDaclFlag = 2147483648u;

		protected const uint ProtectedSaclFlag = 1073741824u;

		protected const uint DefaultDaclFlagMask = 255u;

		protected const uint DefaultDaclGenericAccessInheritFlag = 1u;

		protected const uint DefaultDaclGenericAccessFlag = 2u;

		protected const uint DefaultDaclAllAccessInheritFlag = 3u;

		protected const uint DefaultDaclAllAccessFlag = 4u;

		protected const uint DefaultDaclAllAccessWithCOFlag = 5u;

		protected const uint DefaultDaclServiceAccessFlag = 6u;

		protected const uint DefaultOwnerFlagMask = 65280u;

		protected const uint DefaultOwnerTrustedInstallerFlag = 256u;

		protected const uint DefaultOwnerSystemFlag = 512u;

		protected const uint DefaultOwnerAdminFlag = 768u;

		protected const uint DefaultOwnerAdminSystemFlag = 1024u;

		protected const uint DefaultMandatoryLabelFlagMask = 16711680u;

		protected const uint DefaultMandatoryLabelInheritFlag = 65536u;

		protected const uint DefaultMandatoryLabelFlag = 131072u;

		protected const uint DefaultExecuteMandatoryLabelInheritFlag = 196608u;

		protected const uint DefaultExecuteMandatoryLabelFlag = 262144u;

		protected const uint DefaultWriteMandatoryLabelInheritFlag = 327680u;

		protected const uint DefaultWriteMandatoryLabelFlag = 393216u;

		private uint flags;

		protected CapabilityOwnerType OwnerType
		{
			get
			{
				return ownerType;
			}
			set
			{
				ownerType = value;
			}
		}

		protected string RuleType
		{
			get
			{
				return ruleType;
			}
			set
			{
				ruleType = value;
			}
		}

		protected string ResolvedRights => resolvedRights;

		protected bool ReadOnlyRights => readOnlyRights;

		protected string Inheritance
		{
			get
			{
				return inheritance;
			}
			set
			{
				inheritance = value;
			}
		}

		protected string Rights
		{
			get
			{
				return inRights;
			}
			set
			{
				inRights = value;
			}
		}

		[XmlAttribute(AttributeName = "ElementID")]
		public string ElementId
		{
			get
			{
				return elementId;
			}
			set
			{
				elementId = value;
			}
		}

		[XmlAttribute(AttributeName = "DACL")]
		public string DACL
		{
			get
			{
				return dacl;
			}
			set
			{
				dacl = value;
			}
		}

		[XmlAttribute(AttributeName = "Flags")]
		public uint Flags
		{
			get
			{
				return flags;
			}
			set
			{
				flags = value;
			}
		}

		protected virtual void AddAttributes(XmlElement baseRuleXmlElement)
		{
			if (ownerType == CapabilityOwnerType.Application || ownerType == CapabilityOwnerType.Service)
			{
				string attribute = baseRuleXmlElement.GetAttribute("ReadOnly");
				if (!string.IsNullOrEmpty(attribute) && !attribute.Equals("No", GlobalVariables.GlobalStringComparison))
				{
					readOnlyRights = true;
				}
				if (readOnlyRights)
				{
					Rights = "$(GENERIC_READ)";
				}
				else
				{
					Rights = "$(ALL_ACCESS)";
				}
				flags |= 2147483648u;
			}
			else
			{
				Rights = baseRuleXmlElement.GetAttribute("Rights");
			}
		}

		protected virtual void CompileAttributes(string appCapSID, string svcCapSID)
		{
			resolvedRights = ResolveMacro(inRights, "Rights");
			CalculateDACL(appCapSID, svcCapSID);
		}

		protected virtual void CompileResolvedAttributes(string appCapSID, string svcCapSID, string rights)
		{
			resolvedRights = rights;
			CalculateDACL(appCapSID, svcCapSID);
		}

		protected string ResolveMacro(string valueWithMacro, string type)
		{
			return GlobalVariables.ResolveMacroReference(valueWithMacro, string.Format(GlobalVariables.Culture, "Element={0}, Attribute={1}", new object[2] { ruleType, type }));
		}

		protected void CalculateDACL(string appCapSID, string svcCapSID)
		{
			DACL = string.Empty;
			if (!string.IsNullOrEmpty(appCapSID))
			{
				DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { Inheritance, resolvedRights, appCapSID });
				DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { Inheritance, resolvedRights, "S-1-5-21-2702878673-795188819-444038987-1030" });
			}
			if (!string.IsNullOrEmpty(svcCapSID))
			{
				if (svcCapSID == "ID_CAP_NTSERVICES")
				{
					DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { Inheritance, resolvedRights, "BU" });
					DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { Inheritance, resolvedRights, "S-1-5-33" });
				}
				else
				{
					DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { Inheritance, resolvedRights, svcCapSID });
				}
			}
		}

		protected void CalculateElementId(string value)
		{
			ElementId = HashCalculator.CalculateSha256Hash(ruleType + value, false);
		}

		public override string GetAttributesString()
		{
			return Flags.ToString(GlobalVariables.Culture) + DACL;
		}

		public override void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel5, "ElementID", ElementId);
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel5, "DACL", DACL);
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel5, "Flags", Flags.ToString(GlobalVariables.Culture));
		}
	}
}
