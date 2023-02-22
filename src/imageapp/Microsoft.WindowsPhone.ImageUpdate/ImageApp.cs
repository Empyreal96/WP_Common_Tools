using System;
using System.Globalization;
using System.IO;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.WindowsPhone.ImageUpdate
{
	internal static class ImageApp
	{
		public delegate void LogFunc(string msg);

		private static string c_LogFileBase = "ImageApp.log";

		private static string _LogFile = string.Empty;

		private static CommandLineParser _commandLineParser = null;

		private static string _oemInputFile = string.Empty;

		private static string _oemCustomizationXML = string.Empty;

		private static string _oemCustomizationPPKG = string.Empty;

		private static string _oemCustomizationVersion = string.Empty;

		private static string _msPackagesRoot = string.Empty;

		private static string _updateInputFile = string.Empty;

		private static string _outputFile = string.Empty;

		private static bool _bDoingUpdate = false;

		private static bool _bSkipUpdateMain = false;

		private static string _cpuType = string.Empty;

		private static CpuId _cpuId = CpuId.Invalid;

		private static string _bspProductName;

		private static bool _formatDPP = false;

		private static bool _strictSettingPolicies = false;

		private static bool _skipImaging = false;

		private static bool _randomizeGptIDs = false;

		private static bool _showDebugMessages = false;

		private static readonly object _lock = new object();

		private static void Main()
		{
			LogUtil.LogCopyright();
			IULogger iULogger = new IULogger();
			iULogger.ErrorLogger = LogErrorToFileAndConsole;
			iULogger.WarningLogger = LogWarningToFileAndConsole;
			iULogger.InformationLogger = LogInfoToFileAndConsole;
			iULogger.DebugLogger = null;
			try
			{
				SetCmdLineParams();
				try
				{
					if (!ParseCommandlineParams(Environment.CommandLine))
					{
						Environment.ExitCode = 1;
						ShowUsageString();
						return;
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
					Environment.ExitCode = 1;
					ShowUsageString();
					return;
				}
				if (_showDebugMessages)
				{
					iULogger.DebugLogger = LogDebugToFileAndConsole;
				}
				Microsoft.WindowsPhone.Imaging.Imaging imaging = new Microsoft.WindowsPhone.Imaging.Imaging(iULogger);
				Console.CancelKeyPress += imaging.CleanupHandler;
				if (_bDoingUpdate)
				{
					imaging.UpdateExistingImage(_outputFile, _updateInputFile, _randomizeGptIDs);
					return;
				}
				imaging.SkipImaging = _skipImaging;
				imaging.FormatDPP = _formatDPP;
				imaging.StrictSettingPolicies = _strictSettingPolicies;
				imaging.SkipUpdateMain = _bSkipUpdateMain;
				imaging.CPUId = _cpuId;
				imaging.BSPProductName = _bspProductName;
				imaging.BuildNewImage(_outputFile, _oemInputFile, _msPackagesRoot, _oemCustomizationXML, _oemCustomizationPPKG, _oemCustomizationVersion, _randomizeGptIDs);
			}
			catch (ImageCommonException ex2)
			{
				iULogger.LogError("{0}", ex2.Message);
				if (ex2.InnerException != null)
				{
					iULogger.LogError("\t{0}", ex2.InnerException.ToString());
				}
				Environment.ExitCode = 2;
			}
			catch (Exception ex3)
			{
				iULogger.LogError("{0}", ex3.Message);
				if (ex3.InnerException != null)
				{
					iULogger.LogError("\t{0}", ex3.InnerException.ToString());
				}
				iULogger.LogError("An unhandled exception was thrown: {0}", ex3.ToString());
				Environment.ExitCode = 3;
			}
			finally
			{
				if (Environment.ExitCode != 0)
				{
					Console.WriteLine("ImageApp: Error log can be found at '{0}'.", _LogFile);
				}
			}
		}

		private static void AppendToLog(string prepend, LogFunc LoggingFunc, string format, params object[] list)
		{
			string text = format;
			if (list != null && list.Length != 0)
			{
				text = string.Format(CultureInfo.CurrentCulture, format, list);
			}
			LoggingFunc(text);
			text = $"{{{DateTime.FromFileTime(DateTime.Now.ToFileTime())}}} {prepend} {text}{Environment.NewLine}";
			lock (_lock)
			{
				File.AppendAllText(_LogFile, text);
			}
		}

		private static void LogErrorToFileAndConsole(string format, params object[] list)
		{
			AppendToLog("Error:", LogUtil.Error, format, list);
		}

		private static void LogWarningToFileAndConsole(string format, params object[] list)
		{
			AppendToLog("Warning:", LogUtil.Warning, format, list);
		}

		private static void LogInfoToFileAndConsole(string format, params object[] list)
		{
			AppendToLog(string.Empty, LogUtil.Message, format, list);
		}

		private static void LogDebugToFileAndConsole(string format, params object[] list)
		{
			AppendToLog("Debug:", LogUtil.Diagnostic, format, list);
		}

		private static bool SetCmdLineParams()
		{
			try
			{
				_commandLineParser = new CommandLineParser();
				_commandLineParser.SetRequiredParameterString("OutputFile", "The path to the image to be created\\modified");
				_commandLineParser.SetOptionalParameterString("OEMInputXML", "Path to the OEM Input XML file");
				_commandLineParser.SetOptionalSwitchString("OEMCustomizationXML", "Path to the OEM Customization XML file");
				_commandLineParser.SetOptionalSwitchString("OEMCustomizationPPKG", "Path to the OEM Customization PPKG file");
				_commandLineParser.SetOptionalSwitchString("OEMVersion", "Version to use for OEM inputs such as customizations. e.g. <major>.<minor>.<submajor>.<subminor>");
				_commandLineParser.SetOptionalParameterString("MSPackageRoot", "Path to the Microsoft Package files root. Only used when OEM Input XML");
				_commandLineParser.SetOptionalSwitchString("UpdateInputXML", "Path to update input file file");
				_commandLineParser.SetOptionalSwitchBoolean("FormatDPP", "Formats DPP partition", _formatDPP);
				_commandLineParser.SetOptionalSwitchBoolean("StrictSettingPolicies", "Causes settings without policies to produce errors", _strictSettingPolicies);
				_commandLineParser.SetOptionalSwitchBoolean("SkipImageCreation", "Generates the OEM Customization packages without generating the full image", _skipImaging);
				_commandLineParser.SetOptionalSwitchBoolean("RandomizeGptIDs", "Randomizes the GPT Disk and Partiton IDs for imaging.  Needed to run ImageApp in parallel", _randomizeGptIDs);
				_commandLineParser.SetOptionalSwitchBoolean("ShowDebugMessages", "Show additional debug messages", _showDebugMessages);
				_commandLineParser.SetOptionalSwitchString("CPUType", "Specify target CPU type of x86, ARM, ARM64 or AMD64", "nothing", false, "ARM", "X86", "ARM64", "AMD64", "nothing");
				_commandLineParser.SetOptionalSwitchString("BSPProductName", "Product name which overrides BSP targeting");
			}
			catch (Exception except)
			{
				throw new NoSuchArgumentException("ImageApp!SetCmdLineParams: Unable to set an option", except);
			}
			return true;
		}

		private static bool ParseCommandlineParams(string Commandline)
		{
			string empty3 = string.Empty;
			string empty = string.Empty;
			string empty2 = string.Empty;
			if (!_commandLineParser.ParseString(Commandline, true))
			{
				return false;
			}
			_showDebugMessages = _commandLineParser.GetSwitchAsBoolean("ShowDebugMessages");
			_outputFile = _commandLineParser.GetParameterAsString("OutputFile");
			empty = Path.GetFileNameWithoutExtension(_outputFile);
			if (empty.Length == 0)
			{
				Console.WriteLine("ImageApp!ParseCommandLineParams: The Output File cannot be empty when extension is removed.");
				return false;
			}
			_LogFile = Path.Combine(FileUtils.GetShortPathName(Path.GetDirectoryName(_outputFile)), empty + "." + c_LogFileBase);
			if (_LogFile.Length > 260)
			{
				_LogFile = FileUtils.GetShortPathName(_LogFile);
			}
			if (File.Exists(_LogFile))
			{
				File.WriteAllText(_LogFile, string.Empty);
			}
			empty2 = Path.ChangeExtension(_LogFile, ".cbs.log");
			if (File.Exists(empty2))
			{
				File.WriteAllText(empty2, string.Empty);
			}
			Environment.SetEnvironmentVariable("COMPONENT_BASED_SERVICING_LOGFILE", empty2);
			Environment.SetEnvironmentVariable("WINDOWS_TRACING_FLAGS", "3");
			empty2 = Path.ChangeExtension(_LogFile, ".csi.log");
			if (File.Exists(empty2))
			{
				File.WriteAllText(empty2, string.Empty);
			}
			Environment.SetEnvironmentVariable("WINDOWS_TRACING_LOGFILE", empty2);
			_formatDPP = _commandLineParser.GetSwitchAsBoolean("FormatDPP");
			_strictSettingPolicies = _commandLineParser.GetSwitchAsBoolean("StrictSettingPolicies");
			_skipImaging = _commandLineParser.GetSwitchAsBoolean("SkipImageCreation");
			_randomizeGptIDs = _commandLineParser.GetSwitchAsBoolean("RandomizeGptIDs");
			_msPackagesRoot = _commandLineParser.GetParameterAsString("MSPackageRoot");
			_oemInputFile = _commandLineParser.GetParameterAsString("OEMInputXML");
			_oemCustomizationXML = _commandLineParser.GetSwitchAsString("OEMCustomizationXML");
			_oemCustomizationPPKG = _commandLineParser.GetSwitchAsString("OEMCustomizationPPKG");
			_oemCustomizationVersion = _commandLineParser.GetSwitchAsString("OEMVersion");
			_bspProductName = _commandLineParser.GetSwitchAsString("BSPProductName");
			_updateInputFile = _commandLineParser.GetSwitchAsString("UpdateInputXML");
			if (!string.IsNullOrEmpty(_updateInputFile))
			{
				if (!string.IsNullOrEmpty(_oemInputFile))
				{
					Console.WriteLine("ImageApp!ParseCommandLineParams: The OEMInputXML and UpdateInputXML mutually exclusive. Use the OEMInputXML file for creating images and the UpdateInputXML file for updating an existing image.");
					return false;
				}
				_bDoingUpdate = true;
			}
			_bSkipUpdateMain = string.IsNullOrEmpty(_oemInputFile) && string.IsNullOrEmpty(_updateInputFile);
			_cpuType = _commandLineParser.GetSwitchAsString("CPUType");
			if (string.Compare(_cpuType, "nothing", StringComparison.OrdinalIgnoreCase) != 0)
			{
				try
				{
					_cpuId = CpuIdParser.Parse(_cpuType);
				}
				catch
				{
					Console.WriteLine("ImageApp!ParseCommandLineParams: The CPUType was not a recognized type.");
					return false;
				}
				Console.WriteLine("ImageApp: Setting CPU Type to '" + _cpuType + "'.");
			}
			return true;
		}

		private static void ShowUsageString()
		{
			Console.WriteLine(_commandLineParser.UsageString());
		}
	}
}
