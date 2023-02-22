using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public static class PathCleaner
	{
		private struct IndexEntry
		{
			public string Path { get; set; }

			public long Timestamp { get; set; }

			public bool Equals(IndexEntry other)
			{
				return string.Equals(Path, other.Path, StringComparison.OrdinalIgnoreCase);
			}

			public override bool Equals(object obj)
			{
				if (obj == null)
				{
					return false;
				}
				return obj is IndexEntry && Equals((IndexEntry)obj);
			}

			public override int GetHashCode()
			{
				return (Path != null) ? Path.ToLowerInvariant().GetHashCode() : 0;
			}
		}

		private const string DirectoriesIndexLockName = "PC_DirectoriesIndex";

		private const string DirectoriesListMutexName = "PC_Directories";

		private const string DirectoryMutexNameFormat = "PC_Cleanup_{0}";

		private const int DefaultRetryCount = 5;

		private static readonly TimeSpan DefaultRetryDelay = TimeSpan.FromMilliseconds(200.0);

		private static readonly string DirectoriesIndexFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\\PathCleanup\\DirectoriesToCleanup.txt");

		public static void RegisterForCleanup(IEnumerable<string> paths, TimeSpan expiresIn, TimeSpan timeout)
		{
			if (paths == null || !paths.Any())
			{
				throw new ArgumentNullException("paths");
			}
			TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
			foreach (string path in paths)
			{
				RegisterForCleanup(path, expiresIn, timeoutHelper.Remaining);
			}
		}

		public static void RegisterForCleanup(string path, TimeSpan expiresIn, TimeSpan timeout)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException("path");
			}
			if (expiresIn < TimeSpan.Zero || expiresIn < timeout)
			{
				throw new ArgumentOutOfRangeException("expiresIn", expiresIn, "ExpiresIn is negative or less than timeout");
			}
			TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
			IList<IndexEntry> index;
			using (ReadWriteResourceLock readWriteResourceLock = new ReadWriteResourceLock("PC_DirectoriesIndex"))
			{
				readWriteResourceLock.AcquireReadLock(timeoutHelper.Remaining);
				index = ReadDirectoriesIndex().ToList();
			}
			path = new DirectoryInfo(path).FullName;
			HashSet<IndexEntry> hashSet = new HashSet<IndexEntry>();
			List<IndexEntry> source = GetParentDirectoriesFromIndex(path, index).ToList();
			List<IndexEntry> source2 = GetChildDirectoriesFromIndex(path, index).ToList();
			IEnumerable<Mutex> enumerable = null;
			try
			{
				IList<string> list = new List<string>(source.Select((IndexEntry e) => e.Path));
				list.Add(path);
				enumerable = LockDirectories(list, timeoutHelper.Remaining);
				long val = (source2.Any() ? source2.Max((IndexEntry child) => child.Timestamp) : 0);
				long timeToSet = Math.Max(DateTime.UtcNow.Add(expiresIn).ToBinary(), val);
				hashSet.Add(new IndexEntry
				{
					Path = path,
					Timestamp = timeToSet
				});
				hashSet.UnionWith(source.Where((IndexEntry p) => p.Timestamp < timeToSet).Select(delegate(IndexEntry p)
				{
					IndexEntry result = default(IndexEntry);
					result.Path = p.Path;
					result.Timestamp = timeToSet;
					return result;
				}));
				Directory.CreateDirectory(path);
				using (ReadWriteResourceLock readWriteResourceLock2 = new ReadWriteResourceLock("PC_DirectoriesIndex"))
				{
					readWriteResourceLock2.AcquireWriteLock(timeoutHelper.Remaining);
					HashSet<IndexEntry> hashSet2 = new HashSet<IndexEntry>();
					hashSet2.UnionWith(hashSet);
					hashSet2.UnionWith(ReadDirectoriesIndex());
					WriteDirectoriesIndex(hashSet2);
				}
			}
			finally
			{
				if (enumerable != null)
				{
					foreach (Mutex item in enumerable)
					{
						item.ReleaseMutex();
						item.Dispose();
					}
				}
			}
		}

		public static void CleanupExpiredDirectories(TimeSpan timeout)
		{
			PerformanceCounters.Instance.TimeSpentPurging.Start();
			try
			{
				TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
				IList<IndexEntry> list;
				using (ReadWriteResourceLock readWriteResourceLock = new ReadWriteResourceLock("PC_DirectoriesIndex"))
				{
					if (!readWriteResourceLock.TryToAcquireReadLock())
					{
						return;
					}
					list = ReadDirectoriesIndex().ToList();
				}
				List<IndexEntry> list2 = new List<IndexEntry>();
				foreach (IndexEntry item in list)
				{
					if (timeoutHelper.IsExpired)
					{
						break;
					}
					using (Mutex mutex = CreateDirectoryMutex(item.Path))
					{
						if (!mutex.TryToAcquire())
						{
							continue;
						}
						try
						{
							if (IsExpired(item, timeoutHelper.Remaining) && CleanupDirectory(item.Path))
							{
								list2.Add(item);
							}
						}
						finally
						{
							mutex.ReleaseMutex();
						}
					}
				}
				using (ReadWriteResourceLock readWriteResourceLock2 = new ReadWriteResourceLock("PC_DirectoriesIndex"))
				{
					if (!readWriteResourceLock2.TryToAcquireWriteLock())
					{
						return;
					}
					list = ReadDirectoriesIndex().ToList();
					foreach (IndexEntry item2 in list2)
					{
						list.Remove(item2);
					}
					WriteDirectoriesIndex(list);
				}
			}
			finally
			{
				PerformanceCounters.Instance.TimeSpentPurging.Stop();
			}
		}

		private static bool IsExpired(IndexEntry cachedIndex, TimeSpan timeout)
		{
			if (DateTime.UtcNow.ToBinary() < cachedIndex.Timestamp)
			{
				return false;
			}
			using (ReadWriteResourceLock readWriteResourceLock = new ReadWriteResourceLock("PC_DirectoriesIndex"))
			{
				try
				{
					readWriteResourceLock.AcquireReadLock(timeout);
				}
				catch (TimeoutException)
				{
					return false;
				}
				List<IndexEntry> source = ReadDirectoriesIndex().ToList();
				using (IEnumerator<IndexEntry> enumerator = source.Where((IndexEntry directoryIndex) => string.Equals(directoryIndex.Path, cachedIndex.Path, StringComparison.OrdinalIgnoreCase)).GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						IndexEntry current = enumerator.Current;
						return DateTime.UtcNow.ToBinary() >= current.Timestamp;
					}
				}
			}
			return false;
		}

		private static IEnumerable<Mutex> LockDirectories(IEnumerable<string> directories, TimeSpan timeout)
		{
			TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
			Mutex mutex = new Mutex(false, "PC_Directories");
			mutex.Acquire(timeoutHelper);
			List<Mutex> list = new List<Mutex>();
			try
			{
				list.AddRange(directories.Select(CreateDirectoryMutex));
				WaitHandleHelper.AcquireAll(list, timeoutHelper);
			}
			finally
			{
				mutex.ReleaseMutex();
				mutex.Dispose();
			}
			return list;
		}

		private static bool CleanupDirectory(string path)
		{
			Logger.Info("Directory is expired and will be cleaned up: {0}", path);
			try
			{
				Directory.Delete(path, true);
				return true;
			}
			catch (DirectoryNotFoundException)
			{
				return true;
			}
			catch (Exception ex2)
			{
				Logger.Warning("Unable to delete directory {0}: {1}", path, ex2);
				return false;
			}
		}

		private static Mutex CreateDirectoryMutex(string directory)
		{
			return new Mutex(false, string.Format(CultureInfo.InvariantCulture, "PC_Cleanup_{0}", new object[1] { directory.ToLowerInvariant().GetHashCode() }));
		}

		private static IEnumerable<IndexEntry> ReadDirectoriesIndex()
		{
			return RetryHelper.Retry(delegate
			{
				HashSet<IndexEntry> hashSet = new HashSet<IndexEntry>();
				if (File.Exists(DirectoriesIndexFile))
				{
					foreach (string item2 in from line in File.ReadAllLines(DirectoriesIndexFile)
						where !string.IsNullOrEmpty(line)
						select line)
					{
						try
						{
							IndexEntry item = ParseIndexEntry(item2.Trim());
							hashSet.Add(item);
						}
						catch (InvalidOperationException)
						{
						}
					}
				}
				return hashSet;
			}, 5, DefaultRetryDelay, new Type[1] { typeof(IOException) });
		}

		private static void WriteDirectoriesIndex(IEnumerable<IndexEntry> entries)
		{
			if (entries == null)
			{
				entries = new IndexEntry[0];
			}
			RetryHelper.Retry(delegate
			{
				Directory.CreateDirectory(Path.GetDirectoryName(DirectoriesIndexFile));
				File.WriteAllLines(DirectoriesIndexFile, entries.Select((IndexEntry e) => string.Format(CultureInfo.InvariantCulture, "{0}|{1}", new object[2] { e.Path, e.Timestamp })));
			}, 5, DefaultRetryDelay, new Type[1] { typeof(IOException) });
		}

		private static IEnumerable<IndexEntry> GetParentDirectoriesFromIndex(string path, IEnumerable<IndexEntry> index)
		{
			string pathWithSeparator = PathHelper.EndWithDirectorySeparator(path);
			return index.Where((IndexEntry curEntry) => pathWithSeparator.StartsWith(PathHelper.EndWithDirectorySeparator(curEntry.Path), StringComparison.OrdinalIgnoreCase));
		}

		private static IEnumerable<IndexEntry> GetChildDirectoriesFromIndex(string path, IEnumerable<IndexEntry> index)
		{
			string pathWithSeparator = PathHelper.EndWithDirectorySeparator(path);
			return index.Where((IndexEntry curEntry) => PathHelper.EndWithDirectorySeparator(curEntry.Path).StartsWith(pathWithSeparator, StringComparison.OrdinalIgnoreCase));
		}

		private static IndexEntry ParseIndexEntry(string line)
		{
			string[] array = line.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length != 2)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Invalid entry: {0}", new object[1] { line }));
			}
			IndexEntry indexEntry = default(IndexEntry);
			indexEntry.Path = array[0];
			IndexEntry result = indexEntry;
			long result2;
			if (!long.TryParse(array[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out result2))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Invalid timestamp: {0}", new object[1] { array[1] }));
			}
			result.Timestamp = result2;
			return result;
		}
	}
}
