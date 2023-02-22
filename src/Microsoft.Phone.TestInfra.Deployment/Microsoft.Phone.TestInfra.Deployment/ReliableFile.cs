using System;
using System.IO;
using System.Text;
using Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary;
using Microsoft.Tools.IO;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public static class ReliableFile
	{
		public static bool Exists(string path, int retryCount, TimeSpan retryDelay)
		{
			return RetryHelper.Retry(() => LongPathFile.Exists(path), retryCount, retryDelay, new Type[1] { typeof(IOException) });
		}

		public static string ReadAllText(string path, int retryCount, TimeSpan retryDelay)
		{
			return RetryHelper.Retry(delegate
			{
				using (FileStream stream = LongPathFile.Open(path, FileMode.Open, FileAccess.Read))
				{
					using (StreamReader streamReader = new StreamReader(stream, Encoding.UTF8))
					{
						return streamReader.ReadToEnd();
					}
				}
			}, retryCount, retryDelay, new Type[1] { typeof(IOException) });
		}

		public static void WriteAllText(string path, string contents, int retryCount, TimeSpan retryDelay)
		{
			RetryHelper.Retry(delegate
			{
				using (FileStream stream = LongPathFile.Open(path, FileMode.OpenOrCreate, FileAccess.Write))
				{
					using (StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8))
					{
						streamWriter.Write(contents);
					}
				}
			}, retryCount, retryDelay, new Type[1] { typeof(IOException) });
		}
	}
}
