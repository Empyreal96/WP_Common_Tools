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
	public class AppPackageXAP : IInboxAppPackage, IInboxAppToPkgObjectsMappingStrategy
	{
		private InboxAppParameters _parameters;

		private AppManifestXAP _manifest;

		private AppManifestAppxBase _lightupManifest;

		private IInboxProvXML _provXML;

		private readonly List<string> _capabilities = new List<string>();

		private string _packageFilesDirectory = string.Empty;

		public AppPackageXAP(InboxAppParameters parameters)
		{
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters", "parameters must not be null!");
			}
			_parameters = parameters;
			if (!InboxAppUtils.ExtensionMatches(_parameters.PackageBasePath, ".xap"))
			{
				throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Packages without a \"{0}\" extension are not supported.", new object[1] { ".xap" }));
			}
		}

		public void OpenPackage()
		{
			DecompressPackage();
			ReadManifest();
			ReadProvXML();
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

		public string GetInstallDestinationPath(bool isTopLevelPackage = true)
		{
			string empty = string.Empty;
			string empty2 = string.Empty;
			if (IsPackageSLL())
			{
				empty = (_parameters.InfuseIntoDataPartition ? "$(runtime.data)\\Programs\\WindowsApps" : "$(runtime.windows)\\InfusedApps");
				return Path.Combine(empty, _lightupManifest.PackageFullName);
			}
			if (_parameters.InfuseIntoDataPartition)
			{
				empty = "$(runtime.data)\\Programs\\{0}\\install";
				return string.Format(CultureInfo.InvariantCulture, empty, new object[1] { _manifest.ProductID });
			}
			empty = "$(runtime.commonfiles)\\InboxApps";
			return Path.Combine(empty, _manifest.ProductID);
		}

		public List<PkgObject> Map(IInboxAppPackage appPackage, IPkgProject packageGenerator, OSComponentBuilder osComponent)
		{
			if (osComponent == null)
			{
				throw new ArgumentNullException("osComponent", "osComponent must not be null!");
			}
			string installDestinationPath = InboxAppUtils.ResolveDestinationPath(GetInstallDestinationPath(), _parameters.InfuseIntoDataPartition, packageGenerator);
			string licenseFileDestinationPath = InboxAppUtils.ResolveDestinationPath(_provXML.LicenseDestinationPath, _parameters.InfuseIntoDataPartition, packageGenerator);
			FileGroupBuilder fileGroupBuilder = osComponent.AddFileGroup();
			ProcessDirectory(_packageFilesDirectory, _packageFilesDirectory, fileGroupBuilder);
			_provXML.Update(installDestinationPath, licenseFileDestinationPath);
			string text = Path.Combine(_parameters.WorkingBaseDir, Path.GetFileName(_parameters.ProvXMLBasePath));
			_provXML.Save(text);
			fileGroupBuilder.AddFile(text, Path.GetDirectoryName(_provXML.ProvXMLDestinationPath));
			LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "XAP ProvXML file added to package: Source Path \"{0}\", Destination Dir \"{1}\"", new object[2]
			{
				text,
				Path.GetDirectoryName(_provXML.ProvXMLDestinationPath)
			}));
			if (_parameters.UpdateValue != 0)
			{
				fileGroupBuilder.AddFile(text, Path.GetDirectoryName(_provXML.UpdateProvXMLDestinationPath)).SetName(Path.GetFileName(_provXML.UpdateProvXMLDestinationPath));
				LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "Appx ProvXML file added to package: Source Path \"{0}\", Destination Dir \"{1}\", File \"{2}\"", new object[3]
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
			LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "XAP license file added to package: Source Path \"{0}\", Destination Dir \"{1}\" File Name \"{2}\"", new object[3] { text2, directoryName, fileName }));
			return new List<PkgObject> { osComponent.ToPkgObject() };
		}

		protected void DecompressPackage()
		{
			_packageFilesDirectory = Path.Combine(_parameters.WorkingBaseDir, "Content");
			if (Directory.Exists(_packageFilesDirectory))
			{
				Directory.Delete(_packageFilesDirectory, true);
			}
			InboxAppUtils.Unzip(_parameters.PackageBasePath, _packageFilesDirectory);
		}

		protected void ReadManifest()
		{
			if (_manifest == null)
			{
				string manifestBasePath = Path.Combine(_packageFilesDirectory, "WMAppManifest.xml");
				_manifest = new AppManifestXAP(manifestBasePath);
				_manifest.ReadManifest();
				string text = Path.Combine(_packageFilesDirectory, "appxmanifest.xml");
				if (File.Exists(text))
				{
					LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "XAP file is a Silverlight Lightup app with an extra {0}", new object[1] { "appxmanifest.xml" }));
					_lightupManifest = AppManifestAppxBase.CreateAppxManifest(string.Empty, text, false);
					_lightupManifest.ReadManifest();
				}
			}
		}

		protected void ReadProvXML()
		{
			_provXML = new ProvXMLXAP(_parameters, _manifest);
			_provXML.ReadProvXML();
		}

		protected void ProcessDirectory(string targetBaseDir, string baseBasePath, FileGroupBuilder groupBuilder)
		{
			string[] files = Directory.GetFiles(targetBaseDir);
			foreach (string fileBasePath in files)
			{
				ProcessFile(fileBasePath, baseBasePath, groupBuilder);
			}
			foreach (string item in Directory.EnumerateDirectories(targetBaseDir))
			{
				LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "Processing sub directory: {0}", new object[1] { item }));
				ProcessDirectory(item, baseBasePath, groupBuilder);
			}
		}

		protected void ProcessFile(string fileBasePath, string baseBasePath, FileGroupBuilder groupBuilder)
		{
			string path = string.Empty;
			string directoryName = Path.GetDirectoryName(fileBasePath);
			if (directoryName.StartsWith(baseBasePath, StringComparison.OrdinalIgnoreCase))
			{
				path = directoryName.Remove(0, baseBasePath.Length).TrimStart('\\');
			}
			string text = Path.Combine(GetInstallDestinationPath(), path);
			groupBuilder.AddFile(fileBasePath, text);
			LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "XAP content file added to package: Source Path \"{0}\", Destination Dir \"{1}\"", new object[2] { fileBasePath, text }));
		}

		protected bool IsPackageSLL()
		{
			return _lightupManifest != null;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "XAP package: (Title)=\"{0}\" (BasePath)=\"{1}\"", new object[2] { _manifest.Title, _parameters.PackageBasePath });
		}
	}
}
