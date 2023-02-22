using System;
using System.Diagnostics;
using System.IO;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public static class SdCommand
	{
		public static void Run(string cmd, string file)
		{
			string fileName = Environment.GetEnvironmentVariable("RAZZLETOOLPATH") + "\\x86\\sd.exe";
			Process process = new Process();
			process.StartInfo.FileName = fileName;
			process.StartInfo.Arguments = $"{cmd} {file}";
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.WorkingDirectory = Path.GetDirectoryName(file);
			process.Start();
			string value = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			if (process.ExitCode != 0)
			{
				Console.WriteLine(value);
				throw new Exception($"Failed to execute sd {cmd} {file}");
			}
		}
	}
}
