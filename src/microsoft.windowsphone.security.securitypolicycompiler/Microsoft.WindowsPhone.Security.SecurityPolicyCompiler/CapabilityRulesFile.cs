using System.Xml;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class CapabilityRulesFile : RuleWithPathInput
	{
		public CapabilityRulesFile()
		{
			base.RuleType = "File";
			base.RuleInheritanceInfo = false;
		}

		public CapabilityRulesFile(CapabilityOwnerType value)
			: this()
		{
			base.OwnerType = value;
		}

		protected sealed override void AddAttributes(XmlElement BasicRuleXmlElement)
		{
			base.AddAttributes(BasicRuleXmlElement);
			base.Flags |= 260u;
		}

		protected sealed override void ValidateOutPath()
		{
			ValidateFileOutPath(false);
		}
	}
}
