using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.MCSF.Offline;
using Microsoft.WindowsPhone.Multivariant.Offline;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization
{
	public static class CustomContentGenerator
	{
		private static string ProvPKGDevicePath = "\\ProgramData\\Microsoft\\Provisioning";

		public static CustomContent GenerateCustomContent(Customizations customizationInput)
		{
			bool flag = !string.IsNullOrWhiteSpace(customizationInput.CustomizationXMLFilePath);
			bool flag2 = !string.IsNullOrWhiteSpace(customizationInput.CustomizationPPKGFilePath);
			if (!flag && !flag2)
			{
				throw new ArgumentException("CustomizationXMLFilePath or CustomizationPPKGFilePath must be set");
			}
			if (flag)
			{
				if (!File.Exists(customizationInput.CustomizationXMLFilePath))
				{
					throw new ArgumentException("CustomizationXMLFilePath points to a file that does not exist");
				}
				if (!string.Equals(Path.GetExtension(customizationInput.CustomizationXMLFilePath), ".xml", StringComparison.OrdinalIgnoreCase))
				{
					throw new ArgumentException("CustomizationXMLFilePath points to a file that does not have the correct extension (.xml)");
				}
			}
			if (flag2)
			{
				if (!File.Exists(customizationInput.CustomizationPPKGFilePath))
				{
					throw new ArgumentException("CustomizationPPKGFilePath points to a file that does not exist");
				}
				if (!string.Equals(Path.GetExtension(customizationInput.CustomizationPPKGFilePath), ".ppkg", StringComparison.OrdinalIgnoreCase))
				{
					throw new ArgumentException("CustomizationPPKGFilePath points to a file that does not have the correct extension (.ppkg)");
				}
			}
			if (string.IsNullOrWhiteSpace(customizationInput.OutputDirectory))
			{
				throw new ArgumentException("OutputDirectory must be set");
			}
			if (!Directory.Exists(customizationInput.OutputDirectory))
			{
				throw new ArgumentException("OutputDirectory points to a location that does not exist");
			}
			CustomContent response = new CustomContent();
			List<CustomizationError> list = new List<CustomizationError>();
			PolicyStore policyStore = new PolicyStore();
			try
			{
				policyStore.LoadPolicyFromPackages(customizationInput.ImagePackages);
			}
			catch (MCSFOfflineException ex)
			{
				list.Add(new CustomizationError(CustomizationErrorSeverity.Error, null, ex.Message));
				response.CustomizationErrors = list;
				return response;
			}
			response.CustomizationErrors = new List<CustomizationError>();
			List<string> list2 = new List<string>();
			if (flag2)
			{
				list2.AddRange(ProcessCustomizationPPKG(customizationInput, policyStore, ref response));
				if (response.CustomizationErrors.Any((CustomizationError x) => x.Severity == CustomizationErrorSeverity.Error))
				{
					return response;
				}
			}
			if (flag)
			{
				List<CustomizationError> loadErrors;
				ImageCustomizations customizations = LoadCustomizations(customizationInput.CustomizationXMLFilePath, policyStore, out loadErrors);
				response.CustomizationErrors = response.CustomizationErrors.Concat(loadErrors);
				if (response.CustomizationErrors.Any((CustomizationError x) => x.Severity == CustomizationErrorSeverity.Error))
				{
					return response;
				}
				try
				{
					list2.AddRange(GeneratePackages(customizationInput, customizations, policyStore));
					response.DataContent = GenerateDataPartitionContent(customizations);
				}
				catch (CustomizationException ex2)
				{
					CustomizationError source = new CustomizationError(CustomizationErrorSeverity.Error, null, ex2.ToString());
					response.CustomizationErrors = response.CustomizationErrors.Concat(source.ToEnumerable());
				}
			}
			response.PackagePaths = list2;
			return response;
		}

		private static ImageCustomizations LoadCustomizations(string customizationFilePath, PolicyStore policyStore, out List<CustomizationError> loadErrors)
		{
			loadErrors = new List<CustomizationError>();
			ImageCustomizations imageCustomizations = ImageCustomizations.LoadFromPath(customizationFilePath);
			IEnumerable<CustomizationError> errors;
			imageCustomizations = imageCustomizations.GetMergedCustomizations(out errors);
			loadErrors.AddRange(errors);
			if (errors.Where((CustomizationError x) => x.Severity == CustomizationErrorSeverity.Error).Count() > 0)
			{
				return null;
			}
			errors = VerifyCustomizations(imageCustomizations, policyStore);
			loadErrors.AddRange(errors);
			if (errors.Where((CustomizationError x) => x.Severity == CustomizationErrorSeverity.Error).Count() > 0)
			{
				return null;
			}
			return imageCustomizations;
		}

		private static IEnumerable<string> ProcessCustomizationPPKG(Customizations customizationInput, PolicyStore policyStore, ref CustomContent response)
		{
			IEnumerable<string> result = null;
			try
			{
				string customizationPPKGFilePath = customizationInput.CustomizationPPKGFilePath;
				if (response.CustomizationErrors.Where((CustomizationError x) => x.Severity == CustomizationErrorSeverity.Error).Count() > 0)
				{
					return result;
				}
				response.DataContent = new List<KeyValuePair<string, string>>();
				if (response.CustomizationErrors.Where((CustomizationError x) => x.Severity == CustomizationErrorSeverity.Error).Count() > 0)
				{
					return result;
				}
				string owner = "OEM";
				OwnerType ownerType = OwnerType.OEM;
				result = GeneratePPKGPackage(customizationPPKGFilePath, customizationInput, owner, ownerType, ref response);
				if (response.CustomizationErrors.Where((CustomizationError x) => x.Severity == CustomizationErrorSeverity.Error).Count() > 0)
				{
					return result;
				}
				return result;
			}
			catch (CustomizationException ex)
			{
				CustomizationError source = new CustomizationError(CustomizationErrorSeverity.Error, null, ex.ToString());
				response.CustomizationErrors = response.CustomizationErrors.Concat(source.ToEnumerable());
				return result;
			}
		}

		private static IEnumerable<string> GeneratePPKGPackage(string outputPPKGFile, Customizations config, string owner, OwnerType ownerType, ref CustomContent response)
		{
			List<string> list = new List<string>();
			string destination = Path.Combine(ProvPKGDevicePath, Path.GetFileName(config.CustomizationPPKGFilePath));
			list.Add(new CustomizationPackage(PkgConstants.c_strMainOsPartition)
			{
				Component = config.ImageDeviceName,
				SubComponent = "PPKG",
				Owner = owner,
				OwnerType = ownerType,
				CpuType = config.ImageCpuType,
				BuildType = config.ImageBuildType,
				Version = config.ImageVersion,
				Files = 
				{
					new CustomizationFile(outputPPKGFile, destination)
				}
			}.SavePackage(config.OutputDirectory));
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyCustomizations(ImageCustomizations customizations, IEnumerable<IPkgInfo> packages)
		{
			PolicyStore policyStore = new PolicyStore();
			try
			{
				policyStore.LoadPolicyFromPackages(packages);
			}
			catch (MCSFOfflineException ex)
			{
				return new List<CustomizationError>
				{
					new CustomizationError(CustomizationErrorSeverity.Error, null, ex.Message)
				};
			}
			return VerifyCustomizations(customizations, policyStore);
		}

		public static IEnumerable<CustomizationError> VerifyCustomizations(ImageCustomizations customizations, PolicyStore policyStore)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			IEnumerable<CustomizationError> collection = VerifyTargets(customizations.Targets, customizations);
			list.AddRange(collection);
			if (customizations.StaticVariant != null)
			{
				collection = VerifyApplicationsGroup(customizations.StaticVariant.ApplicationGroups, customizations.StaticVariant);
				list.AddRange(collection);
				collection = VerifySettingGroups(customizations.StaticVariant.SettingGroups, customizations.StaticVariant, customizations, policyStore);
				list.AddRange(collection);
				collection = VerifyDataAssetGroups(customizations.StaticVariant.DataAssetGroups, customizations.StaticVariant);
				list.AddRange(collection);
			}
			foreach (Variant variant in customizations.Variants)
			{
				collection = VerifyTargetRefs(variant.TargetRefs, customizations.Targets, variant);
				list.AddRange(collection);
				collection = VerifyApplicationsGroup(variant.ApplicationGroups, variant);
				list.AddRange(collection);
				collection = VerifySettingGroups(variant.SettingGroups, variant, customizations, policyStore);
				list.AddRange(collection);
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyTargets(List<Target> targets, ImageCustomizations parent)
		{
			return VerifyTargets(targets, parent, true);
		}

		public static IEnumerable<CustomizationError> VerifyTargets(List<Target> targets, ImageCustomizations parent, bool verifyChildren)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (verifyChildren)
			{
				foreach (Target target in targets)
				{
					list.AddRange(VerifyTarget(target, parent));
				}
				return list;
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyTarget(Target target, ImageCustomizations customizations)
		{
			return VerifyTarget(target, customizations, true);
		}

		public static IEnumerable<CustomizationError> VerifyTarget(Target target, ImageCustomizations customizations, bool verifyChildren)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			list.AddRange(VerifyTargetId(target));
			list.AddRange(VerifyTargetList(target));
			if (verifyChildren)
			{
				foreach (TargetState targetState in target.TargetStates)
				{
					list.AddRange(VerifyConditions(targetState, target, customizations));
				}
				return list;
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyImportSource(Import import)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			list.AddRange(VerifyExpandedPath(import.Source, import.ToEnumerable(), "Import source"));
			return list;
		}

		private static IEnumerable<string> GeneratePackages(Customizations config, ImageCustomizations customizations, PolicyStore policyStore)
		{
			List<string> list = new List<string>();
			string tempDirectory = FileUtils.GetTempDirectory();
			try
			{
				MVDatastore mVDatastore = new MVDatastore();
				MVDatastore mVDatastore2 = new MVDatastore();
				if (customizations.StaticVariant != null)
				{
					MVVariant item = GenerateGroupForVariant(customizations.StaticVariant, customizations, policyStore);
					mVDatastore.Variants.Add(item);
				}
				foreach (Variant item4 in (IEnumerable<Variant>)customizations.Variants)
				{
					MVVariant item2 = GenerateGroupForVariant(item4, customizations, policyStore);
					mVDatastore2.Variants.Add(item2);
				}
				string text = Path.Combine(tempDirectory, "StaticDatastore");
				mVDatastore.SaveStaticDatastore(text);
				string text2 = Path.Combine(tempDirectory, "MVDatastore");
				string text3 = Path.Combine(tempDirectory, "Provisioning");
				string text4 = Path.Combine(tempDirectory, "CriticalSettings");
				mVDatastore2.SaveDatastore(text2, text3, text4);
				string shadowRegRoot = Path.Combine(tempDirectory, "MVShadowing");
				bool provisionCab = Directory.EnumerateFiles(text3).Any();
				bool criticalCab = Directory.EnumerateFiles(text4).Any();
				IEnumerable<RegValueInfo> defaultDatastoreRegistration = MVDatastore.GetDefaultDatastoreRegistration(provisionCab, criticalCab);
				HashSet<RegFilePartitionInfo> hashSet = new HashSet<RegFilePartitionInfo>(mVDatastore.SaveShadowRegistry(shadowRegRoot, defaultDatastoreRegistration));
				HashSet<RegFilePartitionInfo> hashSet2 = new HashSet<RegFilePartitionInfo>(hashSet.Where((RegFilePartitionInfo r) => r.partition.Equals(PkgConstants.c_strMainOsPartition, StringComparison.OrdinalIgnoreCase)));
				string text5 = GenerateMainOSPackage(config, customizations, policyStore, hashSet2, text, text2, text3, text4);
				if (text5 != null)
				{
					list.Add(text5);
				}
				text5 = GenerateVariantAppsPackage(config, customizations, policyStore, PkgConstants.c_strDataPartition);
				if (text5 != null)
				{
					list.Add(text5);
				}
				text5 = GenerateVariantAppsPackage(config, customizations, policyStore, PkgConstants.c_strMainOsPartition);
				if (text5 != null)
				{
					list.Add(text5);
				}
				foreach (RegFilePartitionInfo item5 in hashSet.Except(hashSet2))
				{
					CustomizationPackage customizationPackage = new CustomizationPackage(item5.partition);
					customizationPackage.Component = config.ImageDeviceName;
					customizationPackage.Owner = customizations.Owner;
					customizationPackage.OwnerType = customizations.OwnerType;
					customizationPackage.CpuType = config.ImageCpuType;
					customizationPackage.BuildType = config.ImageBuildType;
					customizationPackage.Version = config.ImageVersion;
					customizationPackage.AddFile(FileType.Registry, item5.regFilename, CustomizationPackage.ShadowRegFilePath);
					string item3 = customizationPackage.SavePackage(config.OutputDirectory);
					list.Add(item3);
				}
				text5 = GenerateStaticAppsPackage(config, customizations, policyStore, PkgConstants.c_strDataPartition);
				if (text5 != null)
				{
					list.Add(text5);
				}
				text5 = GenerateStaticAppsPackage(config, customizations, policyStore, PkgConstants.c_strMainOsPartition);
				if (text5 != null)
				{
					list.Add(text5);
					return list;
				}
				return list;
			}
			finally
			{
				FileUtils.DeleteTree(tempDirectory);
			}
		}

		private static string GenerateMainOSPackage(Customizations config, ImageCustomizations customizations, PolicyStore policyStore, IEnumerable<RegFilePartitionInfo> registryFiles, string staticDatastoreOutputRoot, string datastoreOutputRoot, string provisioningOutputRoot, string criticalSettingsOutputRoot)
		{
			CustomizationPackage customizationPackage = new CustomizationPackage(PkgConstants.c_strMainOsPartition);
			customizationPackage.Component = config.ImageDeviceName;
			customizationPackage.Owner = customizations.Owner;
			customizationPackage.OwnerType = customizations.OwnerType;
			customizationPackage.CpuType = config.ImageCpuType;
			customizationPackage.BuildType = config.ImageBuildType;
			customizationPackage.Version = config.ImageVersion;
			customizationPackage.Files.AddRange(GenerateAssetFileList(customizations, policyStore));
			IEnumerable<string> enumerable = Directory.EnumerateFiles(staticDatastoreOutputRoot);
			foreach (string item in enumerable)
			{
				string destinationPath = FileUtils.RerootPath(item, staticDatastoreOutputRoot, "\\Programs\\PhoneProvisioner_OEM\\OEM\\");
				customizationPackage.AddFile(item, destinationPath);
				string text = Path.GetFileName(item).Substring("static_settings_group".Length);
				text = "mxipupdate" + text;
				destinationPath = FileUtils.RerootPath(item, staticDatastoreOutputRoot, "\\Windows\\System32\\Migrators\\DuMigrationProvisionerOEM\\provxml");
				destinationPath = Path.Combine(Path.GetDirectoryName(destinationPath), text);
				customizationPackage.AddFile(item, destinationPath);
			}
			foreach (string item2 in from x in Directory.EnumerateFiles(datastoreOutputRoot).Concat(Directory.EnumerateFiles(provisioningOutputRoot)).Concat(Directory.EnumerateFiles(criticalSettingsOutputRoot))
				where Path.GetExtension(x).Equals(".xml")
				select x)
			{
				string path = FileUtils.RerootPath(item2, datastoreOutputRoot, "\\Programs\\CommonFiles\\ADC\\Microsoft\\");
				path = FileUtils.RerootPath(path, provisioningOutputRoot, "\\Programs\\CommonFiles\\ADC\\Microsoft\\");
				path = FileUtils.RerootPath(path, criticalSettingsOutputRoot, "\\Programs\\CommonFiles\\ADC\\Microsoft\\");
				customizationPackage.AddFile(item2, path);
			}
			enumerable = Directory.EnumerateFiles(datastoreOutputRoot);
			enumerable = enumerable.Concat(Directory.EnumerateFiles(provisioningOutputRoot));
			enumerable = enumerable.Where((string x) => Path.GetExtension(x).Equals(".provxml"));
			if (enumerable.Count() > 0)
			{
				string text2 = Path.Combine(datastoreOutputRoot, "ProvisionData.cab");
				CabApiWrapper.CreateCabSelected(text2, enumerable.ToArray(), provisioningOutputRoot, provisioningOutputRoot, CompressionType.LZX);
				string path2 = FileUtils.RerootPath(text2, datastoreOutputRoot, "\\Programs\\CommonFiles\\ADC\\Microsoft\\");
				path2 = FileUtils.RerootPath(path2, provisioningOutputRoot, "\\Programs\\CommonFiles\\ADC\\Microsoft\\");
				customizationPackage.AddFile(text2, path2);
			}
			enumerable = Directory.EnumerateFiles(criticalSettingsOutputRoot);
			enumerable = enumerable.Where((string x) => Path.GetExtension(x).Equals(".provxml"));
			if (enumerable.Count() > 0)
			{
				string text3 = Path.Combine(criticalSettingsOutputRoot, "ProvisionDataCriticalSettings.cab");
				CabApiWrapper.CreateCabSelected(text3, enumerable.ToArray(), criticalSettingsOutputRoot, criticalSettingsOutputRoot, CompressionType.LZX);
				string destinationPath2 = FileUtils.RerootPath(text3, criticalSettingsOutputRoot, "\\Programs\\CommonFiles\\ADC\\Microsoft\\");
				customizationPackage.AddFile(text3, destinationPath2);
			}
			customizationPackage.Files.AddRange(GenerateStaticAppLicenseList(customizations));
			customizationPackage.Files.AddRange(GenerateStaticAppProvXMLList(customizations));
			foreach (RegFilePartitionInfo registryFile in registryFiles)
			{
				customizationPackage.AddFile(FileType.Registry, registryFile.regFilename, CustomizationPackage.ShadowRegFilePath);
			}
			return customizationPackage.SavePackage(config.OutputDirectory);
		}

		private static string GenerateVariantAppsPackage(Customizations config, ImageCustomizations customizations, PolicyStore policyStore, string partition)
		{
			CustomizationPackage customizationPackage = new CustomizationPackage(partition);
			customizationPackage.Component = config.ImageDeviceName;
			customizationPackage.SubComponent += ".VariantApps";
			customizationPackage.Owner = customizations.Owner;
			customizationPackage.OwnerType = customizations.OwnerType;
			customizationPackage.CpuType = config.ImageCpuType;
			customizationPackage.BuildType = config.ImageBuildType;
			customizationPackage.Version = config.ImageVersion;
			if (partition.Equals(PkgConstants.c_strMainOsPartition))
			{
				customizationPackage.AddFiles(GenerateAssetFileList(customizations, policyStore, "VariantApps"));
			}
			foreach (Application item in from x in customizations.Variants.SelectMany((Variant x) => x.ApplicationGroups).SelectMany((Applications x) => x.Items)
				where x.TargetPartition.Equals(partition, StringComparison.OrdinalIgnoreCase)
				select x)
			{
				if (!string.IsNullOrEmpty(item.Source))
				{
					customizationPackage.AddFile(FileType.Regular, item.ExpandedSourcePath, item.DeviceDestination);
				}
				if (!string.IsNullOrEmpty(item.License))
				{
					customizationPackage.AddFile(FileType.Regular, item.ExpandedLicensePath, item.DeviceLicense);
				}
			}
			if (customizationPackage.Files.Any())
			{
				return customizationPackage.SavePackage(config.OutputDirectory);
			}
			return null;
		}

		private static string GenerateStaticAppsPackage(Customizations config, ImageCustomizations customizations, PolicyStore policyStore, string partition)
		{
			CustomizationPackage customizationPackage = new CustomizationPackage(partition);
			customizationPackage.Component = config.ImageDeviceName;
			customizationPackage.SubComponent += ".StaticApps";
			customizationPackage.Owner = customizations.Owner;
			customizationPackage.OwnerType = customizations.OwnerType;
			customizationPackage.CpuType = config.ImageCpuType;
			customizationPackage.BuildType = config.ImageBuildType;
			customizationPackage.Version = config.ImageVersion;
			if (partition.Equals(PkgConstants.c_strMainOsPartition))
			{
				customizationPackage.AddFiles(GenerateAssetFileList(customizations, policyStore, "StaticApps"));
			}
			if (customizations.StaticVariant != null)
			{
				IEnumerable<Application> source = customizations.StaticVariant.ApplicationGroups.SelectMany((Applications x) => x.Items);
				source = source.Where((Application x) => x.TargetPartition.Equals(partition, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(x.DeviceDestination));
				if (source.Any())
				{
					string tempDirectory = FileUtils.GetTempDirectory();
					foreach (Application item in source)
					{
						if (!string.IsNullOrEmpty(item.Source))
						{
							customizationPackage.AddFile(FileType.Regular, item.ExpandedSourcePath, item.DeviceDestination);
						}
						if (!partition.Equals(PkgConstants.c_strMainOsPartition, StringComparison.OrdinalIgnoreCase))
						{
							customizationPackage.AddFile(FileType.Regular, item.ExpandedLicensePath, item.DeviceLicense);
							XElement rootNode = XElement.Load(item.ExpandedProvXMLPath);
							KeyValuePair<Guid, XElement> keyValuePair = item.UpdateProvXml(rootNode);
							string fileName = Path.GetFileName(item.ProvXML);
							string text = Path.Combine(tempDirectory, fileName);
							keyValuePair.Value.Save(text);
							customizationPackage.AddFile(FileType.Regular, text, item.DeviceProvXML);
						}
					}
				}
			}
			if (customizationPackage.Files.Any())
			{
				return customizationPackage.SavePackage(config.OutputDirectory);
			}
			return null;
		}

		private static IEnumerable<CustomizationFile> GenerateAssetFileList(ImageCustomizations customizations, PolicyStore policyStore, string package = "")
		{
			List<CustomizationFile> list = new List<CustomizationFile>();
			IEnumerable<Variant> enumerable = customizations.Variants;
			if (customizations.StaticVariant != null)
			{
				enumerable = enumerable.Concat(customizations.StaticVariant.ToEnumerable());
			}
			foreach (Settings item in enumerable.SelectMany((Variant x) => x.SettingGroups))
			{
				if (policyStore.SettingGroupByPath(item.Path) == null)
				{
					continue;
				}
				foreach (Asset asset in item.Assets)
				{
					PolicyAssetInfo policyAssetInfo = policyStore.AssetByPathAndName(item.Path, asset.Name);
					if (policyAssetInfo != null && package.Equals(policyAssetInfo.TargetPackage, StringComparison.OrdinalIgnoreCase))
					{
						string devicePath = asset.GetDevicePathWithMacros(policyAssetInfo);
						if (!list.Any((CustomizationFile x) => x.Destination.Equals(devicePath)))
						{
							list.Add(new CustomizationFile(asset.ExpandedSourcePath, devicePath));
						}
					}
				}
			}
			return list;
		}

		private static IEnumerable<CustomizationFile> GenerateStaticAppLicenseList(ImageCustomizations customizations)
		{
			List<CustomizationFile> list = new List<CustomizationFile>();
			if (customizations.StaticVariant == null)
			{
				return list;
			}
			foreach (Application item in from x in customizations.StaticVariant.ApplicationGroups.SelectMany((Applications x) => x.Items)
				where x.TargetPartition.Equals(PkgConstants.c_strMainOsPartition, StringComparison.OrdinalIgnoreCase)
				select x)
			{
				list.Add(new CustomizationFile(item.ExpandedLicensePath, item.DeviceLicense));
			}
			return list;
		}

		private static IEnumerable<CustomizationFile> GenerateStaticAppProvXMLList(ImageCustomizations customizations)
		{
			List<CustomizationFile> list = new List<CustomizationFile>();
			if (customizations.StaticVariant == null)
			{
				return list;
			}
			string tempDirectory = FileUtils.GetTempDirectory();
			foreach (Application item in from x in customizations.StaticVariant.ApplicationGroups.SelectMany((Applications x) => x.Items)
				where x.TargetPartition.Equals(PkgConstants.c_strMainOsPartition, StringComparison.OrdinalIgnoreCase)
				select x)
			{
				XElement rootNode = XElement.Load(item.ExpandedProvXMLPath);
				KeyValuePair<Guid, XElement> keyValuePair = item.UpdateProvXml(rootNode);
				string text = Path.GetFileName(item.ProvXML);
				string text2 = Path.Combine(tempDirectory, text);
				keyValuePair.Value.Save(text2);
				list.Add(new CustomizationFile(text2, item.DeviceProvXML));
				if (text.StartsWith("MPAP_", StringComparison.OrdinalIgnoreCase))
				{
					text = text.Substring(5);
				}
				text = text.Insert(0, "mxipupdate_");
				list.Add(new CustomizationFile(text2, Path.Combine("Windows\\System32\\Migrators\\DuMigrationProvisionerMicrosoft\\provxml", text)));
			}
			return list;
		}

		private static List<KeyValuePair<string, string>> GenerateDataPartitionContent(ImageCustomizations customizations)
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			if (customizations.StaticVariant != null)
			{
				foreach (var item in from x in customizations.StaticVariant.DataAssetGroups
					from item in x.Items
					select new
					{
						Key = x.Type,
						Value = item
					})
				{
					string expandedSourcePath = item.Value.ExpandedSourcePath;
					if (File.Exists(expandedSourcePath))
					{
						string fileName = Path.GetFileName(item.Value.Source);
						string path = item.Key.DestinationPath();
						path = Path.Combine(path, fileName);
						list.Add(new KeyValuePair<string, string>(expandedSourcePath, path));
					}
					else if (Directory.Exists(expandedSourcePath))
					{
						foreach (string item2 in Directory.EnumerateFiles(expandedSourcePath, "*", SearchOption.AllDirectories))
						{
							string newRoot = item.Key.DestinationPath();
							newRoot = FileUtils.RerootPath(item2, expandedSourcePath, newRoot);
							list.Add(new KeyValuePair<string, string>(item2, newRoot));
						}
					}
				}
				return list;
			}
			return list;
		}

		private static MVVariant GenerateGroupForVariant(Variant variant, ImageCustomizations customizations, PolicyStore policyStore)
		{
			MVVariant mVVariant = new MVVariant(variant.Name);
			foreach (TargetRef targetRef in variant.TargetRefs)
			{
				Target targetWithId = customizations.GetTargetWithId(targetRef.Id);
				if (targetWithId == null)
				{
					continue;
				}
				foreach (TargetState targetState in targetWithId.TargetStates)
				{
					MVCondition provisioningCondition = new MVCondition();
					targetState.Items.ForEach(delegate(Condition x)
					{
						provisioningCondition.KeyValues.Add(x.Name, new WPConstraintValue(x.Value, x.IsWildCard));
					});
					mVVariant.Conditions.Add(provisioningCondition);
				}
			}
			if (variant.TargetRefs.Count > 0)
			{
				GenerateAppProvisioningForVariant(mVVariant, variant);
			}
			GenerateSettingsForVariant(mVVariant, variant, policyStore);
			return mVVariant;
		}

		private static void GenerateAppProvisioningForVariant(MVVariant provisioningVariant, Variant variant)
		{
			foreach (Application item2 in variant.ApplicationGroups.SelectMany((Applications x) => x.Items))
			{
				XElement rootNode = XElement.Load(item2.ExpandedProvXMLPath);
				KeyValuePair<Guid, XElement> item = item2.UpdateProvXml(rootNode);
				provisioningVariant.Applications.Add(item);
			}
		}

		private static void GenerateSettingsForVariant(MVVariant provisioningVariant, Variant variant, PolicyStore policyStore)
		{
			foreach (Settings settingGroup in variant.SettingGroups)
			{
				PolicyGroup policyGroup = policyStore.SettingGroupByPath(settingGroup.Path);
				if (policyGroup == null)
				{
					continue;
				}
				MVSettingProvisioning groupProvisioning = MVSettingProvisioning.General;
				if (policyGroup.ImageTimeOnly || variant is StaticVariant)
				{
					groupProvisioning = MVSettingProvisioning.Static;
				}
				else if (policyGroup.FirstVariationOnly)
				{
					groupProvisioning = MVSettingProvisioning.RunOnce;
				}
				else if (policyGroup.CriticalSettings)
				{
					groupProvisioning = MVSettingProvisioning.Connectivity;
				}
				MVSettingGroup mVSettingGroup = new MVSettingGroup(settingGroup.Path, policyGroup.Path);
				PolicyMacroTable groupMacros = policyGroup.GetMacroTable(settingGroup.Path);
				foreach (Setting item2 in settingGroup.Items)
				{
					PolicySetting policySetting = policyStore.SettingByPathAndName(settingGroup.Path, item2.Name);
					if (policySetting == null)
					{
						continue;
					}
					PolicyMacroTable macroTable = policySetting.GetMacroTable(groupMacros, item2.Name);
					IEnumerable<string> resolvedProvisioningPath = policySetting.Destination.GetResolvedProvisioningPath(macroTable);
					if (policySetting.Destination.Destination != 0)
					{
						MVSetting mVSetting = new MVSetting(resolvedProvisioningPath, policySetting.Destination.GetResolvedRegistryKey(macroTable), policySetting.Destination.GetResolvedRegistryValueName(macroTable), policySetting.Destination.Type, policySetting.Partition);
						mVSetting.ProvisioningTime = groupProvisioning;
						mVSetting.Value = policySetting.TransformValue(item2.Value, item2.Type);
						if (item2.Type != null)
						{
							mVSetting.DataType = item2.Type;
						}
						mVSettingGroup.Settings.Add(mVSetting);
					}
				}
				foreach (IGrouping<string, Asset> item3 in from x in settingGroup.Assets
					group x by x.Name)
				{
					PolicyAssetInfo assetInfo = policyGroup.AssetByName(item3.Key);
					if (assetInfo == null || !assetInfo.GenerateAssetProvXML)
					{
						continue;
					}
					if (assetInfo.OemRegValue != null)
					{
						PolicySettingDestination policySettingDestination = new PolicySettingDestination(assetInfo.Name + ".OEMAssets", policyGroup);
						MVSetting item = assetInfo.ToVariantSetting(item3, policySettingDestination.GetResolvedProvisioningPath(groupMacros), groupProvisioning, groupMacros);
						mVSettingGroup.Settings.Add(item);
						continue;
					}
					foreach (IGrouping<CustomizationAssetOwner, Asset> item4 in from x in item3
						group x by x.Type)
					{
						PolicySettingDestination destination = new PolicySettingDestination(assetInfo.Name + ((item4.Key == CustomizationAssetOwner.OEM) ? ".OEMAssets" : ".MOAssets"), policyGroup);
						IEnumerable<MVSetting> collection = item4.Select((Asset x) => x.ToVariantSetting(assetInfo, destination.GetResolvedProvisioningPath(groupMacros), groupProvisioning, groupMacros));
						mVSettingGroup.Settings.AddRange(collection);
					}
				}
				provisioningVariant.SettingsGroups.Add(mVSettingGroup);
			}
		}

		public static IEnumerable<CustomizationError> VerifyTargetList(Target target)
		{
			return VerifyTargetList(target, true);
		}

		public static IEnumerable<CustomizationError> VerifyTargetList(Target target, bool verifyChildren)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (target.TargetStates == null || target.TargetStates.Count() == 0)
			{
				CustomizationError item = new CustomizationError(CustomizationErrorSeverity.Error, target.ToEnumerable(), Strings.EmptyTargetStates);
				list.Add(item);
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyTargetId(Target target)
		{
			return new List<CustomizationError>();
		}

		public static IEnumerable<CustomizationError> VerifyConditions(TargetState conditions, Target target, ImageCustomizations customizations)
		{
			return VerifyConditions(conditions, target, customizations, true);
		}

		public static IEnumerable<CustomizationError> VerifyConditions(TargetState conditions, Target target, ImageCustomizations customizations, bool verifyChildren)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (conditions.Items == null || conditions.Items.Count() == 0)
			{
				CustomizationError item = new CustomizationError(CustomizationErrorSeverity.Warning, target.ToEnumerable(), Strings.EmptyTargetState);
				list.Add(item);
			}
			if (verifyChildren)
			{
				foreach (Condition item2 in conditions.Items)
				{
					list.AddRange(VerifyCondition(item2, target, customizations));
				}
				return list;
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyCondition(Condition condition, Target target, ImageCustomizations customizations)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			list.AddRange(VerifyConditionName(condition, target));
			list.AddRange(VerifyConditionValue(condition, target, customizations));
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyConditionName(Condition condition, Target target)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (!MVCondition.IsValidKey(condition.Name))
			{
				CustomizationError item = new CustomizationError(CustomizationErrorSeverity.Error, target.ToEnumerable(), Strings.InvalidConditionName, target.Id, condition.Name);
				list.Add(item);
			}
			if (!MVCondition.ValidKeys.Contains(condition.Name, StringComparer.OrdinalIgnoreCase))
			{
				CustomizationError item2 = new CustomizationError(CustomizationErrorSeverity.Warning, target.ToEnumerable(), Strings.UnknownConditionName, target.Id, condition.Name);
				list.Add(item2);
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyConditionValue(Condition condition, Target target, ImageCustomizations customizations)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			string errorMessage;
			if (!MVCondition.IsValidValue(condition.Name, new WPConstraintValue(condition.Value, condition.IsWildCard), out errorMessage))
			{
				CustomizationError item = new CustomizationError(CustomizationErrorSeverity.Error, target.ToEnumerable(), Strings.InvalidConditionValue, target.Id, condition.Name, condition.Value, errorMessage);
				list.Add(item);
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyApplicationsGroup(IEnumerable<Applications> groups, Variant variant)
		{
			return VerifyApplicationsGroup(groups, variant, true);
		}

		public static IEnumerable<CustomizationError> VerifyApplicationsGroup(IEnumerable<Applications> groups, Variant variant, bool verifyChildren)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (verifyChildren)
			{
				foreach (Applications group in groups)
				{
					list.AddRange(VerifyApplications(group, variant));
				}
				return list;
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyApplications(Applications applications, Variant variant)
		{
			return VerifyApplications(applications, variant, true);
		}

		public static IEnumerable<CustomizationError> VerifyApplications(Applications applications, Variant variant, bool verifyChildren)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (applications.Items == null || applications.Items.Count() == 0)
			{
				CustomizationError item = new CustomizationError(CustomizationErrorSeverity.Warning, variant.ToEnumerable(), Strings.EmptyApplications);
				list.Add(item);
			}
			if (verifyChildren)
			{
				foreach (Application item2 in applications.Items)
				{
					list.AddRange(VerifyApplication(item2, variant));
				}
				return list;
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyApplication(Application application, Variant variant)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			list.AddRange(VerifyApplicationSource(application, variant));
			list.AddRange(VerifyApplicationLicense(application));
			list.AddRange(VerifyApplicationProvXML(application, variant));
			list.AddRange(VerifyApplicationTargetPartition(application));
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyApplicationSource(Application application, Variant variant)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (variant is StaticVariant || !string.IsNullOrWhiteSpace(application.Source))
			{
				list.AddRange(VerifyExpandedPath(application.Source, application.ToEnumerable(), Application.SourceFieldName));
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyApplicationLicense(Application application)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			list.AddRange(VerifyExpandedPath(application.License, application.ToEnumerable(), Application.LicenseFieldName));
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyApplicationProvXML(Application application, Variant variant)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			list.AddRange(VerifyExpandedPath(application.ProvXML, application.ToEnumerable(), Application.ProvXMLFieldName));
			if (list.Count((CustomizationError x) => x.Severity.Equals(CustomizationErrorSeverity.Error)) == 0 && !(variant is StaticVariant))
			{
				list.AddRange(application.VerifyProvXML());
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyApplicationTargetPartition(Application application)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			string targetPartition = application.TargetPartition;
			if (!Application.ValidPartitions.Contains(application.TargetPartition, StringComparer.OrdinalIgnoreCase))
			{
				string text = (string.IsNullOrWhiteSpace(application.Source) ? application.ProvXML : application.Source);
				list.Add(new CustomizationError(CustomizationErrorSeverity.Error, application.ToEnumerable(), Strings.ApplicationPartitionInvalid, text, targetPartition, string.Join(", ", Application.ValidPartitions)));
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyTargetRefs(IEnumerable<TargetRef> TargetRefs, IEnumerable<Target> targets, Variant parent)
		{
			return VerifyTargetRefs(TargetRefs, targets, parent, true);
		}

		public static IEnumerable<CustomizationError> VerifyTargetRefs(IEnumerable<TargetRef> TargetRefs, IEnumerable<Target> targets, Variant parent, bool verifyChildren)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (TargetRefs == null || TargetRefs.Count() == 0)
			{
				CustomizationError item = new CustomizationError(CustomizationErrorSeverity.Error, parent.ToEnumerable(), Strings.EmptyTargetRefs, parent.Name);
				list.Add(item);
			}
			if (verifyChildren)
			{
				foreach (TargetRef TargetRef in TargetRefs)
				{
					list.AddRange(VerifyTargetRef(TargetRef, targets));
				}
				return list;
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyTargetRef(TargetRef targetRef, IEnumerable<Target> targets)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (targets.Where((Target x) => x.Id.Equals(targetRef.Id)).Count() == 0)
			{
				CustomizationError item = new CustomizationError(CustomizationErrorSeverity.Error, targetRef.ToEnumerable(), Strings.UnknownTarget, targetRef.Id);
				list.Add(item);
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifySettingGroups(IEnumerable<Settings> groups, Variant parent, ImageCustomizations customizations, PolicyStore policyStore)
		{
			return VerifySettingGroups(groups, parent, customizations, policyStore, true);
		}

		public static IEnumerable<CustomizationError> VerifySettingGroups(IEnumerable<Settings> groups, Variant parent, ImageCustomizations customizations, PolicyStore policyStore, bool verifyChildren)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (verifyChildren)
			{
				foreach (Settings group in groups)
				{
					list.AddRange(VerifySettingsGroup(group, parent, customizations, policyStore));
				}
				return list;
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifySettingsGroup(Settings group, Variant variant, ImageCustomizations customizations, PolicyStore policyStore)
		{
			return VerifySettingsGroup(group, variant, customizations, policyStore, true);
		}

		public static IEnumerable<CustomizationError> VerifySettingsGroup(Settings group, Variant variant, ImageCustomizations customizations, PolicyStore policyStore, bool verifyChildren)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			list.AddRange(VerifySettingsGroupList(group, policyStore));
			list.AddRange(VerifySettingsGroupPath(group, variant, policyStore));
			if (verifyChildren)
			{
				foreach (Setting item in group.Items)
				{
					list.AddRange(VerifySetting(item, group, policyStore));
				}
				{
					foreach (Asset asset in group.Assets)
					{
						list.AddRange(VerifyAsset(asset, group, customizations, policyStore));
					}
					return list;
				}
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifySettingsGroupList(Settings group, PolicyStore policyStore)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (group.Items == null || group.Items.Count() == 0)
			{
				CustomizationError item = new CustomizationError(CustomizationErrorSeverity.Warning, group.ToEnumerable(), Strings.EmptySettingsGroup, group.Path);
				list.Add(item);
				return list;
			}
			PolicyGroup policyGroup = policyStore.SettingGroupByPath(group.Path);
			if (policyGroup == null)
			{
				CustomizationError item2 = new CustomizationError(CustomizationErrorSeverity.Warning, group.ToEnumerable(), Strings.UnknownSettingsPath, group.Path);
				list.Add(item2);
				return list;
			}
			if (policyGroup.Atomic)
			{
				foreach (PolicySetting policy in policyGroup.Settings)
				{
					if (!group.Items.Any((Setting x) => policy.Equals(policyGroup.SettingByName(x.Name))))
					{
						CustomizationError item3 = new CustomizationError(CustomizationErrorSeverity.Error, group.ToEnumerable(), Strings.AtomicSettingMissing, group.Path, policy.Name);
						list.Add(item3);
					}
				}
				return list;
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifySettingsGroupPath(Settings group, Variant variant, PolicyStore policyStore)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			PolicyGroup policyGroup = policyStore.SettingGroupByPath(group.Path);
			if (policyGroup == null)
			{
				CustomizationError item = new CustomizationError(CustomizationErrorSeverity.Warning, group.ToEnumerable(), Strings.UnknownSettingsPath, group.Path);
				list.Add(item);
				return list;
			}
			if (policyGroup.ImageTimeOnly && !(variant is StaticVariant))
			{
				CustomizationError item2 = new CustomizationError(CustomizationErrorSeverity.Error, group.ToEnumerable(), Strings.VariantedImageTimeOnlySettingsGroup, group.Path);
				list.Add(item2);
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifySetting(Setting setting, Settings group, PolicyStore policyStore)
		{
			return VerifySetting(setting, group, policyStore, null, true);
		}

		public static IEnumerable<CustomizationError> VerifySetting(Setting setting, Settings group, PolicyStore policyStore, PolicyGroup policyGroup)
		{
			return VerifySetting(setting, group, policyStore, policyGroup, true);
		}

		public static IEnumerable<CustomizationError> VerifySetting(Setting setting, Settings group, PolicyStore policyStore, PolicyGroup policyGroup, bool verifyChildren)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (policyGroup == null)
			{
				policyGroup = policyStore.SettingGroupByPath(group.Path);
				if (policyGroup == null)
				{
					CustomizationError item = new CustomizationError(CustomizationErrorSeverity.Warning, group.ToEnumerable(), Strings.UnknownSettingsPath, group.Path);
					list.Add(item);
				}
			}
			list.AddRange(VerifySettingName(setting, group, policyStore));
			list.AddRange(VerifySettingValue(setting, group, policyStore));
			return list;
		}

		public static IEnumerable<CustomizationError> VerifySettingName(Setting setting, Settings group, PolicyStore policyStore)
		{
			return VerifySettingName(setting, group, policyStore, null);
		}

		public static IEnumerable<CustomizationError> VerifySettingName(Setting setting, Settings group, PolicyStore policyStore, PolicyGroup policyGroup)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (group.Items.Where((Setting x) => setting.Name.Equals(x.Name, StringComparison.OrdinalIgnoreCase)).Count() > 1)
			{
				CustomizationError item = new CustomizationError(CustomizationErrorSeverity.Error, setting.ToEnumerable(), Strings.DuplicateSettings, setting.Name, group.Path);
				list.Add(item);
			}
			if (policyGroup == null)
			{
				policyGroup = policyStore.SettingGroupByPath(group.Path);
				if (policyGroup == null)
				{
					CustomizationError item2 = new CustomizationError(enforcingStrictSettingPolicies(), group.ToEnumerable(), Strings.UnableToValidateSettingNameUnknownSettingsPath, group.Path);
					list.Add(item2);
					return list;
				}
			}
			PolicySetting policySetting = policyGroup.SettingByName(setting.Name);
			if (policySetting == null)
			{
				CustomizationError item3 = new CustomizationError(enforcingStrictSettingPolicies(), setting.ToEnumerable(), Strings.UnknownSettingName, group.Path, setting.Name);
				list.Add(item3);
				return list;
			}
			if (!PolicySetting.ValidPartitions.Contains(policySetting.Partition, StringComparer.OrdinalIgnoreCase))
			{
				CustomizationError item4 = new CustomizationError(CustomizationErrorSeverity.Error, setting.ToEnumerable(), Strings.SettingPartitionInvalid, group.Path, setting.Name, policySetting.Partition, string.Join(", ", PolicySetting.ValidPartitions));
				list.Add(item4);
				return list;
			}
			return list;
		}

		public static CustomizationErrorSeverity enforcingStrictSettingPolicies()
		{
			if (Customizations.StrictSettingPolicies)
			{
				return CustomizationErrorSeverity.Error;
			}
			return CustomizationErrorSeverity.Warning;
		}

		public static IEnumerable<CustomizationError> VerifySettingValue(Setting setting, Settings group, PolicyStore policyStore)
		{
			return VerifySettingValue(setting, group, policyStore, null);
		}

		public static IEnumerable<CustomizationError> VerifySettingValue(Setting setting, Settings group, PolicyStore policyStore, PolicySetting policySetting)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (policySetting == null)
			{
				PolicyGroup policyGroup = policyStore.SettingGroupByPath(group.Path);
				if (policyGroup == null)
				{
					CustomizationError item = new CustomizationError(CustomizationErrorSeverity.Warning, group.ToEnumerable(), Strings.UnableToValidateSettingValueUnknownSettingsPath, group.Path);
					list.Add(item);
					return list;
				}
				if (policyGroup != null)
				{
					policySetting = policyGroup.SettingByName(setting.Name);
					if (policySetting == null)
					{
						CustomizationError item2 = new CustomizationError(CustomizationErrorSeverity.Warning, setting.ToEnumerable(), Strings.UnableToValidateSettingValueUnknownSettingName, group.Path, setting.Name);
						list.Add(item2);
						return list;
					}
				}
			}
			if (setting.Value == null)
			{
				CustomizationError item3 = new CustomizationError(CustomizationErrorSeverity.Error, setting.ToEnumerable(), Strings.NullSettingValue, group.Path, setting.Name);
				list.Add(item3);
				return list;
			}
			if (!policySetting.IsValidValue(setting.Value, setting.Type))
			{
				CustomizationError item4 = new CustomizationError(CustomizationErrorSeverity.Error, setting.ToEnumerable(), Strings.InvalidSettingValue, group.Path, setting.Name, setting.Value);
				list.Add(item4);
				return list;
			}
			if (policySetting.AssetInfo == null)
			{
				return list;
			}
			bool flag = group.Assets.Find((Asset x) => setting.Value.Equals(x.Id, StringComparison.OrdinalIgnoreCase)) != null;
			if (!flag)
			{
				flag = policySetting.AssetInfo.Presets.Find((PolicyEnum preset) => preset.Value.Equals(setting.Value, StringComparison.OrdinalIgnoreCase)) != null;
			}
			if (!flag)
			{
				CustomizationError item5 = new CustomizationError(CustomizationErrorSeverity.Error, setting.ToEnumerable(), Strings.AssetNotFound, setting.Value, group.Path);
				list.Add(item5);
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyAssets(Settings settings, PolicyGroup group)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (settings.Assets != null && settings.Assets.Count() != 0)
			{
				if (group == null)
				{
					CustomizationError item = new CustomizationError(CustomizationErrorSeverity.Warning, settings.ToEnumerable(), Strings.UnableToValidateSettingAssetsUnknownSettingsPath, settings.Path);
					list.Add(item);
				}
				else if (group.Assets == null || group.Assets.Count() == 0)
				{
					CustomizationError item2 = new CustomizationError(CustomizationErrorSeverity.Error, settings.ToEnumerable(), Strings.AssetsNotSupported, settings.Path);
					list.Add(item2);
				}
			}
			return list;
		}

		private static IEnumerable<CustomizationError> GetAssetInfo(Asset asset, string groupPath, string fieldName, PolicyStore policyStore, out PolicyAssetInfo assetInfo)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			assetInfo = null;
			if (policyStore.SettingGroupByPath(groupPath) == null)
			{
				CustomizationError item = new CustomizationError(CustomizationErrorSeverity.Warning, asset.ToEnumerable(), Strings.UnableToValidateAssetFieldUnknownSettingsPath, fieldName, asset.Id, groupPath);
				list.Add(item);
				return list;
			}
			assetInfo = policyStore.AssetByPathAndName(groupPath, asset.Name);
			if (assetInfo == null)
			{
				CustomizationError item2 = new CustomizationError(CustomizationErrorSeverity.Warning, asset.ToEnumerable(), Strings.UnableToValidateAssetFieldUnknownAssetName, fieldName, asset.Id, groupPath, asset.Name);
				list.Add(item2);
				return list;
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyAsset(Asset asset, Settings group, ImageCustomizations customizations, PolicyStore policyStore)
		{
			return VerifyAsset(asset, group, customizations, policyStore, null);
		}

		public static IEnumerable<CustomizationError> VerifyAsset(Asset asset, Settings group, ImageCustomizations customizations, PolicyStore policyStore, PolicyAssetInfo assetInfo)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (assetInfo == null)
			{
				list.AddRange(GetAssetInfo(asset, group.Path, "Asset", policyStore, out assetInfo));
			}
			if (assetInfo == null)
			{
				CustomizationError item = new CustomizationError(CustomizationErrorSeverity.Warning, asset.ToEnumerable(), Strings.AssetNotSupported, asset.Name, group.Path);
				list.Add(item);
			}
			list.AddRange(VerifyAssetType(asset, group, policyStore, assetInfo));
			list.AddRange(VerifyAssetSource(asset, group, customizations, policyStore, assetInfo));
			list.AddRange(VerifyAssetTargetFileName(asset, group, customizations, policyStore, assetInfo));
			list.AddRange(VerifyAssetDisplayName(asset, group));
			list.AddRange(VerifyAssetTargetPackage(asset, group, customizations, policyStore));
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyAssetType(Asset asset, Settings group, PolicyStore policyStore)
		{
			return VerifyAssetType(asset, group, policyStore, null);
		}

		public static IEnumerable<CustomizationError> VerifyAssetType(Asset asset, Settings group, PolicyStore policyStore, PolicyAssetInfo assetInfo)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (assetInfo == null)
			{
				list.AddRange(GetAssetInfo(asset, group.Path, "Type", policyStore, out assetInfo));
			}
			if (assetInfo == null)
			{
				CustomizationError item = new CustomizationError(CustomizationErrorSeverity.Warning, asset.ToEnumerable(), Strings.UnableToValidateAssetTypeUnknownSettingsPath, group.Path);
				list.Add(item);
				return list;
			}
			if (asset.Type == CustomizationAssetOwner.MobileOperator && assetInfo.MORegKey == null)
			{
				CustomizationError item2 = new CustomizationError(CustomizationErrorSeverity.Error, asset.ToEnumerable(), Strings.SettingDoesNotSupportOperatorAssets, group.Path, asset.Name);
				list.Add(item2);
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyAssetSource(Asset asset, Settings group, ImageCustomizations customizations, PolicyStore policyStore)
		{
			return VerifyAssetSource(asset, group, customizations, policyStore, null);
		}

		public static IEnumerable<CustomizationError> VerifyAssetSource(Asset asset, Settings group, ImageCustomizations customizations, PolicyStore policyStore, PolicyAssetInfo assetInfo)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			list.AddRange(VerifyExpandedPath(asset.Source, asset.ToEnumerable(), Asset.SourceFieldName));
			if (assetInfo == null)
			{
				list.AddRange(GetAssetInfo(asset, group.Path, Asset.SourceFieldName, policyStore, out assetInfo));
			}
			if (assetInfo == null)
			{
				CustomizationError item = new CustomizationError(CustomizationErrorSeverity.Warning, asset.ToEnumerable(), Strings.UnableToValidateAssetTypeUnknownSettingsPath, group.Path);
				list.Add(item);
				return list;
			}
			if (string.IsNullOrWhiteSpace(asset.TargetFileName))
			{
				list.AddRange(verifyAssetId(asset, group, customizations, policyStore, assetInfo));
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyAssetDisplayName(Asset asset, Settings group)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (!string.IsNullOrEmpty(asset.DisplayName) && group.Assets.Where((Asset x) => asset.DisplayName.Equals(x.DisplayName, StringComparison.OrdinalIgnoreCase) && asset.Name.Equals(x.Name, StringComparison.OrdinalIgnoreCase)).Count() > 1)
			{
				CustomizationError item = new CustomizationError(CustomizationErrorSeverity.Warning, group.ToEnumerable(), Strings.AssetWithDuplicateDisplayName, group.Path, asset.DisplayName);
				list.Add(item);
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyAssetTargetFileName(Asset asset, Settings group, ImageCustomizations customizations, PolicyStore policyStore)
		{
			return VerifyAssetTargetFileName(asset, group, customizations, policyStore, null);
		}

		public static IEnumerable<CustomizationError> VerifyAssetTargetFileName(Asset asset, Settings group, ImageCustomizations customizations, PolicyStore policyStore, PolicyAssetInfo assetInfo)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (string.IsNullOrWhiteSpace(asset.TargetFileName))
			{
				return list;
			}
			if (asset.TargetFileName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
			{
				CustomizationError item = new CustomizationError(CustomizationErrorSeverity.Error, asset.ToEnumerable(), Strings.AssetInvalidTargetFileName, asset.Name, group.Path, asset.TargetFileName);
				list.Add(item);
				return list;
			}
			if (assetInfo == null)
			{
				list.AddRange(GetAssetInfo(asset, group.Path, "Target Filename", policyStore, out assetInfo));
			}
			if (assetInfo == null)
			{
				CustomizationError item2 = new CustomizationError(CustomizationErrorSeverity.Warning, asset.ToEnumerable(), Strings.UnableToValidateAssetTypeUnknownSettingsPath, group.Path);
				list.Add(item2);
			}
			list.AddRange(verifyAssetId(asset, group, customizations, policyStore, assetInfo));
			return list;
		}

		private static IEnumerable<CustomizationError> verifyAssetId(Asset asset, Settings group, ImageCustomizations customizations, PolicyStore policyStore, PolicyAssetInfo assetInfo = null)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (assetInfo == null)
			{
				assetInfo = policyStore.AssetByPathAndName(group.Path, asset.Name);
			}
			if (assetInfo == null)
			{
				return list;
			}
			if (assetInfo != null && !assetInfo.IsValidFileType(Path.GetFileName(asset.Id)))
			{
				CustomizationError item = new CustomizationError(CustomizationErrorSeverity.Error, asset.ToEnumerable(), Strings.UnsupportedFileType, group.Path, asset.Name, Path.GetExtension(asset.Id), string.Join(", ", assetInfo.FileTypes));
				list.Add(item);
			}
			IEnumerable<Settings> enumerable = customizations.Variants.SelectMany((Variant x) => x.SettingGroups);
			if (customizations.StaticVariant != null)
			{
				enumerable = enumerable.Concat(customizations.StaticVariant.SettingGroups);
			}
			foreach (Settings item2 in enumerable)
			{
				PolicyGroup policyGroup = policyStore.SettingGroupByPath(item2.Path);
				if (policyGroup == null)
				{
					continue;
				}
				List<Asset> list2 = new List<Asset>();
				foreach (Asset asset2 in item2.Assets)
				{
					PolicyAssetInfo policyAssetInfo = policyGroup.AssetByName(asset2.Name);
					if (policyAssetInfo != null && asset.GetDevicePathWithMacros(assetInfo).Equals(asset2.GetDevicePathWithMacros(policyAssetInfo)))
					{
						list2.Add(asset2);
					}
				}
				foreach (Asset item3 in list2)
				{
					if (policyGroup.AssetByName(item3.Name) != null && !asset.ExpandedSourcePath.Equals(item3.ExpandedSourcePath, StringComparison.OrdinalIgnoreCase))
					{
						list.Add(new CustomizationError(CustomizationErrorSeverity.Error, asset.ToEnumerable(), Strings.AssetTargetConflict, asset.Id, asset.Source, item3.Source));
					}
				}
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyAssetTargetPackage(Asset asset, Settings group, ImageCustomizations customizations, PolicyStore policyStore)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			PolicyAssetInfo policyAssetInfo = policyStore.AssetByPathAndName(group.Path, asset.Name);
			if (policyAssetInfo == null)
			{
				return list;
			}
			IEnumerable<Settings> enumerable = customizations.Variants.SelectMany((Variant x) => x.SettingGroups);
			if (customizations.StaticVariant != null)
			{
				enumerable = enumerable.Concat(customizations.StaticVariant.SettingGroups);
			}
			foreach (Settings item in enumerable)
			{
				PolicyGroup policyGroup = policyStore.SettingGroupByPath(item.Path);
				if (policyGroup == null)
				{
					continue;
				}
				List<Asset> list2 = new List<Asset>();
				foreach (Asset asset2 in item.Assets)
				{
					PolicyAssetInfo policyAssetInfo2 = policyGroup.AssetByName(asset2.Name);
					if (policyAssetInfo2 != null && asset.GetDevicePathWithMacros(policyAssetInfo).Equals(asset2.GetDevicePathWithMacros(policyAssetInfo2)))
					{
						list2.Add(asset2);
					}
				}
				foreach (Asset item2 in list2)
				{
					PolicyAssetInfo policyAssetInfo3 = policyGroup.AssetByName(item2.Name);
					if (policyAssetInfo3 != null && !policyAssetInfo.TargetPackage.Equals(policyAssetInfo3.TargetPackage, StringComparison.OrdinalIgnoreCase))
					{
						IEnumerable<IDefinedIn> source = new List<IDefinedIn> { asset, item2 };
						string message = string.Format(Strings.AssetPackageConflict, asset.Name, policyAssetInfo.TargetPackage, item2.Name, policyAssetInfo3.TargetPackage, asset.GetDevicePath(policyAssetInfo.TargetDir));
						list.Add(new CustomizationError(CustomizationErrorSeverity.Error, source.DistinctBy((IDefinedIn x) => x.DefinedInFile), message));
					}
				}
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyDataAssetGroups(IEnumerable<DataAssets> groups, Variant parent)
		{
			return VerifyDataAssetGroups(groups, parent, true);
		}

		public static IEnumerable<CustomizationError> VerifyDataAssetGroups(IEnumerable<DataAssets> groups, Variant parent, bool verifyChildren)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (verifyChildren)
			{
				foreach (DataAssets group in groups)
				{
					list.AddRange(VerifyDataAssetGroup(group, parent));
				}
				return list;
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyDataAssetGroup(DataAssets group, Variant variant)
		{
			return VerifyDataAssetGroup(group, variant, true);
		}

		public static IEnumerable<CustomizationError> VerifyDataAssetGroup(DataAssets group, Variant variant, bool verifyChildren)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (group.Items == null || group.Items.Count() == 0)
			{
				CustomizationError item = new CustomizationError(CustomizationErrorSeverity.Warning, variant.ToEnumerable(), Strings.EmptyDataAssetsGroup);
				list.Add(item);
			}
			if (verifyChildren)
			{
				foreach (DataAsset item2 in group.Items)
				{
					list.AddRange(VerifyDataAsset(item2));
				}
				return list;
			}
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyDataAsset(DataAsset dataAsset)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			list.AddRange(VerifyExpandedPath(dataAsset.Source, dataAsset.ToEnumerable(), DataAsset.SourceFieldName));
			return list;
		}

		public static IEnumerable<CustomizationError> VerifyExpandedPath(string path, IEnumerable<IDefinedIn> defined, string fieldName)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (string.IsNullOrEmpty(path))
			{
				list.Add(new CustomizationError(CustomizationErrorSeverity.Error, defined, Strings.PathEmptySource, fieldName));
				return list;
			}
			if (path.Contains('%'))
			{
				list.Add(new CustomizationError(CustomizationErrorSeverity.Error, defined, Strings.PathInvalidPercent));
				return list;
			}
			string path2;
			try
			{
				path2 = ImageCustomizations.ExpandPath(path, true);
			}
			catch (PkgGenException ex)
			{
				list.Add(new CustomizationError(CustomizationErrorSeverity.Error, defined, Strings.PathUnresolvedVariable, fieldName, path, ex.Message));
				return list;
			}
			if (!VerifyFullPath(path2))
			{
				list.Add(new CustomizationError(CustomizationErrorSeverity.Error, defined, Strings.PathNotAbsolute, fieldName, path));
				return list;
			}
			if (!fieldName.Equals("Import source"))
			{
				string fileName = Path.GetFileName(path);
				string fileName2 = Path.GetFileName(path2);
				if (!string.Equals(fileName, fileName2, StringComparison.OrdinalIgnoreCase))
				{
					list.Add(new CustomizationError(CustomizationErrorSeverity.Error, defined, Strings.PathFileNameContainsVars, fieldName, path));
					return list;
				}
			}
			if (!File.Exists(path2))
			{
				if (fieldName.Equals(DataAsset.SourceFieldName) && !Directory.Exists(path2))
				{
					list.Add(new CustomizationError(CustomizationErrorSeverity.Error, defined, Strings.PathDoesNotExist, fieldName, path));
					return list;
				}
				if (!fieldName.Equals(DataAsset.SourceFieldName))
				{
					list.Add(new CustomizationError(CustomizationErrorSeverity.Error, defined, Strings.PathFileDoesNotExist, fieldName, path));
					return list;
				}
			}
			return list;
		}

		private static bool VerifyFullPath(string path)
		{
			if (!Path.IsPathRooted(path))
			{
				return false;
			}
			if (!Path.GetPathRoot(path).Contains("\\"))
			{
				return false;
			}
			return true;
		}
	}
}
