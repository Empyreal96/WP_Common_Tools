using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class RequiredCapability : IPolicyElement
	{
		private CapabilityOwnerType ownerType = CapabilityOwnerType.Unknown;

		private string capId = "Not Calculated";

		private CapabilityOwnerType OwnerType
		{
			set
			{
				ownerType = value;
			}
		}

		[XmlAttribute(AttributeName = "CapId")]
		public string CapId
		{
			get
			{
				return capId;
			}
			set
			{
				capId = value;
			}
		}

		public RequiredCapability()
		{
		}

		public RequiredCapability(CapabilityOwnerType value)
		{
			OwnerType = value;
		}

		public void Add(IXPathNavigable requiredCapabilityXmlElement)
		{
			AddAttributes((XmlElement)requiredCapabilityXmlElement);
			CompileAttributes();
		}

		public void Add(string inputCapId, bool needCompile)
		{
			capId = inputCapId;
			if (needCompile)
			{
				CompileAttributes();
			}
		}

		private void AddAttributes(XmlElement requiredCapabilityXmlElement)
		{
			CapId = requiredCapabilityXmlElement.GetAttribute("CapId");
		}

		private void CompileAttributes()
		{
			switch (ownerType)
			{
			case CapabilityOwnerType.Application:
				CapId = SidBuilder.BuildApplicationCapabilitySidString(CapId);
				break;
			case CapabilityOwnerType.Service:
				CapId = GlobalVariables.SidMapping[CapId];
				break;
			default:
				throw new PolicyCompilerInternalException("SecurityPolicyCompiler Internal Error: RequiredCapability's OwnerType can't be determined");
			}
		}

		public void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel4, "RequiredCapability");
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel5, "CapId", CapId);
		}
	}
}
