using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.Tools;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public static class Package
	{
		public static CompressionType DefaultCompressionType = CompressionType.FastLZX;

		public static IDeploymentLogger Logger = new IULogger();

		public static IPkgBuilder Create()
		{
			return new PkgBuilder();
		}

		public static Hashtable LoadRegistry(string cabPath)
		{
			Hashtable registryKeysMerged = new Hashtable();
			string tempDirectory = FileUtils.GetTempDirectory();
			IPkgInfo pkgInfo = LoadFromCab(cabPath);
			ArrayList arrayList = new ArrayList();
			foreach (IFileEntry file in pkgInfo.Files)
			{
				if (file.CabPath.EndsWith(".manifest", StringComparison.CurrentCultureIgnoreCase) && !file.CabPath.Contains("deployment"))
				{
					arrayList.Add(file.CabPath);
				}
			}
			if (arrayList.Count == 0)
			{
				return registryKeysMerged;
			}
			try
			{
				CabApiWrapper.ExtractSelected(cabPath, tempDirectory, arrayList.OfType<string>());
				object reg_lock = new object();
				ArrayList arrayList2 = new ArrayList();
				ParallelOptions parallelOptions = new ParallelOptions();
				int num = 0;
				string[] files = Directory.GetFiles(tempDirectory);
				parallelOptions.MaxDegreeOfParallelism = PkgConstants.c_iMaxPackagingThreads;
				if (files.Count() < PkgConstants.c_iMaxPackagingThreads)
				{
					parallelOptions.MaxDegreeOfParallelism = files.Count();
				}
				string[] array;
				for (int i = 0; i < parallelOptions.MaxDegreeOfParallelism - 1; i++)
				{
					array = new string[files.Count() / parallelOptions.MaxDegreeOfParallelism];
					Array.Copy(files, num, array, 0, files.Count() / parallelOptions.MaxDegreeOfParallelism);
					num += files.Count() / parallelOptions.MaxDegreeOfParallelism;
					arrayList2.Add(array);
				}
				array = new string[files.Count() - num];
				Array.Copy(files, num, array, 0, files.Count() - num);
				arrayList2.Add(array);
				Parallel.ForEach(arrayList2.OfType<string[]>(), delegate(string[] file_range)
				{
					Hashtable hashtable = new Hashtable();
					for (int j = 0; j < file_range.Length; j++)
					{
						XDocument xDocument = XDocument.Load(file_range[j]);
						XNamespace @namespace = xDocument.Root.Name.Namespace;
						foreach (XElement item in xDocument.Root.Elements(@namespace + "registryKeys"))
						{
							if (item != null)
							{
								foreach (XElement item2 in item.Elements(@namespace + "registryKey"))
								{
									Hashtable hashtable2 = new Hashtable();
									foreach (XElement item3 in item2.Elements(@namespace + "registryValue"))
									{
										if (item3.Attribute("name") != null)
										{
											string value = null;
											if (item3.Attribute("value") != null)
											{
												value = item3.Attribute("value").Value;
											}
											hashtable2.Add(item3.Attribute("name").Value, value);
										}
									}
									if (!hashtable.ContainsKey(item2.Attribute("keyName").Value))
									{
										hashtable.Add(item2.Attribute("keyName").Value, hashtable2);
									}
									else
									{
										foreach (DictionaryEntry item4 in hashtable2)
										{
											((Hashtable)hashtable[item2.Attribute("keyName").Value])[item4.Key] = item4.Value;
										}
									}
								}
							}
						}
					}
					if (hashtable.Count > 0)
					{
						lock (reg_lock)
						{
							foreach (DictionaryEntry item5 in hashtable)
							{
								registryKeysMerged[item5.Key] = item5.Value;
							}
						}
					}
				});
			}
			finally
			{
				FileUtils.CleanDirectory(tempDirectory);
			}
			return registryKeysMerged;
		}

		public static bool RegistryKeyExist(Hashtable regTable, string keyName)
		{
			return RegistryKeyExist(regTable, keyName, null);
		}

		public static bool RegistryKeyExist(Hashtable regTable, string keyName, string valueName)
		{
			if (!regTable.ContainsKey(keyName))
			{
				return false;
			}
			if (!string.IsNullOrEmpty(valueName) && !((Hashtable)regTable[keyName]).ContainsKey(valueName))
			{
				return false;
			}
			return true;
		}

		public static string RegistryKeyValue(Hashtable regTable, string keyName, string valueName)
		{
			if (!regTable.ContainsKey(keyName))
			{
				return null;
			}
			if (!((Hashtable)regTable[keyName]).ContainsKey(valueName))
			{
				return null;
			}
			return (string)((Hashtable)regTable[keyName])[valueName];
		}

		public static IPkgBuilder Create(string cabPath)
		{
			if (string.IsNullOrEmpty(cabPath))
			{
				throw new ArgumentException("Create: cab path can not be null or empty", "cabPath");
			}
			if (!LongPathFile.Exists(cabPath))
			{
				throw new PackageException("Create: cab file '{0}' doesn't exist", cabPath);
			}
			return PkgBuilder.Load(cabPath);
		}

		public static IPkgInfo LoadFromFolder(string folderPath)
		{
			if (string.IsNullOrEmpty(folderPath))
			{
				throw new ArgumentException("LoadFromFolder: cab path can not be null or empty", "folderPath");
			}
			if (!Directory.Exists(folderPath))
			{
				throw new PackageException("LoadFromFolder: folderPath '{0}' doesn't exist", folderPath);
			}
			return WPExtractedPackage.Load(folderPath);
		}

		public static IPkgInfo LoadFromCab(string cabPath)
		{
			if (string.IsNullOrEmpty(cabPath))
			{
				throw new ArgumentException("LoadFromCab: cab path can not be null or empty", "cabPath");
			}
			if (!LongPathFile.Exists(cabPath))
			{
				throw new PackageException("LoadFromCab: Cab file '{0}' doesn't exist", cabPath);
			}
			string[] array = null;
			try
			{
				array = CabApiWrapper.GetFileList(cabPath);
			}
			catch (CabException innerException)
			{
				throw new PackageException(innerException, "LoadFromCab: Failed to load cab file '{0}'", cabPath);
			}
			catch (IOException innerException2)
			{
				throw new PackageException(innerException2, "LoadFromCab: Failed to load cab file '{0}'", cabPath);
			}
			if (!array.Contains(PkgConstants.c_strDsmFile, StringComparer.OrdinalIgnoreCase) && !array.Contains(PkgConstants.c_strMumFile, StringComparer.OrdinalIgnoreCase) && !array.Contains(PkgConstants.c_strCIX, StringComparer.OrdinalIgnoreCase))
			{
				throw new PackageException("LoadFromCab: No package manifest found in cab file '{0}'", cabPath);
			}
			if (array.Contains(PkgConstants.c_strDiffDsmFile, StringComparer.OrdinalIgnoreCase) || array.Contains(PkgConstants.c_strCIX, StringComparer.OrdinalIgnoreCase))
			{
				return DiffPkg.LoadFromCab(cabPath);
			}
			return WPCanonicalPackage.LoadFromCab(cabPath);
		}

		public static IPkgInfo LoadInstalledPackage(string manifestPath, string installationDir)
		{
			if (string.IsNullOrEmpty(manifestPath))
			{
				throw new ArgumentException("LoadInstalledPackage: Package manifest path can not be null or empty", "manifestPath");
			}
			if (string.IsNullOrEmpty(installationDir))
			{
				throw new ArgumentException("LoadInstalledPackage: Package root directory can not be null or empty", "installationDir");
			}
			if (!LongPathFile.Exists(manifestPath))
			{
				throw new PackageException("LoadInstalledPackage: Package manifest file '{0}' doesn't exist", manifestPath);
			}
			if (!LongPathDirectory.Exists(installationDir))
			{
				throw new PackageException("LoadInstalledPackage: Package root directory '{0}' doesn't exist", installationDir);
			}
			return WPCanonicalPackage.LoadFromInstallationDir(manifestPath, installationDir);
		}

		public static void CreatePKR(string sourceCab, string outputCab)
		{
			if (string.IsNullOrEmpty(sourceCab))
			{
				throw new ArgumentException("CreatePKR: path of source package can't be null or empty", "sourceCab");
			}
			if (string.IsNullOrEmpty(outputCab))
			{
				throw new ArgumentException("CreatePKR: path of the output package can't be null or empty", "outputCab");
			}
			if (!LongPathFile.Exists(sourceCab))
			{
				throw new PackageException("CreatePKR: source package '{0}' doesn't exist", sourceCab);
			}
			PKRBuilder.Create(sourceCab, outputCab);
		}

		public static IDiffPkg CreateDiff(IPkgInfo source, IPkgInfo target, DiffOptions diffOptions, string outputDir)
		{
			throw new NotImplementedException("CreateDiff with IPkgInfo instances is deprecated");
		}

		public static DiffError CreateDiff(string sourceCab, string targetCab, DiffOptions diffOptions, string outputCab)
		{
			return CreateDiff(sourceCab, targetCab, diffOptions, null, outputCab);
		}

		public static DiffError CreateDiff(string sourceCab, string targetCab, DiffOptions diffOptions, Dictionary<DiffOptions, object> diffOptionValues, string outputCab)
		{
			if (string.IsNullOrEmpty(sourceCab))
			{
				throw new ArgumentException("CreateDiff: path of source package can't be null or empty", "sourceCab");
			}
			if (string.IsNullOrEmpty(targetCab))
			{
				throw new ArgumentException("CreateDiff: path of target package can't be null or empty", "targetCab");
			}
			if (string.IsNullOrEmpty(outputCab))
			{
				throw new ArgumentException("CreateDiff: path of the output package can't be null or empty", "outputCab");
			}
			if (!LongPathFile.Exists(sourceCab))
			{
				throw new PackageException("CreateDiff: source package '{0}' doesn't exist", sourceCab);
			}
			if (!LongPathFile.Exists(targetCab))
			{
				throw new PackageException("CreateDiff: target package '{0}' doesn't exist", targetCab);
			}
			return DiffPkgBuilder.CreateDiff(sourceCab, targetCab, diffOptions, diffOptionValues, outputCab);
		}

		public static void MergePackage(string[] inputPkgs, string outputDir, VersionInfo? version, ReleaseType releaseType, CpuId cpuType, BuildType buildType, bool compress)
		{
			if (inputPkgs == null)
			{
				throw new ArgumentNullException("inputPkgs", "MergePackage: inputPkgs can not be null");
			}
			if (string.IsNullOrEmpty(outputDir))
			{
				throw new ArgumentException("MergePackage: outputDir can not be null or empty", "outputDir");
			}
			for (int i = 0; i < inputPkgs.Length; i++)
			{
				if (string.IsNullOrEmpty(inputPkgs[i]))
				{
					throw new ArgumentException("MergePackage: inputPkgs can't contain null or empty paths", "inputPkgs");
				}
			}
			PkgMerger.Merge(inputPkgs, version, releaseType, cpuType, buildType, outputDir, compress, Logger);
		}

		public static MergeResult[] MergePackage(string[] inputPkgs, string outputDir, string featureKey, VersionInfo version, string ownerOverride, OwnerType ownerTypeOverride, ReleaseType releaseType, CpuId cpuType, BuildType buildType, bool compress, bool incremental)
		{
			if (inputPkgs == null)
			{
				throw new ArgumentNullException("inputPkgs", "MergePackage: inputPkgs can not be null");
			}
			if (string.IsNullOrEmpty(outputDir))
			{
				throw new ArgumentException("MergePackage: outputDir can not be null or empty", "outputDir");
			}
			if (string.IsNullOrEmpty(featureKey))
			{
				throw new ArgumentException("MergePackage: featureKey can not be null or empty", "featureKey");
			}
			if (!string.IsNullOrEmpty(ownerOverride) && ownerTypeOverride == OwnerType.Invalid)
			{
				throw new ArgumentException("MergePackage: OwnerType override can not be invalid", "ownerTypeOverride");
			}
			for (int i = 0; i < inputPkgs.Length; i++)
			{
				if (string.IsNullOrEmpty(inputPkgs[i]))
				{
					throw new ArgumentException("MergePackage: inputPkgs can't contain null or empty paths", "inputPkgs");
				}
			}
			return FBMerger.Merge(inputPkgs, featureKey, version, ownerOverride, ownerTypeOverride, releaseType, cpuType, buildType, outputDir, compress, incremental);
		}
	}
}
