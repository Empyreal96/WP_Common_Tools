using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class PolicyCompiler : ISecurityPolicyCompiler
	{
		private ReportingBase report;

		protected XmlDocument driverPolicyXmlDocument;

		protected XmlDocument driverRuleTemplateXmlDocument;

		public static bool BlockPolicyDefinition { get; private set; }

		public PolicyCompiler()
		{
			ReportingBase.UseInternalLogger = false;
			ReportingBase.EnableDebugMessage = false;
			report = ReportingBase.GetInstance();
		}

		public PolicyCompiler(ReportingBase report)
		{
			this.report = report;
		}

		public bool Compile(string packageId, string projectPath, IMacroResolver macroResolver, string policyPath)
		{
			return Compile(packageId, projectPath, null, macroResolver, policyPath);
		}

		public bool Compile(string packageId, string projectPath, XmlDocument projectXml, IMacroResolver macroResolver, string policyPath)
		{
			report.DebugLine("Compiling fileName = " + projectPath);
			if (!File.Exists(projectPath) || string.IsNullOrEmpty(packageId) || macroResolver == null || string.IsNullOrEmpty(policyPath))
			{
				throw new ArgumentException("The input parameter is invalid!");
			}
			BlockPolicyDefinition = false;
			bool readFileFromDisk = projectXml == null;
			XmlDocument xmlDocument = projectXml ?? new XmlDocument();
			GlobalVariables.MacroResolver = macroResolver;
			LoadPolicyFileAndGetLocalMacros(projectPath, xmlDocument, readFileFromDisk, true);
			PolicyXmlClass policyXmlClass = new PolicyXmlClass();
			policyXmlClass.PackageId = packageId;
			policyXmlClass.Add(projectPath, xmlDocument);
			policyXmlClass.Print();
			bool result = policyXmlClass.SaveToXml(policyPath);
			GlobalVariables.CurrentCompilationState = CompilationState.CompletedSuccessfully;
			return result;
		}

		public void DriverSecurityInitialize(string projectPath, IMacroResolver macroResolver)
		{
			report.DebugLine("Compiling fileName = " + projectPath);
			if (!File.Exists(projectPath) || macroResolver == null)
			{
				throw new ArgumentException("The input parameter is invalid!");
			}
			driverPolicyXmlDocument = new XmlDocument();
			GlobalVariables.MacroResolver = macroResolver;
			bool readFileFromDisk = true;
			LoadPolicyFileAndGetLocalMacros(projectPath, driverPolicyXmlDocument, readFileFromDisk, false);
			DriverSecurityTemplateInitialize();
		}

		protected void DriverSecurityTemplateInitialize()
		{
			driverRuleTemplateXmlDocument = new XmlDocument();
			string text = null;
			text = ((!IsPhoneBuild()) ? Environment.ExpandEnvironmentVariables("%RAZZLETOOLPATH%\\managed\\v4.0\\DriverRuleTemplate.xml") : Environment.ExpandEnvironmentVariables("%SDXMAPROOT%\\wm\\tools\\oak\\misc\\DriverRuleTemplate.xml"));
			if (!File.Exists(text))
			{
				text = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DriverRuleTemplate.xml");
				if (!File.Exists(text))
				{
					throw new PolicyCompilerInternalException("DriverRuleTemplate.xml can't be found.");
				}
			}
			driverRuleTemplateXmlDocument.Load(text);
		}

		public string GetDriverSddlString(string infSectionName, string oldSddl)
		{
			if (driverPolicyXmlDocument == null || driverRuleTemplateXmlDocument == null || GlobalVariables.MacroResolver == null || string.IsNullOrEmpty(infSectionName))
			{
				throw new ArgumentException("The operation is invalid!");
			}
			return new DriverSecurity().GetSddlString(infSectionName, oldSddl, driverPolicyXmlDocument, driverRuleTemplateXmlDocument);
		}

		public bool IsPhoneBuild()
		{
			if (!System.IO.Directory.Exists(Environment.ExpandEnvironmentVariables("%_WINPHONEROOT%")))
			{
				return false;
			}
			return true;
		}

		protected void PreprocessPolicy(XmlDocument policyXmlDocument, bool parseLocalMacros)
		{
			string text = null;
			string text2 = null;
			GlobalVariables.NamespaceManager = new XmlNamespaceManager(policyXmlDocument.NameTable);
			GlobalVariables.NamespaceManager.AddNamespace("WP_Policy", "urn:Microsoft.WindowsPhone/PackageSchema.v8.00");
			if (parseLocalMacros)
			{
				foreach (XmlElement item in policyXmlDocument.SelectNodes("//WP_Policy:Macros/WP_Policy:Macro", GlobalVariables.NamespaceManager))
				{
					if (item.HasAttributes)
					{
						text = item.GetAttribute("Id");
						text2 = item.GetAttribute("Value");
						GlobalVariables.MacroResolver.Register(text, text2);
					}
				}
			}
			string text3 = null;
			text3 = ((!IsPhoneBuild()) ? Environment.ExpandEnvironmentVariables("%RAZZLETOOLPATH%\\managed\\v4.0\\capabilitylist.cfg") : Environment.ExpandEnvironmentVariables("%_WINPHONEROOT%\\tools\\oak\\misc\\capabilitylist.cfg"));
			if (!File.Exists(text3))
			{
				text3 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "capabilitylist.cfg");
				if (!File.Exists(text3))
				{
					throw new PolicyCompilerInternalException("capabilitylist.cfg can't be found.");
				}
				BlockPolicyDefinition = true;
			}
			GlobalVariables.SidMapping = SidMapping.CreateInstance(text3);
			SidMapping.CompareToSnapshotMapping(GlobalVariables.SidMapping);
		}

		protected void LoadPolicyFileAndGetLocalMacros(string policyXmlFileFullPath, XmlDocument policyXmlDocument, bool readFileFromDisk, bool parseLocalMacros)
		{
			GlobalVariables.CurrentCompilationState = CompilationState.PolicyFileLoadAndValidation;
			if (readFileFromDisk)
			{
				report.DebugLine("Loading Policy File : " + policyXmlFileFullPath);
				policyXmlDocument.Load(policyXmlFileFullPath);
			}
			try
			{
				PreprocessPolicy(policyXmlDocument, parseLocalMacros);
			}
			catch (XPathException originalException)
			{
				throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "File: {0}", new object[1] { policyXmlFileFullPath }), originalException);
			}
			GlobalVariables.CurrentCompilationState = CompilationState.PolicyMacroDereferencing;
		}
	}
}
