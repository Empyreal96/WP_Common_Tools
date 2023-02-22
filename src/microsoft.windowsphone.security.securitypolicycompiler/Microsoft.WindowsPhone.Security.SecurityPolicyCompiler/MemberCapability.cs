using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class MemberCapability : IPolicyElement
	{
		[XmlAttribute(AttributeName = "Id")]
		public string Id { get; set; }

		[XmlAttribute(AttributeName = "CapId")]
		public string CapId { get; set; }

		public virtual void Add(IXPathNavigable capabilityXmlElement)
		{
			AddAttributes((XmlElement)capabilityXmlElement);
			CompileAttributes();
		}

		protected virtual void AddAttributes(XmlElement capabilityXmlElement)
		{
			Id = capabilityXmlElement.GetAttribute("Id");
		}

		private void CompileAttributes()
		{
			Id = GlobalVariables.ResolveMacroReference(Id, string.Format(GlobalVariables.Culture, "Element={0}, Attribute={1}", new object[2] { "MemberCapability", "Id" }));
			if (Id.StartsWith("S-1-15-3"))
			{
				CapId = SidBuilder.BuildApplicationCapabilitySidString(ConstantStrings.EveryoneCapability);
			}
			else
			{
				CapId = SidBuilder.BuildApplicationCapabilitySidString(Id);
			}
		}

		public virtual void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel4, "Id", Id);
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel4, "CapId", CapId);
		}
	}
}
