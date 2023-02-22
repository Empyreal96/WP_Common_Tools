using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	[XmlRoot("PhoneSecurityPolicy")]
	public class PolicyXmlClass
	{
		private string fileFullPath = string.Empty;

		private string description;

		private string vendor;

		private string requiredOSVersion;

		private string fileVersion;

		private string hashType = "Sha256";

		private string packageId;

		private Capabilities capabilities;

		private Components components;

		private Authorization authorization;

		[XmlAttribute(AttributeName = "Description")]
		public string Description
		{
			get
			{
				return description;
			}
			set
			{
				description = value;
			}
		}

		[XmlAttribute(AttributeName = "Vendor")]
		public string Vendor
		{
			get
			{
				return vendor;
			}
			set
			{
				vendor = value;
			}
		}

		[XmlAttribute(AttributeName = "RequiredOSVersion")]
		public string RequiredOSVersion
		{
			get
			{
				return requiredOSVersion;
			}
			set
			{
				requiredOSVersion = value;
			}
		}

		[XmlAttribute(AttributeName = "FileVersion")]
		public string FileVersion
		{
			get
			{
				return fileVersion;
			}
			set
			{
				fileVersion = value;
			}
		}

		[XmlAttribute(AttributeName = "HashType")]
		public string HashType
		{
			get
			{
				return hashType;
			}
			set
			{
				hashType = value;
			}
		}

		[XmlAttribute(AttributeName = "PackageID")]
		public string PackageId
		{
			get
			{
				return packageId;
			}
			set
			{
				packageId = value;
				GlobalVariables.IsInPackageAllowList = false;
				string[] packageAllowList = ConstantStrings.GetPackageAllowList();
				foreach (string value2 in packageAllowList)
				{
					GlobalVariables.IsInPackageAllowList = packageId.Equals(value2, StringComparison.OrdinalIgnoreCase);
					if (GlobalVariables.IsInPackageAllowList)
					{
						break;
					}
				}
			}
		}

		[XmlElement(ElementName = "Capabilities")]
		public Capabilities OutputCapabilities
		{
			get
			{
				return capabilities;
			}
			set
			{
				capabilities = value;
			}
		}

		[XmlElement(ElementName = "Components")]
		public Components OutputComponents
		{
			get
			{
				return components;
			}
			set
			{
				components = value;
			}
		}

		[XmlElement(ElementName = "Authorization")]
		public Authorization OutputAuthorization
		{
			get
			{
				return authorization;
			}
			set
			{
				authorization = value;
			}
		}

		public void Add(string policyXmlFileFullPath, IXPathNavigable xmlPathNavigator)
		{
			if (xmlPathNavigator == null)
			{
				throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "Error: CompileSecurityPolicy {0}", new object[1] { policyXmlFileFullPath }));
			}
			XmlDocument policyXmlDocument = (XmlDocument)xmlPathNavigator;
			fileFullPath = policyXmlFileFullPath;
			try
			{
				GlobalVariables.CurrentCompilationState = CompilationState.PolicyFileAddHeaderAttributes;
				AddAttributes(policyXmlDocument);
				GlobalVariables.CurrentCompilationState = CompilationState.PolicyFileAddElements;
				AddElements(policyXmlDocument);
			}
			catch (XPathException originalException)
			{
				Print();
				throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "Error: CompileSecurityPolicy {0}", new object[1] { policyXmlFileFullPath }), originalException);
			}
		}

		private void AddAttributes(XmlDocument policyXmlDocument)
		{
			foreach (XmlElement item in policyXmlDocument.SelectNodes("/WP_Policy:PhoneSecurityPolicy", GlobalVariables.NamespaceManager))
			{
				Description = item.GetAttribute("Description");
				Vendor = item.GetAttribute("Vendor");
				RequiredOSVersion = item.GetAttribute("RequiredOSVersion");
				FileVersion = item.GetAttribute("FileVersion");
			}
		}

		private void AddElements(XmlDocument policyXmlDocument)
		{
			AddCapabilites(policyXmlDocument);
			AddComponents(policyXmlDocument);
			AddAuthorization(policyXmlDocument);
		}

		private void AddCapabilites(XmlDocument policyXmlDocument)
		{
			XmlNodeList xmlNodeList = policyXmlDocument.SelectNodes("//WP_Policy:Capabilities", GlobalVariables.NamespaceManager);
			if (xmlNodeList.Count <= 0)
			{
				return;
			}
			if (PolicyCompiler.BlockPolicyDefinition)
			{
				throw new PolicyCompilerInternalException("The package definition file should not have capability definition");
			}
			if (capabilities == null)
			{
				capabilities = new Capabilities();
			}
			foreach (XmlElement item in xmlNodeList)
			{
				capabilities.Add(item);
			}
		}

		private void AddComponents(XmlDocument policyXmlDocument)
		{
			XmlNodeList xmlNodeList = policyXmlDocument.SelectNodes("//WP_Policy:Components", GlobalVariables.NamespaceManager);
			if (xmlNodeList.Count == 0)
			{
				return;
			}
			if (components == null)
			{
				components = new Components();
			}
			foreach (XmlElement item in xmlNodeList)
			{
				components.Add(item);
				if (!components.HasPrivateCapabilities())
				{
					continue;
				}
				if (capabilities == null)
				{
					capabilities = new Capabilities();
				}
				foreach (Capability privateCapability in components.GetPrivateCapabilities())
				{
					capabilities.Append(privateCapability);
				}
			}
		}

		private void AddAuthorization(XmlDocument policyXmlDocument)
		{
			XmlNodeList xmlNodeList = policyXmlDocument.SelectNodes("//WP_Policy:Authorization", GlobalVariables.NamespaceManager);
			if (xmlNodeList.Count == 0)
			{
				return;
			}
			if (authorization == null)
			{
				authorization = new Authorization();
			}
			foreach (XmlElement item in xmlNodeList)
			{
				authorization.Add(item);
			}
		}

		public bool SaveToXml(string policyFile)
		{
			GlobalVariables.CurrentCompilationState = CompilationState.SaveXmlFile;
			bool result = false;
			if ((capabilities != null && capabilities.HasChild()) || (components != null && components.HasChild()) || (authorization != null && authorization.HasChild()))
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(PolicyXmlClass), "urn:Microsoft.WindowsPhone/PhoneSecurityPolicyInternal.v8.00");
				using (TextWriter textWriter = new StreamWriter(policyFile))
				{
					xmlSerializer.Serialize(textWriter, this);
				}
				result = true;
			}
			return result;
		}

		public void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.DebugLine("Policy file: " + fileFullPath);
			instance.DebugLine(string.Empty);
			instance.XmlElementLine("", "PhoneSecurityPolicy");
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel1, "Description", Description);
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel1, "Vendor", Vendor);
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel1, "RequiredOSVersion", RequiredOSVersion);
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel1, "FileVersion", FileVersion);
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel1, "HashType", HashType);
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel1, "PackageID", PackageId);
			if (capabilities != null)
			{
				instance.DebugLine(string.Empty);
				capabilities.Print();
			}
			if (components != null)
			{
				instance.DebugLine(string.Empty);
				components.Print();
			}
			if (authorization != null)
			{
				instance.DebugLine(string.Empty);
				authorization.Print();
			}
		}
	}
}
