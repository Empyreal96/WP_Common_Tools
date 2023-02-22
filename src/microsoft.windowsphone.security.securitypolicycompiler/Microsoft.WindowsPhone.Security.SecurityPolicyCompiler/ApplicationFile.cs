using System;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class ApplicationFile : IPolicyElement
	{
		private string sourcePath;

		private string destDir;

		private string name;

		private string path = "Not Calculated";

		[XmlAttribute(AttributeName = "Path")]
		public string Path
		{
			get
			{
				return path;
			}
			set
			{
				path = value;
			}
		}

		public void Add(IXPathNavigable fileXmlElement)
		{
			AddAttributes((XmlElement)fileXmlElement);
			CompileAttributes();
		}

		private void AddAttributes(XmlElement fileXmlElement)
		{
			if (fileXmlElement.HasAttribute("Path"))
			{
				Path = fileXmlElement.GetAttribute("Path");
				return;
			}
			sourcePath = fileXmlElement.GetAttribute("Source");
			destDir = fileXmlElement.GetAttribute("DestinationDir");
			name = fileXmlElement.GetAttribute("Name");
		}

		private void CompileAttributes()
		{
			if (Path == "Not Calculated")
			{
				if (!string.IsNullOrEmpty(name))
				{
					Path = name;
				}
				else
				{
					int num = sourcePath.LastIndexOf("\\", GlobalVariables.GlobalStringComparison);
					if (num == -1)
					{
						Path = sourcePath;
					}
					else if (sourcePath.Length > num + 1)
					{
						Path = sourcePath.Substring(num + 1);
					}
				}
				if (string.IsNullOrEmpty(destDir))
				{
					Path = "$(runtime.default)\\" + Path;
				}
				else if (destDir.EndsWith("\\", GlobalVariables.GlobalStringComparison))
				{
					Path = destDir + Path;
				}
				else
				{
					Path = destDir + "\\" + Path;
				}
			}
			try
			{
				Path = GlobalVariables.ResolveMacroReference(path, string.Format(GlobalVariables.Culture, "Element={0}, Attribute={1}", new object[2] { "Binary", "Path" }));
			}
			catch (PolicyCompilerInternalException)
			{
				Path = "Not Calculated";
				return;
			}
			if (!IsBinary(path))
			{
				Path = "Not Calculated";
			}
			else
			{
				Path = NormalizedString.Get(path);
			}
		}

		public void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel4, "Binary");
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel5, "Path", path);
		}

		private static bool IsBinary(string filePath)
		{
			string[] array = new string[1] { ".exe" };
			if (!string.IsNullOrEmpty(filePath))
			{
				string[] array2 = array;
				foreach (string value in array2)
				{
					if (filePath.EndsWith(value, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
