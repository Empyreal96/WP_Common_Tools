using System.Xml;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon
{
	public interface ISecurityPolicyCompiler
	{
		bool Compile(string packageName, string projectPath, IMacroResolver macroResolver, string policyPath);

		bool Compile(string packageName, string projectPath, XmlDocument projectXml, IMacroResolver macroResolver, string policyPath);

		void DriverSecurityInitialize(string projectPath, IMacroResolver macroResolver);

		string GetDriverSddlString(string infSectionName, string oldSddl);
	}
}
