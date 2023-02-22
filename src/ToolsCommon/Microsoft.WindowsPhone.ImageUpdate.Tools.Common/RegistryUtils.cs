using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public static class RegistryUtils
	{
		private static Dictionary<string, SystemRegistryHiveFiles> hiveMap = new Dictionary<string, SystemRegistryHiveFiles>(StringComparer.InvariantCultureIgnoreCase)
		{
			{
				"SOFTWARE",
				SystemRegistryHiveFiles.SOFTWARE
			},
			{
				"SYSTEM",
				SystemRegistryHiveFiles.SYSTEM
			},
			{
				"DRIVERS",
				SystemRegistryHiveFiles.DRIVERS
			},
			{
				"DEFAULT",
				SystemRegistryHiveFiles.DEFAULT
			},
			{
				"SAM",
				SystemRegistryHiveFiles.SAM
			},
			{
				"SECURITY",
				SystemRegistryHiveFiles.SECURITY
			},
			{
				"BCD",
				SystemRegistryHiveFiles.BCD
			},
			{
				"NTUSER.DAT",
				SystemRegistryHiveFiles.CURRENTUSER
			}
		};

		private const string STR_REG_LOAD = "LOAD {0} {1}";

		private const string STR_REG_EXPORT = "EXPORT {0} {1}";

		private const string STR_REG_UNLOAD = "UNLOAD {0}";

		private const string STR_REGEXE = "%windir%\\System32\\REG.EXE";

		private static readonly Dictionary<SystemRegistryHiveFiles, string> MountPoints = new Dictionary<SystemRegistryHiveFiles, string>
		{
			{
				SystemRegistryHiveFiles.SOFTWARE,
				"HKEY_LOCAL_MACHINE\\SOFTWARE"
			},
			{
				SystemRegistryHiveFiles.SYSTEM,
				"HKEY_LOCAL_MACHINE\\SYSTEM"
			},
			{
				SystemRegistryHiveFiles.DRIVERS,
				"HKEY_LOCAL_MACHINE\\DRIVERS"
			},
			{
				SystemRegistryHiveFiles.DEFAULT,
				"HKEY_USERS\\.DEFAULT"
			},
			{
				SystemRegistryHiveFiles.SAM,
				"HKEY_LOCAL_MACHINE\\SAM"
			},
			{
				SystemRegistryHiveFiles.SECURITY,
				"HKEY_LOCAL_MACHINE\\SECURITY"
			},
			{
				SystemRegistryHiveFiles.BCD,
				"HKEY_LOCAL_MACHINE\\BCD"
			},
			{
				SystemRegistryHiveFiles.CURRENTUSER,
				"HKEY_CURRENT_USER"
			}
		};

		public static Dictionary<SystemRegistryHiveFiles, string> KnownMountPoints => MountPoints;

		public static void ConvertSystemHiveToRegFile(DriveInfo systemDrive, SystemRegistryHiveFiles hive, string outputRegFile)
		{
			LongPathDirectory.CreateDirectory(LongPath.GetDirectoryName(outputRegFile));
			ConvertHiveToRegFile(Path.Combine(Path.Combine(systemDrive.RootDirectory.FullName, "windows\\system32\\config"), Enum.GetName(typeof(SystemRegistryHiveFiles), hive)), MapHiveToMountPoint(hive), outputRegFile);
		}

		public static void ConvertHiveToRegFile(string inputhive, string targetRootKey, string outputRegFile)
		{
			OfflineRegUtils.ConvertHiveToReg(inputhive, outputRegFile, targetRootKey);
		}

		public static void LoadHive(string inputhive, string mountpoint)
		{
			string args = $"LOAD {mountpoint} {inputhive}";
			string command = Environment.ExpandEnvironmentVariables("%windir%\\System32\\REG.EXE");
			int num = CommonUtils.RunProcess(Environment.CurrentDirectory, command, args, true);
			if (0 < num)
			{
				throw new Win32Exception(num);
			}
			Thread.Sleep(500);
		}

		public static void ExportHive(string mountpoint, string outputfile)
		{
			string args = $"EXPORT {mountpoint} {outputfile}";
			string command = Environment.ExpandEnvironmentVariables("%windir%\\System32\\REG.EXE");
			int num = CommonUtils.RunProcess(Environment.CurrentDirectory, command, args, true);
			if (0 < num)
			{
				throw new Win32Exception(num);
			}
			Thread.Sleep(500);
		}

		public static void UnloadHive(string mountpoint)
		{
			string args = $"UNLOAD {mountpoint}";
			string command = Environment.ExpandEnvironmentVariables("%windir%\\System32\\REG.EXE");
			int num = CommonUtils.RunProcess(Environment.CurrentDirectory, command, args, true);
			if (0 < num)
			{
				throw new Win32Exception(num);
			}
		}

		public static string MapHiveToMountPoint(SystemRegistryHiveFiles hive)
		{
			return KnownMountPoints[hive];
		}

		public static string MapHiveFileToMountPoint(string hiveFile)
		{
			if (string.IsNullOrEmpty(hiveFile))
			{
				throw new InvalidOperationException("hiveFile cannot be empty");
			}
			SystemRegistryHiveFiles value;
			if (!hiveMap.TryGetValue(Path.GetFileName(hiveFile), out value))
			{
				return "";
			}
			return MapHiveToMountPoint(value);
		}
	}
}
