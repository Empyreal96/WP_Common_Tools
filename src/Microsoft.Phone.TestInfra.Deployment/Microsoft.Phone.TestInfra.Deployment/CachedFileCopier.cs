using System;
using Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class CachedFileCopier
	{
		private readonly CacheManager cacheManager;

		public int CopyRetryCount
		{
			get
			{
				return cacheManager.CopyRetryCount;
			}
			set
			{
				cacheManager.CopyRetryCount = value;
			}
		}

		public TimeSpan CopyRetryDelay
		{
			get
			{
				return cacheManager.CopyRetryDelay;
			}
			set
			{
				cacheManager.CopyRetryDelay = value;
			}
		}

		public CachedFileCopier(string cacheRoot)
		{
			if (string.IsNullOrEmpty(cacheRoot))
			{
				throw new ArgumentNullException("cacheRoot");
			}
			cacheManager = new CacheManager(cacheRoot);
		}

		public void CopyFile(string sourceFile, string targetFile)
		{
			CopyFile(sourceFile, targetFile, null);
		}

		public void CopyFile(string sourceFile, string targetFile, Action<string, string, string> copyToDestination)
		{
			if (string.IsNullOrEmpty(sourceFile))
			{
				throw new ArgumentNullException("sourceFile");
			}
			if (string.IsNullOrEmpty(targetFile))
			{
				throw new ArgumentNullException("targetFile");
			}
			cacheManager.AddFileToCache(sourceFile, delegate(string src, string cached)
			{
				CopyToDestination(sourceFile, cached, targetFile, copyToDestination);
			});
		}

		public void CopyFiles(string source, string destination, bool recursive)
		{
			CopyFiles(source, destination, "*", recursive, null);
		}

		public void CopyFiles(string source, string destination, string pattern, bool recursive)
		{
			CopyFiles(source, destination, pattern, recursive, null);
		}

		public void CopyFiles(string source, string destination, string pattern, bool recursive, Action<string, string, string> copyToDestination)
		{
			if (string.IsNullOrEmpty(source))
			{
				throw new ArgumentNullException("source");
			}
			if (string.IsNullOrEmpty(destination))
			{
				throw new ArgumentNullException("destination");
			}
			source = PathHelper.EndWithDirectorySeparator(source);
			destination = PathHelper.EndWithDirectorySeparator(destination);
			pattern = (string.IsNullOrEmpty(pattern) ? "*" : pattern);
			Action<string, string> callback = delegate(string srcFile, string cachedFile)
			{
				string targetFile = PathHelper.ChangeParent(srcFile, source, destination);
				CopyToDestination(srcFile, cachedFile, targetFile, copyToDestination);
			};
			cacheManager.AddFilesToCache(source, pattern, recursive, callback);
		}

		private void CopyToDestination(string sourceFile, string cachedFile, string targetFile, Action<string, string, string> userAction)
		{
			try
			{
				if (userAction == null)
				{
					FileCopyHelper.CopyFile(cachedFile, targetFile, CopyRetryCount, CopyRetryDelay);
				}
				else
				{
					RetryHelper.Retry(delegate
					{
						userAction(sourceFile, cachedFile, targetFile);
					}, CopyRetryCount, CopyRetryDelay);
				}
				Logger.Debug("Copied: {0} to {1}", sourceFile, targetFile);
			}
			catch (Exception ex)
			{
				Logger.Error("Unable to copy file {0}: {1}", sourceFile, ex.Message);
				throw;
			}
		}
	}
}
