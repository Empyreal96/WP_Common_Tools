using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	public abstract class AppManifestAppxBase : IInboxAppManifest
	{
		public enum AppxManifestType
		{
			Undefined,
			MainPackage,
			Bundle
		}

		public sealed class Resource
		{
			private string _key = string.Empty;

			private string _value = string.Empty;

			public string Key => _key;

			public string Value => _value;

			public Resource(string key, string value)
			{
				_key = key;
				_value = value;
			}
		}

		protected string _title = string.Empty;

		protected string _description = string.Empty;

		protected string _publisher = string.Empty;

		protected List<string> _capabilities = new List<string>();

		protected string _version = string.Empty;

		protected string _manifestBasePath = string.Empty;

		protected string _manifestDestinationPath = string.Empty;

		protected string _packageBasePath = string.Empty;

		protected string _productID = string.Empty;

		protected string _packageFullName = string.Empty;

		protected APPX_PACKAGE_ARCHITECTURE _processorArchitecture = APPX_PACKAGE_ARCHITECTURE.APPX_PACKAGE_ARCHITECTURE_NEUTRAL;

		protected string _resourceID = string.Empty;

		protected bool _isFramework;

		protected bool _isBundle;

		protected bool _isResource;

		protected readonly List<PackageDependency> _packageDependencies = new List<PackageDependency>();

		protected readonly List<Resource> _resources = new List<Resource>();

		protected AppxManifestType _manifestType;

		public string Filename => Path.GetFileName(_manifestBasePath);

		public string Title => _title;

		public string Description => _description;

		public string Publisher => _publisher;

		public List<string> Capabilities => _capabilities;

		public string ProductID => _productID;

		public string Version => _version;

		public APPX_PACKAGE_ARCHITECTURE ProcessorArchitecture => _processorArchitecture;

		public string ResourceID => _resourceID;

		public string PackageFullName => _packageFullName;

		public bool IsFramework => _isFramework;

		public bool IsBundle => _isBundle;

		public bool IsResource => _isResource;

		public List<PackageDependency> PackageDependencies => _packageDependencies;

		public List<Resource> Resources => _resources;

		public static AppManifestAppxBase CreateAppxManifest(string packageBasePath, string manifestBasePath, bool isBundle)
		{
			AppManifestAppxBase appManifestAppxBase = null;
			if (!isBundle)
			{
				return new AppManifestAppx(packageBasePath, manifestBasePath);
			}
			return new AppManifestAppxBundle(packageBasePath, manifestBasePath);
		}

		protected AppManifestAppxBase(string packageBasePath, string manifestBasePath)
		{
			if (!string.IsNullOrEmpty(packageBasePath))
			{
				_packageBasePath = InboxAppUtils.ValidateFileOrDir(packageBasePath, false);
				_manifestBasePath = "AppxManifest.xml";
				return;
			}
			if (!string.IsNullOrEmpty(manifestBasePath))
			{
				_manifestBasePath = InboxAppUtils.ValidateFileOrDir(manifestBasePath, false);
				return;
			}
			throw new FileNotFoundException("The package or manifest name needs to be populated.");
		}

		protected void PopulateDefaultPackageProperties(IAppxManifestPackageId packageId)
		{
			_title = packageId.GetName();
			_publisher = packageId.GetPublisher();
			_version = packageId.GetVersion().ToString();
			_processorArchitecture = packageId.GetArchitecture();
			_resourceID = packageId.GetResourceId();
			_packageFullName = packageId.GetPackageFullName();
		}

		public abstract void ReadManifest();

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "Appx manifest: (Type): {0}, (Filename): \"{1}\", (Title): \"{2}\", (PackageFullName): \"{3}\" ", _manifestType, Filename, Title, PackageFullName);
		}
	}
}
