using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary;
using Microsoft.Phone.Test.TestMetadata.Helper;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class BinaryLocator : BaseLocator
	{
		private static bool ignoreResourceSpecificPackages;

		private static bool ignoreLocaleSpecificPackages;

		private static DependencySuppression depSupress;

		private static SerializableDictionary<string, HashSet<string>> generalBinaryLocationCache;

		private PackageLocator packageLocator;

		static BinaryLocator()
		{
			ignoreResourceSpecificPackages = true;
			ignoreLocaleSpecificPackages = true;
			depSupress = null;
			string text = Path.Combine(Constants.AssemblyDirectory, Constants.SupressionFileName);
			if (File.Exists(text))
			{
				depSupress = new DependencySuppression(text);
			}
			else
			{
				Logger.Error("File {0} is missing.", text);
			}
			generalBinaryLocationCache = BaseLocator.ReadGeneralCacheFile(Constants.GeneralBinaryPackageMappingCacheFileName);
		}

		public BinaryLocator(PackageLocator packageLocator)
		{
			if (packageLocator == null)
			{
				throw new ArgumentNullException("packageLocator");
			}
			this.packageLocator = packageLocator;
		}

		public static Dictionary<string, PackageDescription> ScanBuild(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException("path");
			}
			if (!Directory.Exists(path))
			{
				throw new InvalidDataException($"Directory {path} not exist.");
			}
			Dictionary<string, PackageDescription> packageContents = new Dictionary<string, PackageDescription>();
			string searchPattern = "*" + Constants.ManifestFileExtension;
			IEnumerable<string> subdirsLower = Constants.DependencyProjects.Select((string x) => x.ToLowerInvariant());
			IEnumerable<string> enumerable = from x in ReliableDirectory.GetDirectories(path, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs))
				where subdirsLower.Contains(Path.GetFileName(x).ToLowerInvariant())
				select x;
			Queue queue = new Queue();
			Action action = delegate
			{
				while (queue.Count > 0)
				{
					string text = (string)queue.Dequeue();
					try
					{
						Logger.Debug("Loading {0}", text);
						LoadManifest(text, packageContents, path);
					}
					catch (Exception ex2)
					{
						Logger.Warning("Error in Loading file {0}, error: {1}.", text, ex2.Message);
					}
				}
			};
			queue = Queue.Synchronized(queue);
			foreach (string item in enumerable)
			{
				string[] files = ReliableDirectory.GetFiles(item, searchPattern, SearchOption.AllDirectories, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs));
				string[] array = files;
				foreach (string obj in array)
				{
					queue.Enqueue(obj);
				}
			}
			List<IAsyncResult> list = new List<IAsyncResult>();
			for (int j = 0; j < Constants.NumOfLoaders; j++)
			{
				list.Add(action.BeginInvoke(null, null));
			}
			foreach (IAsyncResult item2 in list)
			{
				try
				{
					action.EndInvoke(item2);
				}
				catch (Exception ex)
				{
					Logger.Error(ex.ToString());
				}
			}
			Logger.Info("PERF: Loading Finish for path {0} @ {1}", path, DateTime.UtcNow);
			return packageContents;
		}

		public static SerializableDictionary<string, HashSet<string>> ComputeBinaryPackageMapping(Dictionary<string, PackageDescription> packageContents)
		{
			if (packageContents == null)
			{
				throw new ArgumentNullException("packageContents");
			}
			SerializableDictionary<string, HashSet<string>> serializableDictionary = new SerializableDictionary<string, HashSet<string>>();
			foreach (KeyValuePair<string, PackageDescription> packageContent in packageContents)
			{
				string key = packageContent.Key;
				foreach (string binary in packageContent.Value.Binaries)
				{
					string key2 = binary.ToLowerInvariant();
					if (serializableDictionary.ContainsKey(key2))
					{
						Logger.Debug("File {0} appears in package(s) {1} and {2}. Package owner should be notified.", binary, string.Join(";", serializableDictionary[key2]), packageContent.Key);
						serializableDictionary[key2].Add(key);
					}
					else
					{
						serializableDictionary.Add(key2, new HashSet<string> { packageContent.Key.ToLowerInvariant() });
					}
				}
			}
			return serializableDictionary;
		}

		public static void LoadManifest(string manifestFile, Dictionary<string, PackageDescription> packageContents, string rootPath)
		{
			if (packageContents == null)
			{
				throw new ArgumentNullException("packageContents");
			}
			if (!File.Exists(manifestFile))
			{
				throw new FileNotFoundException(manifestFile);
			}
			string fileName = Path.GetFileName(manifestFile);
			string fileNameWithoutExtension = PathHelper.GetFileNameWithoutExtension(fileName, Constants.ManifestFileExtension);
			PackageDescription packageDescription = ReadPackageDescriptionFromManifestFile(manifestFile, rootPath);
			if (packageDescription == null)
			{
				return;
			}
			lock (packageContents)
			{
				if (packageContents.ContainsKey(fileNameWithoutExtension))
				{
					Logger.Debug($"Found more than one package named {fileNameWithoutExtension}. The package owner should be notified.");
				}
				else
				{
					packageContents.Add(fileNameWithoutExtension.ToLowerInvariant(), packageDescription);
				}
			}
		}

		public static PackageDescription ReadPackageDescriptionFromManifestFile(string manifestFile, string rootPath)
		{
			if (string.IsNullOrEmpty(manifestFile))
			{
				throw new ArgumentNullException("manifestFile");
			}
			string fileName = Path.GetFileName(manifestFile);
			string fileNameWithoutExtension = PathHelper.GetFileNameWithoutExtension(fileName, Constants.ManifestFileExtension);
			string directoryName = Path.GetDirectoryName(manifestFile);
			string path = Path.Combine(directoryName, fileNameWithoutExtension + Constants.SpkgFileExtension);
			if (depSupress != null && depSupress.IsPackageSupressed(fileNameWithoutExtension))
			{
				Logger.Debug("Package {0} is suppressed, so skipped.", fileNameWithoutExtension);
				return null;
			}
			if (ignoreLocaleSpecificPackages && IsLocaleSpecificPackage(directoryName))
			{
				Logger.Debug("Package {0} is a locale specific package, so skipped.", fileNameWithoutExtension);
				return null;
			}
			if (ignoreResourceSpecificPackages && IsResourceSpecificPackage(directoryName))
			{
				Logger.Debug("Package {0} is a resource specific package, so skipped.", fileNameWithoutExtension);
				return null;
			}
			PackageDescription packageDescription = new PackageDescription();
			packageDescription.RelativePath = (string.IsNullOrEmpty(rootPath) ? string.Empty : PathHelper.ChangeParent(path, rootPath, string.Empty));
			PackageDescription packageDescription2 = packageDescription;
			XmlDocument manifestXml = new XmlDocument();
			RetryHelper.Retry(delegate
			{
				manifestXml.Load(manifestFile);
			}, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs));
			XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(manifestXml.NameTable);
			xmlNamespaceManager.AddNamespace("iu", manifestXml.DocumentElement.NamespaceURI);
			XmlNodeList xmlNodeList = manifestXml.SelectNodes("//iu:Package/iu:Files/iu:FileEntry/iu:DevicePath", xmlNamespaceManager);
			foreach (XmlNode item in xmlNodeList)
			{
				string fileName2 = Path.GetFileName(item.InnerText);
				string text = Path.GetExtension(fileName2).ToLowerInvariant();
				if (text == ".sys" || text == ".exe" || text == ".dll")
				{
					packageDescription2.Binaries.Add(fileName2.ToLowerInvariant());
				}
			}
			XmlNodeList xmlNodeList2 = manifestXml.SelectNodes("//iu:Package/iu:Dependencies", xmlNamespaceManager);
			if (xmlNodeList2.Count == 0)
			{
				Logger.Debug("Manifest File {0} does not contain dependency info. ");
			}
			else
			{
				xmlNodeList2 = manifestXml.SelectNodes("//iu:Package/iu:Dependencies/iu:Binary", xmlNamespaceManager);
				foreach (XmlNode item2 in xmlNodeList2.Cast<XmlNode>())
				{
					string text2 = item2.Attributes["Name"].Value ?? string.Empty;
					if (!string.IsNullOrEmpty(text2))
					{
						packageDescription2.Dependencies.Add(new BinaryDependency
						{
							FileName = text2.ToLowerInvariant()
						});
					}
				}
				xmlNodeList2 = manifestXml.SelectNodes("//iu:Package/iu:Dependencies/iu:Package", xmlNamespaceManager);
				foreach (XmlNode item3 in xmlNodeList2.Cast<XmlNode>())
				{
					string text3 = item3.Attributes["Name"].Value ?? string.Empty;
					if (!string.IsNullOrEmpty(text3))
					{
						text3 = text3.ToLowerInvariant();
						packageDescription2.Dependencies.Add(new PackageDependency
						{
							PkgName = text3,
							RelativePath = string.Empty
						});
					}
				}
				xmlNodeList2 = manifestXml.SelectNodes("//iu:Package/iu:Dependencies/iu:RemoteFile", xmlNamespaceManager);
				foreach (XmlNode item4 in xmlNodeList2.Cast<XmlNode>())
				{
					packageDescription2.Dependencies.Add(new RemoteFileDependency
					{
						SourcePath = ((item4.Attributes["SourcePath"] != null) ? item4.Attributes["SourcePath"].Value.ToLowerInvariant() : string.Empty),
						Source = ((item4.Attributes["Source"] != null) ? item4.Attributes["Source"].Value.ToLowerInvariant() : string.Empty),
						DestinationPath = ((item4.Attributes["DestinationPath"] != null) ? item4.Attributes["DestinationPath"].Value.ToLowerInvariant() : string.Empty),
						Destination = ((item4.Attributes["Destination"] != null) ? item4.Attributes["Destination"].Value.ToLowerInvariant() : string.Empty),
						Tags = ((item4.Attributes["Tags"] != null) ? item4.Attributes["Tags"].Value.ToLowerInvariant() : string.Empty)
					});
				}
				xmlNodeList2 = manifestXml.SelectNodes("//iu:Package/iu:Dependencies/iu:EnvrionmentPath", xmlNamespaceManager);
				foreach (XmlNode item5 in xmlNodeList2.Cast<XmlNode>())
				{
					string text4 = item5.Attributes["Name"].Value ?? string.Empty;
					if (!string.IsNullOrEmpty(text4))
					{
						packageDescription2.Dependencies.Add(new EnvironmentPathDependency
						{
							EnvironmentPath = text4
						});
					}
				}
			}
			return packageDescription2;
		}

		public PackageInfo FindContainingPackage(string binary)
		{
			if (string.IsNullOrEmpty(binary))
			{
				throw new ArgumentException("cannot be null or empty", "binary");
			}
			PackageInfo packageInfo = null;
			string text = binary.ToLowerInvariant();
			if (depSupress != null && depSupress.IsFileSupressed(text))
			{
				return null;
			}
			if (packageLocator.AltRootPaths != null && packageLocator.AltRootPaths.Any())
			{
				packageInfo = SearchContainingPackageInRootPathSet(text, packageLocator.AltRootPaths);
			}
			if (packageInfo != null)
			{
				return packageInfo;
			}
			return packageInfo = SearchContainingPackageInRootPathSet(text, packageLocator.RootPaths);
		}

		internal static void UpdateGeneralBinaryLocatorCacheFile()
		{
			string localGeneralCacheFileFullPath = BaseLocator.GetLocalGeneralCacheFileFullPath(Constants.GeneralBinaryPackageMappingCacheFileName);
			using (ReadWriteResourceLock readWriteResourceLock = BaseLocator.CreateLockForIndexFile(localGeneralCacheFileFullPath))
			{
				readWriteResourceLock.AcquireWriteLock(TimeSpan.FromMinutes(1.0));
				generalBinaryLocationCache.SerializeToFile(localGeneralCacheFileFullPath);
			}
		}

		private static HashSet<string> SearchBinaryByScanningBuild(string binary, string path)
		{
			Dictionary<string, PackageDescription> packageContents = ScanBuild(path);
			SerializableDictionary<string, HashSet<string>> serializableDictionary = ComputeBinaryPackageMapping(packageContents);
			BaseLocator.WriteCacheFile(BaseLocator.GetLocationCacheFilePath(path, Constants.BinaryLocationCacheExtension), serializableDictionary);
			return serializableDictionary.ContainsKey(binary) ? serializableDictionary[binary] : null;
		}

		private static bool IsLocaleSpecificPackage(string path)
		{
			return path.IndexOf("_Lang_", StringComparison.OrdinalIgnoreCase) != -1;
		}

		private static bool IsResourceSpecificPackage(string path)
		{
			return path.IndexOf("_RES_", StringComparison.OrdinalIgnoreCase) != -1;
		}

		private static bool PackageContainsBinary(string binaryName, string packageAbsolutPath)
		{
			if (string.IsNullOrWhiteSpace(binaryName))
			{
				throw new ArgumentException("cannot be null or empty.", "binaryName");
			}
			if (string.IsNullOrWhiteSpace(packageAbsolutPath))
			{
				throw new ArgumentException("cannot be null or empty.", "packageAbsolutPath");
			}
			if (string.Compare(Path.GetExtension(packageAbsolutPath), Constants.SpkgFileExtension, true) != 0 && string.Compare(Path.GetExtension(packageAbsolutPath), Constants.CabFileExtension, true) != 0)
			{
				throw new InvalidDataException($"{packageAbsolutPath} is not a spkg or cab file.");
			}
			if (!ReliableFile.Exists(packageAbsolutPath, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs)))
			{
				throw new FileNotFoundException(packageAbsolutPath);
			}
			string text = Path.ChangeExtension(packageAbsolutPath, Constants.ManifestFileExtension);
			if (!ReliableFile.Exists(text, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs)))
			{
				throw new FileNotFoundException(text);
			}
			PackageDescription packageDescription = ReadPackageDescriptionFromManifestFile(text, string.Empty);
			return packageDescription.Binaries.Any((string x) => string.Compare(x, binaryName, true) == 0);
		}

		private PackageInfo SearchContainingPackageInRootPathSet(string binaryName, IEnumerable<string> rootPathSet)
		{
			if (generalBinaryLocationCache != null && generalBinaryLocationCache.ContainsKey(binaryName))
			{
				foreach (string item in generalBinaryLocationCache[binaryName])
				{
					PackageInfo packageInfo = packageLocator.FindPackage(item, rootPathSet);
					if (packageInfo != null && PackageContainsBinary(binaryName, packageInfo.AbsolutePath))
					{
						return packageInfo;
					}
				}
			}
			foreach (string item2 in rootPathSet)
			{
				HashSet<string> hashSet = SearchContainingPackageInRootPath(binaryName, item2);
				if (hashSet == null || !hashSet.Any())
				{
					continue;
				}
				foreach (string item3 in hashSet)
				{
					PackageInfo packageInfo = packageLocator.FindPackage(item3, item2);
					if (packageInfo != null && PackageContainsBinary(binaryName, packageInfo.AbsolutePath))
					{
						return packageInfo;
					}
				}
			}
			Logger.Warning("Did not find binary {0} in any package. Moving on.", binaryName);
			return null;
		}

		private HashSet<string> SearchContainingPackageInRootPath(string binaryName, string rootPath)
		{
			if (string.IsNullOrWhiteSpace(rootPath))
			{
				throw new ArgumentException("Cannot be null or empty", "rootPath");
			}
			if (!Directory.Exists(rootPath))
			{
				throw new DirectoryNotFoundException(rootPath);
			}
			string locationCacheFilePath = BaseLocator.GetLocationCacheFilePath(rootPath, Constants.BinaryLocationCacheExtension);
			if (File.Exists(locationCacheFilePath))
			{
				SerializableDictionary<string, HashSet<string>> serializableDictionary = BaseLocator.ReadCacheFile(locationCacheFilePath);
				return serializableDictionary.ContainsKey(binaryName) ? serializableDictionary[binaryName] : null;
			}
			return SearchBinaryByScanningBuild(binaryName, rootPath);
		}
	}
}
