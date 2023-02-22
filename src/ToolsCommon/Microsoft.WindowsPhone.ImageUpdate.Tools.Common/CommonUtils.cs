using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public static class CommonUtils
	{
		private const int S_OK = 0;

		private const int WimNoCommit = 0;

		private const int WimCommit = 1;

		private static readonly HashAlgorithm Sha256Algorithm = HashAlgorithm.Create("SHA256");

		public static IntPtr MountVHD(string vhdPath, bool fReadOnly)
		{
			VIRTUAL_DISK_ACCESS_MASK accessMask = VIRTUAL_DISK_ACCESS_MASK.VIRTUAL_DISK_ACCESS_ALL;
			if (fReadOnly)
			{
				accessMask = VIRTUAL_DISK_ACCESS_MASK.VIRTUAL_DISK_ACCESS_READ;
			}
			OPEN_VIRTUAL_DISK_FLAG openFlags = OPEN_VIRTUAL_DISK_FLAG.OPEN_VIRTUAL_DISK_FLAG_NONE;
			ATTACH_VIRTUAL_DISK_FLAG attachFlags = ATTACH_VIRTUAL_DISK_FLAG.ATTACH_VIRTUAL_DISK_FLAG_NONE;
			if (fReadOnly)
			{
				attachFlags = ATTACH_VIRTUAL_DISK_FLAG.ATTACH_VIRTUAL_DISK_FLAG_READ_ONLY;
			}
			return MountVHD(vhdPath, accessMask, openFlags, attachFlags);
		}

		public static IntPtr MountVHD(string vhdPath, VIRTUAL_DISK_ACCESS_MASK accessMask, OPEN_VIRTUAL_DISK_FLAG openFlags, ATTACH_VIRTUAL_DISK_FLAG attachFlags)
		{
			IntPtr Handle = IntPtr.Zero;
			VIRTUAL_STORAGE_TYPE VirtualStorageType = default(VIRTUAL_STORAGE_TYPE);
			VirtualStorageType.DeviceId = VHD_STORAGE_TYPE_DEVICE.VIRTUAL_STORAGE_TYPE_DEVICE_VHD;
			VirtualStorageType.VendorId = VIRTUAL_STORAGE_TYPE_VENDOR.VIRTUAL_STORAGE_TYPE_VENDOR_MICROSOFT;
			OPEN_VIRTUAL_DISK_PARAMETERS Parameters = default(OPEN_VIRTUAL_DISK_PARAMETERS);
			Parameters.Version = OPEN_VIRTUAL_DISK_VERSION.OPEN_VIRTUAL_DISK_VERSION_1;
			Parameters.RWDepth = 1u;
			int num = VirtualDiskLib.OpenVirtualDisk(ref VirtualStorageType, vhdPath, accessMask, openFlags, ref Parameters, ref Handle);
			if (0 < num)
			{
				throw new Win32Exception(num);
			}
			ATTACH_VIRTUAL_DISK_PARAMETERS Parameters2 = default(ATTACH_VIRTUAL_DISK_PARAMETERS);
			Parameters2.Version = ATTACH_VIRTUAL_DISK_VERSION.ATTACH_VIRTUAL_DISK_VERSION_1;
			num = VirtualDiskLib.AttachVirtualDisk(Handle, IntPtr.Zero, attachFlags, 0u, ref Parameters2, IntPtr.Zero);
			if (0 < num)
			{
				throw new Win32Exception(num);
			}
			return Handle;
		}

		public static void DismountVHD(IntPtr hndlVirtDisk)
		{
			if (!(hndlVirtDisk == IntPtr.Zero))
			{
				int num = VirtualDiskLib.DetachVirtualDisk(hndlVirtDisk, DETACH_VIRTUAL_DISK_FLAG.DETACH_VIRTUAL_DISK_FLAG_NONE, 0u);
				if (0 < num)
				{
					throw new Win32Exception();
				}
				VirtualDiskLib.CloseHandle(hndlVirtDisk);
			}
		}

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		private static extern int IU_MountWim(string WimPath, string MountPath, string TemporaryPath);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		private static extern int IU_DismountWim(string WimPath, string MountPath, int CommitMode);

		public static bool MountWIM(string wimPath, string mountPoint, string tmpDir)
		{
			return IU_MountWim(wimPath, mountPoint, tmpDir) == 0;
		}

		public static bool DismountWIM(string wimPath, string mountPoint, bool commit)
		{
			return IU_DismountWim(wimPath, mountPoint, commit ? 1 : 0) == 0;
		}

		public static string FindInPath(string filename)
		{
			string text = null;
			text = ((!LongPathFile.Exists(Path.Combine(Environment.CurrentDirectory, filename))) ? Environment.GetEnvironmentVariable("PATH").Split(';').FirstOrDefault((string x) => LongPathFile.Exists(Path.Combine(x, filename))) : Environment.CurrentDirectory);
			if (string.IsNullOrEmpty(text))
			{
				throw new FileNotFoundException($"Can't find file '{filename}' anywhere in the %PATH%");
			}
			return Path.Combine(text, filename);
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

		public static int RunProcessVerbose(string command, string args)
		{
			string processOutput = null;
			int result = RunProcess(null, command, args, true, true, out processOutput);
			Console.WriteLine(processOutput);
			return result;
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
