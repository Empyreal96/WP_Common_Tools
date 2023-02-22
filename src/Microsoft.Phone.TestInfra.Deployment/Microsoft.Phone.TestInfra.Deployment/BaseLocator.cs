using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class BaseLocator
	{
		private const string DeployTestCacheFileLockFormat = "DeployTestLocator_{0}";

		private const int DefaultRetryCount = 6;

		private static readonly TimeSpan DefaultRetryDelay;

		private static readonly string BinaryLocationCacheFolder;

		private static readonly string ImmutableCacheFolderName;

		private static string immutableCacheFolder;

		private static string volatileCacheFolder;

		static BaseLocator()
		{
			DefaultRetryDelay = TimeSpan.FromMilliseconds(200.0);
			BinaryLocationCacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DeployTest\\LocationCache");
			ImmutableCacheFolderName = "Immutable";
			volatileCacheFolder = Path.Combine(BinaryLocationCacheFolder, Guid.NewGuid().GetHashCode().ToString());
			immutableCacheFolder = Path.Combine(BinaryLocationCacheFolder, ImmutableCacheFolderName);
			CleanCacheFiles();
		}

		internal static void WriteCacheFile(string cacheFile, SerializableDictionary<string, HashSet<string>> locatorInfo)
		{
			if (locatorInfo == null)
			{
				throw new ArgumentNullException("locatorInfo");
			}
			using (ReadWriteResourceLock readWriteResourceLock = CreateLockForIndexFile(cacheFile))
			{
				readWriteResourceLock.AcquireWriteLock(TimeSpan.FromSeconds(15.0));
				RetryHelper.Retry(delegate
				{
					locatorInfo.SerializeToFile(cacheFile);
				}, 6, DefaultRetryDelay, new Type[1] { typeof(IOException) });
			}
		}

		internal static string GetLocalGeneralCacheFileFullPath(string cacheFileName)
		{
			return Path.Combine(BinaryLocationCacheFolder, cacheFileName);
		}

		internal static SerializableDictionary<string, HashSet<string>> ReadGeneralCacheFile(string cacheFileName)
		{
			string text = Path.Combine(Constants.AssemblyDirectory, cacheFileName);
			string localGeneralCacheFileFullPath = GetLocalGeneralCacheFileFullPath(cacheFileName);
			if (!File.Exists(text) && !File.Exists(localGeneralCacheFileFullPath))
			{
				Logger.Info("Cache file {0} is not available, probably it is not generated yet. Moving on", cacheFileName);
				return new SerializableDictionary<string, HashSet<string>>();
			}
			string empty = string.Empty;
			if (File.Exists(text))
			{
				empty = text;
				if (File.Exists(localGeneralCacheFileFullPath))
				{
					DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(text);
					DateTime lastWriteTimeUtc2 = File.GetLastWriteTimeUtc(localGeneralCacheFileFullPath);
					if (DateTime.Compare(lastWriteTimeUtc, lastWriteTimeUtc2) < 0)
					{
						empty = localGeneralCacheFileFullPath;
					}
				}
			}
			else
			{
				empty = localGeneralCacheFileFullPath;
			}
			return ReadCacheFile(empty);
		}

		internal static SerializableDictionary<string, HashSet<string>> ReadCacheFile(string cacheFile)
		{
			SerializableDictionary<string, HashSet<string>> serializableDictionary = new SerializableDictionary<string, HashSet<string>>();
			if (!File.Exists(cacheFile))
			{
				Logger.Info("Cache file {0} is not available, probably it is not generated yet. Moving on", cacheFile);
				return serializableDictionary;
			}
			using (ReadWriteResourceLock readWriteResourceLock = CreateLockForIndexFile(cacheFile))
			{
				readWriteResourceLock.AcquireReadLock(TimeSpan.FromSeconds(15.0));
				try
				{
					SerializableDictionary<string, IEnumerable<string>> serializableDictionary2 = SerializableDictionary<string, IEnumerable<string>>.DeserializeFile(cacheFile);
					foreach (KeyValuePair<string, IEnumerable<string>> item in serializableDictionary2)
					{
						HashSet<string> hashSet = new HashSet<string>();
						hashSet.UnionWith(item.Value);
						serializableDictionary.Add(item.Key, hashSet);
					}
					return serializableDictionary;
				}
				catch (Exception ex)
				{
					Logger.Debug("Error occurred in loading the cache file {0}, skipped. Error: {0}. ", cacheFile, ex.ToString());
					return serializableDictionary;
				}
			}
		}

		internal static bool IsPathImmutable(string rootPath)
		{
			if (string.IsNullOrEmpty(rootPath))
			{
				throw new ArgumentNullException(rootPath);
			}
			if (!ReliableDirectory.Exists(rootPath, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs)))
			{
				throw new InvalidDataException($"Directory {rootPath} not exist.");
			}
			PathType pathType = PathHelper.GetPathType(rootPath);
			return pathType == PathType.PhoneBuildPath || pathType == PathType.WinbBuildPath;
		}

		internal static string GetLocationCacheFilePath(string rootPath, string fileExtension)
		{
			if (string.IsNullOrEmpty(rootPath))
			{
				throw new ArgumentNullException(rootPath);
			}
			if (!ReliableDirectory.Exists(rootPath, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs)))
			{
				throw new InvalidDataException($"Directory {rootPath} not exist.");
			}
			string path = rootPath.ToLowerInvariant().GetHashCode() + fileExtension;
			if (IsPathImmutable(rootPath))
			{
				return Path.Combine(immutableCacheFolder, path);
			}
			return Path.Combine(volatileCacheFolder, path);
		}

		internal static void CleanCacheFiles()
		{
			try
			{
				if (!Directory.Exists(BinaryLocationCacheFolder))
				{
					return;
				}
				using (ReadWriteResourceLock readWriteResourceLock = CreateLockForIndexFile(BinaryLocationCacheFolder))
				{
					readWriteResourceLock.AcquireReadLock(TimeSpan.FromSeconds(15.0));
					string[] directories = Directory.GetDirectories(BinaryLocationCacheFolder);
					foreach (string path in directories)
					{
						if (string.Compare(Path.GetFileName(path), ImmutableCacheFolderName, true) != 0)
						{
							Directory.Delete(path, true);
						}
					}
					if (!Directory.Exists(immutableCacheFolder))
					{
						return;
					}
					string[] files = Directory.GetFiles(immutableCacheFolder, "*.*", SearchOption.AllDirectories);
					string[] array = files;
					foreach (string fileName in array)
					{
						FileInfo fileInfo = new FileInfo(fileName);
						if (fileInfo.LastAccessTime < DateTime.Now.AddDays(-Settings.Default.LocationCacheExpiresInDays))
						{
							fileInfo.Delete();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Error in removing expired binary location files. Error: {0}. Moving on", ex.Message);
			}
		}

		protected static ReadWriteResourceLock CreateLockForIndexFile(string cacheFile)
		{
			return new ReadWriteResourceLock(string.Format(CultureInfo.InvariantCulture, "DeployTestLocator_{0}", new object[1] { Path.GetFileNameWithoutExtension(cacheFile) }));
		}
	}
}
