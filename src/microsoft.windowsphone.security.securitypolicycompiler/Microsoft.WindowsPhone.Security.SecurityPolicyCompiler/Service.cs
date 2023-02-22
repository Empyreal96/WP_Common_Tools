using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class Service : Component, IPolicyElement
	{
		private string executable;

		private string isTCB = "No";

		private string logonAccount;

		private string svcHostGroupName;

		private string svcProcessOwnership;

		[XmlAttribute(AttributeName = "Executable")]
		public string Executable
		{
			get
			{
				return executable;
			}
			set
			{
				executable = value;
			}
		}

		[XmlAttribute(AttributeName = "IsTCB")]
		public string IsTCB
		{
			get
			{
				return isTCB;
			}
			set
			{
				isTCB = value;
			}
		}

		[XmlAttribute(AttributeName = "LogonAccount")]
		public string LogonAccount
		{
			get
			{
				return logonAccount;
			}
			set
			{
				logonAccount = value;
			}
		}

		[XmlAttribute(AttributeName = "SvcHostGroupName")]
		public string SvcHostGroupName
		{
			get
			{
				return svcHostGroupName;
			}
			set
			{
				svcHostGroupName = value;
			}
		}

		[XmlAttribute(AttributeName = "OwnedProc")]
		public string SvcProcessOwnership
		{
			get
			{
				return svcProcessOwnership;
			}
			set
			{
				svcProcessOwnership = value;
			}
		}

		public Service()
		{
			base.OwnerType = CapabilityOwnerType.Service;
		}

		public void Add(IXPathNavigable appServiceXmlElement)
		{
			GlobalVariables.MacroResolver.BeginLocal();
			try
			{
				XmlElement componentXmlElement = (XmlElement)appServiceXmlElement;
				AddAttributes(componentXmlElement);
				CompileAttributes(componentXmlElement);
				AddElements(componentXmlElement);
				CalculateAttributeHash();
				AddPrivateCapSIDIfAny();
			}
			finally
			{
				GlobalVariables.MacroResolver.EndLocal();
			}
		}

		protected override void AddAttributes(XmlElement serviceXmlElement)
		{
			string empty = string.Empty;
			string empty2 = string.Empty;
			string empty3 = string.Empty;
			string empty4 = string.Empty;
			base.AddAttributes(serviceXmlElement);
			SvcProcessOwnership = ((serviceXmlElement.GetAttribute("Type") == "Win32OwnProcess") ? "Y" : null);
			empty = serviceXmlElement.GetAttribute("SvcHostGroupName");
			empty2 = serviceXmlElement.GetAttribute("IsTCB");
			empty3 = serviceXmlElement.GetAttribute("LogonAccount");
			if (!string.IsNullOrEmpty(empty))
			{
				SvcHostGroupName = NormalizedString.Get(empty);
				XmlElement xmlElement = (XmlElement)serviceXmlElement.SelectSingleNode("./WP_Policy:ServiceDll", GlobalVariables.NamespaceManager);
				if (xmlElement != null)
				{
					empty4 = xmlElement.GetAttribute("HostExe");
					if (!string.IsNullOrEmpty(empty4))
					{
						Executable = empty4;
					}
					else
					{
						Executable = "$(RUNTIME.SYSTEM32)\\SvcHost.exe";
					}
				}
			}
			if (!string.IsNullOrEmpty(empty2))
			{
				if (!string.IsNullOrEmpty(empty3))
				{
					throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "'{0}' and '{1}' can't be present in the same time in service '{2}'.", new object[3] { "IsTCB", "LogonAccount", base.Name }));
				}
				IsTCB = empty2;
			}
			if (!string.IsNullOrEmpty(empty3))
			{
				LogonAccount = empty3;
			}
		}

		protected override void CompileAttributes(XmlElement serviceXmlElement)
		{
			Executable = GlobalVariables.ResolveMacroReference(Executable, string.Format(GlobalVariables.Culture, "Element={0}, Attribute={1}", new object[2] { "Service", "Executable" }));
			base.CompileAttributes(serviceXmlElement);
		}

		protected override void AddElements(XmlElement serviceXmlElement)
		{
			base.RequiredCapabilities.OwnerType = CapabilityOwnerType.Service;
			XmlElement xmlElement = (XmlElement)serviceXmlElement.SelectSingleNode("./WP_Policy:Executable", GlobalVariables.NamespaceManager);
			if ((xmlElement == null && svcHostGroupName == null) || (xmlElement != null && svcHostGroupName != null))
			{
				throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "Element={0} '{1}', Attributes {2} and {3} are mutually exclusive", "Service", base.Name, "Executable", "SvcHostGroupName"));
			}
			if (xmlElement != null)
			{
				ApplicationFile applicationFile = new ApplicationFile();
				applicationFile.Add(xmlElement);
				if (applicationFile.Path != "Not Calculated")
				{
					Executable = applicationFile.Path;
				}
			}
			base.AddElements(serviceXmlElement);
		}

		private void CalculateAttributeHash()
		{
			StringBuilder stringBuilder = new StringBuilder(base.Name);
			stringBuilder.Append(SvcHostGroupName);
			stringBuilder.Append(Executable);
			stringBuilder.Append(IsTCB);
			if (LogonAccount != null)
			{
				stringBuilder.Append(LogonAccount);
			}
			if (base.OEMExtensible != null)
			{
				stringBuilder.Append(base.OEMExtensible);
			}
			stringBuilder.Append(base.RequiredCapabilities.GetAllCapIds());
			if (HasPrivateResources())
			{
				stringBuilder.Append(base.PrivateResources.AttributeHash);
			}
			base.AttributeHash = HashCalculator.CalculateSha256Hash(stringBuilder.ToString(), true);
		}

		public void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel2, "Service");
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel3, "Name", base.Name);
			PrintIfNotNull("SvcHostGroupName", SvcHostGroupName);
			PrintIfNotNull("IsTCB", IsTCB);
			PrintIfNotNull("LogonAccount", LogonAccount);
			PrintIfNotNull("OEMExtensible", base.OEMExtensible);
			if (base.RequiredCapabilities != null)
			{
				instance.DebugLine(string.Empty);
				base.RequiredCapabilities.Print();
			}
		}

		private void PrintIfNotNull(string attributeString, string Attribute)
		{
			ReportingBase instance = ReportingBase.GetInstance();
			if (!string.IsNullOrEmpty(Attribute))
			{
				instance.XmlAttributeLine(ConstantStrings.IndentationLevel3, attributeString, Attribute);
			}
		}
	}
}
