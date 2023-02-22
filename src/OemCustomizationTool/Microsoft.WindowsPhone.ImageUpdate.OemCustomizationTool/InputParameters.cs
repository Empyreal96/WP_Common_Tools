using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.WindowsPhone.ImageUpdate.OemCustomizationTool
{
	internal class InputParameters
	{
		public bool IsInputParamValid { get; set; }

		public InputParameters(string[] args)
		{
			if (args.Count() == 0 || (args.Count() == 1 && args[0].StartsWith("/?", StringComparison.OrdinalIgnoreCase)))
			{
				Usage();
				return;
			}
			if (args[0] == null || args[1] == null)
			{
				Usage();
				return;
			}
			string text = args[0];
			string path = args[1];
			if (!File.Exists(text) || !Directory.Exists(path) || Directory.GetFiles(path).Count() == 0)
			{
				TraceLogger.LogMessage(TraceLevel.Error, "Ensure that the config xml and customization xml directories and files exist.");
				Usage();
				return;
			}
			Settings.CustomizationFiles = new List<XmlFile>();
			Settings.CustomizationFiles.Add(new XmlFile(text, Settings.CustomizationSchema));
			Settings.CustomizationIncludeDirectory = Path.GetDirectoryName(text);
			Settings.ConfigFiles = new List<XmlFile>();
			string[] files = Directory.GetFiles(path);
			foreach (string text2 in files)
			{
				if (text2.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
				{
					Settings.ConfigFiles.Add(new XmlFile(text2, Settings.ConfigSchema));
				}
			}
			try
			{
				for (int j = 2; j < args.Count(); j++)
				{
					if (args[j] == null || args[j] == "")
					{
						continue;
					}
					if (args[j].StartsWith("/output=", StringComparison.OrdinalIgnoreCase))
					{
						Settings.OutputDirectoryPath = args[j].Split('=')[1];
						continue;
					}
					if (args[j].StartsWith("/version=", StringComparison.OrdinalIgnoreCase))
					{
						Settings.PackageAttributes.VersionString = args[j].Split('=')[1];
						continue;
					}
					if (args[j].StartsWith("/cpu=", StringComparison.OrdinalIgnoreCase))
					{
						Settings.PackageAttributes.CpuTypeString = args[j].Split('=')[1];
						continue;
					}
					if (args[j].StartsWith("/warnOnMappingNotFound", StringComparison.OrdinalIgnoreCase))
					{
						Settings.WarnOnMappingNotFound = true;
						continue;
					}
					if (args[j].StartsWith("/diagnostic", StringComparison.OrdinalIgnoreCase))
					{
						Settings.Diagnostics = true;
						continue;
					}
					TraceLogger.LogMessage(TraceLevel.Error, "Unexpected parameter: " + args[j]);
					Usage();
					return;
				}
				TraceLogger.LogMessage(TraceLevel.Info, "Validated input parameters.");
				IsInputParamValid = true;
			}
			catch (Exception ex)
			{
				TraceLogger.LogMessage(TraceLevel.Error, ex.ToString());
				Usage();
			}
		}

		private void Usage()
		{
			string arg = Assembly.GetExecutingAssembly().FullName.Split(',')[0] + ".exe";
			string text = string.Format("\r\n{0}: Generates customization packages.\r\n\r\nUsage:\r\n------\r\n\r\n{0}     <Customization File> <Config File Directory> [/output=Output Directory] \r\n        [/version=Version String] [/warnOnMappingNotFound] [/diagnostic]\r\n        \r\nParameters:\r\n-----------\r\n\r\n<Customization File>       Required. The path to the Customization XML file. \r\n                           If drawing from multiple sources, this file must \r\n                           include the other Customization XMLs. If a \r\n                           customization is repeated in multiple files in the \r\n                           include hierarchy, the last one in the hierarchy wins.\r\n                           Environment variables should be quoted with the percent \r\n                           sign character, e.g., %CUSTOM_PATH%.\r\n\t\r\n<Config File Directory>    Required. The path to the directory which contains Config \r\n                           XML files. All *.xml files in this directory are processed. \r\n                           Mappings specified in config files must be unique, i.e., if \r\n                           a mapping appears more than once, an exception is thrown.\r\n                           Environment variables should be quoted with the percent \r\n                           sign character, e.g., %CUSTOM_PATH%.\r\n                           \r\n/output=Output Directory   Optional. The path to the directory where the output package(s)\r\n                           should be saved. Default location is present working directory.\r\n                           Environment variables should be quoted with the percent \r\n                           sign character, e.g., %CUSTOM_PATH%.\r\n                           \r\n/version=Version String    Optional. Specifies the version of the package using the format \r\n                           “<major>.<minor>.<hotfix>.<build>”. Default is “0.0.0.0”.\r\n                           \r\n/warnOnMappingNotFound     Optional. If a setting in the CustomizationXML does not have a \r\n                           corresponding mapping in the ConfigXML, the tool will raise an \r\n                           exception and stop processing by default. If this option is \r\n                           specified, the tool will issue a warning and continue processing.\r\n                           \r\n/diagnostic                Optional. Enable verbose debugging messages to console. Default \r\n                           is disabled.", arg);
			TraceLogger.LogMessage(TraceLevel.Error, string.Format(Environment.NewLine + text + Environment.NewLine));
		}
	}
}
