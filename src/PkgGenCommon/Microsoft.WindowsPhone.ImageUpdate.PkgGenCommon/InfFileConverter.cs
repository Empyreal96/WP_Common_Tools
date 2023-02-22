using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon
{
	public class InfFileConverter
	{
		public class RegKeyValue
		{
			public bool IsMultiSz;

			public RegValue Value;

			public string MultiSzName;

			public string[] MultiSzValue;
		}

		public class RegKeyData
		{
			public string SDDL;

			public List<RegKeyValue> ValueList = new List<RegKeyValue>();
		}

		public delegate void Operation();

		private static class NativeMethods
		{
			[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
			public static extern int ExportRegistryHiveDeltas(string baseHivesPath, string modifiedHivesPath, string outputHivesPath);
		}

		public const string NO_HIVES = "no:hives";

		private static bool _noHives = false;

		private static readonly List<string> RegKeyExclusionListAll = new List<string> { "hkey_local_machine\\system\\controlset001\\control\\grouporderlist", "hkey_local_machine\\system\\controlset001\\services\\wudfrd" };

		private static readonly List<string> RegKeyExclusionListNoHives = new List<string> { "hkey_local_machine\\software\\microsoft\\windows\\currentversion\\setup", "hkey_local_machine\\software\\microsoft\\windows\\currentversion\\setup\\pnplockdownfiles", "hkey_local_machine\\software\\microsoft\\windows\\currentversion\\setup\\pnpresources", "hkey_local_machine\\system\\controlset001\\control\\class", "hkey_local_machine\\system\\driverdatabase", "hkey_local_machine\\system\\driverdatabase\\deviceids", "hkey_local_machine\\system\\driverdatabase\\driverinffiles", "hkey_local_machine\\system\\driverdatabase\\driverpackages" };

		internal const int c_retries = 10;

		internal static readonly TimeSpan c_wait = TimeSpan.FromSeconds(2.0);

		private const string STR_HIVEPATH = "Windows\\System32\\Config";

		private const string c_defaultWimFileName = "mobilecoreprod.wim";

		private static readonly TimeSpan MutexTimeout = new TimeSpan(0, 6, 0, 0);

		public static bool NoHives => _noHives;

		public static void DoConvert(string infFile, string[] references, string[] stagingSubdirs, string hiveRoot, string wimRoot, string targetPartition, CpuId cpu, string diffedHivesPath, string productName, string toolPaths)
		{
			if (string.IsNullOrEmpty(infFile))
			{
				throw new ArgumentNullException("infFile");
			}
			if (diffedHivesPath == null)
			{
				throw new ArgumentNullException("diffedHivesPath");
			}
			if (string.IsNullOrEmpty(hiveRoot))
			{
				throw new ArgumentNullException("hiveRoot");
			}
			if (string.IsNullOrEmpty(targetPartition))
			{
				throw new ArgumentNullException("targetPartition");
			}
			if (string.IsNullOrEmpty(diffedHivesPath))
			{
				throw new ArgumentNullException("diffedHivesPath");
			}
			if (references != null)
			{
				if (stagingSubdirs == null)
				{
					throw new ArgumentNullException("stagingSubdirs");
				}
				if (references.Length != stagingSubdirs.Length)
				{
					throw new ArgumentException("Input parameters 'References' and 'StagingSubDirs' should have same size");
				}
			}
			string baselineHivesPath = FileUtils.GetTempDirectory();
			string mountPath = FileUtils.GetTempDirectory();
			string text = null;
			string defaultDriveLetter = PackageTools.GetDefaultDriveLetter(targetPartition);
			string path = ((!string.IsNullOrEmpty(productName) && !(productName == "$(PRODUCT_NAME)")) ? (productName + ".wim") : "mobilecoreprod.wim");
			string text2 = string.Empty;
			LongPathDirectory.CreateDirectory(mountPath);
			if (hiveRoot.Equals("no:hives"))
			{
				_noHives = true;
			}
			try
			{
				DrvStore driverStore = new DrvStore(mountPath, defaultDriveLetter);
				try
				{
					if (!_noHives && driverStore.DriverIncludesInfs(infFile, cpu))
					{
						text2 = Path.Combine(wimRoot, path);
						if (!LongPathFile.Exists(text2))
						{
							text2 = Path.Combine(hiveRoot, path);
							LogUtil.Warning("WIM_ROOT parameter is needed but no valid path was passed, trying {0}", text2);
							if (File.Exists(text2))
							{
								LogUtil.Diagnostic("WIM located");
							}
							else
							{
								LogUtil.Warning("Unable to locate WIM, falling back to hives");
								text2 = string.Empty;
							}
						}
						if (text2 != string.Empty)
						{
							ApplyWIM(text2, mountPath, toolPaths);
						}
					}
					if (_noHives)
					{
						LogUtil.Diagnostic("Creating driver store with deconstructed state");
						driverStore.Create();
						driverStore.SetupConfigOptions(51u);
						driverStore.Close();
						LogUtil.Diagnostic("Saving hive baselines");
						CopyFromMappings(CreateHiveCopyMappings(driverStore.HivePath, baselineHivesPath));
						driverStore.Create();
						driverStore.SetupConfigOptions(51u);
					}
					else
					{
						driverStore.Create();
						driverStore.Close();
						if (text2 == string.Empty)
						{
							LogUtil.Diagnostic("Building driverstore from hives");
							LogUtil.Diagnostic("Copying hives from HIVE_ROOT into image");
							CopyFromMappings(CreateHiveCopyMappings(hiveRoot, driverStore.HivePath));
						}
						else
						{
							LogUtil.Diagnostic("Using driverstore from WIM");
						}
						LogUtil.Diagnostic("Saving hive baselines");
						CopyFromMappings(CreateHiveCopyMappings(driverStore.HivePath, baselineHivesPath));
						driverStore.Create();
					}
					text = driverStore.ImportLogPath;
					LogUtil.Diagnostic("importLogPath = {0}", text);
					if (!string.IsNullOrEmpty(text))
					{
						RenameLogFile(text);
					}
					driverStore.ImportDriver(infFile, references, stagingSubdirs, cpu);
					driverStore.Close();
					RetryWithWait(delegate
					{
						ComputeHiveDiff(baselineHivesPath, driverStore.HivePath, diffedHivesPath);
					}, 10, c_wait);
				}
				finally
				{
					if (driverStore != null)
					{
						((IDisposable)driverStore).Dispose();
					}
				}
			}
			catch (Win32Exception ex)
			{
				LogUtil.Diagnostic(ex.ToString());
				throw new IUException($"Error encountered staging {infFile}", ex);
			}
			catch (InvalidOperationException ex2)
			{
				LogUtil.Diagnostic(ex2.ToString());
				throw new IUException($"Error encountered staging {infFile}", ex2);
			}
			catch (ArgumentNullException ex3)
			{
				LogUtil.Diagnostic(ex3.ToString());
				throw new IUException($"Error encountered staging {infFile}", ex3);
			}
			finally
			{
				if (!string.IsNullOrEmpty(text))
				{
					LogFile(text, "Import Log: ");
				}
				RetryWithWait(delegate
				{
					FileUtils.DeleteTree(mountPath);
					FileUtils.DeleteTree(baselineHivesPath);
				}, 10, c_wait);
			}
		}

		public static Dictionary<string, RegKeyData> ExtractKeys(string diffedHivesPath, SystemRegistryHiveFiles hive)
		{
			Dictionary<string, RegKeyData> regKeyTable = new Dictionary<string, RegKeyData>(StringComparer.OrdinalIgnoreCase);
			string hivefile = Path.Combine(diffedHivesPath, Enum.GetName(typeof(SystemRegistryHiveFiles), hive));
			string prefix = RegistryUtils.MapHiveToMountPoint(hive);
			using (ORRegistryKey key = ORRegistryKey.OpenHive(hivefile, prefix))
			{
				PopulateRegKeyTable(key, ref regKeyTable);
				return regKeyTable;
			}
		}

		private static void PopulateRegKeyTable(ORRegistryKey key, ref Dictionary<string, RegKeyData> regKeyTable)
		{
			if (RegKeyExclusionListAll.Contains(key.FullName.ToLowerInvariant()))
			{
				return;
			}
			bool flag = false;
			bool flag2 = false;
			if (!regKeyTable.ContainsKey(key.FullName))
			{
				if (NoHives && RegKeyExclusionListNoHives.Contains(key.FullName.ToLowerInvariant()))
				{
					flag = true;
					flag2 = true;
				}
				if (!flag)
				{
					regKeyTable.Add(key.FullName, new RegKeyData());
				}
			}
			if (!flag2)
			{
				List<RegKeyValue> valueList = regKeyTable[key.FullName].ValueList;
				foreach (KeyValuePair<string, RegistryValueType> valueNameAndType in key.ValueNameAndTypes)
				{
					RegKeyValue regKeyValue = new RegKeyValue();
					if (valueNameAndType.Value == RegistryValueType.MultiString)
					{
						regKeyValue.IsMultiSz = true;
						regKeyValue.MultiSzName = valueNameAndType.Key;
						regKeyValue.MultiSzValue = key.GetMultiStringValue(valueNameAndType.Key);
					}
					else
					{
						regKeyValue.IsMultiSz = false;
						regKeyValue.Value = ConvertRegValue(key, valueNameAndType.Key, valueNameAndType.Value);
					}
					valueList.Add(regKeyValue);
				}
			}
			string[] subKeys = key.SubKeys;
			foreach (string subkeyname in subKeys)
			{
				PopulateRegKeyTable(key.OpenSubKey(subkeyname), ref regKeyTable);
			}
		}

		private static RegValue ConvertRegValue(ORRegistryKey key, string valueName, RegistryValueType type)
		{
			RegValue regValue = new RegValue();
			regValue.Name = valueName;
			switch (type)
			{
			case RegistryValueType.String:
				regValue.RegValType = RegValueType.String;
				regValue.Value = key.GetStringValue(valueName);
				break;
			case RegistryValueType.ExpandString:
				regValue.RegValType = RegValueType.ExpandString;
				regValue.Value = key.GetStringValue(valueName);
				break;
			case RegistryValueType.DWord:
				regValue.RegValType = RegValueType.DWord;
				regValue.Value = key.GetDwordValue(valueName).ToString("X8");
				break;
			case RegistryValueType.QWord:
				regValue.RegValType = RegValueType.QWord;
				regValue.Value = key.GetQwordValue(valueName).ToString("X16");
				break;
			case RegistryValueType.Binary:
				regValue.RegValType = RegValueType.Binary;
				regValue.Value = BitConverter.ToString(key.GetByteValue(valueName)).Replace('-', ',');
				break;
			default:
				regValue.RegValType = RegValueType.Hex;
				regValue.Value = $"hex({(int)key.GetValueKind(valueName):X}):" + BitConverter.ToString(key.GetByteValue(valueName)).Replace('-', ',');
				break;
			}
			return regValue;
		}

		public static void RetryWithWait(Operation operation, int retries, TimeSpan wait)
		{
			while (retries > 0)
			{
				try
				{
					operation();
					break;
				}
				catch (Exception ex)
				{
					LogUtil.Diagnostic(ex.ToString());
					retries--;
					if (retries <= 0)
					{
						throw;
					}
					Thread.Sleep(wait);
				}
			}
		}

		public static void LogFile(string file, string prefix)
		{
			if (!LongPathFile.Exists(file))
			{
				return;
			}
			using (StreamReader streamReader = new StreamReader(file))
			{
				while (!streamReader.EndOfStream)
				{
					string text = streamReader.ReadLine();
					if (text.StartsWith("!!!", StringComparison.InvariantCulture))
					{
						LogUtil.Error("{0}: {1}", prefix, text);
					}
					else if (text.StartsWith("!", StringComparison.InvariantCulture))
					{
						LogUtil.Warning("{0}: {1}", prefix, text);
					}
					else
					{
						LogUtil.Message("{0}: {1}", prefix, text);
					}
				}
			}
		}

		public static void RenameLogFile(string file)
		{
			if (LongPathFile.Exists(file))
			{
				try
				{
					LongPathFile.Move(file, file + ".old");
				}
				catch
				{
				}
			}
		}

		public static void ComputeHiveDiff(string baseHivePath, string newHivePath, string diffHivePath)
		{
			LogUtil.Diagnostic("Computing difference hives between {0} and {1} and exporting to {2}", baseHivePath, newHivePath, diffHivePath);
			int num = NativeMethods.ExportRegistryHiveDeltas(baseHivePath, newHivePath, diffHivePath);
			if (num != 0)
			{
				throw new Win32Exception(num);
			}
			ComputeMultiSZDiff(baseHivePath, diffHivePath);
		}

		private static void ComputeMultiSZDiff(string baseHivePath, string diffHivePath)
		{
			string[] files = LongPathDirectory.GetFiles(diffHivePath);
			foreach (string text in files)
			{
				string text2 = Path.Combine(baseHivePath, Path.GetFileName(text));
				if (!LongPathFile.Exists(text2))
				{
					continue;
				}
				using (ORRegistryKey oRRegistryKey = ORRegistryKey.OpenHive(text))
				{
					using (ORRegistryKey baseKey = ORRegistryKey.OpenHive(text2))
					{
						string fullPath = LongPath.GetFullPath(text);
						try
						{
							RemoveMultiSZDuplicates(oRRegistryKey, baseKey);
						}
						catch (Exception innerException)
						{
							throw new Exception($"Error computing Multi_SZ diff in {fullPath}.", innerException);
						}
						oRRegistryKey.SaveHive(fullPath);
					}
				}
			}
		}

		private static void RemoveMultiSZDuplicates(ORRegistryKey diffKey, ORRegistryKey baseKey)
		{
			foreach (string item in diffKey.SubKeys.Intersect(baseKey.SubKeys, StringComparer.OrdinalIgnoreCase))
			{
				using (ORRegistryKey diffKey2 = diffKey.OpenSubKey(item))
				{
					using (ORRegistryKey baseKey2 = baseKey.OpenSubKey(item))
					{
						RemoveMultiSZDuplicates(diffKey2, baseKey2);
					}
				}
			}
			foreach (string item2 in diffKey.ValueNames.Intersect(baseKey.ValueNames, StringComparer.OrdinalIgnoreCase))
			{
				if (diffKey.GetValueKind(item2) == RegistryValueType.MultiString && baseKey.GetValueKind(item2) == RegistryValueType.MultiString)
				{
					string[] multiStringValue = diffKey.GetMultiStringValue(item2);
					string[] multiStringValue2 = baseKey.GetMultiStringValue(item2);
					if (multiStringValue2.Except(multiStringValue, StringComparer.OrdinalIgnoreCase).Any())
					{
						throw new PkgGenException("Multi_SZ elements were removed during driver ingestion in {0}\\{1}", diffKey.FullName, item2);
					}
					string[] values = multiStringValue.Except(multiStringValue2, StringComparer.OrdinalIgnoreCase).ToArray();
					diffKey.SetValue(item2, values);
				}
			}
		}

		public static Dictionary<string, string> CreateHiveCopyMappings(string srcHivePath, string dstHivePath)
		{
			string[] obj = new string[3] { "SYSTEM", "DRIVERS", "SOFTWARE" };
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string[] array = obj;
			foreach (string path in array)
			{
				dictionary.Add(Path.Combine(srcHivePath, path), Path.Combine(dstHivePath, path));
			}
			return dictionary;
		}

		public static void CopyFromMappings(Dictionary<string, string> mappings)
		{
			foreach (string key in mappings.Keys)
			{
				string text = mappings[key];
				LogUtil.Diagnostic("Copying {0} to {1}", key, text);
				try
				{
					LongPathFile.Copy(key, text, true);
				}
				catch (FileNotFoundException)
				{
					if (!text.Contains("SOFTWARE"))
					{
						LogUtil.Error("Failed to copy {0}", key);
						throw;
					}
				}
			}
		}

		public static void ApplyWIM(string inputWIM, string targetDirectory, string toolPaths)
		{
			LogUtil.Diagnostic("Applying WIM {0} to directory {1}", inputWIM, targetDirectory);
			Process process = new Process();
			process.StartInfo.FileName = LocateUtil("dism.exe", toolPaths);
			process.StartInfo.Arguments = $"/apply-image /imagefile=\"{inputWIM}\" /Index:1 /applydir=\"{targetDirectory}\"";
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			string text = string.Empty;
			short num = 5;
			while (num > 0)
			{
				process.Start();
				text = process.StandardOutput.ReadToEnd();
				process.WaitForExit();
				if (process.ExitCode == 0)
				{
					num = 0;
					continue;
				}
				num = (short)(num - 1);
				Thread.Sleep(2000);
			}
			if (process.ExitCode != 0)
			{
				LogUtil.Error(text);
				throw new IUException("Failed to apply {0}, exit code {1}", inputWIM, process.ExitCode);
			}
			LogUtil.Message(text);
		}

		private static string LocateUtil(string razzleCmd, string toolPaths)
		{
			if (LongPathFile.Exists(Path.Combine(Directory.GetCurrentDirectory(), razzleCmd)))
			{
				return Path.Combine(Directory.GetCurrentDirectory(), razzleCmd);
			}
			if (!string.IsNullOrWhiteSpace(toolPaths))
			{
				string[] array = toolPaths.Split(';');
				for (int i = 0; i < array.Length; i++)
				{
					string text = Path.Combine(Environment.ExpandEnvironmentVariables(array[i]), razzleCmd);
					if (LongPathFile.Exists(text))
					{
						return text;
					}
				}
			}
			string environmentVariable = Environment.GetEnvironmentVariable("PATH");
			string text2 = string.Empty;
			if (!string.IsNullOrEmpty(environmentVariable))
			{
				text2 = (from p in Environment.GetEnvironmentVariable("PATH").Split(';')
					where LongPathFile.Exists(Path.Combine(p, razzleCmd))
					select p).FirstOrDefault();
			}
			if (string.IsNullOrEmpty(text2))
			{
				throw new FileNotFoundException("Could not find {0} in the environment path", razzleCmd);
			}
			return Path.Combine(text2, razzleCmd);
		}
	}
}
