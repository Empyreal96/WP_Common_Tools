using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;

namespace Microsoft.WindowsPhone.MCSF.Offline
{
	public class PolicyStore
	{
		private const string PolicyDocumentDeviceRoot = "\\Windows\\CustomizationPolicy\\";

		private List<PolicyGroup> settingGroups;

		public IEnumerable<PolicyGroup> SettingGroups => settingGroups;

		public PolicyStore()
		{
			settingGroups = new List<PolicyGroup>();
		}

		public void LoadPolicyXML(string policyDocumentPath)
		{
			LoadPolicyXML(policyDocumentPath, null, null);
		}

		public void LoadPolicyXML(string policyDocumentPath, string definedInOverride)
		{
			LoadPolicyXML(policyDocumentPath, definedInOverride, null);
		}

		public void LoadPolicyXML(string policyDocumentPath, string definedInOverride, string partition)
		{
			string text = (string.IsNullOrEmpty(definedInOverride) ? policyDocumentPath : definedInOverride);
			try
			{
				XDocument policyDocument = XDocument.Load(policyDocumentPath);
				LoadPolicyXML(policyDocument, text, partition);
			}
			catch (MCSFOfflineException ex)
			{
				throw ex;
			}
			catch (Exception ex2)
			{
				throw new ArgumentException($"Unable to load policy from file '{text}': {ex2.Message}", ex2);
			}
		}

		public void LoadPolicyXML(XDocument policyDocument)
		{
			LoadPolicyXML(policyDocument, null, null);
		}

		public void LoadPolicyXML(XDocument policyDocument, string definedInFile)
		{
			LoadPolicyXML(policyDocument, definedInFile, null);
		}

		public void LoadPolicyXML(XDocument policyDocument, string definedInFile, string partition)
		{
			foreach (XElement item2 in policyDocument.Root.Elements())
			{
				PolicyGroup item = new PolicyGroup(item2, definedInFile, partition);
				settingGroups.Add(item);
			}
		}

		public void LoadPolicyXML(IEnumerable<XDocument> policyDocuments)
		{
			foreach (XDocument policyDocument in policyDocuments)
			{
				LoadPolicyXML(policyDocument);
			}
		}

		public void LoadPolicyXML(IEnumerable<string> policyDocumentPaths)
		{
			foreach (string policyDocumentPath in policyDocumentPaths)
			{
				LoadPolicyXML(policyDocumentPath);
			}
		}

		public void LoadPolicyFromPackage(IPkgInfo policyPackage)
		{
			foreach (IFileEntry file in policyPackage.Files)
			{
				if (file.DevicePath.StartsWith("\\Windows\\CustomizationPolicy\\", StringComparison.InvariantCultureIgnoreCase))
				{
					string text = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
					policyPackage.ExtractFile(file.DevicePath, text, true);
					try
					{
						LoadPolicyXML(text, policyPackage.Name, policyPackage.Partition);
					}
					catch (MCSFOfflineException ex)
					{
						throw ex;
					}
					catch (Exception ex2)
					{
						throw new ArgumentException($"Failed to load policy from file '{file.DevicePath}' inside of the given package:{ex2.Message}", ex2);
					}
					try
					{
						File.Delete(text);
					}
					catch (Exception)
					{
					}
				}
			}
		}

		public void LoadPolicyFromPackage(string policyPackagePath)
		{
			try
			{
				LoadPolicyFromPackage(Package.LoadFromCab(policyPackagePath));
			}
			catch (MCSFOfflineException ex)
			{
				throw ex;
			}
			catch (Exception ex2)
			{
				throw new ArgumentException($"Failed to load policy from package '{policyPackagePath}':{ex2.Message}", ex2);
			}
		}

		public void LoadPolicyFromPackages(IEnumerable<IPkgInfo> policyPackages)
		{
			foreach (IPkgInfo policyPackage in policyPackages)
			{
				LoadPolicyFromPackage(policyPackage);
			}
		}

		public void LoadPolicyFromPackages(IEnumerable<string> policyPackagePaths)
		{
			foreach (string policyPackagePath in policyPackagePaths)
			{
				LoadPolicyFromPackage(policyPackagePath);
			}
		}

		public PolicyGroup SettingGroupByPath(string settingPath)
		{
			IEnumerable<PolicyGroup> enumerable = settingGroups.Where((PolicyGroup x) => PolicyMacroTable.IsMatch(x.Path, settingPath, StringComparison.OrdinalIgnoreCase));
			if (enumerable.Count() > 1)
			{
				enumerable = enumerable.Where((PolicyGroup x) => x.Path.Equals(settingPath, StringComparison.OrdinalIgnoreCase));
			}
			if (enumerable.Count() > 1)
			{
				string text = "";
				foreach (PolicyGroup item in enumerable)
				{
					if (!string.IsNullOrWhiteSpace(text))
					{
						text += ", ";
					}
					text += item.DefinedIn;
				}
				throw new MCSFOfflineException(string.Format(CultureInfo.InvariantCulture, "Multiple definitions when searching for setting group '{0}', defined in '{1}'.", new object[2] { settingPath, text }));
			}
			return enumerable.SingleOrDefault();
		}

		public PolicySetting SettingByPathAndName(string settingPath, string settingName)
		{
			return SettingGroupByPath(settingPath)?.SettingByName(settingName);
		}

		public PolicyAssetInfo AssetByPathAndName(string settingPath, string assetName)
		{
			return SettingGroupByPath(settingPath)?.AssetByName(assetName);
		}
	}
}
