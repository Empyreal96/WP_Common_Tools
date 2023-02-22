#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Phone.TestInfra.Deployment;

namespace Microsoft.Phone.TestInfra
{
	public class CacheManager
	{
		private readonly MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

		private Stopwatch timeCopyingToCache = new Stopwatch();

		private Stopwatch timeCopyingToDest = new Stopwatch();

		private Stopwatch timeWaitingOnWritelock = new Stopwatch();

		private Stopwatch timeWaitingOnReadlock = new Stopwatch();

		private Stopwatch timeWaitingOnNetThrottle = new Stopwatch();

		private Stopwatch timeWaitingOnLocalThrottle = new Stopwatch();

		private Stopwatch timeSpentPurging = new Stopwatch();

		private Stopwatch totalElapsedTime = new Stopwatch();

		private int numFilesFound;

		private int cacheMisses;

		private int cacheHits;

		private int filesCopiedToCache;

		private int filesCopiedFromCache;

		private int filesCopiedFromSource;

		private int numRetriesToCache;

		private int numRetriesFromCache;

		private int numRetriesFromSource;

		private string cacheDir;

		private object syncLock = new object();

		public int CacheTimeoutInMs { get; set; }

		public int CacheExpirationInDays { get; set; }

		public int MaxConcurrentDownloads { get; set; }

		public int MaxConcurrentLocalCopies { get; set; }

		public int CopyRetryCount { get; set; }

		public int CopyRetryDelayInMs { get; set; }

		public int MaxConcurrentReaders { get; set; }

		public string DownloadSemaphoreName { get; set; }

		public string LocalCopySemaphoreName { get; set; }

		public bool ContinueOnError { get; set; }

		public string CacheRoot { get; private set; }

		public TimeSpan TotalElapsedTime => totalElapsedTime.Elapsed;

		public TimeSpan TimeSpentPurging => timeSpentPurging.Elapsed;

		public TimeSpan TimeWaitingOnWriteLock => timeWaitingOnWritelock.Elapsed;

		public TimeSpan TimeWaitingOnReadLock => timeWaitingOnReadlock.Elapsed;

		public TimeSpan TimeWaitingOnNetThrottle => timeWaitingOnNetThrottle.Elapsed;

		public TimeSpan TimeWaitingOnLocalThrottle => timeWaitingOnLocalThrottle.Elapsed;

		public TimeSpan TimeCopyingToCache => timeCopyingToCache.Elapsed;

		public TimeSpan TimeCopyingToDest => timeCopyingToDest.Elapsed;

		public int NumFilesFound => numFilesFound;

		public int CacheMisses => cacheMisses;

		public int CacheHits => cacheHits;

		public int FilesCopiedToCache => filesCopiedToCache;

		public int FilesCopiedFromCache => filesCopiedFromCache;

		public int FilesCopiedFromSource => filesCopiedFromSource;

		public int NumRetriesToCache => numRetriesToCache;

		public int NumRetriesFromCache => numRetriesFromCache;

		public int NumRetriesFromSource => numRetriesFromSource;

		public event Action<DateTime, TraceLevel, string> LogMessage;

		public CacheManager(string cacheRoot)
		{
			CacheRoot = cacheRoot;
			CacheTimeoutInMs = Settings.Default.CacheTimeoutInMs;
			CacheExpirationInDays = Settings.Default.CacheExpirationInDays;
			MaxConcurrentDownloads = Settings.Default.MaxConcurrentDownloads;
			MaxConcurrentLocalCopies = Settings.Default.MaxConcurrentLocalCopies;
			CopyRetryCount = Settings.Default.CopyRetryCount;
			CopyRetryDelayInMs = Settings.Default.CopyRetryDelayInMs;
			MaxConcurrentReaders = Settings.Default.MaxConcurrentReaders;
			DownloadSemaphoreName = Settings.Default.DownloadSemaphoreName;
			LocalCopySemaphoreName = Settings.Default.LocalCopySemaphoreName;
		}

		public string GetCacheDirectory(string pathToSource)
		{
			pathToSource = Path.GetFullPath(pathToSource);
			if (!ExistsOnDisk(pathToSource))
			{
				throw new ArgumentException("pathToSource must contain the path to a valid file or directory on disk.");
			}
			if (File.Exists(pathToSource))
			{
				pathToSource = Path.GetDirectoryName(pathToSource);
			}
			byte[] array = Encoding.ASCII.GetBytes(pathToSource.ToUpperInvariant());
			lock (md5)
			{
				array = md5.ComputeHash(array);
			}
			string path = BitConverter.ToString(array).Replace("-", string.Empty);
			return Path.Combine(CacheRoot, path);
		}

		public string GetCacheLocation(string pathToSource)
		{
			string cacheDirectory = GetCacheDirectory(pathToSource);
			return Directory.Exists(pathToSource) ? cacheDirectory : Path.Combine(cacheDirectory, Path.GetFileName(pathToSource));
		}

		public DateTime GetLastAccess(string pathToSource)
		{
			string timeStampFile = GetTimeStampFile(GetCacheDirectory(pathToSource));
			if (File.Exists(timeStampFile))
			{
				return new FileInfo(timeStampFile).LastWriteTime;
			}
			return DateTime.MinValue;
		}

		public void ResetPerfCounters()
		{
			timeCopyingToCache.Reset();
			timeCopyingToDest.Reset();
			totalElapsedTime.Reset();
			timeSpentPurging.Reset();
			timeWaitingOnWritelock.Reset();
			timeWaitingOnReadlock.Reset();
			timeWaitingOnNetThrottle.Reset();
			timeWaitingOnLocalThrottle.Reset();
			numFilesFound = 0;
			cacheMisses = 0;
			cacheHits = 0;
			filesCopiedToCache = 0;
			filesCopiedFromCache = 0;
			filesCopiedFromSource = 0;
			numRetriesToCache = 0;
			numRetriesFromCache = 0;
			numRetriesFromSource = 0;
		}

		public void CopyFile(string pathToSource, string pathToDest, Action<string, string> readFromCache, Action<string, string> writeToCache)
		{
			CopyFiles(Path.GetDirectoryName(pathToSource), Path.GetFileName(pathToSource), pathToDest, null, readFromCache, writeToCache);
		}

		public void CopyFile(string pathToSource, string pathToDest)
		{
			CopyFiles(Path.GetDirectoryName(pathToSource), Path.GetFileName(pathToSource), pathToDest, null);
		}

		public void CopyFiles(string pathToSource, string pattern, string pathToDest, string exclude)
		{
			CopyFiles(pathToSource, pattern, pathToDest, exclude, HandleCopy, HandleCopy);
		}

		public void CopyFiles(string pathToSource, string pattern, string pathToDest)
		{
			CopyFiles(pathToSource, pattern, pathToDest, null, HandleCopy, HandleCopy);
		}

		public void CopyFiles(string pathToSource, string pattern, string pathToDest, string exclude, Action<string, string> readFromCache, Action<string, string> writeToCache)
		{
			CheckForNullArguments(pathToSource, pathToDest, readFromCache, writeToCache);
			totalElapsedTime.Start();
			TimeoutHelper timeoutHelper = new TimeoutHelper(CacheTimeoutInMs);
			pattern = (string.IsNullOrEmpty(pattern) ? "*" : pattern);
			exclude = exclude ?? string.Empty;
			cacheDir = GetCacheDirectory(pathToSource);
			bool flag = pattern.Contains("*");
			if (flag && File.Exists(pathToDest))
			{
				totalElapsedTime.Stop();
				throw new InvalidOperationException("Cannot copy multiple source files to a single destination file.");
			}
			string[] array = ReliableDirectory.GetFiles(pathToSource, pattern, CopyRetryCount, TimeSpan.FromMilliseconds(CopyRetryDelayInMs));
			if (array.Length == 0)
			{
				AddToLog(TraceLevel.Info, "No files found in {0} matching pattern {1}.", pathToSource, pattern);
				totalElapsedTime.Stop();
				return;
			}
			string[] files = ReliableDirectory.GetFiles(pathToSource, exclude, CopyRetryCount, TimeSpan.FromMilliseconds(CopyRetryDelayInMs));
			if (files.Length != 0)
			{
				List<string> list = new List<string>(array);
				string[] array2 = files;
				foreach (string item in array2)
				{
					list.Remove(item);
				}
				array = list.ToArray();
			}
			numFilesFound += array.Length;
			if (flag && !Directory.Exists(pathToDest))
			{
				try
				{
					Directory.CreateDirectory(pathToDest);
					AddToLog(TraceLevel.Info, "Created destination directory {0}.", pathToDest);
				}
				catch
				{
					AddToLog(TraceLevel.Warning, "Was unable to create destination directory: {0}.", pathToDest);
					if (!Directory.Exists(pathToDest))
					{
						totalElapsedTime.Stop();
						throw;
					}
				}
			}
			bool flag2 = CreateCacheRoot();
			string text = MakeSafe(cacheDir);
			string text2 = "Mutex" + text;
			string text3 = "Semaphore" + text;
			bool createdNew;
			using (Mutex mutex = new Mutex(false, text2, out createdNew))
			{
				Trace.TraceInformation("writeLock {0} was createdNew: {1}.", text2, createdNew);
				AddToLog(TraceLevel.Verbose, "Waiting to write to cache for source folder {0}, cache folder {1}.", pathToSource, cacheDir);
				timeWaitingOnWritelock.Start();
				WaitOneAbandonAware(mutex, text2, timeoutHelper);
				timeWaitingOnWritelock.Stop();
				bool flag3 = true;
				try
				{
					using (Semaphore semaphore = new Semaphore(MaxConcurrentReaders, MaxConcurrentReaders, text3, out createdNew))
					{
						Trace.TraceInformation("readLock {0} was createdNew: {1}.", text3, createdNew);
						AddToLog(TraceLevel.Verbose, "Waiting to read from cache.");
						timeWaitingOnReadlock.Start();
						WaitOneAbandonAware(semaphore, text3, timeoutHelper);
						timeWaitingOnReadlock.Stop();
						try
						{
							if (flag2)
							{
								using (Semaphore semaphore2 = new Semaphore(MaxConcurrentDownloads, MaxConcurrentDownloads, DownloadSemaphoreName, out createdNew))
								{
									Trace.TraceInformation("downloadThrottle {0} was createdNew: {1}.", DownloadSemaphoreName, createdNew);
									timeWaitingOnNetThrottle.Start();
									AddToLog(TraceLevel.Verbose, "Throttling download.");
									WaitOneAbandonAware(semaphore2, DownloadSemaphoreName, timeoutHelper);
									timeWaitingOnNetThrottle.Stop();
									try
									{
										AddToLog(TraceLevel.Verbose, "Copying necessary files to cache.");
										CopyFilesToCache(pathToSource, writeToCache, array);
									}
									finally
									{
										int num = semaphore2.Release();
										Trace.TraceInformation("downloadThrottle {0} was releaseCount: {1}.", DownloadSemaphoreName, num);
									}
								}
							}
						}
						finally
						{
							mutex.ReleaseMutex();
							flag3 = false;
						}
						using (Semaphore semaphore3 = new Semaphore(MaxConcurrentLocalCopies, MaxConcurrentLocalCopies, LocalCopySemaphoreName, out createdNew))
						{
							Trace.TraceInformation("localCopyThrottle {0} was createdNew: {1}.", LocalCopySemaphoreName, createdNew);
							timeWaitingOnLocalThrottle.Start();
							AddToLog(TraceLevel.Verbose, "Throttling local copies.");
							WaitOneAbandonAware(semaphore3, LocalCopySemaphoreName, timeoutHelper);
							timeWaitingOnLocalThrottle.Stop();
							try
							{
								AddToLog(TraceLevel.Verbose, "Copying necessary files to destination.");
								CopyFilesToDestination(pathToDest, readFromCache, array);
							}
							finally
							{
								int num2 = semaphore3.Release();
								Trace.TraceInformation("localCopyThrottle {0} was releaseCount: {1}.", LocalCopySemaphoreName, num2);
							}
						}
						int num3 = semaphore.Release();
						Trace.TraceInformation("readLock {0} was releaseCount: {1}.", text3, num3);
					}
				}
				finally
				{
					if (flag3)
					{
						mutex.ReleaseMutex();
					}
					totalElapsedTime.Stop();
				}
			}
		}

		public void Purge(TimeSpan timeout)
		{
			TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
			timeSpentPurging.Start();
			AddToLog(TraceLevel.Verbose, "Purging cache.");
			foreach (string item in Directory.EnumerateDirectories(CacheRoot))
			{
				string timeStampFile = GetTimeStampFile(item);
				if (!((DateTime.Now - File.GetLastWriteTime(timeStampFile)).TotalDays > (double)CacheExpirationInDays))
				{
					continue;
				}
				string text = MakeSafe(item);
				string text2 = "Mutex" + text;
				string text3 = "Semaphore" + text;
				bool createdNew;
				using (Mutex mutex = new Mutex(false, text2, out createdNew))
				{
					Trace.TraceInformation("writeLock {0} was createdNew: {1}.", text2, createdNew);
					AddToLog(TraceLevel.Verbose, "Waiting to write to cache folder {0}.", item);
					WaitOneAbandonAware(mutex, text2, timeoutHelper);
					try
					{
						using (Semaphore semaphore = new Semaphore(MaxConcurrentReaders, MaxConcurrentReaders, text3, out createdNew))
						{
							Trace.TraceInformation("readLock {0} was createdNew: {1}.", text3, createdNew);
							AddToLog(TraceLevel.Verbose, "Waiting to read from cache folder.", item);
							WaitOneAbandonAware(semaphore, text3, timeoutHelper);
							int num = 0;
							try
							{
							}
							finally
							{
								num = semaphore.Release();
								Trace.TraceInformation("readLock {0} was releaseCount: {1}.", text3, num);
							}
							if (num != MaxConcurrentReaders - 1)
							{
								continue;
							}
							timeStampFile = GetTimeStampFile(item);
							if ((DateTime.Now - File.GetLastWriteTime(timeStampFile)).TotalDays > (double)CacheExpirationInDays)
							{
								try
								{
									AddToLog(TraceLevel.Verbose, "Purging cache folder.", item);
									Directory.Delete(item, true);
								}
								catch (Exception ex)
								{
									AddToLog(TraceLevel.Warning, "Cache Purge failed for {0} with exception {1}", item, ex.Message);
								}
							}
						}
					}
					finally
					{
						mutex.ReleaseMutex();
					}
				}
			}
			timeSpentPurging.Stop();
		}

		private static void CheckForNullArguments(string pathToSource, string pathToDest, Action<string, string> readFromCache, Action<string, string> writeToCache)
		{
			if (pathToSource == null)
			{
				throw new ArgumentNullException("pathToSource");
			}
			if (pathToDest == null)
			{
				throw new ArgumentNullException("pathToDest");
			}
			if (readFromCache == null)
			{
				throw new ArgumentNullException("readFromCache");
			}
			if (writeToCache == null)
			{
				throw new ArgumentNullException("writeToCache");
			}
			if (!Directory.Exists(pathToSource))
			{
				throw new ArgumentException("pathToSource must reference a valid directory.");
			}
		}

		private static string FixDestinationName(string pathToSource, string pathToDest)
		{
			return Directory.Exists(pathToDest) ? Path.Combine(pathToDest, Path.GetFileName(pathToSource)) : pathToDest;
		}

		private static void WaitOneAbandonAware(WaitHandle wh, string name, TimeoutHelper timeoutHelper)
		{
			try
			{
				TimeSpan remaining = timeoutHelper.Remaining;
				Trace.TraceInformation("Now waiting on {0} remaining milliseconds: {1:0.000} seconds", name, remaining.TotalMilliseconds / 1000.0);
				if (remaining <= TimeSpan.Zero || !wh.WaitOne(remaining, false))
				{
					throw new TimeoutException(string.Format(CultureInfo.InvariantCulture, "Timeout waiting for {0} ({1}).", new object[2] { name, timeoutHelper.Status }));
				}
				Trace.TraceInformation("Wait completed for {0} ({1}).", name, timeoutHelper.Status);
			}
			catch (AbandonedMutexException)
			{
				Trace.TraceInformation("Wait completed (because the WaitHandle was abandoned) for {0} ({1}).", name, timeoutHelper.Status);
			}
		}

		private static bool AreMatchingFiles(string source, string destination)
		{
			FileInfo fileInfo = new FileInfo(source);
			FileInfo fileInfo2 = new FileInfo(destination);
			return File.Exists(destination) && fileInfo.Length == fileInfo2.Length && fileInfo.LastWriteTime == fileInfo2.LastWriteTime;
		}

		private static string MakeSafe(string fileName)
		{
			return fileName.Replace('\\', '_').Replace('$', '_').Replace(':', '_')
				.ToUpperInvariant();
		}

		private static bool ExistsOnDisk(string pathToSource)
		{
			return File.Exists(pathToSource) || Directory.Exists(pathToSource);
		}

		private bool CreateCacheRoot()
		{
			if (!Directory.Exists(CacheRoot))
			{
				try
				{
					Directory.CreateDirectory(CacheRoot);
					AddToLog(TraceLevel.Info, "Created cache root {0}.", CacheRoot);
				}
				catch
				{
					AddToLog(TraceLevel.Warning, "Unable to create cache root: {0}. Possibly created by another process.", CacheRoot);
					if (!Directory.Exists(CacheRoot))
					{
						AddToLog(TraceLevel.Warning, "Cache root still does not exist. Caching disabled.");
						return false;
					}
				}
			}
			return true;
		}

		private void CopyFilesToDestination(string pathToDest, Action<string, string> copyFile, string[] sourceFiles)
		{
			try
			{
				timeCopyingToDest.Start();
				Parallel.ForEach(sourceFiles, delegate(string sourceFile)
				{
					string cacheLocation = GetCacheLocation(sourceFile);
					string text = FixDestinationName(sourceFile, pathToDest);
					bool flag = false;
					if (File.Exists(cacheLocation) && !AreMatchingFiles(cacheLocation, text))
					{
						int num2;
						for (int num = 0; num <= CopyRetryCount; num2 = num + 1, num = num2)
						{
							try
							{
								copyFile(cacheLocation, text);
								Interlocked.Increment(ref filesCopiedFromCache);
								AddToLog(TraceLevel.Verbose, "Copied from cache to {0}.", text);
								flag = true;
							}
							catch (Exception ex)
							{
								AddToLog(TraceLevel.Warning, "Unable to copy file {0} to the destination due to {1}.", cacheLocation, ex.Message);
								if ((ex is UnauthorizedAccessException || ex is IOException) && num < CopyRetryCount)
								{
									Thread.Sleep(CopyRetryDelayInMs);
									Interlocked.Increment(ref numRetriesFromCache);
									continue;
								}
							}
							break;
						}
					}
					if (!flag)
					{
						if (!AreMatchingFiles(sourceFile, text))
						{
							int num3 = 0;
							while (num3 <= CopyRetryCount)
							{
								try
								{
									copyFile(sourceFile, text);
									Interlocked.Increment(ref filesCopiedFromSource);
									AddToLog(TraceLevel.Verbose, "Copied from source to {0}.", text);
									break;
								}
								catch (Exception ex2)
								{
									AddToLog(TraceLevel.Warning, "Unable to copy file {0} to the destination due to {1}.", sourceFile, ex2.Message);
									if ((ex2 is UnauthorizedAccessException || ex2 is IOException) && num3 < CopyRetryCount)
									{
										Thread.Sleep(CopyRetryDelayInMs);
										Interlocked.Increment(ref numRetriesFromSource);
									}
									else
									{
										if (!ContinueOnError)
										{
											throw;
										}
										AddToLog(TraceLevel.Warning, "Unable to copy file {0} to destination. Continuing anyways.", sourceFile);
									}
								}
								int num2 = num3 + 1;
								num3 = num2;
							}
						}
						else
						{
							AddToLog(TraceLevel.Verbose, "Source file {0} exists at destination.", sourceFile);
						}
					}
				});
			}
			finally
			{
				timeCopyingToDest.Stop();
			}
		}

		private void CopyFilesToCache(string pathToSource, Action<string, string> copyFile, string[] sourceFiles)
		{
			try
			{
				if (!Directory.Exists(cacheDir))
				{
					Directory.CreateDirectory(cacheDir);
				}
				timeCopyingToCache.Start();
				Parallel.ForEach(sourceFiles, delegate(string sourceFile)
				{
					string cacheLocation = GetCacheLocation(sourceFile);
					if (AreMatchingFiles(sourceFile, cacheLocation))
					{
						AddToLog(TraceLevel.Verbose, "Source file {0}, exists in cache.", sourceFile);
						Interlocked.Increment(ref cacheHits);
					}
					else
					{
						Interlocked.Increment(ref cacheMisses);
						int num = 0;
						while (num <= CopyRetryCount)
						{
							try
							{
								copyFile(sourceFile, cacheLocation);
								Interlocked.Increment(ref filesCopiedToCache);
								AddToLog(TraceLevel.Verbose, "Copied source file {0} to cache.", sourceFile);
								break;
							}
							catch (Exception ex)
							{
								AddToLog(TraceLevel.Warning, "Unable to copy file {0} to the cache due to {1}.", sourceFile, ex.Message);
								if ((!(ex is UnauthorizedAccessException) && !(ex is IOException)) || num >= CopyRetryCount)
								{
									break;
								}
								Thread.Sleep(CopyRetryDelayInMs);
								Interlocked.Increment(ref numRetriesToCache);
							}
							int num2 = num + 1;
							num = num2;
						}
					}
				});
			}
			finally
			{
				timeCopyingToCache.Stop();
				UpdateTimeStamp();
			}
		}

		private void HandleCopy(string pathToSource, string pathToDest)
		{
			File.Copy(pathToSource, pathToDest, true);
		}

		private string GetTimeStampFile(string cacheDir)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(cacheDir);
			return Path.Combine(cacheDir, "TimeStamp_" + directoryInfo.Name.Substring(0, 8));
		}

		private void UpdateTimeStamp()
		{
			string timeStampFile = GetTimeStampFile(cacheDir);
			if (timeStampFile != null)
			{
				FileStream fileStream = File.Create(timeStampFile);
				fileStream.Close();
			}
		}

		private void AddToLog(TraceLevel level, string pattern, params object[] list)
		{
			DateTime now = DateTime.Now;
			string arg = string.Format(pattern, list);
			if (this.LogMessage != null)
			{
				lock (syncLock)
				{
					this.LogMessage(now, level, arg);
				}
			}
		}
	}
}
