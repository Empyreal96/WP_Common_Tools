using System.Xml;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class DriverRule : BaseRule
	{
		public DriverRule(string ruleType)
		{
			base.RuleType = ruleType;
		}

		public override void Add(IXPathNavigable driverRuleXmlElement, string appCapSID, string svcCapSID)
		{
			base.AddAttributes((XmlElement)driverRuleXmlElement);
			base.CompileAttributes(appCapSID, svcCapSID);
		}

		public void Add(string appCapSID, string svcCapSID, string rights)
		{
			base.CompileResolvedAttributes(appCapSID, svcCapSID, rights);
		}
	}
}
