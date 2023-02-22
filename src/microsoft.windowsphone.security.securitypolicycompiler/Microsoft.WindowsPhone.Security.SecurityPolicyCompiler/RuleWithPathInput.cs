using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public abstract class RuleWithPathInput : BaseRule
	{
		private string inPath;

		private bool pathContainsInheritanceInfo;

		private string path = "Not Calculated";

		protected bool RuleInheritanceInfo
		{
			get
			{
				return pathContainsInheritanceInfo;
			}
			set
			{
				pathContainsInheritanceInfo = value;
			}
		}

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

		[XmlIgnore]
		protected string NormalizedPath => NormalizedString.Get(path);

		public override void Add(IXPathNavigable BasicRuleXmlElement, string appCapSID, string svcCapSID)
		{
			AddAttributes((XmlElement)BasicRuleXmlElement);
			CompileAttributes(appCapSID, svcCapSID);
		}

		protected override void AddAttributes(XmlElement BasicRuleXmlElement)
		{
			base.AddAttributes(BasicRuleXmlElement);
			inPath = BasicRuleXmlElement.GetAttribute("Path");
		}

		protected override void CompileAttributes(string appCapSID, string svcCapSID)
		{
			ResolvePathMacroAndInheritance();
			base.CompileAttributes(appCapSID, svcCapSID);
			ValidateOutPath();
			CalculateElementId(NormalizedPath);
		}

		protected virtual PathInheritanceType ResolvePathMacroAndInheritance()
		{
			string text = inPath;
			PathInheritanceType result = PathInheritanceType.NotInheritable;
			if (pathContainsInheritanceInfo)
			{
				int num = text.IndexOf("\\(*)", GlobalVariables.GlobalStringComparison);
				if (num >= 0 && num == text.Length - "\\(*)".Length)
				{
					result = PathInheritanceType.ContainerObjectInheritable;
				}
				else
				{
					if (num != -1)
					{
						throw new PolicyCompilerInternalException("Compile path does not have required inheritance info: type= " + base.RuleType);
					}
					num = text.IndexOf("\\(+)", GlobalVariables.GlobalStringComparison);
					if (num == -1 || num != text.Length - "\\(+)".Length)
					{
						throw new PolicyCompilerInternalException("Compile path does not have required inheritance info: type= " + base.RuleType);
					}
					result = PathInheritanceType.ContainerObjectInheritable_InheritOnly;
				}
				text = text.Substring(0, num);
			}
			path = ResolveMacro(text, "Path");
			return result;
		}

		protected void ValidateFileOutPath(bool IsChamberProfile)
		{
			if (!IsChamberProfile && ((NormalizedPath.StartsWith("\\DATA\\USERS\\", GlobalVariables.GlobalStringComparison) && !NormalizedPath.StartsWith("\\DATA\\USERS\\PUBLIC", GlobalVariables.GlobalStringComparison)) || NormalizedPath.Equals("\\DATA\\USERS", GlobalVariables.GlobalStringComparison)))
			{
				throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "'{0}' should not be defined in capability rule or private resource.", new object[1] { Path }));
			}
			if (ConstantStrings.BlockedFilePathRegexes.Any((Regex blockedPathRegex) => blockedPathRegex.IsMatch(NormalizedPath)))
			{
				throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "'{0}' should not be defined in capability rule or private resource.", new object[1] { Path }));
			}
			if (NormalizedPath.StartsWith("\\DATA\\", GlobalVariables.GlobalStringComparison) || NormalizedPath.Equals("\\DATA", GlobalVariables.GlobalStringComparison))
			{
				if (base.RuleType == "File")
				{
					throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "It is not allowed to define a capability rule for a file '{0}' under \\DATA, only Directory rule is allowed.", new object[1] { Path }));
				}
			}
			else if (!GlobalVariables.IsInPackageAllowList && !IsChamberProfile)
			{
				uint num = 1343029526u;
				if ((AccessRightHelper.MergeAccessRight(base.ResolvedRights, Path, base.RuleType) & num) != 0)
				{
					throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "It is not allowed to grants write access on '{0}'. Only the folders under \\DATA can be granted write access.", new object[1] { Path }));
				}
			}
		}

		protected virtual void ValidateOutPath()
		{
		}

		public override string GetAttributesString()
		{
			return base.GetAttributesString() + NormalizedPath;
		}

		public override void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel4, base.RuleType + "Rule");
			base.Print();
			instance.XmlAttributeLine(ConstantStrings.IndentationLevel5, "Path", Path);
		}
	}
}
