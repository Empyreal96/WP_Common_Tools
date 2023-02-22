using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class RequiredCapabilities : IPolicyElement
	{
		private CapabilityOwnerType ownerType = CapabilityOwnerType.Unknown;

		private List<RequiredCapability> requiredCapabilityCollection = new List<RequiredCapability>();

		internal CapabilityOwnerType OwnerType
		{
			set
			{
				ownerType = value;
			}
		}

		[XmlElement(ElementName = "RequiredCapability")]
		public List<RequiredCapability> RequiredCapabilityCollection => requiredCapabilityCollection;

		public void Add(IXPathNavigable requiredCapabilitiesXmlElement)
		{
			AddElements((XmlElement)requiredCapabilitiesXmlElement);
		}

		public void Add(RequiredCapability requiredCapability)
		{
			requiredCapabilityCollection.Add(requiredCapability);
		}

		private void AddElements(XmlElement requiredCapabilitiesXmlElement)
		{
			foreach (XmlElement item in requiredCapabilitiesXmlElement.SelectNodes("./WP_Policy:RequiredCapability", GlobalVariables.NamespaceManager))
			{
				bool flag = false;
				string attribute = item.GetAttribute("CapId");
				string[] componentCapabilityIdFilter = ConstantStrings.ComponentCapabilityIdFilter;
				for (int i = 0; i < componentCapabilityIdFilter.Length; i++)
				{
					if (componentCapabilityIdFilter[i] == attribute)
					{
						flag = true;
						break;
					}
				}
				if (ownerType == CapabilityOwnerType.Service)
				{
					componentCapabilityIdFilter = ConstantStrings.ServiceCapabilityIdFilter;
					for (int i = 0; i < componentCapabilityIdFilter.Length; i++)
					{
						if (componentCapabilityIdFilter[i] == attribute)
						{
							flag = true;
							break;
						}
					}
				}
				if (ownerType == CapabilityOwnerType.Application)
				{
					componentCapabilityIdFilter = ConstantStrings.BlockedCapabilityIdForApplicationFilter;
					for (int i = 0; i < componentCapabilityIdFilter.Length; i++)
					{
						if (componentCapabilityIdFilter[i] == attribute)
						{
							throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "The capability '{0}' can't be used in application.", new object[1] { attribute }));
						}
					}
				}
				if (!flag)
				{
					RequiredCapability requiredCapability = new RequiredCapability(ownerType);
					requiredCapability.Add(item);
					Add(requiredCapability);
				}
			}
		}

		public string GetAllCapIds()
		{
			string text = string.Empty;
			if (requiredCapabilityCollection != null)
			{
				foreach (RequiredCapability item in requiredCapabilityCollection)
				{
					text += item.CapId;
				}
				return text;
			}
			return text;
		}

		public void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel3, "RequiredCapabilities");
			foreach (RequiredCapability item in requiredCapabilityCollection)
			{
				instance.DebugLine(string.Empty);
				item.Print();
			}
		}
	}
}
