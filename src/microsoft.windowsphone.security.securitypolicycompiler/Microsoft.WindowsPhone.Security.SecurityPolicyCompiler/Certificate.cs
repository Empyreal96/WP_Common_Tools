using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class Certificate : IPolicyElement
	{
		[XmlAttribute(AttributeName = "Type")]
		public string Type { get; set; }

		[XmlAttribute(AttributeName = "EKU")]
		public string EKU { get; set; }

		[XmlAttribute(AttributeName = "Alg")]
		public string ThumbprintAlgorithm { get; set; }

		[XmlAttribute(AttributeName = "Thumbprint")]
		public string Thumbprint { get; set; }

		public virtual void Add(IXPathNavigable certificateXmlElement)
		{
			AddAttributes((XmlElement)certificateXmlElement);
		}

		protected virtual void AddAttributes(XmlElement certificateXmlElement)
		{
			Type = certificateXmlElement.GetAttribute("Type");
			if (certificateXmlElement.HasAttribute("EKU"))
			{
				EKU = certificateXmlElement.GetAttribute("EKU");
			}
			if (certificateXmlElement.HasAttribute("Thumbprint"))
			{
				ThumbprintAlgorithm = "Sha256";
				uint num = 0u;
				if (!string.IsNullOrEmpty(certificateXmlElement.GetAttribute("Alg")))
				{
					ThumbprintAlgorithm = certificateXmlElement.GetAttribute("Alg");
				}
				Thumbprint = NormalizedString.Get(certificateXmlElement.GetAttribute("Thumbprint"));
				switch (ThumbprintAlgorithm)
				{
				case "Sha256":
					num = 64u;
					break;
				case "Sha384":
					num = 96u;
					break;
				case "Sha512":
					num = 128u;
					break;
				}
				if (Thumbprint.Length != num)
				{
					throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "Invalid {0} attribute. Length should be {1} characters", new object[2] { "Thumbprint", num }));
				}
			}
			if (EKU == null && Thumbprint == null)
			{
				throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "{0} element must have an {1} attribute or a {2} attribute", new object[3] { "Certificate", "EKU", "Thumbprint" }));
			}
		}

		public virtual void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel3, "Certificate");
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel4, "Type", Type);
			if (EKU != null)
			{
				instance.XmlAttributeLine(ConstantStrings.IndentationLevel4, "EKU", EKU);
			}
			if (Thumbprint != null)
			{
				instance.XmlAttributeLine(ConstantStrings.IndentationLevel4, "Alg", ThumbprintAlgorithm);
				instance.XmlAttributeLine(ConstantStrings.IndentationLevel4, "Thumbprint", Thumbprint);
			}
		}
	}
}
