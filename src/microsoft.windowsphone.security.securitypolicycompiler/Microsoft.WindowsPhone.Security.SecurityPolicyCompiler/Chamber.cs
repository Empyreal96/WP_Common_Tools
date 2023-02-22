using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class Chamber : IPolicyElement
	{
		[XmlAttribute(AttributeName = "Name")]
		public string Name { get; set; }

		public virtual void Add(IXPathNavigable chamberXmlElement)
		{
			AddAttributes((XmlElement)chamberXmlElement);
		}

		protected virtual void AddAttributes(XmlElement chamberXmlElement)
		{
			Name = chamberXmlElement.GetAttribute("Name");
		}

		public virtual void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel3, "Chamber");
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel4, "Name", Name);
		}
	}
}
