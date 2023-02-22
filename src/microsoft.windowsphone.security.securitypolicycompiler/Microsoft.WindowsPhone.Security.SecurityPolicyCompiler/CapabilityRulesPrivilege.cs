using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class CapabilityRulesPrivilege : RulePolicyElement
	{
		private CapabilityOwnerType ownerType = CapabilityOwnerType.Unknown;

		private string elementId = "Not Calculated";

		private string id = "Not Calculated";

		protected CapabilityOwnerType OwnerType => ownerType;

		[XmlAttribute(AttributeName = "ElementID")]
		public string ElementId
		{
			get
			{
				return elementId;
			}
			set
			{
				elementId = value;
			}
		}

		[XmlAttribute(AttributeName = "Id")]
		public string Id
		{
			get
			{
				return id;
			}
			set
			{
				id = value;
			}
		}

		public CapabilityRulesPrivilege()
		{
		}

		public CapabilityRulesPrivilege(CapabilityOwnerType value)
		{
			ownerType = value;
		}

		public override void Add(IXPathNavigable privilegeRuleXmlElement, string appCapSID, string svcCapSID)
		{
			if (OwnerType == CapabilityOwnerType.Application || OwnerType == CapabilityOwnerType.Service)
			{
				throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "{0} cannot be private resource", new object[1] { "Privilege" }));
			}
			AddAttributes((XmlElement)privilegeRuleXmlElement);
			CompileAttributes();
		}

		protected void AddAttributes(IXPathNavigable privilegeRuleXmlElement)
		{
			id = ((XmlElement)privilegeRuleXmlElement).GetAttribute("Id");
		}

		protected void CompileAttributes()
		{
			Id = GlobalVariables.ResolveMacroReference(id, string.Format(GlobalVariables.Culture, "Element={0}, Attribute={1}", new object[2] { "Privilege", "Id" }));
			ElementId = HashCalculator.CalculateSha256Hash(id, true);
		}

		public override string GetAttributesString()
		{
			return id;
		}

		public override void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel4, "Privilege");
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel5, "Id", Id);
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel5, "Id", ElementId);
		}
	}
}
