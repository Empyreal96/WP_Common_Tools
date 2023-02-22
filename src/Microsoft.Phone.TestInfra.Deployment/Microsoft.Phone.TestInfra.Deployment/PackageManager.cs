using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Mobile;
using Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary;
using Microsoft.Tools.IO;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class PackageManager
	{
		private const string FilesDir = "files";

		private const string OrigCatPath = "Windows\\System32\\catroot\\{F750E6C3-38EE-11D1-85E5-00C04FC295EE}";

		private const string OrigDsmPath = "windows\\packages\\DsmFiles";

		private const string DataDsmPath = "data\\windows\\packages\\DsmFiles";

		private const string OrigRegistryPath = "windows\\packages\\RegistryFiles";

		private const string DataCatPath = "data\\Windows\\System32\\catroot\\{F750E6C3-38EE-11D1-85E5-00C04FC295EE}";

		private const string DsmPath = "data\\test\\packages\\DsmFiles";

		private const string RegistryPath = "data\\test\\packages\\RegistryFiles";

		private const string MetadataPath = "files\\data\\test\\metadata";

		private const int DefaultRetryCount = 3;

		private static readonly TimeSpan DefaultRetryDelay = TimeSpan.FromMilliseconds(300.0);

		private readonly PackageManagerConfiguration config;

		private readonly PackageLocator packageLocator;

		private readonly CacheManager cacheManager;

		private readonly PackageExtractor packageExtractor;

		private readonly string packageExtractionRoot;

		private readonly HashSet<string> deployedPackages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		private volatile bool useSymlinks = true;

		private HashSet<string> newManifestFormatRootPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		public HashSet<string> DeployedPackages => deployedPackages;

		public int ErrorCount { get; private set; }

		public PackageManager(PackageManagerConfiguration config)
		{
			ValidateConfig(config);
			this.config = config;
			cacheManager = new CacheManager(this.config.CachePath, this.config.ExpiresIn);
			packageLocator = new PackageLocator(this.config.RootPaths, this.config.AlternateRootPaths);
			packageExtractionRoot = this.config.PackagesExtractionCachePath;
			packageExtractor = new PackageExtractor();
			useSymlinks = IsUserAdministrator();
			PathCleaner.RegisterForCleanup(config.RootPaths.Select((string path) => Path.Combine(packageExtractionRoot, ComputeBinaryRootPathHash(path))), this.config.ExpiresIn, TimeSpan.FromMinutes(5.0));
		}

		public void DeployPackages(IEnumerable<string> packages)
		{
			if (packages == null || !packages.Any() || packages.Any(string.IsNullOrEmpty))
			{
				throw new ArgumentNullException("packages");
			}
			UpdatePackagesInstallationInfo(packages);
			foreach (string package in packages)
			{
				DeployPackage(package);
			}
			UpdateLocatorCacheFiles();
		}

		private static void UpdateLocatorCacheFiles()
		{
			PackageLocator.UpdateGeneralPackageLocatorCacheFile();
			BinaryLocator.UpdateGeneralBinaryLocatorCacheFile();
			BaseLocator.CleanCacheFiles();
		}

		private static bool IsUserAdministrator()
		{
			try
			{
				WindowsIdentity current = WindowsIdentity.GetCurrent();
				WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
				return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
			}
			catch (UnauthorizedAccessException)
			{
				return false;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private void ValidateConfig(PackageManagerConfiguration config)
		{
			if (config == null)
			{
				throw new ArgumentNullException("config");
			}
			if (string.IsNullOrEmpty(config.OutputPath))
			{
				throw new ArgumentNullException("config", "Output path is not set");
			}
			if (config.RootPaths == null || !config.RootPaths.Any() || config.RootPaths.Any(string.IsNullOrEmpty))
			{
				throw new ArgumentNullException("config", "Root paths are not set");
			}
			if (string.IsNullOrEmpty(config.CachePath))
			{
				throw new ArgumentNullException("config", "Cache path is not set");
			}
			if (string.IsNullOrEmpty(config.PackagesExtractionCachePath))
			{
				throw new ArgumentNullException("config", "Packages extraction cache path is not set");
			}
			if (config.Macros == null)
			{
				config.Macros = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			}
		}

		private void DeployPackage(string package)
		{
			Logger.Info("Deploying package {0}", package);
			try
			{
				DeployPackage(package, true);
			}
			catch (Exception ex)
			{
				Logger.Error("Unhandled exception: {0}", ex);
				ErrorCount++;
			}
			Logger.Info("Finished deploying package {0}", package);
		}

		private string BuildDepXmlWithNewManifest(PackageInfo pkgInfo, bool updateOrCreate)
		{
			if (pkgInfo == null)
			{
				throw new ArgumentNullException("pkgInfo");
			}
			Logger.Debug("Build DepXml With New Manifest files which include dependencies info.");
			string path = Path.Combine("DependencyXml", Guid.NewGuid().GetHashCode().ToString());
			path = Path.Combine(config.CachePath, path);
			NewDepXmlGenerator newDepXmlGenerator = new NewDepXmlGenerator(path, pkgInfo, false, packageLocator);
			string depXml = newDepXmlGenerator.GetDepXml();
			PathCleaner.RegisterForCleanup(path, config.ExpiresIn, TimeSpan.FromMinutes(5.0));
			return depXml;
		}

		private static string GetDependencyPathString(PackageInfo pkgInfo, IEnumerable<string> rootPaths)
		{
			if (pkgInfo == null)
			{
				throw new ArgumentNullException("pkgInfo");
			}
			HashSet<string> hashSet = new HashSet<string>();
			string containingFolderPath = PathHelper.GetContainingFolderPath(pkgInfo.AbsolutePath, "Prebuilt");
			if (!string.IsNullOrEmpty(containingFolderPath))
			{
				hashSet.Add(containingFolderPath.Trim().TrimEnd('\\'));
			}
			else
			{
				Logger.Info("Unable to find the Prebuilt folder in the path {0}. Ignore this path in dependency calculation.", pkgInfo.AbsolutePath);
			}
			string empty = string.Empty;
			foreach (string rootPath in rootPaths)
			{
				if (PathHelper.GetPathType(rootPath) != PathType.PhoneBuildPath)
				{
					empty = PathHelper.GetContainingFolderPath(rootPath, "Prebuilt");
					string item = (string.IsNullOrEmpty(empty) ? rootPath.Trim().TrimEnd('\\') : empty.Trim().TrimEnd('\\'));
					hashSet.Add(item);
				}
			}
			return string.Join(";", hashSet);
		}

		private string GenerateTestMetaDataToolArgString(PackageInfo pkgInfo, bool updateOrCreate, string outputFolder)
		{
			if (pkgInfo == null)
			{
				throw new ArgumentNullException("pkgInfo");
			}
			string text = Path.Combine(Constants.AssemblyDirectory, Constants.SupressionFileName);
			string text2 = Path.Combine(outputFolder, pkgInfo.PackageName + "DepGenerate.log");
			string dependencyPathString = GetDependencyPathString(pkgInfo, config.RootPaths);
			string text3 = $"pkgdep -f \"{pkgInfo.PackageName}\" -i \"{dependencyPathString}\" -o \"{outputFolder}\" -rl -rr -s \"{text}\" -p windows -p test -l \"{text2}\" -q -n {Constants.NumOfLoaders}";
			if (updateOrCreate)
			{
				string winBuildPath = PathHelper.GetWinBuildPath(pkgInfo.AbsolutePath);
				if (!string.IsNullOrEmpty(winBuildPath))
				{
					text3 += $" -Up {winBuildPath}";
				}
			}
			return text3;
		}

		private static string GenerateTestMetadataOutputFolder()
		{
			string path = Path.Combine(Environment.GetEnvironmentVariable("UserProfile"), "AppData\\Local\\Temp\\TestMetaData");
			path = Path.Combine(path, Guid.NewGuid().GetHashCode().ToString());
			Directory.CreateDirectory(path);
			return path;
		}

		private string CallTestMetaDatToolToGenerateDepXml(PackageInfo pkgInfo, bool updateOrCreate)
		{
			string text = Path.Combine(Constants.AssemblyDirectory, "TestMetaDataTool.exe");
			if (!File.Exists(text))
			{
				throw new InvalidOperationException($"Failed to find File {text}.");
			}
			string text2 = GenerateTestMetadataOutputFolder();
			string text3 = GenerateTestMetaDataToolArgString(pkgInfo, updateOrCreate, text2);
			Logger.Info("Running {0} {1} to generate dep.xml.", text, text3);
			TimeSpan timeout = TimeSpan.FromMinutes(30.0);
			ProcessLauncher processLauncher = new ProcessLauncher(text, text3, delegate(string m)
			{
				Logger.Error(m);
			}, delegate(string m)
			{
				Logger.Info(m);
			}, delegate(string m)
			{
				Logger.Info(m);
			});
			processLauncher.TimeoutHandler = delegate(Process p)
			{
				throw new TimeoutException($"Process {p.StartInfo.FileName} did not exit in {timeout.Minutes} minutes");
			};
			ProcessLauncher processLauncher2 = processLauncher;
			processLauncher2.RunToExit(Convert.ToInt32(timeout.TotalMilliseconds, CultureInfo.InvariantCulture));
			if (!processLauncher2.Process.HasExited)
			{
				throw new InvalidOperationException($"Error: Process {text} has not exited.");
			}
			if (processLauncher2.Process.ExitCode != 0)
			{
				throw new InvalidOperationException($"{text} return an error exit code {processLauncher2.Process.ExitCode}");
			}
			string text4 = Path.Combine(Path.Combine(text2, "packagedep"), pkgInfo.PackageName + ".dep.xml");
			if (!File.Exists(text4))
			{
				Logger.Error("TestMetaDataTool.exe returns, but the dep.xml file is missing. \n");
				throw new FileNotFoundException(text4);
			}
			Logger.Info("TestMetaDataTool.exe generated the dep.xml file.\n");
			return text4;
		}

		private string CreateDepXmlUsingTestMetaDataTool(PackageInfo pkgInfo)
		{
			if (pkgInfo == null)
			{
				throw new ArgumentNullException("pkgInfo");
			}
			return CallTestMetaDatToolToGenerateDepXml(pkgInfo, false);
		}

		private string UpdateDepXml(PackageInfo pkgInfo)
		{
			if (pkgInfo == null)
			{
				throw new ArgumentNullException("pkgInfo");
			}
			return CallTestMetaDatToolToGenerateDepXml(pkgInfo, true);
		}

		private static bool ManifestFileIncludeDependencies(string manifestFile)
		{
			if (string.IsNullOrEmpty(manifestFile))
			{
				throw new ArgumentException("manifestFile");
			}
			if (!ReliableFile.Exists(manifestFile, 3, DefaultRetryDelay))
			{
				throw new FileNotFoundException(manifestFile);
			}
			bool result = false;
			try
			{
				RetryHelper.Retry(delegate
				{
					using (XmlTextReader xmlTextReader = new XmlTextReader(manifestFile))
					{
						xmlTextReader.MoveToContent();
						result = xmlTextReader.ReadToDescendant("Dependencies");
					}
				}, 3, DefaultRetryDelay);
			}
			catch (InvalidOperationException)
			{
				result = false;
			}
			return result;
		}

		private bool ManifestFileInNewFormat(string manifestFile)
		{
			if (string.IsNullOrEmpty(manifestFile))
			{
				throw new ArgumentException("manifestFile");
			}
			return ManifestFileIncludeDependencies(manifestFile);
		}

		private string GetDepXml(PackageInfo pkgInfo)
		{
			if (pkgInfo == null)
			{
				throw new ArgumentNullException("pkgInfo");
			}
			if (!LongPathFile.Exists(pkgInfo.AbsolutePath))
			{
				throw new ArgumentException($"File {pkgInfo.AbsolutePath} does not exist.");
			}
			string text = Path.ChangeExtension(pkgInfo.AbsolutePath, ".dep.xml");
			string text2 = Path.ChangeExtension(pkgInfo.AbsolutePath, Constants.ManifestFileExtension);
			if (File.Exists(text))
			{
				if (PathHelper.GetPathType(pkgInfo.AbsolutePath) == PathType.PhoneBuildPath)
				{
					Logger.Info("It is a test package from Phone Build share, using the existing dep.xml.");
					return text;
				}
				if (!string.IsNullOrEmpty(PathHelper.GetWinBuildPath(pkgInfo.AbsolutePath)))
				{
					return UpdateDepXml(pkgInfo);
				}
				return text;
			}
			if (File.Exists(text2))
			{
				if (ManifestFileInNewFormat(text2))
				{
					return BuildDepXmlWithNewManifest(pkgInfo, true);
				}
				return CreateDepXmlUsingTestMetaDataTool(pkgInfo);
			}
			return string.Empty;
		}

		private void DeployPackage(string package, bool deployDependencies)
		{
			IEnumerable<string> tagsForPackage = GetTagsForPackage(package);
			string text = package;
			package = PathHelper.GetPackageNameWithoutExtension(package);
			package = NormalizePackageName(package);
			if (string.IsNullOrEmpty(package))
			{
				Logger.Warning("Skipping package {0} as the name is empty after normalization.", text);
				return;
			}
			if (deployedPackages.Contains(package))
			{
				Logger.Info("{0} is already deployed, skipping", package);
				return;
			}
			deployedPackages.Add(package);
			PackageInfo packageInfo = packageLocator.FindPackage(package);
			if (packageInfo == null)
			{
				Logger.Error("Unable to find package {0}", package);
				ErrorCount++;
				return;
			}
			string cachedPackageFile = null;
			string cachedDependencyFile = null;
			try
			{
				Logger.Debug("Adding package to cache: {0}", packageInfo.PackageName);
				cacheManager.AddFileToCache(packageInfo.AbsolutePath, delegate(string sourcePackage, string cachedPackage)
				{
					cachedPackageFile = cachedPackage;
				});
			}
			catch (Exception ex)
			{
				Logger.Error("Unable to cache package {0}: {1}", packageInfo.AbsolutePath, ex);
				ErrorCount++;
				return;
			}
			if (deployDependencies)
			{
				try
				{
					string depXml = GetDepXml(packageInfo);
					Logger.Debug("Adding dependency XML to cache");
					if (File.Exists(depXml))
					{
						cacheManager.AddFileToCache(depXml, delegate(string sourceDependency, string cachedDependency)
						{
							cachedDependencyFile = cachedDependency;
						});
					}
				}
				catch (Exception ex2)
				{
					Logger.Warning("Error in getting dependency file for package: {0}. Error: {1}. Ignored.", package, ex2.ToString());
				}
			}
			try
			{
				string text2 = Path.Combine(config.OutputPath, "files");
				string packageExtractionRootPath = Path.Combine(packageExtractionRoot, ComputeBinaryRootPathHash(packageInfo.RootPath));
				bool newPackage = false;
				string source = RetryHelper.Retry(() => packageExtractor.ExtractPackage(cachedPackageFile, packageExtractionRootPath, out newPackage), 3, DefaultRetryDelay);
				CopyFilesToOutput(source, text2, newPackage);
				MoveCustomFiles(package, text2);
				if (!string.IsNullOrEmpty(cachedDependencyFile))
				{
					string path = Path.Combine(config.OutputPath, "files\\data\\test\\metadata");
					CopyFileToOutput(cachedDependencyFile, Path.Combine(path, Path.GetFileName(cachedDependencyFile)), true);
				}
				Logger.Info("Deployed {0}", package);
			}
			catch (Exception ex3)
			{
				Logger.Error("Unable to extract package {0}: {1}", packageInfo.AbsolutePath, ex3);
				ErrorCount++;
				return;
			}
			if ((config.RecursiveDeployment || deployDependencies) && !string.IsNullOrEmpty(cachedDependencyFile) && File.Exists(cachedDependencyFile))
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(cachedDependencyFile);
				ParseDependencyXml(xmlDocument.FirstChild, tagsForPackage);
			}
		}

		private void ParseDependencyXml(XmlNode node, IEnumerable<string> tags)
		{
			do
			{
				if (!node.HasChildNodes)
				{
					continue;
				}
				if (!string.Equals(node.Name, "Required", StringComparison.OrdinalIgnoreCase))
				{
					ParseDependencyXml(node.FirstChild, tags);
					continue;
				}
				foreach (XmlNode childNode in node.ChildNodes)
				{
					if (string.Equals(childNode.Name, "EnvironmentPath", StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					if (string.Equals(childNode.Name, "Package", StringComparison.OrdinalIgnoreCase))
					{
						if (childNode.Attributes != null)
						{
							foreach (XmlAttribute attribute in childNode.Attributes)
							{
								if (string.Equals(attribute.Name, "Name", StringComparison.OrdinalIgnoreCase))
								{
									DeployPackage(attribute.Value, false);
									break;
								}
								Logger.Error("Unexpected XML Attribute {0} found in Package node", attribute.Name);
								ErrorCount++;
							}
						}
						else
						{
							Logger.Error("No attributes found for Package node");
							ErrorCount++;
						}
					}
					else if (string.Equals(childNode.Name, "RemoteFile", StringComparison.OrdinalIgnoreCase))
					{
						if (childNode.Attributes == null)
						{
							Logger.Error("RemoteFile attributes not found");
							ErrorCount++;
							continue;
						}
						string text = null;
						string text2 = null;
						string text3 = null;
						string text4 = null;
						List<string> itemTags = new List<string>();
						foreach (XmlAttribute attribute2 in childNode.Attributes)
						{
							if (string.Equals(attribute2.Name, "SourcePath", StringComparison.OrdinalIgnoreCase))
							{
								text = MacroReplace(attribute2.Value, true);
							}
							else if (string.Equals(attribute2.Name, "Source", StringComparison.OrdinalIgnoreCase))
							{
								text2 = MacroReplace(attribute2.Value, false);
							}
							else if (string.Equals(attribute2.Name, "DestinationPath", StringComparison.OrdinalIgnoreCase))
							{
								text3 = MacroReplace(attribute2.Value, true);
							}
							else if (string.Equals(attribute2.Name, "Destination", StringComparison.OrdinalIgnoreCase))
							{
								text4 = MacroReplace(attribute2.Value, false);
							}
							else if (string.Equals(attribute2.Name, "Tags", StringComparison.OrdinalIgnoreCase))
							{
								itemTags = new List<string>(attribute2.Value.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries));
							}
							else
							{
								Logger.Error("Unexpected XML Attribute {0} found in RemoteFile node", attribute2.Name);
								ErrorCount++;
							}
						}
						if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text2) || string.IsNullOrEmpty(text3) || string.IsNullOrEmpty(text4))
						{
							Logger.Error("Missing attribute for RemoteFile");
							ErrorCount++;
							if (string.IsNullOrEmpty(text))
							{
								Logger.Error("Missing SourcePath");
							}
							if (string.IsNullOrEmpty(text2))
							{
								Logger.Error("Missing Source");
							}
							if (string.IsNullOrEmpty(text3))
							{
								Logger.Error("Missing DestinationPath");
							}
							if (string.IsNullOrEmpty(text4))
							{
								Logger.Error("Missing Destination");
							}
						}
						else if (!itemTags.Any() || tags.Any((string tag) => itemTags.Contains(tag, StringComparer.OrdinalIgnoreCase)))
						{
							CopyRemoteFiles(text, text2, text3, text4);
						}
					}
					else
					{
						Logger.Error("Unexpected XML tag {0}", childNode.Name);
						ErrorCount++;
					}
				}
			}
			while ((node = node.NextSibling) != null);
		}

		private void CustomFileHelper(string searchPath, string fromPath, string toPath, string ext)
		{
			string path = Path.Combine(fromPath, searchPath);
			if (!ReliableDirectory.Exists(path, 3, DefaultRetryDelay))
			{
				return;
			}
			foreach (string item in Directory.EnumerateFiles(path, "*" + ext, SearchOption.TopDirectoryOnly))
			{
				string text = PathHelper.Combine(fromPath, toPath);
				Logger.Debug("{0} Directory = {1}", ext, text);
				Directory.CreateDirectory(text);
				string text2 = PathHelper.Combine(text, Path.GetFileName(item));
				try
				{
					File.Delete(text2);
					File.Move(item, text2);
					Logger.Debug("Moved {0} to {1}", item, text2);
				}
				catch (IOException ex)
				{
					Logger.Warning("Unable to move {0} file {1}: {2}", ext, item, ex.ToString());
				}
			}
		}

		private void MoveCustomFiles(string package, string outputPath)
		{
			CustomFileHelper("data\\Windows\\System32\\catroot\\{F750E6C3-38EE-11D1-85E5-00C04FC295EE}", outputPath, "Windows\\System32\\catroot\\{F750E6C3-38EE-11D1-85E5-00C04FC295EE}", ".cat");
			CustomFileHelper("windows\\packages\\DsmFiles", outputPath, "data\\test\\packages\\DsmFiles", ".dsm.xml");
			CustomFileHelper("data\\windows\\packages\\DsmFiles", outputPath, "data\\test\\packages\\DsmFiles", ".dsm.xml");
			CustomFileHelper("windows\\packages\\RegistryFiles", outputPath, "data\\test\\packages\\RegistryFiles", ".reg");
			CustomFileHelper("windows\\packages\\RegistryFiles", outputPath, "data\\test\\packages\\RegistryFiles", ".rga");
		}

		private void CopyRemoteFiles(string sourcePath, string source, string destinationPath, string destination)
		{
			string remoteFileSource = PathHelper.Combine(sourcePath, source);
			string remoteFileDestination = PathHelper.Combine(PathHelper.Combine(Path.Combine(config.OutputPath, "files"), destinationPath), destination);
			try
			{
				if (ReliableDirectory.Exists(remoteFileSource, 3, DefaultRetryDelay))
				{
					cacheManager.AddFilesToCache(remoteFileSource, "*", true, delegate(string sourceFile, string cachedFile)
					{
						CopyFileToOutput(cachedFile, PathHelper.ChangeParent(sourceFile, remoteFileSource, remoteFileDestination), true);
					});
				}
				else
				{
					cacheManager.AddFileToCache(remoteFileSource, delegate(string sourceFile, string cachedFile)
					{
						CopyFileToOutput(cachedFile, remoteFileDestination, true);
					});
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Unable to copy remote files from {0}: {1}", remoteFileSource, ex);
				ErrorCount++;
			}
		}

		private void CopyFileToOutput(string sourceFile, string targetFile, bool overwrite)
		{
			if (useSymlinks)
			{
				try
				{
					RetryHelper.Retry(delegate
					{
						SymlinkHelper.CreateSymlink(sourceFile, targetFile, overwrite);
					}, 3, DefaultRetryDelay);
				}
				catch (Exception ex)
				{
					Logger.Error("Unable to create symbolic link, falling back to File.Copy: {0}", ex);
					useSymlinks = false;
				}
			}
			if (!useSymlinks)
			{
				FileCopyHelper.CopyFile(sourceFile, targetFile, 5, TimeSpan.FromMilliseconds(200.0));
			}
		}

		private void CopyFilesToOutput(string source, string destination, bool overwrite)
		{
			if (useSymlinks)
			{
				try
				{
					RetryHelper.Retry(delegate
					{
						SymlinkHelper.CreateSymlinks(source, destination, overwrite);
					}, 3, DefaultRetryDelay);
				}
				catch (Exception ex)
				{
					Logger.Error("Unable to create symbolic links, falling back to File.Copy: {0}", ex);
					useSymlinks = false;
				}
			}
			if (!useSymlinks)
			{
				FileCopyHelper.CopyFiles(source, destination, "*", true, 3, DefaultRetryDelay);
			}
		}

		private void UpdatePackagesInstallationInfo(IEnumerable<string> packages)
		{
			IEnumerable<string> contents = packages.Select(NormalizePackageName);
			string text = Path.Combine(config.OutputPath, "files\\data\\test\\metadata");
			Directory.CreateDirectory(text);
			File.WriteAllLines(Path.Combine(text, "currentpkg.txt"), contents);
		}

		private string NormalizePackageName(string package)
		{
			if (string.IsNullOrEmpty(package))
			{
				throw new ArgumentNullException("package");
			}
			string fileNameWithoutExtension = PathHelper.GetFileNameWithoutExtension(package.Trim().ToLowerInvariant(), ".spkg");
			int num = fileNameWithoutExtension.IndexOf("[", StringComparison.OrdinalIgnoreCase);
			return (num == -1) ? fileNameWithoutExtension : fileNameWithoutExtension.Substring(0, num);
		}

		private IEnumerable<string> GetTagsForPackage(string package)
		{
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			Match match = Regex.Match(package, ".+?\\[tags=(.+?)\\]", RegexOptions.IgnoreCase);
			if (match.Success)
			{
				string[] array = match.Groups[1].Value.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string item in array)
				{
					hashSet.Add(item);
				}
				Logger.Debug("Tags:");
				foreach (string item2 in hashSet)
				{
					Logger.Debug("  {0}", item2);
				}
			}
			return hashSet;
		}

		private string MacroReplace(string str, bool fullStringMacro)
		{
			string text = str;
			if (fullStringMacro && !text.StartsWith("$(", StringComparison.OrdinalIgnoreCase))
			{
				text = "$(" + text + ")";
			}
			foreach (KeyValuePair<string, string> macro in config.Macros)
			{
				string pattern = $"\\$\\({macro.Key}\\)";
				text = Regex.Replace(text, pattern, macro.Value, RegexOptions.IgnoreCase);
			}
			if (text.IndexOf("$(", StringComparison.OrdinalIgnoreCase) != -1)
			{
				Logger.Error("Unknown Macro referenced in string {0}", str);
				ErrorCount++;
			}
			if (!string.Equals(str, text, StringComparison.OrdinalIgnoreCase))
			{
				Logger.Debug("MacroReplace: {0} => {1}", str, text);
			}
			return text;
		}

		private string ComputeBinaryRootPathHash(string path)
		{
			return PathHelper.EndWithDirectorySeparator(path).ToUpperInvariant().GetHashCode()
				.ToString(CultureInfo.InvariantCulture);
		}
	}
}
