using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public abstract class Component
	{
		private CapabilityOwnerType ownerType = CapabilityOwnerType.Unknown;

		private string elementId = "Not Calculated";

		private string OEMExtensibleValue;

		private string attributeHash = "Not Calculated";

		private string componentSid = "Not Calculated";

		private string name = "Not Calculated";

		private string privateCapSID;

		private RequiredCapabilities requiredCapabilities = new RequiredCapabilities();

		private PrivateResources privateResources;

		protected CapabilityOwnerType OwnerType
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

		[XmlAttribute(AttributeName = "OEMExtensible")]
		public string OEMExtensible
		{
			get
			{
				return OEMExtensibleValue;
			}
			set
			{
				OEMExtensibleValue = value;
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

		[XmlAttribute(AttributeName = "SID")]
		public string ComponentSID
		{
			get
			{
				return componentSid;
			}
			set
			{
				componentSid = value;
			}
		}

		[XmlAttribute(AttributeName = "Name")]
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		[XmlAttribute(AttributeName = "PrivateCapSID")]
		public string PrivateCapSID
		{
			get
			{
				return privateCapSID;
			}
			set
			{
				privateCapSID = value;
			}
		}

		[XmlElement(ElementName = "RequiredCapabilities")]
		public RequiredCapabilities RequiredCapabilities
		{
			get
			{
				return requiredCapabilities;
			}
			set
			{
				requiredCapabilities = value;
			}
		}

		[XmlIgnore]
		public PrivateResources PrivateResources
		{
			get
			{
				return privateResources;
			}
			set
			{
				privateResources = value;
			}
		}

		private XmlElement CreateNewPrivateCapabilityXmlElementForApplicationCodeFolders(XmlElement componentXmlElement, string FolderName, string FolderPath)
		{
			XmlDocument ownerDocument = componentXmlElement.OwnerDocument;
			XmlElement xmlElement = ownerDocument.CreateElement(null, "PrivateResources", "urn:Microsoft.WindowsPhone/PackageSchema.v8.00");
			XmlElement xmlElement2 = ownerDocument.CreateElement(null, FolderName, "urn:Microsoft.WindowsPhone/PackageSchema.v8.00");
			xmlElement2.SetAttribute("Path", string.Format(GlobalVariables.Culture, FolderPath, new object[1] { Name }));
			xmlElement.AppendChild(xmlElement2);
			return xmlElement;
		}

		private XmlElement CreateNewPrivateCapabilityXmlElementForApplicationDataFolders(XmlElement componentXmlElement, string FolderName, string FolderPath)
		{
			XmlDocument ownerDocument = componentXmlElement.OwnerDocument;
			XmlElement xmlElement = ownerDocument.CreateElement(null, "PrivateResources", "urn:Microsoft.WindowsPhone/PackageSchema.v8.00");
			XmlElement xmlElement2 = ownerDocument.CreateElement(null, FolderName, "urn:Microsoft.WindowsPhone/PackageSchema.v8.00");
			xmlElement2.SetAttribute("Path", string.Format(GlobalVariables.Culture, FolderPath, new object[2] { "DA0", Name }));
			xmlElement.AppendChild(xmlElement2);
			return xmlElement;
		}

		protected virtual void AddAttributes(XmlElement componentXmlElement)
		{
			Name = NormalizedString.Get(componentXmlElement.GetAttribute("Name"));
			KeyValuePair<string, string>[] attributes = new KeyValuePair<string, string>[1]
			{
				new KeyValuePair<string, string>("Name", Name)
			};
			string empty = string.Empty;
			empty = componentXmlElement.GetAttribute("OEMExtensible");
			if (!string.IsNullOrEmpty(empty))
			{
				OEMExtensible = empty;
			}
			switch (ownerType)
			{
			case CapabilityOwnerType.Application:
				MiscUtils.RegisterObjectSpecificMacros(GlobalVariables.MacroResolver, ObjectType.Application, attributes);
				break;
			case CapabilityOwnerType.Service:
				MiscUtils.RegisterObjectSpecificMacros(GlobalVariables.MacroResolver, ObjectType.Service, attributes);
				break;
			default:
				throw new PolicyCompilerInternalException("SecurityPolicyCompiler Internal Error: Component's OwnerType can't be determined.");
			}
		}

		protected virtual void CompileAttributes(XmlElement componentXmlElement)
		{
			ElementId = HashCalculator.CalculateSha256Hash(Name, true);
			switch (ownerType)
			{
			case CapabilityOwnerType.Application:
				ComponentSID = SidBuilder.BuildApplicationSidString(Name);
				break;
			case CapabilityOwnerType.Service:
				ComponentSID = SidBuilder.BuildServiceSidString(Name);
				break;
			default:
				throw new PolicyCompilerInternalException("SecurityPolicyCompiler Internal Error: Component's OwnerType can't be determined");
			}
		}

		protected virtual void AddElements(XmlElement componentXmlElement)
		{
			AddRequiredCapabilities(componentXmlElement);
			AddPrivateResources(componentXmlElement);
		}

		protected void AddRequiredCapabilities(XmlElement componentXmlElement)
		{
			foreach (XmlElement item in componentXmlElement.SelectNodes("./WP_Policy:RequiredCapabilities", GlobalVariables.NamespaceManager))
			{
				requiredCapabilities.Add(item);
			}
			string[] componentCapabilityIdFilter = ConstantStrings.ComponentCapabilityIdFilter;
			foreach (string inputCapId in componentCapabilityIdFilter)
			{
				RequiredCapability requiredCapability = new RequiredCapability(ownerType);
				requiredCapability.Add(inputCapId, true);
				requiredCapabilities.Add(requiredCapability);
			}
			if (ownerType == CapabilityOwnerType.Service)
			{
				componentCapabilityIdFilter = ConstantStrings.ServiceCapabilityIdFilter;
				foreach (string inputCapId2 in componentCapabilityIdFilter)
				{
					RequiredCapability requiredCapability2 = new RequiredCapability(ownerType);
					requiredCapability2.Add(inputCapId2, true);
					requiredCapabilities.Add(requiredCapability2);
				}
			}
		}

		protected void AddPrivateResources(XmlElement componentXmlElement)
		{
			XmlNodeList xmlNodeList = componentXmlElement.SelectNodes("./WP_Policy:PrivateResources", GlobalVariables.NamespaceManager);
			if (ownerType == CapabilityOwnerType.Application)
			{
				XmlElement capabilityXmlElement = CreateNewPrivateCapabilityXmlElementForApplicationCodeFolders(componentXmlElement, "InstallationFolder", "\\PROGRAMS\\{0}\\(*)");
				XmlElement capabilityXmlElement2 = CreateNewPrivateCapabilityXmlElementForApplicationDataFolders(componentXmlElement, "ChamberProfileDataDefaultFolder", "\\DATA\\{0}\\{1}\\(*)");
				XmlElement capabilityXmlElement3 = CreateNewPrivateCapabilityXmlElementForApplicationDataFolders(componentXmlElement, "ChamberProfileDataShellContentFolder", "\\DATA\\{0}\\{1}\\Local\\Shared\\ShellContent\\(*)");
				XmlElement capabilityXmlElement4 = CreateNewPrivateCapabilityXmlElementForApplicationDataFolders(componentXmlElement, "ChamberProfileDataMediaFolder", "\\DATA\\{0}\\{1}\\Local\\Shared\\Media\\(*)");
				XmlElement capabilityXmlElement5 = CreateNewPrivateCapabilityXmlElementForApplicationDataFolders(componentXmlElement, "ChamberProfileDataPlatformDataFolder", "\\DATA\\{0}\\{1}\\PlatformData\\(*)");
				XmlElement capabilityXmlElement6 = CreateNewPrivateCapabilityXmlElementForApplicationDataFolders(componentXmlElement, "ChamberProfileDataLiveTilesFolder", "\\DATA\\{0}\\{1}\\PlatformData\\LiveTiles\\(*)");
				if (privateResources == null)
				{
					privateResources = new PrivateResources();
					privateResources.SetPrivateResourcesOwner(Name, ownerType, componentSid);
				}
				privateResources.Add(capabilityXmlElement);
				privateResources.Add(capabilityXmlElement2);
				privateResources.Add(capabilityXmlElement3);
				privateResources.Add(capabilityXmlElement4);
				privateResources.Add(capabilityXmlElement5);
				privateResources.Add(capabilityXmlElement6);
			}
			if (xmlNodeList.Count <= 0)
			{
				return;
			}
			if (PolicyCompiler.BlockPolicyDefinition)
			{
				throw new PolicyCompilerInternalException("The private resources can't be defined in this package.");
			}
			if (privateResources == null)
			{
				privateResources = new PrivateResources();
				privateResources.SetPrivateResourcesOwner(Name, ownerType, componentSid);
			}
			foreach (XmlElement item in xmlNodeList)
			{
				privateResources.Add(item);
			}
		}

		protected void AddPrivateCapSIDIfAny()
		{
			if (HasPrivateResources())
			{
				switch (ownerType)
				{
				case CapabilityOwnerType.Application:
					PrivateCapSID = privateResources.AppCapSID;
					break;
				case CapabilityOwnerType.Service:
					PrivateCapSID = privateResources.SvcCapSID;
					break;
				default:
					throw new PolicyCompilerInternalException("SecurityPolicyCompiler Internal Error: ComponentCapabilities's OwnerType can't be determined.");
				}
			}
		}

		public bool HasPrivateResources()
		{
			return privateResources != null;
		}
	}
}
