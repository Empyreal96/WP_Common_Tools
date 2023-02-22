using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	[XmlInclude(typeof(PrivateResources))]
	[XmlInclude(typeof(WindowsRules))]
	public class Capability : IPolicyElement
	{
		private CapabilityOwnerType ownerType = CapabilityOwnerType.Unknown;

		private string elementId = "Not Calculated";

		private string attributeHash = "Not Calculated";

		private string id = "Not Calculated";

		private string appCapSID;

		private string svcCapSID;

		private string friendlyName = "Not Calculated";

		private string visibility = "Not Calculated";

		private CapabilityRules capabilityRules;

		[XmlIgnore]
		public CapabilityOwnerType OwnerType
		{
			get
			{
				return ownerType;
			}
			set
			{
				ownerType = value;
			}
		}

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

		[XmlAttribute(AttributeName = "AttributeHash")]
		public string AttributeHash
		{
			get
			{
				return attributeHash;
			}
			set
			{
				attributeHash = value;
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

		[XmlAttribute(AttributeName = "AppCapSID")]
		public string AppCapSID
		{
			get
			{
				return appCapSID;
			}
			set
			{
				appCapSID = value;
			}
		}

		[XmlAttribute(AttributeName = "SvcCapSID")]
		public string SvcCapSID
		{
			get
			{
				return svcCapSID;
			}
			set
			{
				svcCapSID = value;
			}
		}

		[XmlAttribute(AttributeName = "FriendlyName")]
		public string FriendlyName
		{
			get
			{
				return friendlyName;
			}
			set
			{
				friendlyName = value;
			}
		}

		[XmlAttribute(AttributeName = "Visibility")]
		public string Visibility
		{
			get
			{
				return visibility;
			}
			set
			{
				visibility = value;
			}
		}

		[XmlElement(ElementName = "CapabilityRules")]
		public CapabilityRules CapabilityRules
		{
			get
			{
				return capabilityRules;
			}
			set
			{
				capabilityRules = value;
			}
		}

		public Capability()
		{
		}

		public Capability(CapabilityOwnerType value)
		{
			OwnerType = value;
		}

		public void Add(IXPathNavigable capabilityXmlElement)
		{
			XmlElement capabilityXmlElement2 = (XmlElement)capabilityXmlElement;
			AddAttributes(capabilityXmlElement2);
			CompileAttributes();
			AddElements(capabilityXmlElement2);
			CalculateAttributeHash();
		}

		protected virtual void AddAttributes(XmlElement capabilityXmlElement)
		{
			Id = capabilityXmlElement.GetAttribute("Id");
			FriendlyName = capabilityXmlElement.GetAttribute("FriendlyName");
			Visibility = capabilityXmlElement.GetAttribute("Visibility");
		}

		protected virtual void CompileAttributes()
		{
			bool flag = false;
			string[] validCapabilityVisibilityList = ConstantStrings.GetValidCapabilityVisibilityList();
			foreach (string text in validCapabilityVisibilityList)
			{
				if (Visibility == text)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Print();
				throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "Capability visibility is invalid, Capability ID ={0}\nNote visibility value is case sensitive", new object[1] { Id }));
			}
			ElementId = HashCalculator.CalculateSha256Hash(id, true);
			if (ConstantStrings.PredefinedServiceCapabilities.Contains(id))
			{
				SvcCapSID = SidBuilder.BuildSidString("S-1-5-21-2702878673-795188819-444038987", HashCalculator.CalculateSha256Hash(id, true), 8);
				AppCapSID = null;
			}
			else
			{
				AppCapSID = SidBuilder.BuildApplicationCapabilitySidString(id);
				SvcCapSID = GlobalVariables.SidMapping[id];
			}
		}

		protected virtual void AddElements(XmlElement capabilityXmlElement)
		{
			XmlNodeList xmlNodeList = capabilityXmlElement.SelectNodes("./WP_Policy:CapabilityRules", GlobalVariables.NamespaceManager);
			if (xmlNodeList.Count <= 0)
			{
				return;
			}
			if (capabilityRules == null)
			{
				capabilityRules = new CapabilityRules(OwnerType);
			}
			bool flag = ConstantStrings.PredefinedServiceCapabilities.Contains(id);
			foreach (XmlElement item in xmlNodeList)
			{
				if (flag)
				{
					capabilityRules.Add(item, null, id);
				}
				else
				{
					capabilityRules.Add(item, AppCapSID, SvcCapSID);
				}
			}
		}

		protected virtual void CalculateAttributeHash()
		{
			StringBuilder stringBuilder = new StringBuilder(Id);
			stringBuilder.Append(Visibility);
			if (capabilityRules != null)
			{
				stringBuilder.Append(capabilityRules.GetAllAttributesString());
			}
			else
			{
				if (AppCapSID != null)
				{
					stringBuilder.Append(AppCapSID);
				}
				if (SvcCapSID != null)
				{
					stringBuilder.Append(SvcCapSID);
				}
			}
			AttributeHash = HashCalculator.CalculateSha256Hash(stringBuilder.ToString(), true);
		}

		public void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel2, "Capability");
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel3, "ElementID", ElementId);
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel3, "AttributeHash", AttributeHash);
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel3, "Id", Id);
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel3, "AppCapSID", AppCapSID);
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel3, "SvcCapSID", SvcCapSID);
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel3, "FriendlyName", FriendlyName);
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel3, "Visibility", Visibility);
			if (capabilityRules != null)
			{
				instance.DebugLine(string.Empty);
				capabilityRules.Print();
			}
		}
	}
}
