using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class PackageDeployer : IDisposable
	{
		private readonly TimeSpan filePurgingTimeout = TimeSpan.FromMinutes(10.0);

		private HashSet<string> packagesToInstall;

		private PackageManagerConfiguration packageManagerConfig;

		private bool disposedValue = false;

		public string LogFile { get; private set; }

		public PackageDeployer(PackageDeployerParameters packageDeployerParameters)
		{
			LogFile = (Path.IsPathRooted(packageDeployerParameters.LogFile) ? packageDeployerParameters.LogFile : Path.Combine(packageDeployerParameters.OutputPath, packageDeployerParameters.LogFile));
			Logger.Configure(packageDeployerParameters.ConsoleTraceLevel, packageDeployerParameters.FileTraceLevel, LogFile, false);
			char[] separator = new char[1] { ';' };
			packagesToInstall = (string.IsNullOrWhiteSpace(packageDeployerParameters.Packages) ? new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) : new HashSet<string>(from x in packageDeployerParameters.Packages.Split(separator, StringSplitOptions.RemoveEmptyEntries)
				where !string.IsNullOrWhiteSpace(x)
				select x, StringComparer.InvariantCultureIgnoreCase));
			if (!string.IsNullOrWhiteSpace(packageDeployerParameters.PackageFile))
			{
				if (!ReliableFile.Exists(packageDeployerParameters.PackageFile, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs)))
				{
					throw new FileNotFoundException(packageDeployerParameters.PackageFile);
				}
				foreach (string item in from line in File.ReadLines(packageDeployerParameters.PackageFile)
					select line.Trim() into line
					where !string.IsNullOrWhiteSpace(line)
					select line)
				{
					packagesToInstall.Add(item);
				}
			}
			string empty = string.Empty;
			if (string.IsNullOrWhiteSpace(packageDeployerParameters.CacheRoot))
			{
				empty = packageDeployerParameters.OutputPath;
			}
			else
			{
				empty = FindBestCacheRoot(packageDeployerParameters.CacheRoot, packageDeployerParameters.OutputPath);
				try
				{
					if (!Directory.Exists(empty))
					{
						Directory.CreateDirectory(empty);
					}
				}
				catch (Exception ex)
				{
					Logger.Warning("Exception creating cache directory {0}: {1} ", empty, ex.Message);
				}
			}
			IEnumerable<string> outRootPathSet;
			IEnumerable<string> outAlternateRootPathSet;
			GetAllRootPaths(packageDeployerParameters.RootPaths, packageDeployerParameters.AlternateRoots, out outRootPathSet, out outAlternateRootPathSet);
			packageManagerConfig = new PackageManagerConfiguration
			{
				ExpiresIn = packageDeployerParameters.ExpiresIn,
				RootPaths = outRootPathSet,
				AlternateRootPaths = outAlternateRootPathSet,
				OutputPath = packageDeployerParameters.OutputPath,
				CachePath = empty,
				PackagesExtractionCachePath = Path.Combine(empty, "p2"),
				Macros = GetMacros(packageDeployerParameters.Macros),
				SourceRootIsVolatile = packageDeployerParameters.SourceRootIsVolatile,
				RecursiveDeployment = packageDeployerParameters.Recurse
			};
		}

		public PackageDeployerOutput Run()
		{
			Logger.Info("DeployTest Version {0}", Settings.Default.Version);
			Logger.Debug("OutputDirectory = {0}", packageManagerConfig.OutputPath);
			Logger.Debug("CacheRoot = {0}", packageManagerConfig.CachePath);
			DateTime utcNow = DateTime.UtcNow;
			TelemetryLogging.LogEvent(TelemetryLogging.SpkgDeployTelemetryEventType.SpkgDeployStart, packagesToInstall, packageManagerConfig.RootPaths, new string[0], string.Empty, utcNow);
			PackageDeployerOutput packageDeployerOutput = new PackageDeployerOutput();
			int num = 0;
			string text = string.Empty;
			try
			{
				PathCleaner.CleanupExpiredDirectories(filePurgingTimeout);
				if (packagesToInstall.Count != 0)
				{
					PackageManager packageManager = new PackageManager(packageManagerConfig);
					packageManager.DeployPackages(packagesToInstall);
					num += packageManager.ErrorCount;
					ConfigCommandAggregator configCommandAggregator = new ConfigCommandAggregator();
					packageDeployerOutput.ConfigurationCommands = configCommandAggregator.GetConfigCommands(packageManager.DeployedPackages, packageManagerConfig.OutputPath);
				}
				else
				{
					Logger.Info("No packages specified to deploy");
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Unhandled exception: {0}", ex);
				num++;
			}
			DateTime utcNow2 = DateTime.UtcNow;
			if (num != 0)
			{
				text = $"{num} Errors found, view logfile for more info.";
				Logger.Info(text);
				TelemetryLogging.LogEvent(TelemetryLogging.SpkgDeployTelemetryEventType.SpkgErrorOccurred, packagesToInstall, packageManagerConfig.RootPaths, new string[0], text, utcNow2);
			}
			else
			{
				string message = $"Total time spent: {utcNow2 - utcNow}.";
				TelemetryLogging.LogEvent(TelemetryLogging.SpkgDeployTelemetryEventType.SpkgDeployFinished, packagesToInstall, packageManagerConfig.RootPaths, new string[0], message, utcNow2);
			}
			Logger.Info("Done package deployment.");
			Logger.Close();
			packageDeployerOutput.Success = num == 0;
			packageDeployerOutput.ErrorMessage = text;
			return packageDeployerOutput;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					Logger.Close();
				}
				disposedValue = true;
			}
		}

		private void GetAllRootPaths(string rootPaths, string alternateRoots, out IEnumerable<string> outRootPathSet, out IEnumerable<string> outAlternateRootPathSet)
		{
			char[] separator = new char[1] { ';' };
			HashSet<string> hashSet = new HashSet<string>(from x in rootPaths.Split(separator, StringSplitOptions.RemoveEmptyEntries)
				where !string.IsNullOrWhiteSpace(x)
				select Path.GetFullPath(x), StringComparer.InvariantCultureIgnoreCase);
			HashSet<string> hashSet2 = (string.IsNullOrWhiteSpace(alternateRoots) ? new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) : new HashSet<string>(from x in alternateRoots.Split(separator, StringSplitOptions.RemoveEmptyEntries)
				where !string.IsNullOrWhiteSpace(x)
				select Path.GetFullPath(x), StringComparer.InvariantCultureIgnoreCase));
			string environmentVariable = Environment.GetEnvironmentVariable("_WINPHONEROOT");
			string environmentVariable2 = Environment.GetEnvironmentVariable("RazzleDataPath");
			if (!string.IsNullOrWhiteSpace(environmentVariable2) && string.IsNullOrWhiteSpace(environmentVariable))
			{
				hashSet.Add(Environment.GetEnvironmentVariable("_NTTREE"));
			}
			else if (string.IsNullOrWhiteSpace(environmentVariable2) && !string.IsNullOrWhiteSpace(environmentVariable))
			{
				hashSet.Add(Environment.GetEnvironmentVariable("BINARY_ROOT"));
			}
			if (hashSet.Count == 0)
			{
				throw new ArgumentNullException("rootPathSet");
			}
			Action<HashSet<string>> action = delegate(HashSet<string> rooPaths)
			{
				HashSet<string> hashSet4 = new HashSet<string>();
				foreach (string rooPath in rooPaths)
				{
					if (!ReliableDirectory.Exists(rooPath, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs)))
					{
						Logger.Info("Path {0} is not accessible, remove it from the root path set.", rooPath);
						hashSet4.Add(rooPath);
					}
				}
				rooPaths.ExceptWith(hashSet4);
			};
			action(hashSet);
			if (hashSet.Count == 0)
			{
				throw new InvalidDataException("No path specified in the root path set is usable.");
			}
			HashSet<string> paths = PathHelper.AddRelatedPathsToSet(hashSet);
			outRootPathSet = PathHelper.SortPathSetOnPathType(paths);
			HashSet<string> hashSet3 = new HashSet<string>();
			outAlternateRootPathSet = null;
			if (hashSet2 != null && hashSet2.Count() > 0)
			{
				action(hashSet2);
				hashSet3 = PathHelper.AddRelatedPathsToSet(hashSet2);
				IEnumerable<string> outRootPathSetCopy = outRootPathSet;
				outAlternateRootPathSet = from x in PathHelper.SortPathSetOnPathType(hashSet3)
					where !outRootPathSetCopy.Contains(x)
					select x;
			}
		}

		private Dictionary<string, string> GetMacros(string userMacros)
		{
			Dictionary<string, string> macros = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			Action<string> action = delegate(string macrosInString)
			{
				IEnumerable<string[]> enumerable = (from x in macrosInString.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries)
					where x.Split(new char[1] { '=' }, StringSplitOptions.RemoveEmptyEntries).Length == 2
					select x).Select(delegate(string x)
				{
					int num = x.IndexOf('=');
					return new string[2]
					{
						x.Substring(0, num),
						x.Substring(num + 1)
					};
				});
				foreach (string[] item in enumerable)
				{
					if (!string.IsNullOrWhiteSpace(item[0]))
					{
						string key = item[0];
						string value = item[1];
						if (macros.ContainsKey(key))
						{
							macros[key] = value;
						}
						else
						{
							macros.Add(key, value);
						}
					}
				}
			};
			action(Settings.Default.DefaultMacros);
			if (!string.IsNullOrWhiteSpace(userMacros))
			{
				action(userMacros);
			}
			return macros;
		}

		private string FindBestCacheRoot(string currentCacheRoot, string fallbackLocation)
		{
			try
			{
				string pathRoot = Path.GetPathRoot(currentCacheRoot);
				DriveInfo[] drives = DriveInfo.GetDrives();
				DriveInfo driveInfo = null;
				DriveInfo[] array = drives;
				foreach (DriveInfo driveInfo2 in array)
				{
					if (driveInfo2.DriveType == DriveType.Fixed && driveInfo2.IsReady)
					{
						if (driveInfo2.Name == pathRoot)
						{
							driveInfo = driveInfo2;
							break;
						}
						if (driveInfo == null || driveInfo2.AvailableFreeSpace > driveInfo.AvailableFreeSpace)
						{
							driveInfo = driveInfo2;
						}
					}
				}
				if (driveInfo != null)
				{
					string text = currentCacheRoot.Replace(pathRoot, driveInfo.Name);
					if (text != currentCacheRoot)
					{
						Logger.Warning("Relocated cache directory to {0}", text);
					}
					return text;
				}
			}
			catch (Exception ex)
			{
				Logger.Warning("Exception relocating the cache directory: ", ex.Message);
			}
			Logger.Warning("Unable to relocate the cache directory.");
			return fallbackLocation;
		}
	}
}
