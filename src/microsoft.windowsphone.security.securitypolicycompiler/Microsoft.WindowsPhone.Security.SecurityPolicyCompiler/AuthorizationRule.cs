using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public abstract class AuthorizationRule : IPolicyElement
	{
		[XmlAttribute(AttributeName = "ElementID")]
		public string ElementId { get; set; }

		[XmlAttribute(AttributeName = "AttributeHash")]
		public string AttributeHash { get; set; }

		[XmlAttribute(AttributeName = "Name")]
		public string Name { get; set; }

		public AuthorizationRule()
		{
		}

		public virtual void Add(IXPathNavigable authorizationRuleXmlElement)
		{
			XmlElement authorizationRuleXmlElement2 = (XmlElement)authorizationRuleXmlElement;
			AddAttributes(authorizationRuleXmlElement2);
		}

		private void AddAttributes(XmlElement authorizationRuleXmlElement)
		{
			Name = authorizationRuleXmlElement.GetAttribute("Name");
		}

		public abstract void Print();
	}
}
