using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class Application : Component, IPolicyElement
	{
		private ApplicationFiles applicationFiles;

		[XmlElement(ElementName = "Binaries")]
		public ApplicationFiles ApplicationFiles
		{
			get
			{
				return applicationFiles;
			}
			set
			{
				applicationFiles = value;
			}
		}

		public Application()
		{
			base.OwnerType = CapabilityOwnerType.Application;
		}

		public void Add(IXPathNavigable applicationXmlElement)
		{
			GlobalVariables.MacroResolver.BeginLocal();
			try
			{
				XmlElement componentXmlElement = (XmlElement)applicationXmlElement;
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

		protected override void AddElements(XmlElement applicationXmlElement)
		{
			base.RequiredCapabilities.OwnerType = CapabilityOwnerType.Application;
			AddBinaries(applicationXmlElement);
			base.AddElements(applicationXmlElement);
		}

		private void AddBinaries(XmlElement applicationXmlElement)
		{
			ApplicationFiles = new ApplicationFiles();
			applicationFiles.Add(applicationXmlElement);
		}

		private void CalculateAttributeHash()
		{
			StringBuilder stringBuilder = new StringBuilder(base.Name);
			stringBuilder.Append(applicationFiles.GetAllBinPaths());
			stringBuilder.Append(base.RequiredCapabilities.GetAllCapIds());
			stringBuilder.Append(base.OEMExtensible);
			if (HasPrivateResources())
			{
				stringBuilder.Append(base.PrivateResources.AttributeHash);
			}
			base.AttributeHash = HashCalculator.CalculateSha256Hash(stringBuilder.ToString(), true);
		}

		public void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel2, "Application");
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel3, "Name", base.Name);
			if (ApplicationFiles != null)
			{
				instance.DebugLine(string.Empty);
				applicationFiles.Print();
			}
			if (base.RequiredCapabilities != null)
			{
				instance.DebugLine(string.Empty);
				base.RequiredCapabilities.Print();
			}
		}
	}
}
