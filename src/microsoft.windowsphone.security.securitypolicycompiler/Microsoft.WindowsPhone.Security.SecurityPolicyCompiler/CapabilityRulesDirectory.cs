using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	[XmlInclude(typeof(CapabilityRulesInstallationFolder))]
	[XmlInclude(typeof(CapabilityRulesChamberProfileDefaultDataFolder))]
	[XmlInclude(typeof(CapabilityRulesChamberProfileShellContentFolder))]
	[XmlInclude(typeof(CapabilityRulesChamberProfileMediaFolder))]
	[XmlInclude(typeof(CapabilityRulesChamberProfilePlatformDataFolder))]
	[XmlInclude(typeof(CapabilityRulesChamberProfileLiveTilesFolder))]
	public class CapabilityRulesDirectory : RuleWithPathInput
	{
		public CapabilityRulesDirectory()
		{
			base.RuleType = "Directory";
			base.RuleInheritanceInfo = true;
			base.Inheritance = "CIOI";
		}

		public CapabilityRulesDirectory(CapabilityOwnerType value)
			: this()
		{
			base.OwnerType = value;
		}

		protected override void AddAttributes(XmlElement BasicRuleXmlElement)
		{
			base.AddAttributes(BasicRuleXmlElement);
			base.Flags |= 259u;
		}

		protected override PathInheritanceType ResolvePathMacroAndInheritance()
		{
			PathInheritanceType pathInheritanceType = base.ResolvePathMacroAndInheritance();
			if (PathInheritanceType.ContainerObjectInheritable == pathInheritanceType)
			{
				base.Inheritance = "CIOI";
			}
			else if (PathInheritanceType.ContainerObjectInheritable_InheritOnly == pathInheritanceType)
			{
				base.Inheritance = "IOCIOI";
			}
			return pathInheritanceType;
		}

		protected override void ValidateOutPath()
		{
			ValidateFileOutPath(false);
		}
	}
}
