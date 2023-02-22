using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class CapabilityClass : IPolicyElement
	{
		[XmlAttribute(AttributeName = "ElementID")]
		public string ElementId { get; set; }

		[XmlAttribute(AttributeName = "AttributeHash")]
		public string AttributeHash { get; set; }

		[XmlAttribute(AttributeName = "Name")]
		public string Name { get; set; }

		[XmlElement(ElementName = "MemberCapability")]
		public List<MemberCapability> CapabilityCollection { get; set; }

		[XmlElement(ElementName = "MemberCapabilityClass")]
		public List<MemberCapabilityClass> CapabilityClassCollection { get; set; }

		public void Add(IXPathNavigable capabilityClassXmlElement)
		{
			XmlElement capabilityClassXmlElement2 = (XmlElement)capabilityClassXmlElement;
			AddAttributes(capabilityClassXmlElement2);
			CompileAttributes();
			AddElements(capabilityClassXmlElement2);
			CalculateAttributeHash();
		}

		private void AddAttributes(XmlElement capabilityClassXmlElement)
		{
			Name = capabilityClassXmlElement.GetAttribute("Name");
		}

		private void CompileAttributes()
		{
			ElementId = HashCalculator.CalculateSha256Hash(Name, true);
		}

		private void AddElements(XmlElement capabilityClassXmlElement)
		{
			XmlNodeList xmlNodeList = capabilityClassXmlElement.SelectNodes("./WP_Policy:MemberCapability", GlobalVariables.NamespaceManager);
			if (xmlNodeList.Count > 0)
			{
				if (CapabilityCollection == null)
				{
					CapabilityCollection = new List<MemberCapability>();
				}
				foreach (XmlElement item in xmlNodeList)
				{
					MemberCapability memberCapability = new MemberCapability();
					memberCapability.Add(item);
					CapabilityCollection.Add(memberCapability);
				}
			}
			XmlNodeList xmlNodeList2 = capabilityClassXmlElement.SelectNodes("./WP_Policy:MemberCapabilityClass", GlobalVariables.NamespaceManager);
			if (xmlNodeList2.Count <= 0)
			{
				return;
			}
			if (CapabilityClassCollection == null)
			{
				CapabilityClassCollection = new List<MemberCapabilityClass>();
			}
			foreach (XmlElement item2 in xmlNodeList2)
			{
				MemberCapabilityClass memberCapabilityClass = new MemberCapabilityClass();
				memberCapabilityClass.Add(item2);
				CapabilityClassCollection.Add(memberCapabilityClass);
			}
		}

		private void CalculateAttributeHash()
		{
			StringBuilder stringBuilder = new StringBuilder(Name);
			if (CapabilityCollection != null)
			{
				foreach (MemberCapability item in CapabilityCollection)
				{
					stringBuilder.Append(item.Id);
				}
			}
			if (CapabilityClassCollection != null)
			{
				foreach (MemberCapabilityClass item2 in CapabilityClassCollection)
				{
					stringBuilder.Append(item2.Name);
				}
			}
			AttributeHash = HashCalculator.CalculateSha256Hash(stringBuilder.ToString(), true);
		}

		public virtual void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel2, "CapabilityClass");
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel3, "Name", Name);
			if (CapabilityCollection != null)
			{
				foreach (MemberCapability item in CapabilityCollection)
				{
					instance.DebugLine(string.Empty);
					item.Print();
				}
			}
			if (CapabilityClassCollection == null)
			{
				return;
			}
			foreach (MemberCapabilityClass item2 in CapabilityClassCollection)
			{
				instance.DebugLine(string.Empty);
				item2.Print();
			}
		}
	}
}
