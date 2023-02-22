using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class Certificates : IPolicyElement
	{
		[XmlElement(ElementName = "Certificate")]
		public List<Certificate> CertificateCollection { get; set; }

		public void Add(IXPathNavigable certificatesXmlElement)
		{
			XmlElement certificatesXmlElement2 = (XmlElement)certificatesXmlElement;
			AddElements(certificatesXmlElement2);
		}

		public void AddElements(XmlElement certificatesXmlElement)
		{
			XmlNodeList xmlNodeList = certificatesXmlElement.SelectNodes("./WP_Policy:Certificate", GlobalVariables.NamespaceManager);
			if (xmlNodeList.Count <= 0)
			{
				return;
			}
			if (CertificateCollection == null)
			{
				CertificateCollection = new List<Certificate>();
			}
			foreach (XmlElement item in xmlNodeList)
			{
				Certificate certificate = new Certificate();
				certificate.Add(item);
				CertificateCollection.Add(certificate);
			}
		}

		public string GetAllCertificateProperties()
		{
			string text = string.Empty;
			if (CertificateCollection != null)
			{
				foreach (Certificate item in CertificateCollection)
				{
					text += item.Type;
					if (item.EKU != null)
					{
						text += item.EKU;
					}
					if (item.Thumbprint != null)
					{
						text += item.ThumbprintAlgorithm;
						text += item.Thumbprint;
					}
				}
				return text;
			}
			return text;
		}

		public void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel3, "Certificate");
			foreach (Certificate item in CertificateCollection)
			{
				instance.DebugLine(string.Empty);
				item.Print();
			}
		}
	}
}
