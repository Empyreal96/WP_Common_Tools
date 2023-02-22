using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.OemCustomizationTool
{
	internal class RegFileHandler
	{
		internal class RegKeyInfoTable
		{
			public readonly string regFilename;

			public readonly string partition;

			public List<RegValueInfo> regValueInfoList;

			public RegKeyInfoTable(string partitionName)
			{
				partition = partitionName;
				regFilename = FileUtils.GetTempFile(Settings.TempDirectoryPath);
				TraceLogger.LogMessage(TraceLevel.Info, $"{partition} reg file path is: {regFilename}");
				regValueInfoList = new List<RegValueInfo>();
			}
		}

		private static string[,] regKeyToPartionMapping = new string[2, 2]
		{
			{
				"HKEY_LOCAL_MACHINE\\BCD",
				Settings.PackageAttributes.efiPartitionString
			},
			{
				string.Empty,
				Settings.PackageAttributes.mainOSPartitionString
			}
		};

		private static MacroResolver s_macroResolver = null;

		private Dictionary<string, RegKeyInfoTable> regKeyTables = new Dictionary<string, RegKeyInfoTable>();

		private static string FindPartitionFromKeyName(string keyName)
		{
			for (int i = 0; i < regKeyToPartionMapping.GetLength(0); i++)
			{
				if (keyName.StartsWith(regKeyToPartionMapping[i, 0], StringComparison.OrdinalIgnoreCase))
				{
					return regKeyToPartionMapping[i, 1];
				}
			}
			throw new Exception("Should never reach this");
		}

		private static string GetExpandedRegKeyName(string strKeyName)
		{
			if (s_macroResolver == null)
			{
				s_macroResolver = new MacroResolver();
				using (XmlReader macroDefinitionReader = XmlReader.Create(Settings.PkgGenCfgXml))
				{
					s_macroResolver.Load(macroDefinitionReader);
				}
			}
			string text = s_macroResolver.Resolve(strKeyName, MacroResolveOptions.ErrorOnUnknownMacro);
			if (string.Empty == text)
			{
				throw new ConfigXmlException("Unexpected registry key name:" + strKeyName + ". Please check that you are using the correct macro.");
			}
			return text;
		}

		public void AddRegValue(RegValueInfo regValueInfo, string partitionName)
		{
			regValueInfo.KeyName = GetExpandedRegKeyName(regValueInfo.KeyName);
			if (string.IsNullOrEmpty(partitionName))
			{
				partitionName = FindPartitionFromKeyName(regValueInfo.KeyName);
			}
			RegKeyInfoTable value = null;
			if (!regKeyTables.TryGetValue(partitionName, out value))
			{
				value = new RegKeyInfoTable(partitionName);
				regKeyTables.Add(partitionName, value);
			}
			TraceLogger.LogMessage(TraceLevel.Info, $"Added RegKey. KeyName={regValueInfo.KeyName}, using file={value.regFilename}, partition={value.partition}");
			TraceLogger.LogMessage(TraceLevel.Info, $"Added RegValue. Name={regValueInfo.ValueName}, val={regValueInfo.Value}, type={regValueInfo.Type}");
			value.regValueInfoList.Add(regValueInfo);
		}

		public List<RegFilePartitionInfo> Write()
		{
			try
			{
				foreach (RegKeyInfoTable value in regKeyTables.Values)
				{
					RegFileWriter.Write(value.regValueInfoList, value.regFilename);
				}
				return regKeyTables.Values.Select((RegKeyInfoTable x) => new RegFilePartitionInfo(x.regFilename, x.partition)).ToList();
			}
			catch (Exception ex)
			{
				TraceLogger.LogMessage(TraceLevel.Error, "Exception: " + Environment.NewLine + ex.ToString());
				Delete();
				return null;
			}
		}

		public void Delete()
		{
			foreach (RegKeyInfoTable value in regKeyTables.Values)
			{
				File.Delete(value.regFilename);
			}
		}
	}
}
