using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Tools.IO;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class PackageLocator : BaseLocator
	{
		private const int DefaultDirectoryEnumerateRetryCount = 10;

		private static readonly TimeSpan DefaultDirectoryEnumerateRetryDelay;

		private static SerializableDictionary<string, HashSet<string>> generalPackageLocationCache;

		private readonly List<string> cachedDirectories = new List<string>();

		private readonly IDictionary<string, PackageInfo> packageDictionary = new Dictionary<string, PackageInfo>(StringComparer.OrdinalIgnoreCase);

		public IEnumerable<string> RootPaths { get; private set; }

		public IEnumerable<string> AltRootPaths { get; private set; }

		static PackageLocator()
		{
			DefaultDirectoryEnumerateRetryDelay = TimeSpan.FromMinutes(1.0);
			generalPackageLocationCache = BaseLocator.ReadGeneralCacheFile(Constants.GeneralPackageLocationCacheFileName);
		}

		public PackageLocator(IEnumerable<string> rootPaths, IEnumerable<string> altRootPaths = null)
		{
			if (rootPaths == null || !rootPaths.Any() || rootPaths.Any(string.IsNullOrEmpty))
			{
				throw new ArgumentNullException("rootPaths");
			}
			RootPaths = rootPaths.Select((string x) => string.IsNullOrEmpty(PathHelper.GetPrebuiltPath(x)) ? x : PathHelper.GetPrebuiltPath(x));
			AltRootPaths = altRootPaths;
			if (AltRootPaths != null && AltRootPaths.Any())
			{
				AltRootPaths = AltRootPaths.Select((string x) => string.IsNullOrEmpty(PathHelper.GetPrebuiltPath(x)) ? x : PathHelper.GetPrebuiltPath(x));
			}
		}

		public PackageInfo FindPackage(string package)
		{
			if (string.IsNullOrEmpty(package))
			{
				throw new ArgumentNullException("package");
			}
			if (AltRootPaths != null && AltRootPaths.Count() > 0)
			{
				PackageInfo packageInfo = FindPackage(package, AltRootPaths);
				if (packageInfo != null)
				{
					return packageInfo;
				}
			}
			return FindPackage(package, RootPaths);
		}

		public PackageInfo FindPackage(string package, string directory)
		{
			return FindPackage(package, new string[1] { directory });
		}

		public PackageInfo FindPackage(string package, IEnumerable<string> directories)
		{
			PopulateFromCache(directories);
			PackageInfo packageInfoFromDictionary = GetPackageInfoFromDictionary(package);
			if (packageInfoFromDictionary != null)
			{
				return packageInfoFromDictionary;
			}
			packageInfoFromDictionary = GetPackageInfoFromGeneralCache(package, directories);
			if (packageInfoFromDictionary != null)
			{
				return packageInfoFromDictionary;
			}
			foreach (string directory in directories)
			{
				string locationCacheFilePath = BaseLocator.GetLocationCacheFilePath(directory, Constants.PackageLocationCacheExtension);
				if (IsDirectoryCached(directory))
				{
					Logger.Info("Cache File {0} for root {1} already loaded. Not scanning this directory again.", locationCacheFilePath, directory);
					continue;
				}
				if (File.Exists(locationCacheFilePath))
				{
					Logger.Info("Loading newly created cache file {0} for root {1}. ", locationCacheFilePath, directory);
					PopulateFromCache(directory);
				}
				else
				{
					Logger.Info("Scanning {0} for packages...", directory);
					ScanDirectory(directory);
				}
				packageInfoFromDictionary = GetPackageInfoFromDictionary(package);
				if (packageInfoFromDictionary == null)
				{
					continue;
				}
				return packageInfoFromDictionary;
			}
			foreach (string directory2 in directories)
			{
				string locationCacheFilePath2 = BaseLocator.GetLocationCacheFilePath(directory2, Constants.PackageLocationCacheExtension);
				if (File.Exists(locationCacheFilePath2))
				{
					Logger.Info("Saving package location cache file {0} for root {1} for bug analysis.", locationCacheFilePath2, directory2);
					File.Copy(locationCacheFilePath2, Path.Combine(Constants.AssemblyDirectory, Path.GetFileName(locationCacheFilePath2)), true);
				}
				else
				{
					Logger.Warning("package location cache file {0} for root {1} is missing.", locationCacheFilePath2, directory2);
				}
			}
			return null;
		}

		internal static void UpdateGeneralPackageLocatorCacheFile()
		{
			string localGeneralCacheFileFullPath = BaseLocator.GetLocalGeneralCacheFileFullPath(Constants.GeneralPackageLocationCacheFileName);
			using (ReadWriteResourceLock readWriteResourceLock = BaseLocator.CreateLockForIndexFile(localGeneralCacheFileFullPath))
			{
				readWriteResourceLock.AcquireWriteLock(TimeSpan.FromMinutes(1.0));
				generalPackageLocationCache.SerializeToFile(localGeneralCacheFileFullPath);
			}
		}

		private PackageInfo GetPackageInfoFromDictionary(string package)
		{
			string fileNameWithoutExtension = PathHelper.GetFileNameWithoutExtension(package, ".spkg");
			if (!packageDictionary.ContainsKey(fileNameWithoutExtension))
			{
				return null;
			}
			PackageInfo packageInfo = packageDictionary[fileNameWithoutExtension];
			Logger.Info("Found package {0} in {1}", package, LongPathPath.GetDirectoryName(packageInfo.AbsolutePath));
			if (packageInfo.Count > 1)
			{
				Logger.Warning("Package {0} found {1} times, using one from {2}", package, packageInfo.Count, LongPathPath.GetDirectoryName(packageInfo.AbsolutePath));
			}
			return packageInfo;
		}

		private void ScanDirectory(string rootDirectory)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(rootDirectory);
			if (!ReliableDirectory.Exists(rootDirectory, 10, DefaultDirectoryEnumerateRetryDelay))
			{
				throw new DirectoryNotFoundException(string.Format(CultureInfo.InvariantCulture, "Directory {0} cannot be found", new object[1] { rootDirectory }));
			}
			string fullName = directoryInfo.FullName;
			IEnumerable<string> packageFilesUnderPath = PathHelper.GetPackageFilesUnderPath(fullName);
			List<PackageInfo> list = new List<PackageInfo>();
			SerializableDictionary<string, HashSet<string>> serializableDictionary = new SerializableDictionary<string, HashSet<string>>();
			foreach (string item2 in packageFilesUnderPath)
			{
				PackageInfo packageInfo = new PackageInfo(rootDirectory, PathHelper.ChangeParent(item2, rootDirectory, string.Empty));
				AddPackageInfo(packageInfo);
				list.Add(packageInfo);
				string key = PathHelper.GetPackageNameWithoutExtension(item2).ToLowerInvariant();
				if (serializableDictionary.ContainsKey(key))
				{
					foreach (string item3 in serializableDictionary[key])
					{
						Logger.Debug("There are more than one packages named {0} under {1}. Package owner should be notified.", LongPathPath.GetFileName(item2), fullName);
					}
					serializableDictionary[key].Add(packageInfo.AbsolutePath);
				}
				else
				{
					serializableDictionary.Add(key, new HashSet<string> { packageInfo.AbsolutePath });
				}
			}
			AddCachedDirectory(rootDirectory);
			try
			{
				BaseLocator.WriteCacheFile(BaseLocator.GetLocationCacheFilePath(fullName, Constants.PackageLocationCacheExtension), serializableDictionary);
				foreach (PackageInfo item4 in list)
				{
					string key2 = PathHelper.GetFileNameWithoutExtension(item4.PackageName, Constants.SpkgFileExtension).ToLowerInvariant();
					string item = PathHelper.ChangeParent(LongPathPath.GetDirectoryName(item4.AbsolutePath), fullName, string.Empty).ToLowerInvariant();
					if (generalPackageLocationCache.ContainsKey(key2))
					{
						generalPackageLocationCache[key2].Add(item);
						continue;
					}
					generalPackageLocationCache[key2] = new HashSet<string> { item };
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Unable to create package location cache file: {0}", ex);
			}
		}

		private void AddCachedDirectory(string directory)
		{
			cachedDirectories.Add(directory.ToLower(CultureInfo.InvariantCulture));
		}

		private bool IsDirectoryCached(string directory)
		{
			return cachedDirectories.Contains(directory.ToLower(CultureInfo.InvariantCulture));
		}

		private void PopulateFromCache(string directory)
		{
			try
			{
				string locationCacheFilePath = BaseLocator.GetLocationCacheFilePath(directory, Constants.PackageLocationCacheExtension);
				if (!File.Exists(locationCacheFilePath))
				{
					return;
				}
				SerializableDictionary<string, HashSet<string>> serializableDictionary = BaseLocator.ReadCacheFile(locationCacheFilePath);
				if (!serializableDictionary.Any())
				{
					return;
				}
				AddCachedDirectory(directory);
				foreach (KeyValuePair<string, HashSet<string>> item in serializableDictionary)
				{
					PackageInfo packageInfo = new PackageInfo(directory, PathHelper.ChangeParent(item.Value.ElementAt(0), directory, string.Empty));
					AddPackageInfo(packageInfo);
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Unable to get cached info: {0}", ex);
			}
		}

		private void PopulateFromCache(IEnumerable<string> directories)
		{
			foreach (string directory in directories)
			{
				PopulateFromCache(directory);
			}
		}

		private void AddPackageInfo(PackageInfo packageInfo)
		{
			if (packageDictionary.ContainsKey(packageInfo.PackageName))
			{
				if (!packageDictionary[packageInfo.PackageName].Equals(packageInfo))
				{
					Logger.Debug("Duplicate package {0} found in {1} and {2}", packageInfo.PackageName, LongPathPath.GetDirectoryName(packageDictionary[packageInfo.PackageName].AbsolutePath), LongPathPath.GetDirectoryName(packageInfo.AbsolutePath));
					packageDictionary[packageInfo.PackageName].Count++;
				}
			}
			else
			{
				packageDictionary[packageInfo.PackageName] = packageInfo;
			}
		}

		private PackageInfo GetPackageInfoFromGeneralCache(string package, IEnumerable<string> paths)
		{
			if (generalPackageLocationCache == null)
			{
				throw new InvalidDataException("General Package Location Cache is null");
			}
			string text = package + Constants.SpkgFileExtension;
			string text2 = package + Constants.CabFileExtension;
			if (!generalPackageLocationCache.ContainsKey(text) && !generalPackageLocationCache.ContainsKey(text2))
			{
				return null;
			}
			foreach (string path3 in paths)
			{
				string prebuiltPath = PathHelper.GetPrebuiltPath(path3);
				if (string.IsNullOrWhiteSpace(prebuiltPath))
				{
					continue;
				}
				HashSet<string> hashSet = generalPackageLocationCache[package];
				foreach (string item in hashSet)
				{
					string path = Path.Combine(prebuiltPath, item, text2);
					string path2 = Path.Combine(prebuiltPath, item, text);
					if (ReliableFile.Exists(path2, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs)))
					{
						return new PackageInfo(prebuiltPath, LongPathPath.Combine(item, text));
					}
					if (ReliableFile.Exists(path, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs)))
					{
						return new PackageInfo(prebuiltPath, Path.Combine(item, text2));
					}
				}
			}
			return null;
		}
	}
}
