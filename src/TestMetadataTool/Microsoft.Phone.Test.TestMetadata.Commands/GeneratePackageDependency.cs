using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Phone.Test.TestMetadata.CommandLine;
using Microsoft.Tools.IO;

namespace Microsoft.Phone.Test.TestMetadata.Commands
{
	[Command("pkgdep", BriefUsage = "testmetadatatool.exe pkgdep -i <prebuilt package path> -p <project> -o <output folder>", BriefDescription = "Generate dependencies for packages.", GeneralInformation = "This command generates package dependencies for all the packages.", AllowNoNameOptions = true)]
	[CommandAlias("pdep")]
	internal class GeneratePackageDependency : CommandBase
	{
		private string _preBuiltFolder;

		private string _outputFolder;

		[Option("i", OptionValueType.ValueRequired, Description = "-{0} <prebuilt package folder>: Full path to the prebuilt package folder.")]
		public string PreBuiltFolder
		{
			get
			{
				return _preBuiltFolder;
			}
			set
			{
				_preBuiltFolder = value.ToLowerInvariant();
			}
		}

		[Option("o", OptionValueType.ValueRequired, Description = "-{0} <output folder>: Output folder.")]
		public string OutputFolder
		{
			get
			{
				return _outputFolder;
			}
			set
			{
				_outputFolder = value.ToLowerInvariant();
			}
		}

		[Option("p", OptionValueType.ValueRequired, IsMultipleValue = true, CollectionType = typeof(string), Description = "-{0} <project name>: The name of the project.")]
		public List<string> Projects { get; set; }

		[Option("s", OptionValueType.ValueOptional, Description = "-{0} <supression file>: Supression file.")]
		public string SupressionFile { get; set; }

		[Option("a", OptionValueType.NoValue, Description = "-{0} : Generate all product and test dependencies.")]
		public bool AllDependencies { get; set; }

		[Option("c", OptionValueType.ValueOptional, Description = "-{0} <csv folder>: Generate CSV files for package analysis in specified folder.")]
		public string CsvOutputFolder { get; set; }

		[Option("w", OptionValueType.NoValue, Description = "-{0} : Treat error as warnings.")]
		public bool ErrorAsWarnings
		{
			get
			{
				return Log.LogErrorAsWarning;
			}
			set
			{
				Log.LogErrorAsWarning = value;
			}
		}

		[Option("l", OptionValueType.ValueOptional, Description = "-{0} <log file>: Log errors and warnings in specified file.")]
		public string LogFile
		{
			get
			{
				return Log.LogFile;
			}
			set
			{
				Log.LogFile = value;
			}
		}

		[Option("t", OptionValueType.NoValue, DefaultValue = "\\Data\\Test", Description = "-{0} : The test deployment root on the device.")]
		public string TestRoot { get; set; }

		[Option("f", OptionValueType.ValueOptional, Description = "-{0} <package file>: package file.")]
		public string PackageName { get; set; }

		[Option("rl", OptionValueType.NoValue, Description = "-{0} : Ignore locale specific packages.")]
		public bool IgnoreLocaleSpecificPackages { get; set; }

		[Option("rr", OptionValueType.NoValue, Description = "-{0} : Ignore resource specific packages.")]
		public bool IgnoreResourceSpecificPackages { get; set; }

		[Option("n", OptionValueType.ValueRequired, Description = "-{0} <int>: Number of parallel threads for loading. Default: 1")]
		public int NumberOfLoaders { get; set; }

		[Option("q", OptionValueType.NoValue, Description = "-{0} : create the dep.xml in the output folder instead of the folder of spkg.")]
		public bool DepXmlToOutputFolder { get; set; }

		[Option("Up", OptionValueType.ValueRequired, Description = "-{0} : Update the dep.xml with searching for more dependency spkgs in the path specified. \n This option requires an existing dep.xml file and  only works when Package Name is specified.")]
		public string RazzlePkgPath { get; set; }

		public GeneratePackageDependency()
		{
			Projects = new List<string>();
		}

		private void ValidateParameters()
		{
			if (string.IsNullOrWhiteSpace(PreBuiltFolder))
			{
				PrintError("PreBuilt Folder not specified.\n");
			}
			string[] array;
			if (PreBuiltFolder != null && !LongPathDirectory.Exists(PreBuiltFolder))
			{
				array = PreBuiltFolder.Split(';');
				foreach (string text in array)
				{
					if (!LongPathDirectory.Exists(text))
					{
						PrintError(string.Format(CultureInfo.InvariantCulture, "PreBuilt folder {0} does not exist", new object[1] { text }));
					}
				}
			}
			if (string.IsNullOrWhiteSpace(OutputFolder))
			{
				PrintError("Output Folder not specified.\n");
			}
			if (OutputFolder != null && !LongPathDirectory.Exists(OutputFolder))
			{
				PrintError(string.Format(CultureInfo.InvariantCulture, "Output folder {0} does not exist", new object[1] { base.Output }));
			}
			if (Projects.Count == 0)
			{
				PrintError(string.Format(CultureInfo.InvariantCulture, "Project not specified.\n"));
			}
			if (!string.IsNullOrEmpty(RazzlePkgPath) && string.IsNullOrEmpty(PackageName))
			{
				PrintError(string.Format(CultureInfo.InvariantCulture, "-Up option is not valid when the PackagenName is empty or missing.\n"));
			}
			if (string.IsNullOrEmpty(RazzlePkgPath))
			{
				return;
			}
			char[] separator = new char[1] { ';' };
			array = RazzlePkgPath.Split(separator);
			foreach (string text2 in array)
			{
				if (!LongPathDirectory.Exists(text2))
				{
					PrintError($"The path {text2} specified in commandline option -Up does not Exist.");
				}
				if (!LongPathDirectory.EnumerateFiles(text2, "*man.dsm.xml", SearchOption.AllDirectories).Any())
				{
					PrintError($"There is no  packageManfiest file under path {text2}. Invliad Razzle spkg path.");
				}
				if (!LongPathDirectory.EnumerateFiles(text2, "*.spkg", SearchOption.AllDirectories).Any() && !LongPathDirectory.EnumerateFiles(text2, "*.cab", SearchOption.AllDirectories).Any())
				{
					PrintError($"There is no spkg or cab file under path {text2}. Invliad Razzle spkg path.");
				}
			}
		}

		private void SetupEnvironment()
		{
			OutputFolder = LongPathPath.Combine(OutputFolder, "packagedep");
			if (LongPathDirectory.Exists(OutputFolder))
			{
				ClearAttributes(OutputFolder);
				LongPathDirectoryRecursiveDeleteWorkaround(OutputFolder);
			}
			LongPathDirectory.Create(OutputFolder);
			Projects = Projects.Select((string p) => p.ToLowerInvariant()).ToList();
			if (LogFile != null && LongPathFile.Exists(LogFile))
			{
				LongPathFile.SetAttributes(LogFile, FileAttributes.Normal);
				LongPathFile.Delete(LogFile);
			}
			if (CsvOutputFolder == null)
			{
				return;
			}
			if (LongPathDirectory.Exists(CsvOutputFolder))
			{
				ClearAttributes(CsvOutputFolder);
				{
					foreach (string item in LongPathDirectory.EnumerateFiles(CsvOutputFolder))
					{
						LongPathFile.Delete(item);
					}
					return;
				}
			}
			LongPathDirectory.Create(CsvOutputFolder);
		}

		private void LongPathDirectoryRecursiveDeleteWorkaround(string path)
		{
			try
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(path);
				int milliseconds = (int)TimeSpan.FromMinutes(10.0).TotalMilliseconds;
				DirectoryInfo directoryInfo2 = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "IOHelperEmptyDirectory"));
				directoryInfo2.Create();
				string arguments = $"\"{directoryInfo2.FullName}\" \"{directoryInfo.FullName}\" /MIR";
				using (Process process = Process.Start(new ProcessStartInfo("robocopy")
				{
					Arguments = arguments,
					CreateNoWindow = true,
					UseShellExecute = false
				}))
				{
					process.WaitForExit(milliseconds);
				}
				directoryInfo.Delete();
			}
			catch (Exception ex)
			{
				Log.Warning("Failed to the output folder. Moving on. Error: {0}", ex.Message);
			}
		}

		public static void ClearAttributes(string directoryName)
		{
			if (!LongPathDirectory.Exists(directoryName))
			{
				return;
			}
			foreach (string item in LongPathDirectory.EnumerateDirectories(directoryName))
			{
				ClearAttributes(item);
			}
			foreach (string item2 in LongPathDirectory.EnumerateFiles(directoryName))
			{
				LongPathFile.SetAttributes(item2, FileAttributes.Normal);
			}
		}

		protected override void RunImplementation()
		{
			ValidateParameters();
			SetupEnvironment();
			if (NumberOfLoaders < 1)
			{
				NumberOfLoaders = 1;
			}
			PackageRepository packageRepository = new PackageRepository(PreBuiltFolder, OutputFolder, Projects, SupressionFile, TestRoot, IgnoreLocaleSpecificPackages, IgnoreResourceSpecificPackages, NumberOfLoaders, DepXmlToOutputFolder, RazzlePkgPath);
			if (string.IsNullOrEmpty(PackageName))
			{
				packageRepository.ResolveDependency(AllDependencies, CsvOutputFolder);
			}
			else if (string.IsNullOrEmpty(RazzlePkgPath))
			{
				packageRepository.ResolveDependency(PackageName, AllDependencies, CsvOutputFolder);
			}
			else
			{
				packageRepository.UpdateDependency(PackageName, AllDependencies, CsvOutputFolder);
			}
		}
	}
}
