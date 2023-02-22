using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class Capabilities : IPolicyElement
	{
		private List<Capability> capabilityCollection = new List<Capability>();

		[XmlElement(ElementName = "Capability")]
		public List<Capability> CapabilityCollection => capabilityCollection;

		public void Add(IXPathNavigable capabilitiesXmlElement)
		{
			AddElements((XmlElement)capabilitiesXmlElement);
		}

		public void Append(Capability capability)
		{
			capabilityCollection.Add(capability);
		}

		private void AddElements(XmlElement capabilitiesXmlElement)
		{
			foreach (XmlElement item in capabilitiesXmlElement.SelectNodes("./WP_Policy:Capability", GlobalVariables.NamespaceManager))
			{
				Capability capability = new Capability(CapabilityOwnerType.StandAlone);
				capability.Add(item);
				capabilityCollection.Add(capability);
			}
			foreach (XmlElement item2 in capabilitiesXmlElement.SelectNodes("./WP_Policy:WindowsRules", GlobalVariables.NamespaceManager))
			{
				Capability capability2 = new WindowsRules();
				capability2.Add(item2);
				capabilityCollection.Add(capability2);
			}
		}

		public bool HasChild()
		{
			if (capabilityCollection != null)
			{
				return capabilityCollection.Count > 0;
			}
			return false;
		}

		public void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel1, "Capabilities");
			if (capabilityCollection == null)
			{
				return;
			}
			foreach (Capability item in capabilityCollection)
			{
				instance.DebugLine(string.Empty);
				item.Print();
			}
		}
	}
}
