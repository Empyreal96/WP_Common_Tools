using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class AppBinaries : ApplicationFiles
	{
		private string elementId = "Not Calculated";

		private string attributeHash = "Not Calculated";

		private string appName = "Not Calculated";

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

		[XmlAttribute(AttributeName = "AppName")]
		public string AppName
		{
			get
			{
				return appName;
			}
			set
			{
				appName = value;
			}
		}

		public AppBinaries()
		{
			checkBinaryFile = false;
		}

		public override void Add(IXPathNavigable appBinariesXmlElement)
		{
			GlobalVariables.MacroResolver.BeginLocal();
			try
			{
				XmlElement xmlElement = (XmlElement)appBinariesXmlElement;
				AddAttributes(xmlElement);
				CompileAttributes();
				base.Add(xmlElement);
				CalculateAttributeHash();
			}
			finally
			{
				GlobalVariables.MacroResolver.EndLocal();
			}
		}

		private void AddAttributes(XmlElement appBinariesXmlElement)
		{
			AppName = NormalizedString.Get(appBinariesXmlElement.GetAttribute("Name"));
			KeyValuePair<string, string>[] attributes = new KeyValuePair<string, string>[1]
			{
				new KeyValuePair<string, string>("Name", AppName)
			};
			MiscUtils.RegisterObjectSpecificMacros(GlobalVariables.MacroResolver, ObjectType.AppResource, attributes);
		}

		private void CompileAttributes()
		{
			ElementId = HashCalculator.CalculateSha256Hash(AppName, true);
		}

		private void CalculateAttributeHash()
		{
			string empty = string.Empty;
			empty = GetAllBinPaths();
			AttributeHash = HashCalculator.CalculateSha256Hash(AppName + empty, true);
		}

		public override void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel2, "AppBinaries");
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel3, "AppName", AppName);
			if (applicationFileCollection == null)
			{
				return;
			}
			instance.DebugLine(string.Empty);
			instance.XmlElementLine(ConstantStrings.IndentationLevel3, "Binaries");
			foreach (ApplicationFile item in applicationFileCollection)
			{
				instance.DebugLine(string.Empty);
				item.Print();
			}
		}
	}
}
