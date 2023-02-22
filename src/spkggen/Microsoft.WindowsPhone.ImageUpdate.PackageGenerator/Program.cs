using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Security.SecurityPolicyCompiler;

namespace Microsoft.WindowsPhone.ImageUpdate.PackageGenerator
{
	internal class Program
	{
		private const string c_strProjExtension = ".pkg.xml";

		private static readonly List<string> ErrorBuffer = new List<string>();

		private static void LogErrorToBuffer(string logString)
		{
			ErrorBuffer.Add(logString);
		}

		private static void WriteErrorBuffer()
		{
			if (ErrorBuffer.Count > 0)
			{
				LogUtil.Error("Previous errors from native components:");
				foreach (string item in ErrorBuffer)
				{
					LogUtil.Error(item);
				}
			}
			ErrorBuffer.Clear();
		}

		private static Dictionary<string, IPkgPlugin> LoadPackagePlugins()
		{
			CompositionContainer compositionContainer = null;
			try
			{
				string directoryName = LongPath.GetDirectoryName(typeof(IPkgPlugin).Assembly.Location);
				AggregateCatalog aggregateCatalog = new AggregateCatalog();
				aggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(IPkgPlugin).Assembly));
				if (LongPathDirectory.Exists(directoryName))
				{
					aggregateCatalog.Catalogs.Add(new DirectoryCatalog(directoryName, "PkgGen.Plugin.*.dll"));
				}
				CompositionBatch batch = new CompositionBatch();
				compositionContainer = new CompositionContainer(aggregateCatalog);
				compositionContainer.Compose(batch);
			}
			catch (CompositionException innerException)
			{
				throw new PkgGenException(innerException, "Failed to load package plugins.");
			}
			Dictionary<string, IPkgPlugin> dictionary = new Dictionary<string, IPkgPlugin>();
			foreach (IPkgPlugin exportedValue in compositionContainer.GetExportedValues<IPkgPlugin>())
			{
				if (string.IsNullOrEmpty(exportedValue.XmlElementName))
				{
					throw new PkgGenException("Failed to load package plugin '{0}'. Invalid XmlElementName.", exportedValue.Name);
				}
				try
				{
					dictionary.Add(exportedValue.XmlElementName, exportedValue);
				}
				catch (ArgumentException innerException2)
				{
					string fileName = Path.GetFileName(exportedValue.GetType().Assembly.Location);
					IPkgPlugin pkgPlugin = dictionary[exportedValue.XmlElementName];
					string fileName2 = Path.GetFileName(pkgPlugin.GetType().Assembly.Location);
					throw new PkgGenException(innerException2, "Failed to load package plugin '{0}' ({1}). Uses a duplicate XmlElementName with '{2}' ({3}).", exportedValue.Name, fileName, pkgPlugin.Name, fileName2);
				}
			}
			return dictionary;
		}

		private static void OnSchemaValidationEvent(object sender, ValidationEventArgs e)
		{
			if (e.Exception != null)
			{
				throw e.Exception;
			}
			throw new PkgGenException("Schema validation error: {0}", e.Message);
		}

		private static void WriteXmlSchema(XmlSchema schema, string filePath)
		{
			LogUtil.Message("Writing out XSD to '{0}'", filePath);
			filePath = LongPath.GetFullPath(filePath);
			if (!LongPathDirectory.Exists(LongPath.GetDirectoryName(filePath)))
			{
				LongPathDirectory.CreateDirectory(LongPath.GetDirectoryName(filePath));
			}
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				NewLineHandling = NewLineHandling.Replace
			};
			using (XmlWriter writer = XmlWriter.Create(filePath, settings))
			{
				schema.Write(writer);
			}
		}

		private static void ImportCommandLineMacros(MacroResolver macroResolver, string variables)
		{
			Regex regex = new Regex("^(?<name>[A-Za-z.0-9_{-][A-Za-z.0-9_+{}-]*)=\\s*(?<value>.*?)\\s*$");
			Dictionary<string, Macro> dictionary = new Dictionary<string, Macro>(StringComparer.OrdinalIgnoreCase);
			string[] array = variables.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string text in array)
			{
				Match match = regex.Match(text);
				if (match == null || !match.Success)
				{
					throw new PkgGenException("Incorrect syntax in variable definition '{0}'", text);
				}
				string value = match.Groups["name"].Value;
				string text2 = match.Groups["value"].Value;
				if (text2.StartsWith("\"", StringComparison.InvariantCulture))
				{
					if (!text2.EndsWith("\"", StringComparison.InvariantCulture))
					{
						throw new PkgGenException("Incorrect syntax in variable definition '{0}'", text);
					}
					text2 = text2.Substring(1, text2.Length - 2);
				}
				Macro value2 = null;
				if (dictionary.TryGetValue(value, out value2))
				{
					LogUtil.Warning("Command line macro value overwriting, macro name:'{0}', old value:'{1}', new value:'{2}'", value, value2.Value, text2);
				}
				dictionary[value] = new Macro(value, text2);
			}
			macroResolver.Register(dictionary);
		}

		private static void ImportGlobalMacros(MacroResolver macroResolver, XmlValidator schemaValidator)
		{
			try
			{
				using (XmlReader macroDefinitionReader = schemaValidator.GetXmlReader(PkgGenResources.GetGlobalMacroStream()))
				{
					macroResolver.Load(macroDefinitionReader);
				}
			}
			catch (XmlSchemaValidationException ex)
			{
				throw new PkgGenException(ex, "Schema validation failed on embedded global macro file at line '{0}'", ex.LineNumber);
			}
			catch (Exception innerException)
			{
				throw new PkgGenException(innerException, "Failed to load global macro definitions from embeded stream");
			}
		}

		private static void ImportGlobalMacros(MacroResolver macroResolver, string file, XmlValidator schemaValidator)
		{
			if (!LongPathFile.Exists(file))
			{
				throw new PkgGenException("Global macro file '{0}' doesn't exist", file);
			}
			try
			{
				using (XmlReader macroDefinitionReader = schemaValidator.GetXmlReader(file))
				{
					macroResolver.Load(macroDefinitionReader);
				}
			}
			catch (XmlSchemaValidationException ex)
			{
				throw new PkgGenException(ex, "Schema validation failed on global macro file '{0}' at line '{1}'.", file, ex.LineNumber);
			}
			catch (Exception innerException)
			{
				throw new PkgGenException(innerException, "Failed to load global macro definitions from file '{0}'", file);
			}
		}

		private static int Main(string[] args)
		{
			try
			{
				LogUtil.IULogTo(LogErrorToBuffer, LogUtil.Warning, LogUtil.Message, LogUtil.Diagnostic);
				LogUtil.LogCopyright();
				CommandLineParser commandLineParser = new CommandLineParser();
				commandLineParser.SetOptionalParameterString("project", "Full path to the package project file");
				commandLineParser.SetOptionalSwitchString("config", "File with globally defined variables");
				commandLineParser.SetOptionalSwitchString("xsd", "Path to write the PkgGen auto-generated schema to");
				commandLineParser.SetOptionalSwitchString("output", "Output directory", ".");
				commandLineParser.SetOptionalSwitchString("version", "Version string in the form of <major>.<minor>.<qfe>.<build>", "0.0.0.0");
				commandLineParser.SetOptionalSwitchString("build", "Build type string", "fre", false, "fre", "chk");
				commandLineParser.SetOptionalSwitchString("cpu", "CPU type", "ARM", false, "X86", "ARM", "ARM64", "AMD64");
				commandLineParser.SetOptionalSwitchString("languages", "Supported language identifier list, separated by ';'", string.Empty);
				commandLineParser.SetOptionalSwitchString("resolutions", "Supported resolution identifier list, separated by ';'", string.Empty);
				commandLineParser.SetOptionalSwitchString("variables", "Additional variables used in the project file, syntax:<name>=<value>;<name>=<value>;...");
				commandLineParser.SetOptionalSwitchString("spkgsout", "Create an output file containing a list of generated SPKG's", string.Empty);
				commandLineParser.SetOptionalSwitchString("toolPaths", "Directories containing tools needed by spkggen.exe", string.Empty);
				commandLineParser.SetOptionalSwitchBoolean("toc", "Building TOC files instead of the actual package", false);
				commandLineParser.SetOptionalSwitchBoolean("compress", "Compressing the generated package", false);
				commandLineParser.SetOptionalSwitchBoolean("diagnostic", "Enable debug output", false);
				commandLineParser.SetOptionalSwitchBoolean("nohives", "Indicates whether or not this package has no hive dependency", false);
				commandLineParser.SetOptionalSwitchBoolean("isRazzleEnv", "Indicates whether or not spkggen is running in a razzle environment", false);
				if (!commandLineParser.ParseCommandLine())
				{
					LogUtil.Error("Invalid command line arguments:{0}", commandLineParser.LastError);
					LogUtil.Message(commandLineParser.UsageString());
					return -1;
				}
				LogUtil.SetVerbosity(commandLineParser.GetSwitchAsBoolean("diagnostic"));
				string parameterAsString = commandLineParser.GetParameterAsString("project");
				string switchAsString = commandLineParser.GetSwitchAsString("xsd");
				string switchAsString2 = commandLineParser.GetSwitchAsString("config");
				if (string.IsNullOrEmpty(switchAsString) && string.IsNullOrEmpty(parameterAsString))
				{
					throw new PkgGenException("Must provide a project path or use the /xsd switch.");
				}
				Dictionary<string, IPkgPlugin> dictionary = LoadPackagePlugins();
				MergingSchemaValidator mergingSchemaValidator = new MergingSchemaValidator(OnSchemaValidationEvent);
				XmlSchema schema = mergingSchemaValidator.AddSchemaWithPlugins(PkgGenResources.GetProjSchemaStream(), dictionary.Values);
				if (!string.IsNullOrEmpty(switchAsString))
				{
					WriteXmlSchema(schema, switchAsString);
				}
				if (string.IsNullOrEmpty(parameterAsString))
				{
					return 0;
				}
				parameterAsString = LongPath.GetFullPath(parameterAsString);
				if (!parameterAsString.EndsWith(".pkg.xml", StringComparison.OrdinalIgnoreCase))
				{
					throw new PkgGenException("Invalid input project file '{0}', project file must have an extension of '{1}'", parameterAsString, ".pkg.xml");
				}
				string switchAsString3 = commandLineParser.GetSwitchAsString("variables");
				MacroResolver macroResolver = new MacroResolver();
				if (string.IsNullOrEmpty(switchAsString2))
				{
					LogUtil.Message("Using embedded macro file");
					ImportGlobalMacros(macroResolver, mergingSchemaValidator);
				}
				else
				{
					LogUtil.Message("Using external macro file: '{0}'", switchAsString2);
					ImportGlobalMacros(macroResolver, switchAsString2, mergingSchemaValidator);
				}
				if (switchAsString3 != null)
				{
					ImportCommandLineMacros(macroResolver, switchAsString3);
				}
				bool switchAsBoolean = commandLineParser.GetSwitchAsBoolean("toc");
				string switchAsString4 = commandLineParser.GetSwitchAsString("cpu");
				string switchAsString5 = commandLineParser.GetSwitchAsString("build");
				string switchAsString6 = commandLineParser.GetSwitchAsString("version");
				string switchAsString7 = commandLineParser.GetSwitchAsString("languages");
				string switchAsString8 = commandLineParser.GetSwitchAsString("resolutions");
				string switchAsString9 = commandLineParser.GetSwitchAsString("toolPaths");
				bool switchAsBoolean2 = commandLineParser.GetSwitchAsBoolean("compress");
				if (commandLineParser.GetSwitchAsBoolean("nohives"))
				{
					macroResolver.Register("__nohives", "true", true);
				}
				bool switchAsBoolean3 = commandLineParser.GetSwitchAsBoolean("isRazzleEnv");
				string text = commandLineParser.GetSwitchAsString("spkgsout");
				if (string.IsNullOrEmpty(text))
				{
					text = null;
				}
				if (text != null)
				{
					string directoryName = LongPath.GetDirectoryName(text);
					if (!LongPathDirectory.Exists(directoryName))
					{
						LongPathDirectory.CreateDirectory(directoryName);
					}
					if (LongPathFile.Exists(text))
					{
						LongPathFile.Delete(text);
					}
					using (new StreamWriter(text))
					{
					}
				}
				Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal.PackageGenerator packageGenerator = new Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal.PackageGenerator(dictionary, (!switchAsBoolean) ? BuildPass.BuildPkg : BuildPass.BuildTOC, CpuIdParser.Parse(switchAsString4), BuildTypeParser.Parse(switchAsString5), VersionInfo.Parse(switchAsString6), new PolicyCompiler(), macroResolver, mergingSchemaValidator, switchAsString7, switchAsString8, switchAsString9, switchAsBoolean3, switchAsBoolean2, text);
				string switchAsString10 = commandLineParser.GetSwitchAsString("output");
				ProcessPrivilege.Adjust(PrivilegeNames.BackupPrivilege, true);
				ProcessPrivilege.Adjust(PrivilegeNames.SecurityPrivilege, true);
				LogUtil.Message("Building project file {0}", parameterAsString);
				packageGenerator.Build(parameterAsString, switchAsString10, switchAsBoolean2);
				LogUtil.Message("Packages are generated to {0} successfully", switchAsString10);
				ProcessPrivilege.Adjust(PrivilegeNames.BackupPrivilege, false);
				ProcessPrivilege.Adjust(PrivilegeNames.SecurityPrivilege, false);
				return 0;
			}
			catch (PkgGenException ex)
			{
				WriteErrorBuffer();
				LogUtil.Error(ex.MessageTrace);
				return Marshal.GetHRForException(ex);
			}
			catch (PackageException ex2)
			{
				WriteErrorBuffer();
				LogUtil.Error(ex2.MessageTrace);
				return -1;
			}
			catch (PolicyCompilerInternalException ex3)
			{
				WriteErrorBuffer();
				LogUtil.Error(ex3.ToString());
				return -1;
			}
			catch (Exception ex4)
			{
				WriteErrorBuffer();
				LogUtil.Error("Uncaught exception occured: {0}", ex4.ToString());
				return -1;
			}
		}
	}
}
