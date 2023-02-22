using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public abstract class PathElement : IPolicyElement
	{
		protected string ElementName { get; set; }

		protected bool WildcardSupport { get; set; }

		[XmlAttribute(AttributeName = "Path")]
		public string Path { get; set; }

		public PathElement()
		{
		}

		public virtual void Add(IXPathNavigable pathXmlElement)
		{
			AddAttributes((XmlElement)pathXmlElement);
			CompileAttributes();
		}

		protected virtual void AddAttributes(XmlElement pathXmlElement)
		{
			Path = pathXmlElement.GetAttribute("Path");
		}

		protected virtual void CompileAttributes()
		{
			ResolveMacrosAndWildcard();
			Path = NormalizedString.Get(Path);
		}

		private void ResolveMacrosAndWildcard()
		{
			if (WildcardSupport)
			{
				int num = Path.IndexOf("\\(*)", GlobalVariables.GlobalStringComparison);
				if (num == -1 || num != Path.Length - "\\(*)".Length)
				{
					throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "Element {0}: Path does not have required wildcard", new object[1] { ElementName }));
				}
				Path = Path.Substring(0, num + 1);
			}
			Path = GlobalVariables.ResolveMacroReference(Path, string.Format(GlobalVariables.Culture, "Element={0}, Attribute={1}", new object[2] { ElementName, "Path" }));
		}

		public virtual void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel3, ElementName);
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel4, "Path", Path);
		}
	}
}
