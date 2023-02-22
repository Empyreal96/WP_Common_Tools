using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	public class AppPackageAppx : IInboxAppPackage, IInboxAppToPkgObjectsMappingStrategy
	{
		protected InboxAppParameters _parameters;

		protected bool _isTopLevelPackage;

		protected string _packageSubBaseDir = string.Empty;

		protected AppManifestAppxBase _manifest;

		protected ProvXMLAppx _provXML;

		private readonly List<string> _capabilities = new List<string>();

		protected string _packageFilesDirectory = string.Empty;

		public AppPackageAppx(InboxAppParameters parameters, bool isTopLevelPackage = true, string packageSubBaseDir = "")
		{
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters", "parameters must not be null!");
			}
			_parameters = parameters;
			_isTopLevelPackage = isTopLevelPackage;
			_packageSubBaseDir = packageSubBaseDir;
			if (!_isTopLevelPackage && !InboxAppUtils.ExtensionMatches(_parameters.PackageBasePath, ".appx"))
			{
				throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Packages without a \"{0}\" extension are not supported.", new object[1] { ".appx" }));
			}
		}

		public virtual void OpenPackage()
		{
			Path.Combine(DecompressPackage(_parameters.PackageBasePath, _parameters.WorkingBaseDir), "AppxManifest.xml");
			ReadManifest(_parameters.PackageBasePath, false);
			if (_isTopLevelPackage && !string.IsNullOrWhiteSpace(_parameters.ProvXMLBasePath))
			{
				ReadProvXML();
				_provXML.DependencyHash = InboxAppUtils.CalcHash(_parameters.PackageBasePath) + InboxAppUtils.CalcHash(_parameters.LicenseBasePath);
			}
		}

		public List<string> GetCapabilities()
		{
			return _manifest.Capabilities;
		}

		public IInboxAppManifest GetManifest()
		{
			return _manifest;
		}

		public IInboxAppToPkgObjectsMappingStrategy GetPkgObjectsMappingStrategy()
		{
			return this;
		}

		public string GetInstallDestinationPath(bool isTopLevelPackage)
		{
			string path = string.Empty;
			string path2 = (_parameters.InfuseIntoDataPartition ? "$(runtime.data)\\Programs\\WindowsApps" : "$(runtime.windows)\\InfusedApps");
			if (!_parameters.InfuseIntoDataPartition)
			{
				path = (_manifest.IsFramework ? "Frameworks" : ((!(_manifest.IsBundle || isTopLevelPackage)) ? "Packages" : "Applications"));
			}
			return Path.Combine(path2, path, _manifest.PackageFullName);
		}

		public virtual List<PkgObject> Map(IInboxAppPackage appPackage, IPkgProject packageGenerator, OSComponentBuilder osComponent)
		{
			if (osComponent == null)
			{
				throw new ArgumentNullException("osComponent", "INTERNAL ERROR:osComponent must not be null!");
			}
			FileGroupBuilder fileGroupBuilder = osComponent.AddFileGroup();
			if ((AppManifestAppxBase)appPackage.GetManifest() == null)
			{
				throw new ArgumentException("INTERNAL ERROR: appPackage is not AppManifestAppxBase or subclass!", "appPackage");
			}
			if (!_isTopLevelPackage)
			{
				SetFileGroupFilter(packageGenerator, fileGroupBuilder);
			}
			ProcessDirectory(_packageFilesDirectory, _packageFilesDirectory, fileGroupBuilder);
			if (_isTopLevelPackage)
			{
				string installDestinationPath = InboxAppUtils.ResolveDestinationPath(GetInstallDestinationPath(_isTopLevelPackage), _parameters.InfuseIntoDataPartition, packageGenerator);
				string licenseFileDestinationPath = InboxAppUtils.ResolveDestinationPath(_provXML.LicenseDestinationPath, _parameters.InfuseIntoDataPartition, packageGenerator);
				ValidateDependencies(packageGenerator);
				if (string.IsNullOrWhiteSpace(_parameters.ProvXMLBasePath) || _provXML == null)
				{
					throw new InvalidOperationException("INTERNAL ERROR: Attempting to process a provXML for an .appx that is contained within a bundle is not allowed!");
				}
				_provXML.Update(installDestinationPath, licenseFileDestinationPath);
				string text = Path.Combine(_parameters.WorkingBaseDir, Path.GetFileName(_parameters.ProvXMLBasePath));
				_provXML.Save(text);
				fileGroupBuilder.AddFile(text, Path.GetDirectoryName(_provXML.ProvXMLDestinationPath)).SetName(Path.GetFileName(_provXML.ProvXMLDestinationPath).RemoveSrcExtension());
				LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "Appx ProvXML file added to package: Source Path \"{0}\", Destination Dir \"{1}\"", new object[2]
				{
					text,
					Path.GetDirectoryName(_provXML.ProvXMLDestinationPath)
				}));
				if (_parameters.UpdateValue != 0)
				{
					fileGroupBuilder.AddFile(text, Path.GetDirectoryName(_provXML.UpdateProvXMLDestinationPath)).SetName(Path.GetFileName(_provXML.UpdateProvXMLDestinationPath).RemoveSrcExtension());
					LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "Appx ProvXML file added to package: Source Path \"{0}\", Destination Dir \"{1}\", File \"{2}\"", new object[3]
					{
						text,
						Path.GetDirectoryName(_provXML.UpdateProvXMLDestinationPath),
						Path.GetFileName(_provXML.UpdateProvXMLDestinationPath)
					}));
				}
				if (!_manifest.IsFramework)
				{
					if (string.IsNullOrWhiteSpace(_parameters.LicenseBasePath))
					{
						throw new InvalidOperationException("INTERNAL ERROR: Attempting to process a license for an .appx that is contained within a bundle is not allowed!");
					}
					string text2 = Path.Combine(_parameters.WorkingBaseDir, Path.GetFileName(_parameters.LicenseBasePath));
					File.Copy(_parameters.LicenseBasePath, text2);
					string directoryName = Path.GetDirectoryName(_provXML.LicenseDestinationPath);
					string fileName = Path.GetFileName(_provXML.LicenseDestinationPath);
					fileGroupBuilder.AddFile(text2, directoryName).SetName(fileName);
					LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "Appx license file added to package: Source Path \"{0}\", Destination Dir \"{1}\" File Name \"{2}\"", new object[3] { text2, directoryName, fileName }));
				}
			}
			return new List<PkgObject> { osComponent.ToPkgObject() };
		}

		protected string DecompressPackage(string packageBasePath, string workingBaseDir)
		{
			return DecompressPackageUsingAppxPackaging(packageBasePath, workingBaseDir);
		}

		protected string DecompressPackageUsingAppxPackaging(string packageBasePath, string workingBaseDir)
		{
			_packageFilesDirectory = Path.Combine(workingBaseDir, "Content");
			if (!_isTopLevelPackage && !string.IsNullOrWhiteSpace(_packageSubBaseDir))
			{
				_packageFilesDirectory = Path.Combine(_packageFilesDirectory, _packageSubBaseDir);
			}
			if (Directory.Exists(_packageFilesDirectory))
			{
				Directory.Delete(_packageFilesDirectory, true);
			}
			string destinationDirectory = string.Empty;
			int num = NativeMethods.Unpack(packageBasePath, _packageFilesDirectory, false, IntPtr.Zero, ref destinationDirectory);
			if (num != 0)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Unpack returned error {3}. One of the following fields may be empty or have an invalid value:\n(inputPackagePath)=\"{0}\" (outputDirectoryPath)=\"{1}\" (destinationDirectory)=\"{2}\"", packageBasePath, _packageFilesDirectory, destinationDirectory, num));
			}
			return _packageFilesDirectory;
		}

		protected void ReadManifest(string packageFileBasePath, bool isBundle)
		{
			if (_manifest == null)
			{
				_manifest = AppManifestAppxBase.CreateAppxManifest(packageFileBasePath, string.Empty, isBundle);
				_manifest.ReadManifest();
			}
		}

		protected void ReadProvXML()
		{
			_provXML = ProvXMLAppx.CreateAppxProvXML(_parameters, _manifest);
			_provXML.ReadProvXML();
		}

		private void ValidateDependencies(IPkgProject packageGenerator)
		{
			if (_manifest.PackageDependencies.Count <= 0)
			{
				return;
			}
			foreach (PackageDependency packageDependency2 in _manifest.PackageDependencies)
			{
				if (!packageDependency2.IsValid())
				{
					LogUtil.Message("One or more of the PackageDependency elements in the AppX manifest is invalid (Hint: \"{0}\"). Skipping.", packageDependency2);
					continue;
				}
				StringBuilder stringBuilder = new StringBuilder("$(");
				stringBuilder.Append("appxframework.");
				stringBuilder.Append(packageDependency2.Name);
				stringBuilder.Append(")");
				string text = stringBuilder.ToString();
				string text2 = packageGenerator.MacroResolver.Resolve(text, MacroResolveOptions.SkipOnUnknownMacro);
				if (string.IsNullOrEmpty(text2) || text2 == text)
				{
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "The package dependency \"{0}\" is not registered with PkgGen. Please check pkggen.cfg.xml for a list of valid macros with ID names that start with {1}.", new object[2] { packageDependency2.Name, "appxframework." }));
				}
				if (packageDependency2.MeetsVersionRequirements(text2))
				{
					continue;
				}
				PackageDependency packageDependency = new PackageDependency(packageDependency2.Name, text2);
				throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "The MinVersion specified in the application's package dependency {0} is not supported by this build of Windows Phone (which is built with {1}). Please lower the MinVersion or make sure you are using an updated Windows Phone Adaptation Kit.", new object[2] { packageDependency2, packageDependency }));
			}
		}

		private void SetFileGroupFilter(IPkgProject packageGenerator, FileGroupBuilder fileGroupBuilder)
		{
			if (fileGroupBuilder == null)
			{
				throw new ArgumentNullException("fileGroupBuilder", "INTERNAL ERROR: fileGroupBuilder must not be null!");
			}
			if (packageGenerator == null)
			{
				throw new ArgumentNullException("packageGenerator", "INTERNAL ERROR: packageGenerator must not be null!");
			}
			if (_manifest.IsResource)
			{
				if (_manifest.Resources.Count > 0)
				{
					AppManifestAppxBase.Resource resource = _manifest.Resources[0];
					if (resource.Key.Equals("Language", StringComparison.OrdinalIgnoreCase))
					{
						string text = MapBCP47TagToSupportedNLSLocaleName(resource.Value, packageGenerator);
						if (!string.IsNullOrWhiteSpace(text))
						{
							LogUtil.Warning(string.Format(CultureInfo.InvariantCulture, "(appxbundle lang pack splitting feature disabled temporarily) Setting file group filter: Language = {0} ManifestLanuage = {1} for appx \"{2}\"", new object[3] { text, resource.Value, _parameters.PackageBasePath }));
						}
						else
						{
							LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "NOTE: The language value {0} listed in the Appx manifest as a resource language is not a supported value - skipping mapping to PkgGen's language values.", new object[1] { resource.Value }));
						}
					}
					else if (resource.Key.Equals("Scale", StringComparison.OrdinalIgnoreCase))
					{
						string text2 = MapScaleToSupportedResolution(resource.Value);
						if (!string.IsNullOrWhiteSpace(text2))
						{
							LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "Setting file group filter: Resolution = {0}", new object[1] { text2 }));
						}
						else
						{
							LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "NOTE: The scale value {0} listed in the Appx manifest as a resource scale is not a supported value - skipping mapping to PkgGen's resolution values.", new object[1] { resource.Value }));
						}
					}
					else
					{
						resource.Key.Equals("DXFeatureLevel", StringComparison.OrdinalIgnoreCase);
					}
				}
				else
				{
					LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "The appx manifest {0} for bundle subpackage {1} contains no resources - skipping setting file group filters.", new object[2] { _manifest.Filename, _parameters.PackageBasePath }));
				}
			}
			else
			{
				CpuId cpuId = CpuId.Invalid;
				if (_manifest.ProcessorArchitecture.Equals(APPX_PACKAGE_ARCHITECTURE.APPX_PACKAGE_ARCHITECTURE_X86))
				{
					cpuId = CpuId.X86;
				}
				else if (_manifest.ProcessorArchitecture.Equals(APPX_PACKAGE_ARCHITECTURE.APPX_PACKAGE_ARCHITECTURE_ARM))
				{
					cpuId = CpuId.ARM;
				}
				else if (_manifest.ProcessorArchitecture.Equals(APPX_PACKAGE_ARCHITECTURE.APPX_PACKAGE_ARCHITECTURE_X64))
				{
					cpuId = CpuId.AMD64;
				}
				else if (_manifest.ProcessorArchitecture.Equals(APPX_PACKAGE_ARCHITECTURE.APPX_PACKAGE_ARCHITECTURE_ARM64))
				{
					cpuId = CpuId.ARM64;
				}
				if (cpuId != 0)
				{
					fileGroupBuilder.SetCpuId(cpuId);
					LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "Setting file group filter: Cpu Id = {0}", new object[1] { cpuId.ToString() }));
				}
			}
		}

		protected void ProcessDirectory(string targetBaseDir, string baseBasePath, FileGroupBuilder groupBuilder, string specificSubdirectoryPattern = "")
		{
			LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "Target directory to process: {0}", new object[1] { targetBaseDir }));
			string[] files = Directory.GetFiles(targetBaseDir);
			foreach (string fileBasePath in files)
			{
				ProcessFile(fileBasePath, baseBasePath, groupBuilder);
			}
			if (string.IsNullOrWhiteSpace(specificSubdirectoryPattern))
			{
				foreach (string item in Directory.EnumerateDirectories(targetBaseDir))
				{
					LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "Processing sub directory: {0}", new object[1] { item }));
					ProcessDirectory(item, baseBasePath, groupBuilder);
				}
				return;
			}
			foreach (string item2 in Directory.EnumerateDirectories(targetBaseDir, specificSubdirectoryPattern))
			{
				LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "Processing specific sub directory: {0}", new object[1] { item2 }));
				ProcessDirectory(item2, baseBasePath, groupBuilder);
			}
		}

		protected void ProcessFile(string fileBasePath, string baseBasePath, FileGroupBuilder groupBuilder)
		{
			if (!InboxAppUtils.ExtensionMatches(fileBasePath, ".appx"))
			{
				string path = string.Empty;
				string empty = string.Empty;
				string directoryName = Path.GetDirectoryName(fileBasePath);
				if (directoryName.StartsWith(baseBasePath, StringComparison.OrdinalIgnoreCase))
				{
					path = directoryName.Remove(0, baseBasePath.Length).TrimStart('\\');
				}
				string text = Path.Combine(GetInstallDestinationPath(_isTopLevelPackage), path);
				groupBuilder.AddFile(fileBasePath, text);
				LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "AppX content file added to package: Source Path \"{0}\", Destination Dir \"{1}\"", new object[2] { fileBasePath, text }));
			}
		}

		private string MapBCP47TagToSupportedNLSLocaleName(string bcp47Tag, IPkgProject packageGenerator)
		{
			string empty = string.Empty;
			string empty2 = string.Empty;
			if (packageGenerator == null)
			{
				throw new ArgumentNullException("packageGenerator", "INTERNAL ERROR: packageGenerator must not be null!");
			}
			if (string.IsNullOrWhiteSpace(bcp47Tag))
			{
				throw new ArgumentNullException("bcp47Tag", "INTERNAL ERROR: bcp47Tag must not be null or empty!");
			}
			empty2 = InboxAppUtils.MapNeutralToSpecificCulture(bcp47Tag);
			if (string.IsNullOrWhiteSpace(empty2))
			{
				empty2 = bcp47Tag.ToLowerInvariant();
			}
			foreach (SatelliteId satelliteValue in packageGenerator.GetSatelliteValues(SatelliteType.Language))
			{
				if (satelliteValue.Id.ToLowerInvariant() == empty2)
				{
					return string.Format(CultureInfo.InvariantCulture, "({0})", new object[1] { satelliteValue.Id });
				}
			}
			return empty;
		}

		private string MapScaleToSupportedResolution(string scale)
		{
			if (string.IsNullOrWhiteSpace(scale))
			{
				throw new ArgumentNullException("scale", "INTERNAL ERROR: scale must not be null or empty!");
			}
			string result = string.Empty;
			switch (scale)
			{
			case "225":
			case "180":
				result = "(1080x1920)";
				break;
			case "160":
				result = "(768x1280)";
				break;
			case "150":
				result = "(720x1280)";
				break;
			case "100":
			case "120":
			case "140":
				result = "(480x800)";
				break;
			}
			return result;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "AppX package: (Title)=\"{0}\" (BasePath)=\"{1}\"", new object[2] { _manifest.Title, _parameters.PackageBasePath });
		}
	}
}
