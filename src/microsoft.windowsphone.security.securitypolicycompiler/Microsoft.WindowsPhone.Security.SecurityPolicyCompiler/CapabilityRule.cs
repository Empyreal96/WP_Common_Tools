using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class CapabilityRule : AuthorizationRule
	{
		[XmlAttribute(AttributeName = "CapabilityClass")]
		public string CapabilityClass { get; set; }

		[XmlAttribute(AttributeName = "PrincipalClass")]
		public string PrincipalClass { get; set; }

		public override void Add(IXPathNavigable capabilityRuleXmlElement)
		{
			base.Add(capabilityRuleXmlElement);
			XmlElement capabilityRuleXmlElement2 = (XmlElement)capabilityRuleXmlElement;
			AddAttributes(capabilityRuleXmlElement2);
			CompileAttributes();
			CalculateAttributeHash();
		}

		private void AddAttributes(XmlElement capabilityRuleXmlElement)
		{
			CapabilityClass = capabilityRuleXmlElement.GetAttribute("CapabilityClass");
			PrincipalClass = capabilityRuleXmlElement.GetAttribute("PrincipalClass");
		}

		private void CompileAttributes()
		{
			base.ElementId = HashCalculator.CalculateSha256Hash(base.Name, true);
		}

		private void CalculateAttributeHash()
		{
			base.AttributeHash = HashCalculator.CalculateSha256Hash(base.Name + CapabilityClass + PrincipalClass, true);
		}

		public override void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel2, "CapabilityRule");
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel3, "Name", base.Name);
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel3, "CapabilityClass", CapabilityClass);
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel3, "PrincipalClass", PrincipalClass);
		}
	}
}
