using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class CapabilityRulesRegKey : RuleWithPathInput
	{
		public CapabilityRulesRegKey()
		{
			base.RuleType = "RegKey";
			base.RuleInheritanceInfo = true;
			base.Inheritance = "CI";
		}

		public CapabilityRulesRegKey(CapabilityOwnerType value)
			: this()
		{
			base.OwnerType = value;
		}

		protected sealed override void AddAttributes(XmlElement BasicRuleXmlElement)
		{
			base.AddAttributes(BasicRuleXmlElement);
			base.Flags |= 515u;
		}

		protected sealed override void ValidateOutPath()
		{
			if ((base.NormalizedPath.StartsWith("HKEY_USERS", GlobalVariables.GlobalStringComparison) && !base.NormalizedPath.StartsWith("HKEY_USERS\\.DEFAULT", GlobalVariables.GlobalStringComparison)) || base.NormalizedPath.StartsWith("HKEY_CURRENT_USER", GlobalVariables.GlobalStringComparison))
			{
				throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "It is not allowed to define a capability rule or private resource for registry key '{0}' under HKEY_USERS or HKEY_CURRENT_USER.", new object[1] { base.Path }));
			}
			if (!ConstantStrings.BlockedRegPathRegexes.Any((Regex blockedPathRegex) => blockedPathRegex.IsMatch(base.NormalizedPath)))
			{
				return;
			}
			bool flag = false;
			string[] allowedRegPaths = ConstantStrings.AllowedRegPaths;
			foreach (string value in allowedRegPaths)
			{
				if (base.NormalizedPath.Equals(value, GlobalVariables.GlobalStringComparison))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				uint num = 1343029254u;
				if ((AccessRightHelper.MergeAccessRight(base.ResolvedRights, base.Path, base.RuleType) & num) != 0)
				{
					throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "It is not allowed to define a capability rule or private resource for write access on registry key '{0}'.", new object[1] { base.Path }));
				}
			}
		}
	}
}
