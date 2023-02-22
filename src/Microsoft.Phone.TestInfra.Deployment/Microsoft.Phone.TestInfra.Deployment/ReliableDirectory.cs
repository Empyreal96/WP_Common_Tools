using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary;
using Microsoft.Tools.IO;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public static class ReliableDirectory
	{
		public static bool Exists(string path, int retryCount, TimeSpan retryDelay)
		{
			return RetryHelper.Retry(() => LongPathDirectory.Exists(path), retryCount, retryDelay, new Type[1] { typeof(IOException) });
		}

		public static string[] GetDirectories(string path, int retryCount, TimeSpan retryDelay)
		{
			return RetryHelper.Retry(() => LongPathDirectory.EnumerateDirectories(path).ToArray(), retryCount, retryDelay, new Type[1] { typeof(IOException) });
		}

		public static string[] GetDirectories(string path, string searchPattern, int retryCount, TimeSpan retryDelay)
		{
			return RetryHelper.Retry(() => LongPathDirectory.EnumerateDirectories(path, searchPattern).ToArray(), retryCount, retryDelay, new Type[1] { typeof(IOException) });
		}

		public static string[] GetFiles(string path, string searchPattern, int retryCount, TimeSpan retryDelay)
		{
			return RetryHelper.Retry(() => LongPathDirectory.EnumerateFiles(path, searchPattern).ToArray(), retryCount, retryDelay, new Type[1] { typeof(IOException) });
		}

		public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption, int retryCount, TimeSpan retryDelay)
		{
			return RetryHelper.Retry(() => LongPathDirectory.EnumerateFiles(path, searchPattern, searchOption).ToArray(), retryCount, retryDelay, new Type[1] { typeof(IOException) });
		}

		public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, int retryCount, TimeSpan retryDelay)
		{
			return RetryHelper.Retry(() => LongPathDirectory.EnumerateFiles(path, searchPattern), retryCount, retryDelay, new Type[1] { typeof(IOException) });
		}

		public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption, int retryCount, TimeSpan retryDelay)
		{
			return RetryHelper.Retry(() => LongPathDirectory.EnumerateFiles(path, searchPattern, searchOption), retryCount, retryDelay, new Type[1] { typeof(IOException) });
		}
	}
}
