using System.Xml;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class CapabilityRulesTransientObject : RuleWithPathInput
	{
		private string transientObjectType;

		public CapabilityRulesTransientObject()
		{
			base.RuleType = "TransientObject";
			base.RuleInheritanceInfo = false;
		}

		public CapabilityRulesTransientObject(CapabilityOwnerType value)
			: this()
		{
			base.OwnerType = value;
		}

		protected sealed override void AddAttributes(XmlElement BasicRuleXmlElement)
		{
			base.AddAttributes(BasicRuleXmlElement);
			transientObjectType = BasicRuleXmlElement.GetAttribute("Type");
			base.Flags |= 5u;
		}

		protected sealed override void CompileAttributes(string appCapSID, string svcCapSID)
		{
			base.CompileAttributes(appCapSID, svcCapSID);
			transientObjectType = ResolveMacro(transientObjectType, "Type");
			CalculatePath();
			CalculateElementId(string.Format(GlobalVariables.Culture, "{0}{1}", new object[2] { transientObjectType, base.NormalizedPath }));
		}

		private void CalculatePath()
		{
			base.Path = string.Format(GlobalVariables.Culture, "{0}{1}{2}{3}", "%5C%5C.%5C", transientObjectType, "%5C", base.Path);
			base.Path = base.Path.Replace("\\", "%5C");
		}
	}
}
