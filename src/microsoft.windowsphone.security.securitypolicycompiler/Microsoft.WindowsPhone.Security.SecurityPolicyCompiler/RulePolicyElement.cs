using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public abstract class RulePolicyElement
	{
		public abstract void Add(IXPathNavigable xmlPathNavigator, string appCapSID, string svcCapSID);

		public abstract string GetAttributesString();

		public abstract void Print();
	}
}
