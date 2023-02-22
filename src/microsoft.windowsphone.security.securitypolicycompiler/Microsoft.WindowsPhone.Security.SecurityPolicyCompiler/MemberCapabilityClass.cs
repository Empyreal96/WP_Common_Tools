using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class MemberCapabilityClass : IPolicyElement
	{
		[XmlAttribute(AttributeName = "Name")]
		public string Name { get; set; }

		public virtual void Add(IXPathNavigable capabilityClassXmlElement)
		{
			AddAttributes((XmlElement)capabilityClassXmlElement);
		}

		protected virtual void AddAttributes(XmlElement capabilityClassXmlElement)
		{
			Name = capabilityClassXmlElement.GetAttribute("Name");
		}

		public virtual void Print()
		{
			ReportingBase.GetInstance().XmlAttributeLine(ConstantStrings.IndentationLevel4, "Name", Name);
		}
	}
}
