using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class PrincipalClass : IPolicyElement
	{
		[XmlAttribute(AttributeName = "ElementID")]
		public string ElementId { get; set; }

		[XmlAttribute(AttributeName = "AttributeHash")]
		public string AttributeHash { get; set; }

		[XmlAttribute(AttributeName = "Name")]
		public string Name { get; set; }

		[XmlElement(ElementName = "Executables")]
		public Executables Executables { get; set; }

		[XmlElement(ElementName = "Directories")]
		public Directories Directories { get; set; }

		[XmlElement(ElementName = "Certificates")]
		public Certificates Certificates { get; set; }

		[XmlElement(ElementName = "Chambers")]
		public Chambers Chambers { get; set; }

		public void Add(IXPathNavigable principalClassXmlElement)
		{
			XmlElement principalClassXmlElement2 = (XmlElement)principalClassXmlElement;
			AddAttributes(principalClassXmlElement2);
			CompileAttributes();
			AddElements(principalClassXmlElement2);
			CalculateAttributeHash();
		}

		private void AddAttributes(XmlElement principalClassXmlElement)
		{
			Name = principalClassXmlElement.GetAttribute("Name");
		}

		private void CompileAttributes()
		{
			ElementId = HashCalculator.CalculateSha256Hash(Name, true);
		}

		private void AddElements(XmlElement principalClassXmlElement)
		{
			XmlNodeList xmlNodeList = principalClassXmlElement.SelectNodes("./WP_Policy:Executables", GlobalVariables.NamespaceManager);
			if (xmlNodeList.Count > 0)
			{
				Executables = new Executables();
				Executables.Add(xmlNodeList[0]);
			}
			XmlNodeList xmlNodeList2 = principalClassXmlElement.SelectNodes("./WP_Policy:Directories", GlobalVariables.NamespaceManager);
			if (xmlNodeList2.Count > 0)
			{
				Directories = new Directories();
				Directories.Add(xmlNodeList2[0]);
			}
			XmlNodeList xmlNodeList3 = principalClassXmlElement.SelectNodes("./WP_Policy:Certificates", GlobalVariables.NamespaceManager);
			if (xmlNodeList3.Count > 0)
			{
				Certificates = new Certificates();
				Certificates.Add(xmlNodeList3[0]);
			}
			XmlNodeList xmlNodeList4 = principalClassXmlElement.SelectNodes("./WP_Policy:Chambers", GlobalVariables.NamespaceManager);
			if (xmlNodeList4.Count > 0)
			{
				Chambers = new Chambers();
				Chambers.Add(xmlNodeList4[0]);
			}
		}

		private void CalculateAttributeHash()
		{
			StringBuilder stringBuilder = new StringBuilder(Name);
			if (Executables != null)
			{
				stringBuilder.Append(Executables.GetAllPaths());
			}
			if (Directories != null)
			{
				stringBuilder.Append(Directories.GetAllPaths());
			}
			if (Certificates != null)
			{
				stringBuilder.Append(Certificates.GetAllCertificateProperties());
			}
			if (Chambers != null)
			{
				stringBuilder.Append(Chambers.GetAllChamberProperties());
			}
			AttributeHash = HashCalculator.CalculateSha256Hash(stringBuilder.ToString(), true);
		}

		public virtual void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel2, "PrincipalClass");
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel3, "Name", Name);
			if (Executables != null)
			{
				instance.DebugLine(string.Empty);
				Executables.Print();
			}
			if (Directories != null)
			{
				instance.DebugLine(string.Empty);
				Directories.Print();
			}
			if (Certificates != null)
			{
				instance.DebugLine(string.Empty);
				Certificates.Print();
			}
			if (Chambers != null)
			{
				instance.DebugLine(string.Empty);
				Chambers.Print();
			}
		}
	}
}
