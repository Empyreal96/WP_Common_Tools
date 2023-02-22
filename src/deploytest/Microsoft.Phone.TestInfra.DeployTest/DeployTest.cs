using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Mobile;
using Microsoft.Phone.TestInfra.Deployment;

namespace Microsoft.Phone.TestInfra.DeployTest
{
	public class DeployTest
	{
		public static int Main(string[] args)
		{
			DeployTest deployTest = new DeployTest();
			string rootPath = string.Empty;
			string alternateRoots = string.Empty;
			string packages = string.Empty;
			string packageFile = string.Empty;
			string empty = string.Empty;
			string text = string.Empty;
			string cacheRoot = string.Empty;
			int num = 24;
			TraceLevel result = TraceLevel.Info;
			TraceLevel result2 = TraceLevel.Info;
			bool recurse = false;
			bool sourceRootIsVolatile = false;
			bool flag = false;
			bool flag2 = false;
			List<string> list = new List<string>();
			string logFile = null;
			if (args.Length == 0)
			{
				Usage();
				return 0;
			}
			try
			{
				for (int i = 0; i < args.Length; i++)
				{
					if (Regex.IsMatch(args[i], "^[/-]out$", RegexOptions.IgnoreCase))
					{
						i++;
						if (i >= args.Length)
						{
							Console.Error.WriteLine("ERROR: -out requires path");
							Usage();
							return -1;
						}
						text = args[i];
					}
					else if (Regex.IsMatch(args[i], "^[/-]root$", RegexOptions.IgnoreCase))
					{
						i++;
						if (i >= args.Length)
						{
							Console.Error.WriteLine("ERROR: -root requires path");
							Usage();
							return -2;
						}
						rootPath = args[i];
					}
					else if (Regex.IsMatch(args[i], "^[/-]altroot$", RegexOptions.IgnoreCase))
					{
						i++;
						if (i >= args.Length)
						{
							Console.Error.WriteLine("ERROR: -altroot requires path");
							Usage();
							return -3;
						}
						alternateRoots = args[i];
					}
					else if (Regex.IsMatch(args[i], "^[/-]pkg$", RegexOptions.IgnoreCase))
					{
						i++;
						if (i >= args.Length)
						{
							Console.Error.WriteLine("ERROR: -pkg requires path");
							Usage();
							return -4;
						}
						packages = args[i];
					}
					else if (Regex.IsMatch(args[i], "^[/-]pkgfile$", RegexOptions.IgnoreCase))
					{
						i++;
						if (i >= args.Length)
						{
							Console.Error.WriteLine("ERROR: -pkgfile requires file");
							Usage();
							return -5;
						}
						packageFile = args[i];
					}
					else if (Regex.IsMatch(args[i], "^[/-]cache$", RegexOptions.IgnoreCase))
					{
						i++;
						if (i >= args.Length)
						{
							Console.Error.WriteLine("ERROR: -cache requires path");
							Usage();
							return -6;
						}
						cacheRoot = args[i];
					}
					else if (Regex.IsMatch(args[i], "^[/-]robocopy$", RegexOptions.IgnoreCase))
					{
						i++;
						if (i >= args.Length)
						{
							Console.Error.WriteLine("ERROR: -robocopy requires <option> parameter");
							Usage();
							return -7;
						}
					}
					else if (Regex.IsMatch(args[i], "^[/-]expiresIn$", RegexOptions.IgnoreCase))
					{
						i++;
						if (i >= args.Length)
						{
							Console.Error.WriteLine("ERROR: -expiresIn requires <option> parameter");
							Usage();
							return -8;
						}
						int result3;
						if (int.TryParse(args[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out result3))
						{
							Console.Error.WriteLine("ERROR: -expiresIn value cannot be parsed");
							Usage();
							return -8;
						}
						if (result3 < 1)
						{
							Console.Error.WriteLine("ERROR: -expiresIn value should be at least 1 (hour)");
							Usage();
							return -8;
						}
						num = result3;
					}
					else if (Regex.IsMatch(args[i], "^[/-]ConsoleOutputLevel$", RegexOptions.IgnoreCase))
					{
						i++;
						if (i >= args.Length)
						{
							Console.Error.WriteLine("ERROR: -ConsoleOutputLevel requires a TraceLevel");
							Usage();
							return -9;
						}
						if (!Enum.TryParse<TraceLevel>(args[i], true, out result))
						{
							Console.Error.WriteLine("ERROR: {0} is not a valid TraceLevel", args[i]);
							Usage();
							return -9;
						}
					}
					else if (Regex.IsMatch(args[i], "^[/-]FileOutputLevel$", RegexOptions.IgnoreCase))
					{
						i++;
						if (i >= args.Length)
						{
							Console.Error.WriteLine("ERROR: -FileOutputLevel requires a TraceLevel");
							Usage();
							return -9;
						}
						if (!Enum.TryParse<TraceLevel>(args[i], true, out result2))
						{
							Console.Error.WriteLine("ERROR: {0} is not a valid TraceLevel", args[i]);
							Usage();
							return -9;
						}
					}
					else if (Regex.IsMatch(args[i], "^[/-]?verbose$", RegexOptions.IgnoreCase))
					{
						Console.WriteLine("-verbose is deprecated.  Use '-ConsoleOutputLevel Verbose' for verbose console log.");
					}
					else if (Regex.IsMatch(args[i], "^[/-]?fileverbose$", RegexOptions.IgnoreCase))
					{
						Console.WriteLine("-fileverbose is deprecated.  Use '-FileOutputLevel Verbose' for verbose console log.");
					}
					else if (Regex.IsMatch(args[i], "^[/-]?recurse$", RegexOptions.IgnoreCase))
					{
						recurse = true;
					}
					else
					{
						if (Regex.IsMatch(args[i], "^[/-]?norobo$", RegexOptions.IgnoreCase))
						{
							continue;
						}
						if (Regex.IsMatch(args[i], "^[/-]volatilesource", RegexOptions.IgnoreCase))
						{
							sourceRootIsVolatile = true;
							continue;
						}
						if (Regex.IsMatch(args[i], "^[/-]RunConfigAction", RegexOptions.IgnoreCase))
						{
							flag = true;
							continue;
						}
						if (Regex.IsMatch(args[i], "^[/-]GenerateGeneralCache", RegexOptions.IgnoreCase))
						{
							flag2 = true;
							continue;
						}
						if (Regex.IsMatch(args[i], "^[/-]LogFile$", RegexOptions.IgnoreCase))
						{
							i++;
							if (i < args.Length)
							{
								logFile = args[i];
								continue;
							}
							Console.Error.WriteLine("ERROR: -LogFile requires path");
							Usage();
							return -10;
						}
						if (!args[i].Contains("="))
						{
							Console.Error.WriteLine("ERROR: parameter '{0}' not supported", args[i]);
							Usage();
							return -12;
						}
						string[] array = args[i].Split(new char[1] { '=' }, StringSplitOptions.RemoveEmptyEntries);
						if (array.Length != 2)
						{
							Console.Error.WriteLine("Macro {0} wrong length {1}", array[0], array.Length);
							Usage();
							return -11;
						}
						list.Add(args[i]);
					}
				}
			}
			catch (ArgumentException ex)
			{
				Console.Error.WriteLine("ERROR: Path has invalid characters. Check that there are no trailing backslashes in paths.");
				Logger.Error(ex.ToString());
				Usage();
				return -13;
			}
			catch (Exception ex2)
			{
				Console.Error.WriteLine("ERROR: Unknown exception while parsing arguments.");
				Logger.Error(ex2.ToString());
				Usage();
				return -14;
			}
			if (flag2)
			{
				try
				{
					Logger.Configure(TraceLevel.Info, TraceLevel.Info, "RootCacheGenerate.log", true);
					Logger.Info("Start generating root cache files ...");
					GeneralCacheGenerator.DoWork(text, rootPath);
					Logger.Info("Root cache files are generated in {0}", Path.GetFullPath(text));
				}
				catch (Exception ex3)
				{
					Logger.Error("Error Occurred: {0}", ex3.ToString());
					return 1;
				}
				finally
				{
					Logger.Close();
				}
				return 0;
			}
			string macros = string.Join(";", list.ToArray());
			PackageDeployerParameters packageDeployerParameters = new PackageDeployerParameters(text, rootPath);
			packageDeployerParameters.Packages = packages;
			packageDeployerParameters.PackageFile = packageFile;
			packageDeployerParameters.AlternateRoots = alternateRoots;
			packageDeployerParameters.ExpiresIn = TimeSpan.FromHours(num);
			packageDeployerParameters.CacheRoot = cacheRoot;
			packageDeployerParameters.Macros = macros;
			packageDeployerParameters.SourceRootIsVolatile = sourceRootIsVolatile;
			packageDeployerParameters.Recurse = recurse;
			packageDeployerParameters.ConsoleTraceLevel = result;
			packageDeployerParameters.FileTraceLevel = result2;
			packageDeployerParameters.LogFile = logFile;
			PackageDeployerParameters packageDeployerParameters2 = packageDeployerParameters;
			PackageDeployer packageDeployer = new PackageDeployer(packageDeployerParameters2);
			PackageDeployerOutput packageDeployerOutput = packageDeployer.Run();
			if (!packageDeployerOutput.Success || !flag || packageDeployerOutput.ConfigurationCommands == null || packageDeployerOutput.ConfigurationCommands.Count == 0)
			{
				return (!packageDeployerOutput.Success) ? 1 : 0;
			}
			bool flag3 = true;
			try
			{
				Logger.Configure(result, result2, packageDeployer.LogFile, true);
				Logger.Info("Found config commands to run after package is deployed.");
				foreach (ConfigCommand configurationCommand in packageDeployerOutput.ConfigurationCommands)
				{
					Logger.Info("Running config command {0}.", configurationCommand.CommandLine);
					TimeSpan timeout = TimeSpan.FromMinutes(3.0);
					ProcessLauncher processLauncher = new ProcessLauncher("cmd.exe", "/c " + configurationCommand.CommandLine, delegate(string m)
					{
						if (!string.IsNullOrWhiteSpace(m))
						{
							Logger.Info("Command Output: " + m);
						}
					}, delegate(string m)
					{
						if (!string.IsNullOrWhiteSpace(m))
						{
							Logger.Info("Command Output: " + m);
						}
					}, delegate(string m)
					{
						if (!string.IsNullOrWhiteSpace(m))
						{
							Logger.Info("Command Output: " + m);
						}
					});
					processLauncher.TimeoutHandler = delegate(Process p)
					{
						throw new TimeoutException($"Process {p.StartInfo.FileName} did not exit in {timeout.Minutes} minutes");
					};
					ProcessLauncher processLauncher2 = processLauncher;
					processLauncher2.RunToExit(Convert.ToInt32(timeout.TotalMilliseconds, CultureInfo.InvariantCulture));
					if (!processLauncher2.Process.HasExited)
					{
						processLauncher2.Process.Kill();
						Logger.Error("Error: Process {0} has not exited. Killed.", processLauncher2);
						flag3 = false;
					}
					else if (!configurationCommand.IgnoreExitCode && processLauncher2.Process.ExitCode != configurationCommand.SuccessExitCode)
					{
						Logger.Error("{0} return an error exit code {1}", configurationCommand.CommandLine, processLauncher2.Process.ExitCode);
						flag3 = false;
					}
					else
					{
						Logger.Info("{0} returns.", configurationCommand.CommandLine);
					}
				}
				Logger.Info("Configuration commands are done.");
			}
			catch (Exception ex4)
			{
				flag3 = false;
				Logger.Error("Exception: {0}", ex4.ToString());
			}
			finally
			{
				Logger.Close();
			}
			return (!flag3) ? 1 : 0;
		}

		private static void Usage()
		{
			Console.WriteLine("DeployTest");
			Console.WriteLine("    Deploys test packages and dependencies.");
			Console.WriteLine("\nUsage:");
			Console.WriteLine("    DeployTest -out <OutputPath> -root <BinaryRoot> -pkg <PkgName>");
			Console.WriteLine("               [-pkgfile <PkgFile>] [-altroot <AltRoot>] ");
			Console.WriteLine("               [-cache <CacheDir>] [MACRO=<value>] [MACRO2=<value2>] ...");
			Console.WriteLine("\nParameters:");
			Console.WriteLine("    -out <OutputPath>   : Directory for output files.");
			Console.WriteLine("    -root <BinaryRoot>  : Location of Binary Root. If you specify multiple paths,");
			Console.WriteLine("                          separate them with semicolons. Default is current");
			Console.WriteLine("                          $(BINARY_ROOT) value in Phone Build environment or .");
			Console.WriteLine("                          $(_NTTREE) value in Razzle Build environment or .");
			Console.WriteLine("    -pkg <PkgName>      : Name of package, will search under");
			Console.WriteLine("                          BinaryRoot\\Prebuilt to find the package.");
			Console.WriteLine("                          Can also be \"PkgName;PkgName2;PkgName3\".");
			Console.WriteLine("                          You can specify optional tags in each PkgName as");
			Console.WriteLine("                          \"PkgName[tags=Tag1,Tag2,Tag3]\".");
			Console.WriteLine("    -pkgfile <PkgFile>  : File with package names, one per line.");
			Console.WriteLine("    -altroot <AltRoot>  : Alternate location to search for packages");
			Console.WriteLine("                          (like \\\\build\\release\\<...>).");
			Console.WriteLine("    -cache <CacheDir>   : Location to cache local copies of packages, meant for");
			Console.WriteLine("                          lab test machines.");
			Console.WriteLine("    -expiresIn <value>  : Deployment folder expiration time, in hours.");
			Console.WriteLine("                          Default is 24.");
			Console.WriteLine("    -recurse            : Default is to only process the first level of dep.xml");
			Console.WriteLine("                          Add -recurse to process for all packages.");
			Console.WriteLine("    -RunConfigAction    : If the package contains configuration actions, whether run");
			Console.WriteLine("                          these actions. Default is no.");
			Console.WriteLine("    -ConsoleOutputLevel : Maximum verbosity level to output to console.");
			Console.WriteLine("                          Default is Info.  Values are Off, Error, Warning, Info, Verbose.");
			Console.WriteLine("    -FileOutputLevel    : Maximum verbosity level to output to the log file.");
			Console.WriteLine("                          Default is Info.  Values are Off, Error, Warning, Info, Verbose.");
			Console.WriteLine("    -LogFile            : The path to a log file to use.");
			Console.WriteLine("    -norobo             : [Ignored] Default is to use RoboCopy to copy files.");
			Console.WriteLine("    -robocopy <Options> : [Ignored] Overrides the default RoboCopy options.");
			Console.WriteLine("\nUsage: ");
			Console.WriteLine("   DeployTest -GenerateGeneralCache -out <OutputPath> -root <PhoneBuildPath>");
			Console.WriteLine("    -GenerateGeneralCache: The general cache files are generated when this option is on.");
			Console.WriteLine("                           A txt file containing binary names that should be appended to ");
			Console.WriteLine("                           pkgdep_supress.txt is also generated.");
			Console.WriteLine("                           This option is for TDX team internal use only.");
			Console.WriteLine("                           When this option is on, the options other than -out and -root are ignored");
			Console.WriteLine("                           Note that the root path must be a public phone build path. ");
			Console.WriteLine("We can deduce the winbuild path from phone build path, but cannot do it the other way.");
		}
	}
}
