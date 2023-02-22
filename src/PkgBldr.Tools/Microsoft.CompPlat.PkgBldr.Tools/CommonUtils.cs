using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public static class CommonUtils
	{
		private static readonly HashAlgorithm Sha256Algorithm = HashAlgorithm.Create("SHA256");

		public static string FindInPath(string filename)
		{
			string text = null;
			text = ((!LongPathFile.Exists(LongPath.Combine(Environment.CurrentDirectory, filename))) ? Environment.GetEnvironmentVariable("PATH").Split(';').FirstOrDefault((string x) => LongPathFile.Exists(LongPath.Combine(x, filename))) : Environment.CurrentDirectory);
			if (string.IsNullOrEmpty(text))
			{
				throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, "Can't find file '{0}' anywhere in the %PATH%", new object[1] { filename }));
			}
			return LongPath.Combine(text, filename);
		}

		public static int RunProcess(string workingDir, string command, string args, bool hiddenWindow)
		{
			string processOutput = null;
			return RunProcess(workingDir, command, args, hiddenWindow, false, out processOutput);
		}

		public static int RunProcess(string command, string args)
		{
			string processOutput = null;
			int num = RunProcess(null, command, args, true, true, out processOutput);
			if (num != 0)
			{
				Console.WriteLine(processOutput);
			}
			return num;
		}

		private static int RunProcess(string workingDir, string command, string args, bool hiddenWindow, bool captureOutput, out string processOutput)
		{
			int result = 0;
			processOutput = string.Empty;
			command = Environment.ExpandEnvironmentVariables(command);
			args = Environment.ExpandEnvironmentVariables(args);
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
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
				FindInPath(command);
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
						throw new IUException("Process <{0}> didn't exit correctly", command);
					}
					return process.ExitCode;
				}
				return result;
			}
		}

		public static string BytesToHexicString(byte[] bytes)
		{
			if (bytes == null)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder(bytes.Length * 2);
			for (int i = 0; i < bytes.Length; i++)
			{
				stringBuilder.Append(bytes[i].ToString("X2", CultureInfo.InvariantCulture.NumberFormat));
			}
			return stringBuilder.ToString();
		}

		public static byte[] HexicStringToBytes(string text)
		{
			if (text == null)
			{
				return new byte[0];
			}
			if (text.Length % 2 != 0)
			{
				throw new IUException("Incorrect length of a hexic string:\"{0}\"", text);
			}
			List<byte> list = new List<byte>(text.Length / 2);
			for (int i = 0; i < text.Length; i += 2)
			{
				string text2 = text.Substring(i, 2);
				byte result;
				if (!byte.TryParse(text2, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out result))
				{
					throw new IUException("Failed to parse hexic string: \"{0}\"", text2);
				}
				list.Add(result);
			}
			return list.ToArray();
		}

		public static bool ByteArrayCompare(byte[] array1, byte[] array2)
		{
			if (array1 == array2)
			{
				return true;
			}
			if (array1 == null || array2 == null)
			{
				return false;
			}
			if (array1.Length != array2.Length)
			{
				return false;
			}
			for (int i = 0; i < array1.Length; i++)
			{
				if (array1[i] != array2[i])
				{
					return false;
				}
			}
			return true;
		}

		public static string GetCopyrightString()
		{
			string format = "Microsoft (C) {0} {1}";
			string processName = Process.GetCurrentProcess().ProcessName;
			string currentAssemblyFileVersion = FileUtils.GetCurrentAssemblyFileVersion();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat(format, processName, currentAssemblyFileVersion);
			stringBuilder.AppendLine();
			return stringBuilder.ToString();
		}

		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlPrincipal)]
		public static bool IsCurrentUserAdmin()
		{
			return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole("BUILTIN\\\\Administrators");
		}

		public static string GetSha256Hash(byte[] buffer)
		{
			return BitConverter.ToString(Sha256Algorithm.ComputeHash(buffer)).Replace("-", string.Empty);
		}
	}
}
