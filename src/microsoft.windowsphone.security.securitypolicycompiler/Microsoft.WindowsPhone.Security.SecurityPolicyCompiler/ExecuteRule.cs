using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class ExecuteRule : AuthorizationRule
	{
		[XmlAttribute(AttributeName = "PrincipalClass")]
		public string PrincipalClass { get; set; }

		[XmlAttribute(AttributeName = "TargetChamber")]
		public string TargetChamber { get; set; }

		public override void Add(IXPathNavigable executeRuleXmlElement)
		{
			base.Add(executeRuleXmlElement);
			XmlElement executeRuleXmlElement2 = (XmlElement)executeRuleXmlElement;
			AddAttributes(executeRuleXmlElement2);
			CompileAttributes();
			CalculateAttributeHash();
		}

		private void AddAttributes(XmlElement executeRuleXmlElement)
		{
			PrincipalClass = executeRuleXmlElement.GetAttribute("PrincipalClass");
			TargetChamber = NormalizedString.Get(executeRuleXmlElement.GetAttribute("TargetChamber"));
		}

		private void CompileAttributes()
		{
			base.ElementId = HashCalculator.CalculateSha256Hash(base.Name, true);
			TargetChamber = GlobalVariables.ResolveMacroReference(TargetChamber, string.Format(GlobalVariables.Culture, "Element={0}, Attribute={1}", new object[2] { "ExecuteRule", "TargetChamber" }));
		}

		private void CalculateAttributeHash()
		{
			base.AttributeHash = HashCalculator.CalculateSha256Hash(base.Name + PrincipalClass + TargetChamber, true);
		}

		public override void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel2, "ExecuteRule");
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel3, "Name", base.Name);
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel3, "PrincipalClass", PrincipalClass);
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel3, "TargetChamber", TargetChamber);
		}
	}
}
