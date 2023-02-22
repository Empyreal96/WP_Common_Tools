using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Multivariant.Offline
{
	internal class RegFileHandler
	{
		internal class RegKeyInfoTable
		{
			public readonly string regFilename;

			public readonly string partition;

			public List<RegValueInfo> regValueInfoList;

			public RegKeyInfoTable(string partitionName, string tempDirectory)
			{
				partition = partitionName;
				regFilename = FileUtils.GetTempFile(tempDirectory);
				regValueInfoList = new List<RegValueInfo>();
			}
		}

		private Stream cfgXml;

		private string tempDirectory;

		private static MacroResolver s_macroResolver;

		private Dictionary<string, RegKeyInfoTable> regKeyTables = new Dictionary<string, RegKeyInfoTable>(StringComparer.OrdinalIgnoreCase);

		private static string GetExpandedRegKeyName(string strKeyName, Stream cfgXml)
		{
			if (s_macroResolver == null)
			{
				s_macroResolver = new MacroResolver();
				using (XmlReader macroDefinitionReader = XmlReader.Create(cfgXml))
				{
					s_macroResolver.Load(macroDefinitionReader);
				}
			}
			string text = s_macroResolver.Resolve(strKeyName, MacroResolveOptions.ErrorOnUnknownMacro);
			if (string.Empty == text)
			{
				throw new XmlException("Unexpected registry key name:" + strKeyName + ". Please check that you are using the correct macro.");
			}
			return text;
		}

		public RegFileHandler(string tempDir, Stream cfgXml)
		{
			tempDirectory = tempDir;
			this.cfgXml = cfgXml;
		}

		public void AddRegValue(RegValueInfo regValueInfo)
		{
			regValueInfo.KeyName = GetExpandedRegKeyName(regValueInfo.KeyName, cfgXml);
			string text = regValueInfo.Partition;
			if (string.IsNullOrEmpty(text))
			{
				text = PkgConstants.c_strMainOsPartition;
			}
			RegKeyInfoTable value = null;
			if (!regKeyTables.TryGetValue(text, out value))
			{
				value = new RegKeyInfoTable(text, tempDirectory);
				regKeyTables.Add(text, value);
			}
			value.regValueInfoList.Add(regValueInfo);
		}

		public List<RegFilePartitionInfo> Write()
		{
			try
			{
				foreach (RegKeyInfoTable value in regKeyTables.Values)
				{
					List<RegValueInfo> regValueInfoList = value.regValueInfoList;
					foreach (RegValueInfo item in regValueInfoList)
					{
						if (item.Type == RegValueType.Binary)
						{
							byte[] data = Convert.FromBase64String(item.Value);
							StringBuilder stringBuilder = new StringBuilder();
							RegUtil.ByteArrayToRegString(stringBuilder, data);
							item.Value = stringBuilder.ToString();
						}
					}
					RegFileWriter.Write(regValueInfoList, value.regFilename);
				}
				return regKeyTables.Values.Select((RegKeyInfoTable x) => new RegFilePartitionInfo(x.regFilename, x.partition)).ToList();
			}
			catch
			{
				Delete();
				throw;
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
