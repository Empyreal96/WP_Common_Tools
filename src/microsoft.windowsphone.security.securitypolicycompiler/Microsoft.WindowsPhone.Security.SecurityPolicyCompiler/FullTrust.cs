using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class FullTrust : IPolicyElement
	{
		private string elementId = "Not Calculated";

		private string attributeHash = "Not Calculated";

		private string name = "Not Calculated";

		private bool skip = true;

		private ApplicationFiles applicationFiles;

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

		[XmlAttribute(AttributeName = "Skip")]
		public bool Skip
		{
			get
			{
				return skip;
			}
			set
			{
				skip = value;
			}
		}

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

		public void Add(IXPathNavigable fullTrustXmlElement)
		{
			GlobalVariables.MacroResolver.BeginLocal();
			try
			{
				XmlElement xmlElement = (XmlElement)fullTrustXmlElement;
				AddAttributes(xmlElement);
				CompileAttributes(xmlElement);
				AddElements(xmlElement);
				CalculateAttributeHash();
			}
			finally
			{
				GlobalVariables.MacroResolver.EndLocal();
			}
		}

		private void AddAttributes(XmlElement fullTrustXmlElement)
		{
			string empty = string.Empty;
			Name = NormalizedString.Get(fullTrustXmlElement.GetAttribute("Name"));
			empty = fullTrustXmlElement.GetAttribute("Skip");
			if (!string.IsNullOrEmpty(empty) && empty.Equals("No"))
			{
				Skip = false;
			}
			KeyValuePair<string, string>[] attributes = new KeyValuePair<string, string>[1]
			{
				new KeyValuePair<string, string>("Name", Name)
			};
			MiscUtils.RegisterObjectSpecificMacros(GlobalVariables.MacroResolver, ObjectType.FullTrust, attributes);
		}

		private void CompileAttributes(XmlElement componentXmlElement)
		{
			ElementId = HashCalculator.CalculateSha256Hash(Name, true);
		}

		private void AddElements(XmlElement fullTrustXmlElement)
		{
			AddBinaries(fullTrustXmlElement);
		}

		private void AddBinaries(XmlElement fullTrustXmlElement)
		{
			ApplicationFiles = new ApplicationFiles();
			applicationFiles.Add(fullTrustXmlElement);
		}

		private void CalculateAttributeHash()
		{
			StringBuilder stringBuilder = new StringBuilder(Name);
			stringBuilder.Append(applicationFiles.GetAllBinPaths());
			stringBuilder.Append(Skip);
			AttributeHash = HashCalculator.CalculateSha256Hash(stringBuilder.ToString(), true);
		}

		public void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel2, "Application");
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel3, "Name", Name);
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel3, "Skip", Skip.ToString());
			if (ApplicationFiles != null)
			{
				instance.DebugLine(string.Empty);
				applicationFiles.Print();
			}
		}
	}
}
