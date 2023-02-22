using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class PackageExtractor
	{
		private const string PackgeExtractionStringFormat = "PE_{0}";

		private const int DefaultRetryCount = 3;

		private static readonly TimeSpan DefaultRetryDelay = TimeSpan.FromMilliseconds(300.0);

		public string ExtractPackage(string packagePath, string extractionPath, out bool newPackage)
		{
			if (string.IsNullOrEmpty(packagePath))
			{
				throw new ArgumentNullException("packagePath");
			}
			if (string.IsNullOrEmpty(extractionPath))
			{
				throw new ArgumentNullException("extractionPath");
			}
			if (!File.Exists(packagePath))
			{
				throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, "Package does not exist: {0}", new object[1] { packagePath }));
			}
			extractionPath = new DirectoryInfo(extractionPath).FullName;
			string fileNameWithoutExtension = PathHelper.GetFileNameWithoutExtension(packagePath, ".spkg");
			string text = GenerateShortName(fileNameWithoutExtension);
			string text2 = Path.Combine(extractionPath, text);
			using (Mutex mutex = CreatePackageExtractionLock(packagePath))
			{
				Logger.Debug("Locking package for extraction");
				mutex.Acquire(TimeSpan.FromMinutes(15.0));
				try
				{
					long num = new FileInfo(packagePath).LastWriteTimeUtc.ToBinary();
					newPackage = num != GetDeployedPackageTime(extractionPath, text);
					if (newPackage)
					{
						Directory.CreateDirectory(text2);
						IPkgInfo pkgInfo = RetryHelper.Retry(() => Package.LoadFromCab(packagePath), 3, DefaultRetryDelay);
						string text3 = text2;
						if (pkgInfo.Partition.Equals("Data", StringComparison.OrdinalIgnoreCase))
						{
							text3 = PathHelper.Combine(text3, "data");
						}
						pkgInfo.ExtractAll("\\\\?\\" + text3, true);
						SetDeployedPackageTime(extractionPath, text, num);
						Logger.Debug("Extracted {0}, {1} Files", fileNameWithoutExtension, pkgInfo.FileCount);
					}
					else
					{
						Logger.Debug("Package {0} is already extracted", fileNameWithoutExtension);
					}
				}
				finally
				{
					mutex.ReleaseMutex();
				}
			}
			return text2;
		}

		private long GetDeployedPackageTime(string path, string packageShortName)
		{
			string path2 = Path.Combine(path, packageShortName + ".metadata");
			if (!File.Exists(path2))
			{
				return 0L;
			}
			string s = ReliableFile.ReadAllText(path2, 3, DefaultRetryDelay);
			long result;
			return long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out result) ? result : 0;
		}

		private void SetDeployedPackageTime(string path, string packageShortName, long value)
		{
			string path2 = Path.Combine(path, packageShortName + ".metadata");
			ReliableFile.WriteAllText(path2, value.ToString(CultureInfo.InvariantCulture), 3, DefaultRetryDelay);
		}

		private string GenerateShortName(string package)
		{
			return package.Trim().ToLowerInvariant().GetHashCode()
				.ToString(CultureInfo.InvariantCulture);
		}

		private Mutex CreatePackageExtractionLock(string packagePath)
		{
			return new Mutex(false, string.Format(CultureInfo.InvariantCulture, "PE_{0}", new object[1] { packagePath.ToLowerInvariant().GetHashCode() }));
		}
	}
}
