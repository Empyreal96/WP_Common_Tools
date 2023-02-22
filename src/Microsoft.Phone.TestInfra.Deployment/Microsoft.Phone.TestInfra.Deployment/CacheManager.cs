using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Tools.IO;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class CacheManager
	{
		private const string DirectoryLockFormat = "CM_{0}";

		private readonly MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

		private int copyRetryCount;

		private TimeSpan copyRetryDelay;

		private TimeSpan cacheTimeout;

		private int maxConcurrentDownloads;

		private string downloadSemaphoreName;

		private int maxConcurrentLocalCopies;

		private string localCopySemaphoreName;

		public TimeSpan CacheTimeout
		{
			get
			{
				return cacheTimeout;
			}
			set
			{
				if (value < TimeSpan.Zero)
				{
					throw new ArgumentOutOfRangeException("value", value, "Cache timeout is negative");
				}
				cacheTimeout = value;
			}
		}

		public int CopyRetryCount
		{
			get
			{
				return copyRetryCount;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException("value", value, "Retry count is negative");
				}
				copyRetryCount = value;
			}
		}

		public TimeSpan CopyRetryDelay
		{
			get
			{
				return copyRetryDelay;
			}
			set
			{
				if (value < TimeSpan.Zero)
				{
					throw new ArgumentOutOfRangeException("value", value, "Retry delay is negative");
				}
				copyRetryDelay = value;
			}
		}

		public int MaxConcurrentDownloads
		{
			get
			{
				return maxConcurrentDownloads;
			}
			set
			{
				if (value < 1)
				{
					throw new ArgumentOutOfRangeException("value", value, "Max concurrent download is zero or negative");
				}
				maxConcurrentDownloads = value;
			}
		}

		public string DownloadSemaphoreName
		{
			get
			{
				return downloadSemaphoreName;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					throw new ArgumentNullException("value");
				}
				downloadSemaphoreName = value;
			}
		}

		public int MaxConcurrentLocalCopies
		{
			get
			{
				return maxConcurrentLocalCopies;
			}
			set
			{
				if (value < 1)
				{
					throw new ArgumentOutOfRangeException("value", value, "Max concurrent local copies is zero or negative");
				}
				maxConcurrentLocalCopies = value;
			}
		}

		public string LocalCopySemaphoreName
		{
			get
			{
				return localCopySemaphoreName;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					throw new ArgumentNullException("value");
				}
				localCopySemaphoreName = value;
			}
		}

		public string CacheRoot { get; private set; }

		public CacheManager(string cacheRoot, TimeSpan? cacheTimeout = null)
		{
			if (string.IsNullOrEmpty(cacheRoot))
			{
				throw new ArgumentNullException("cacheRoot");
			}
			CacheRoot = cacheRoot;
			CacheTimeout = (cacheTimeout.HasValue ? cacheTimeout.Value : TimeSpan.FromMilliseconds(Settings.Default.CacheTimeoutInMs));
			CopyRetryCount = Settings.Default.CopyRetryCount;
			CopyRetryDelay = TimeSpan.FromMilliseconds(Settings.Default.CopyRetryDelayInMs);
			MaxConcurrentDownloads = Settings.Default.MaxConcurrentDownloads;
			DownloadSemaphoreName = Settings.Default.DownloadSemaphoreName;
			MaxConcurrentLocalCopies = Settings.Default.MaxConcurrentLocalCopies;
			LocalCopySemaphoreName = Settings.Default.LocalCopySemaphoreName;
		}

		public void AddFileToCache(string filePath, Action<string, string> callback)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException("filePath");
			}
			if (!LongPathFile.Exists(filePath))
			{
				throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, "Source file does not exist: {0}", new object[1] { filePath }));
			}
			CacheFiles(LongPathPath.GetDirectoryName(filePath), LongPathPath.GetFileName(filePath), new TimeoutHelper(CacheTimeout), callback);
		}

		public void AddFilesToCache(string directory, string pattern, bool recursive, Action<string, string> callback)
		{
			if (directory == null)
			{
				throw new ArgumentNullException("directory");
			}
			if (!Directory.Exists(directory))
			{
				throw new DirectoryNotFoundException(string.Format(CultureInfo.InvariantCulture, "Source directory does not exist: {0}", new object[1] { directory }));
			}
			pattern = (string.IsNullOrEmpty(pattern) ? "*" : pattern);
			bool flag = pattern.Contains("*");
			TimeoutHelper timeoutHelper = new TimeoutHelper(CacheTimeout);
			CacheFiles(directory, pattern, timeoutHelper, callback);
			if (!(flag && recursive))
			{
				return;
			}
			foreach (string item in Directory.EnumerateDirectories(directory, "*", SearchOption.AllDirectories))
			{
				CacheFiles(item, pattern, timeoutHelper, callback);
			}
		}

		private void CacheFiles(string directory, string pattern, TimeoutHelper timeoutHelper, Action<string, string> callback)
		{
			string cacheDir = CreateCacheDirectory(directory, timeoutHelper);
			using (ReadWriteResourceLock readWriteResourceLock = CreateDirectoryLock(cacheDir))
			{
				readWriteResourceLock.AcquireWriteLock(timeoutHelper.Remaining);
				IEnumerable<string> filesAffected = new string[0];
				DoWithNetThrottle(delegate
				{
					try
					{
						IEnumerable<string> skippedFiles;
						filesAffected = FileCopyHelper.CopyFiles(directory, cacheDir, pattern, false, copyRetryCount, copyRetryDelay, out skippedFiles);
						int num = filesAffected.Count();
						int num2 = skippedFiles.Count();
						PerformanceCounters.Instance.AddNumFilesFound(num);
						PerformanceCounters.Instance.AddFilesCopiedToCache(num - num2);
						PerformanceCounters.Instance.AddFilesCopiedFromSource(num - num2);
						PerformanceCounters.Instance.AddCacheHits(num2);
						PerformanceCounters.Instance.AddCacheMisses(num - num2);
					}
					catch (Exception ex)
					{
						Logger.Error("Unable to copy files to cache: {0}", ex);
						throw;
					}
				}, timeoutHelper);
				if (callback == null)
				{
					return;
				}
				DoWithLocalThrottle(delegate
				{
					foreach (string item in filesAffected)
					{
						string sourceFile = PathHelper.ChangeParent(item, cacheDir, directory);
						InvokeCallback(callback, sourceFile, item);
					}
				}, timeoutHelper);
			}
		}

		private void DoWithNetThrottle(Action copyToCacheAction, TimeoutHelper timeoutHelper)
		{
			using (Semaphore semaphore = new Semaphore(MaxConcurrentDownloads, MaxConcurrentDownloads, DownloadSemaphoreName))
			{
				PerformanceCounters.Instance.TimeWaitingOnNetThrottle.Start();
				try
				{
					semaphore.Acquire(timeoutHelper);
				}
				finally
				{
					PerformanceCounters.Instance.TimeWaitingOnNetThrottle.Stop();
				}
				PerformanceCounters.Instance.TimeCopyingToCache.Start();
				try
				{
					copyToCacheAction();
				}
				finally
				{
					PerformanceCounters.Instance.TimeCopyingToCache.Stop();
					semaphore.Release();
				}
			}
		}

		private void DoWithLocalThrottle(Action copyToDestinationAction, TimeoutHelper timeoutHelper)
		{
			using (Semaphore semaphore = new Semaphore(MaxConcurrentLocalCopies, MaxConcurrentLocalCopies, LocalCopySemaphoreName))
			{
				PerformanceCounters.Instance.TimeWaitingOnLocalThrottle.Start();
				try
				{
					semaphore.Acquire(timeoutHelper);
				}
				finally
				{
					PerformanceCounters.Instance.TimeWaitingOnLocalThrottle.Stop();
				}
				PerformanceCounters.Instance.TimeCopyingToDest.Start();
				try
				{
					copyToDestinationAction();
					PerformanceCounters.Instance.AddFilesCopiedFromCache(1);
				}
				finally
				{
					PerformanceCounters.Instance.TimeCopyingToDest.Stop();
					semaphore.Release();
				}
			}
		}

		private void InvokeCallback(Action<string, string> callback, string sourceFile, string cachedFile)
		{
			try
			{
				callback(sourceFile, cachedFile);
			}
			catch (Exception ex)
			{
				Logger.Error("Callback error: {0}", ex);
			}
		}

		private ReadWriteResourceLock CreateDirectoryLock(string path)
		{
			return new ReadWriteResourceLock(string.Format(CultureInfo.InvariantCulture, "CM_{0}", new object[1] { path.ToLowerInvariant().GetHashCode() }));
		}

		private string CreateCacheDirectory(string sourceDir, TimeoutHelper timeoutHelper)
		{
			byte[] array = Encoding.Unicode.GetBytes(PathHelper.EndWithDirectorySeparator(sourceDir).ToLowerInvariant());
			lock (md5)
			{
				array = md5.ComputeHash(array);
			}
			string path = BitConverter.ToString(array).Replace("-", string.Empty);
			string path2 = Path.Combine(CacheRoot, path);
			Directory.CreateDirectory(path2);
			PathCleaner.RegisterForCleanup(path2, CacheTimeout, timeoutHelper.Remaining);
			return PathHelper.EndWithDirectorySeparator(path2);
		}
	}
}
