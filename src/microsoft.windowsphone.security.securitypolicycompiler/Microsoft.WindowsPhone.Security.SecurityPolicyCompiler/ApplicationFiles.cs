using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class ApplicationFiles : IPolicyElement
	{
		protected bool checkBinaryFile = true;

		protected List<ApplicationFile> applicationFileCollection = new List<ApplicationFile>();

		[XmlElement(ElementName = "Binary")]
		public List<ApplicationFile> ApplicationFileCollection => applicationFileCollection;

		public virtual void Add(IXPathNavigable applicationXmlElement)
		{
			AddElements((XmlElement)applicationXmlElement);
		}

		private void AddElements(XmlElement applicationXmlElement)
		{
			foreach (XmlElement item in applicationXmlElement.SelectNodes("./WP_Policy:Files/WP_Policy:File", GlobalVariables.NamespaceManager))
			{
				ApplicationFile applicationFile = new ApplicationFile();
				applicationFile.Add(item);
				if (applicationFile.Path != "Not Calculated")
				{
					applicationFileCollection.Add(applicationFile);
				}
			}
			foreach (XmlElement item2 in applicationXmlElement.SelectNodes("./WP_Policy:Executable", GlobalVariables.NamespaceManager))
			{
				ApplicationFile applicationFile2 = new ApplicationFile();
				applicationFile2.Add(item2);
				if (applicationFile2.Path != "Not Calculated")
				{
					applicationFileCollection.Add(applicationFile2);
				}
			}
			if (checkBinaryFile && applicationFileCollection.Count == 0)
			{
				throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "No binary file has been defined in the '{0}' element.", new object[1] { applicationXmlElement.LocalName }));
			}
		}

		public string GetAllBinPaths()
		{
			string text = string.Empty;
			if (applicationFileCollection != null)
			{
				foreach (ApplicationFile item in applicationFileCollection)
				{
					text += item.Path;
				}
				return text;
			}
			return text;
		}

		public virtual void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel3, "Binaries");
			foreach (ApplicationFile item in applicationFileCollection)
			{
				instance.DebugLine(string.Empty);
				item.Print();
			}
		}
	}
}
