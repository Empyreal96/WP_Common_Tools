using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public interface IPolicyElement
	{
		void Add(IXPathNavigable rulePolicyXmlElement);

		void Print();
	}
}
