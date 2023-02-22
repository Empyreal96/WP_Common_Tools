using System.Xml;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class CapabilityRulesWindows : CapabilityRulesPrivilege
	{
		public CapabilityRulesWindows()
		{
		}

		public CapabilityRulesWindows(CapabilityOwnerType value)
			: base(value)
		{
		}

		public override void Add(IXPathNavigable windowsCapbilityRuleXmlElement, string appCapSID, string svcCapSID)
		{
			if (base.OwnerType == CapabilityOwnerType.Application || base.OwnerType == CapabilityOwnerType.Service)
			{
				throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "{0} cannot be private resource", new object[1] { "WindowsCapability" }));
			}
			AddAttributes((XmlElement)windowsCapbilityRuleXmlElement);
			CompileAttributes();
		}

		public override void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel4, "WindowsCapability");
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel5, "Id", base.Id);
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel5, "Id", base.ElementId);
		}
	}
}
