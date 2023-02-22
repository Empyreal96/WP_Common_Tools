using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	internal class SidMapping
	{
		private Dictionary<string, uint> sidDictionary;

		private Dictionary<uint, string> reverseSidDictionary;

		private string sidMappingFilePath;

		public string this[string capabilityId]
		{
			get
			{
				if (sidDictionary != null && sidDictionary.ContainsKey(capabilityId))
				{
					return string.Format(GlobalVariables.Culture, "{0}-{1}", new object[2]
					{
						"S-1-5-21-2702878673-795188819-444038987",
						1031 + sidDictionary[capabilityId] - 1
					});
				}
				if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("POLICY_COMPILER_TEST")))
				{
					return SidBuilder.BuildSidString("S-1-5-21-2702878673-795188819-444038987", HashCalculator.CalculateSha256Hash(capabilityId, true), 8);
				}
				string text = string.Format(GlobalVariables.Culture, "The capability Id {0} is not defined", new object[1] { capabilityId });
				if (!string.IsNullOrEmpty(sidMappingFilePath))
				{
					text = text + " in capability list file " + sidMappingFilePath;
				}
				throw new PolicyCompilerInternalException(text);
			}
		}

		private static SidMapping CreateInstance(StreamReader reader)
		{
			SidMapping sidMapping = new SidMapping();
			sidMapping.sidDictionary = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
			sidMapping.reverseSidDictionary = new Dictionary<uint, string>();
			uint num = 0u;
			uint num2 = 1u;
			char[] separator = new char[1] { ',' };
			while (reader.Peek() >= 0)
			{
				string text = reader.ReadLine();
				num++;
				if (!text.StartsWith(";", GlobalVariables.GlobalStringComparison) && !string.IsNullOrWhiteSpace(text))
				{
					string[] array = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
					if (array.Length > 2 || array.Length < 1)
					{
						throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "Line {0} in capability list file is invalid. The line must start with semicolon for comment OR number, comma(,) and the capability Id.", new object[1] { num }));
					}
					uint num3 = 0u;
					try
					{
						num3 = uint.Parse(array[0], NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, GlobalVariables.Culture);
					}
					catch (FormatException originalException)
					{
						throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "Line {0} in capability list file is invalid. Unable to convert '{1}' to integer.", new object[2]
						{
							num,
							array[0]
						}), originalException);
					}
					catch (OverflowException originalException2)
					{
						throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "Line {0} in capability list file is invalid. '{1}' is out of range of the integer type.", new object[2]
						{
							num,
							array[0]
						}), originalException2);
					}
					if (num3 == 0 || num3 > 1750)
					{
						throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "Line {0} in capability list file is invalid. '{1}' is out of range of 1 to {2}.", new object[3] { num, num3, 1750 }));
					}
					if (num3 != num2)
					{
						throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "Line {0} in capability list file is out of order. The expected index should be {1} instead of {2}.", new object[3] { num, num2, num3 }));
					}
					string text2 = array[1].Trim();
					if (!text2.EndsWith("PLACEHOLDER") && !text2.EndsWith("DONOTUSE") && sidMapping.sidDictionary.ContainsKey(text2))
					{
						throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "Line {0} in capability list file contains duplicate capability {1}. Remove all duplicate capabilities.", new object[2] { num, text2 }));
					}
					if (array.Length == 2)
					{
						sidMapping.sidDictionary[array[1].Trim()] = num2;
						sidMapping.reverseSidDictionary.Add(num2, array[1].Trim());
					}
					num2++;
				}
			}
			return sidMapping;
		}

		public static SidMapping CreateInstance(Stream sidMappingFileStream)
		{
			SidMapping sidMapping = null;
			using (StreamReader reader = new StreamReader(sidMappingFileStream))
			{
				return CreateInstance(reader);
			}
		}

		public static SidMapping CreateInstance(string sidMappingFilePath)
		{
			SidMapping sidMapping = null;
			using (StreamReader reader = new StreamReader(sidMappingFilePath))
			{
				sidMapping = CreateInstance(reader);
				sidMapping.sidMappingFilePath = sidMappingFilePath;
				return sidMapping;
			}
		}

		public static void CompareToSnapshotMapping(SidMapping NewSidMapping)
		{
			string path = Environment.ExpandEnvironmentVariables("%_WINPHONEROOT%\\tools\\oak\\misc\\snapshotcapabilitylist.cfg");
			if (!File.Exists(path))
			{
				return;
			}
			foreach (KeyValuePair<uint, string> item in CreateInstance(path).reverseSidDictionary)
			{
				if (!item.Value.EndsWith("PLACEHOLDER"))
				{
					string value;
					if (!NewSidMapping.reverseSidDictionary.TryGetValue(item.Key, out value))
					{
						throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "Capability list file is missing slot {0}.", new object[1] { item.Key }));
					}
					if (!value.EndsWith("DONOTUSE") && !value.Equals(item.Value))
					{
						throw new PolicyCompilerInternalException(string.Format(GlobalVariables.Culture, "Capability list file cannot reuse slots. Slot {0} was {1} and has been changed to {2}. If you wish to remove a capability rename it ID_CAP_DONOTUSE", new object[3] { item.Key, item.Value, value }));
					}
				}
			}
		}
	}
}
