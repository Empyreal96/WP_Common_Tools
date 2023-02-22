using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	public class AppPackageAppxBundle : AppPackageAppx
	{
		private Dictionary<string, AppPackageAppx> _appxs = new Dictionary<string, AppPackageAppx>();

		public AppPackageAppxBundle(InboxAppParameters parameters)
			: base(parameters)
		{
			_isTopLevelPackage = true;
			if (!InboxAppUtils.ExtensionMatches(_parameters.PackageBasePath, ".appxbundle"))
			{
				throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Packages without a \"{0}\" extension are not supported.", new object[1] { ".appxbundle" }));
			}
		}

		public override void OpenPackage()
		{
			Path.Combine(Path.Combine(DecompressBundle(), "AppxMetadata"), "AppxBundleManifest.xml");
			ReadManifest(_parameters.PackageBasePath, true);
			ReadProvXML();
			_provXML.DependencyHash = InboxAppUtils.CalcHash(_parameters.PackageBasePath) + InboxAppUtils.CalcHash(_parameters.LicenseBasePath);
		}

		public override List<PkgObject> Map(IInboxAppPackage appPackage, IPkgProject packageGenerator, OSComponentBuilder osComponent)
		{
			if (osComponent == null)
			{
				throw new ArgumentNullException("osComponent", "osComponent must not be null!");
			}
			string installDestinationPath = InboxAppUtils.ResolveDestinationPath(GetInstallDestinationPath(_isTopLevelPackage), _parameters.InfuseIntoDataPartition, packageGenerator);
			string licenseFileDestinationPath = InboxAppUtils.ResolveDestinationPath(_provXML.LicenseDestinationPath, _parameters.InfuseIntoDataPartition, packageGenerator);
			FileGroupBuilder fileGroupBuilder = osComponent.AddFileGroup();
			ProcessDirectory(_packageFilesDirectory, _packageFilesDirectory, fileGroupBuilder, "AppxMetadata");
			_provXML.Update(installDestinationPath, licenseFileDestinationPath);
			string text = Path.Combine(_parameters.WorkingBaseDir, Path.GetFileName(_parameters.ProvXMLBasePath));
			_provXML.Save(text);
			fileGroupBuilder.AddFile(text, Path.GetDirectoryName(_provXML.ProvXMLDestinationPath)).SetName(Path.GetFileName(_provXML.ProvXMLDestinationPath).RemoveSrcExtension());
			LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "AppxBundle ProvXML file added to package: Source Path \"{0}\", Destination Dir \"{1}\"", new object[2]
			{
				text,
				Path.GetDirectoryName(_provXML.ProvXMLDestinationPath)
			}));
			if (_parameters.UpdateValue != 0)
			{
				fileGroupBuilder.AddFile(text, Path.GetDirectoryName(_provXML.UpdateProvXMLDestinationPath)).SetName(Path.GetFileName(_provXML.UpdateProvXMLDestinationPath));
				LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "Appxbundle ProvXML file added to package: Source Path \"{0}\", Destination Dir \"{1}\", File \"{2}\"", new object[3]
				{
					text,
					Path.GetDirectoryName(_provXML.UpdateProvXMLDestinationPath),
					Path.GetFileName(_provXML.UpdateProvXMLDestinationPath)
				}));
			}
			string text2 = Path.Combine(_parameters.WorkingBaseDir, Path.GetFileName(_parameters.LicenseBasePath));
			File.Copy(_parameters.LicenseBasePath, text2);
			string directoryName = Path.GetDirectoryName(_provXML.LicenseDestinationPath);
			string fileName = Path.GetFileName(_provXML.LicenseDestinationPath);
			fileGroupBuilder.AddFile(text2, directoryName).SetName(fileName);
			LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "AppxBundle license file added to package: Source Path \"{0}\", Destination Dir \"{1}\" File Name \"{2}\"", new object[3] { text2, directoryName, fileName }));
			AppManifestAppxBundle appManifestAppxBundle = _manifest as AppManifestAppxBundle;
			foreach (KeyValuePair<string, AppPackageAppx> appx in _appxs)
			{
				AppManifestAppxBundle.BundlePackage bundlePackage = new AppManifestAppxBundle.BundlePackage(APPX_BUNDLE_PAYLOAD_PACKAGE_TYPE.APPX_BUNDLE_PAYLOAD_PACKAGE_TYPE_RESOURCE);
				bundlePackage.FileName = appx.Key;
				if (appManifestAppxBundle.BundlePackages.IndexOf(bundlePackage) >= 0)
				{
					IInboxAppPackage value = appx.Value;
					((IInboxAppToPkgObjectsMappingStrategy)value).Map(value, packageGenerator, osComponent);
				}
				else
				{
					LogUtil.Warning(string.Format(CultureInfo.InvariantCulture, "Could not find an AppX payload file with the filename {0}. Please make sure the appx bundle manifest contains correct filenames for each Package element.", new object[1] { bundlePackage.FileName }));
				}
			}
			return new List<PkgObject> { osComponent.ToPkgObject() };
		}

		private string DecompressBundle()
		{
			string destinationDirectory = string.Empty;
			int num = NativeMethods.Unbundle(_parameters.PackageBasePath, _parameters.WorkingBaseDir, false, IntPtr.Zero, ref destinationDirectory);
			if (num != 0)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Unpack returned error {3}. One of the following fields may be empty or have an invalid value:\n(inputBundlePath)=\"{0}\" (outputDirectoryPath)=\"{1}\" (destinationDirectory)=\"{2}\"", _parameters.PackageBasePath, _parameters.WorkingBaseDir, destinationDirectory, num));
			}
			_packageFilesDirectory = destinationDirectory;
			string[] files = Directory.GetFiles(destinationDirectory, "*.appx", SearchOption.TopDirectoryOnly);
			if (files.Length == 0)
			{
				throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "The Appx Bundle \"{0}\" is empty!", new object[1] { _parameters.PackageBasePath }));
			}
			string[] array = files;
			foreach (string text in array)
			{
				AppPackageAppx appPackageAppx = new AppPackageAppx(new InboxAppParameters(text, string.Empty, string.Empty, _parameters.InfuseIntoDataPartition, _parameters.UpdateValue, _parameters.Category, _parameters.WorkingBaseDir), false, Path.GetFileNameWithoutExtension(text));
				appPackageAppx.OpenPackage();
				_appxs.Add(Path.GetFileName(text), appPackageAppx);
			}
			return destinationDirectory;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "AppX Bundle package: (Title)=\"{0}\" (BasePath)=\"{1}\"", new object[2] { _manifest.Title, _parameters.PackageBasePath });
		}
	}
}
