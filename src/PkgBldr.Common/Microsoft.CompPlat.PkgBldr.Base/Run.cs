using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CompPlat.PkgBldr.Base.Tools;
using Microsoft.CompPlat.PkgBldr.Tools;

namespace Microsoft.CompPlat.PkgBldr.Base
{
	public static class Run
	{
		public static string RunSPkgGen(List<string> pkgGenArgs, PkgBldrCmd pkgBldrArgs, List<string> spkgList = null)
		{
			return RunSPkgGen(pkgGenArgs, new Logger(), pkgBldrArgs, spkgList);
		}

		public static string RunSPkgGen(List<string> pkgGenArgs, IDeploymentLogger logger, PkgBldrCmd pkgBldrArgs, List<string> spkgList = null)
		{
			return RunSPkgGen(pkgGenArgs, false, logger, pkgBldrArgs, spkgList);
		}

		public static string RunSPkgGen(List<string> pkgGenArgs, bool inWindows, PkgBldrCmd pkgBldrArgs, List<string> spkgList = null)
		{
			return RunSPkgGen(pkgGenArgs, inWindows, new Logger(), pkgBldrArgs, spkgList);
		}

		public static string RunSPkgGen(List<string> pkgGenArgs, bool inWindows, IDeploymentLogger logger, PkgBldrCmd pkgBldrArgs, List<string> spkgList = null)
		{
			string text = null;
			if (spkgList != null)
			{
				text = Path.GetTempFileName();
				pkgGenArgs.Add(string.Format(CultureInfo.InvariantCulture, "/spkgsout:{0}", new object[1] { text }));
				spkgList.Clear();
			}
			string command;
			string text2;
			string workingDir;
			if (inWindows)
			{
				command = Environment.ExpandEnvironmentVariables(pkgBldrArgs.toolPaths["urtrun"]);
				text2 = Environment.ExpandEnvironmentVariables(string.Format(CultureInfo.InvariantCulture, "4.0 {0} ", new object[1] { pkgBldrArgs.toolPaths["spkggen"] }));
				workingDir = Environment.ExpandEnvironmentVariables(LongPath.GetDirectoryName(pkgBldrArgs.toolPaths["spkggen"]));
			}
			else
			{
				command = "SPkgGen.exe";
				text2 = null;
				workingDir = Directory.GetCurrentDirectory();
			}
			foreach (string pkgGenArg in pkgGenArgs)
			{
				text2 = text2 + pkgGenArg + " ";
			}
			string processOutput = null;
			logger.LogInfo(string.Format(CultureInfo.InvariantCulture, "Running SPkgGen.exe {0}", new object[1] { text2 }));
			if (RunProcess(workingDir, command, text2, false, true, out processOutput) != 0)
			{
				throw new PkgGenException(string.Format(CultureInfo.InvariantCulture, "{0}", new object[1] { processOutput }));
			}
			if (pkgGenArgs.Contains("/diagnostic"))
			{
				logger.LogSpkgGenOutput(processOutput);
			}
			if (spkgList != null)
			{
				List<string> collection = LongPathFile.ReadAllLines(text).ToList();
				spkgList.AddRange(collection);
				LongPathFile.Delete(text);
			}
			return processOutput;
		}

		public static string RunDsmConverter(string spkg, bool wow, bool ignoreConvertDsmError)
		{
			return RunDsmConverter(spkg, LongPath.GetDirectoryName(spkg), wow, ignoreConvertDsmError);
		}

		public static string RunDsmConverter(string input, string output, bool wow, bool ignoreConvertDsmError)
		{
			uint flags = 11u;
			ConvertDSM.RunDsmConverter(input, output, wow, ignoreConvertDsmError, flags);
			return string.Format(CultureInfo.InvariantCulture, "Completed converting package {0}.", new object[1] { input });
		}

		public static string RunProcess(string workingDirectory, string processName, string arguments)
		{
			return RunProcess(workingDirectory, processName, arguments, new Logger());
		}

		public static string RunProcess(string workingDirectory, string processName, string arguments, IDeploymentLogger logger)
		{
			string processOutput = null;
			logger.LogInfo(string.Format(CultureInfo.InvariantCulture, "Running {0} {1}", new object[2] { processName, arguments }));
			int num = RunProcess(workingDirectory, processName, arguments, true, true, out processOutput);
			if (num != 0)
			{
				throw new PkgGenException(string.Format(CultureInfo.InvariantCulture, "Call to {0} failed with error {1}", new object[2] { processName, num }));
			}
			return processOutput;
		}

		public static string RunProcessQuiet(string workingDirectory, string processName, string arguments, string envName = null, string envValue = null)
		{
			return RunProcessQuiet(workingDirectory, processName, arguments, new Logger(), envName, envValue);
		}

		public static string RunProcessQuiet(string workingDirectory, string processName, string arguments, IDeploymentLogger logger, string envName = null, string envValue = null)
		{
			string processOutput = null;
			int num = RunProcess(workingDirectory, processName, arguments, true, true, out processOutput, envName, envValue);
			if (num != 0)
			{
				logger.LogInfo(string.Format(CultureInfo.InvariantCulture, "Running {0} {1}", new object[2] { processName, arguments }));
				logger.LogInfo(string.Format(CultureInfo.InvariantCulture, "\n{0}\n", new object[1] { processOutput }));
				throw new PkgGenException(string.Format(CultureInfo.InvariantCulture, "Call to {0} failed with error {1}", new object[2] { processName, num }));
			}
			return processOutput;
		}

		public static string RunProcess(string workingDirectory, string processName, string arguments, ref int iExitCode)
		{
			return RunProcess(workingDirectory, processName, arguments, new Logger(), ref iExitCode);
		}

		public static string RunProcess(string workingDirectory, string processName, string arguments, IDeploymentLogger logger, ref int iExitCode)
		{
			string processOutput = null;
			logger.LogInfo(string.Format(CultureInfo.InvariantCulture, "Running {0} {1}", new object[2] { processName, arguments }));
			iExitCode = RunProcess(workingDirectory, processName, arguments, true, true, out processOutput);
			return processOutput;
		}

		public static int RunProcess(string workingDir, string command, string args, bool hiddenWindow, bool captureOutput, out string processOutput, string envName = null, string envValue = null)
		{
			int result = 0;
			processOutput = string.Empty;
			command = Environment.ExpandEnvironmentVariables(command);
			args = Environment.ExpandEnvironmentVariables(args);
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			if (envName != null && envValue != null)
			{
				processStartInfo.EnvironmentVariables[envName] = envValue;
			}
			processStartInfo.CreateNoWindow = true;
			if (hiddenWindow)
			{
				processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			}
			if (workingDir != null)
			{
				processStartInfo.WorkingDirectory = workingDir;
			}
			processStartInfo.RedirectStandardInput = false;
			processStartInfo.RedirectStandardOutput = captureOutput;
			processStartInfo.UseShellExecute = !captureOutput;
			if (!string.IsNullOrEmpty(command) && !LongPathFile.Exists(command))
			{
				CommonUtils.FindInPath(command);
			}
			processStartInfo.FileName = command;
			processStartInfo.Arguments = args;
			using (Process process = Process.Start(processStartInfo))
			{
				if (process != null)
				{
					if (captureOutput)
					{
						processOutput = process.StandardOutput.ReadToEnd();
					}
					process.WaitForExit();
					if (!process.HasExited)
					{
						throw new PkgGenException("Run proccess failed");
					}
					return process.ExitCode;
				}
				return result;
			}
		}

		public static string BinPlace(string source, string rootDir, string path, PkgBldrCmd pkgBldrArgs, bool temporary = false)
		{
			return BinPlace(source, rootDir, path, new Logger(), pkgBldrArgs, temporary);
		}

		public static string BinPlace(string source, string rootDir, string path, IDeploymentLogger logger, PkgBldrCmd pkgBldrArgs, bool temporary = false)
		{
			rootDir = Environment.ExpandEnvironmentVariables(rootDir);
			if (!LongPathDirectory.Exists(rootDir))
			{
				throw new PkgGenException("Can't find root directory {0}", rootDir);
			}
			if (!LongPathFile.Exists(source))
			{
				throw new PkgGenException("Can't find source file {0}", source);
			}
			if (!temporary)
			{
				string arguments = string.Format(CultureInfo.InvariantCulture, "-LeaveBinaryAlone -r {0} -:DEST {1} {2}", new object[3] { rootDir, path, source });
				RunProcessQuiet(Environment.ExpandEnvironmentVariables(pkgBldrArgs.razzleToolPath), Environment.ExpandEnvironmentVariables(pkgBldrArgs.toolPaths["binplace"]), arguments, logger);
			}
			else
			{
				string path2 = LongPath.Combine(rootDir, path);
				LongPathDirectory.CreateDirectory(path2);
				path2 = LongPath.Combine(path2, LongPath.GetFileName(source));
				LongPathFile.Copy(source, path2);
			}
			if (rootDir.Equals(Environment.ExpandEnvironmentVariables(pkgBldrArgs.buildNttree), StringComparison.OrdinalIgnoreCase))
			{
				rootDir = "$(build.nttree)";
			}
			return LongPath.Combine(rootDir, path);
		}
	}
}
