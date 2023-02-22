using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	public class AppManifestAppxBundle : AppManifestAppxBase
	{
		public sealed class BundlePackage : IComparable, IComparer
		{
			private readonly List<Resource> _resources = new List<Resource>();

			public APPX_BUNDLE_PAYLOAD_PACKAGE_TYPE PackageType { get; set; }

			public string Version { get; set; }

			public string ProcessorArchitecture { get; set; }

			public string FileName { get; set; }

			public string ResourceID { get; set; }

			public List<Resource> Resources => _resources;

			public BundlePackage(APPX_BUNDLE_PAYLOAD_PACKAGE_TYPE packageType)
			{
				PackageType = packageType;
			}

			public int CompareTo(object obj)
			{
				return Compare(this, obj);
			}

			public int Compare(object obj1, object obj2)
			{
				if (obj1 is BundlePackage && obj2 is BundlePackage)
				{
					BundlePackage obj3 = obj1 as BundlePackage;
					BundlePackage bundlePackage = obj2 as BundlePackage;
					return string.Compare(obj3.FileName, bundlePackage.FileName, StringComparison.OrdinalIgnoreCase);
				}
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "cannot compare objects of type {0} against type {1}", new object[2]
				{
					obj1.GetType(),
					obj2.GetType()
				}));
			}

			public override bool Equals(object obj)
			{
				return Compare(this, obj) == 0;
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			public override string ToString()
			{
				return string.Format(CultureInfo.InvariantCulture, "(PackageType)=\"{0}\", (FileName)=\"{1}\", (Architecture)=\"{2}\"", new object[3] { PackageType, FileName, ProcessorArchitecture });
			}
		}

		private readonly List<BundlePackage> _bundlePackages = new List<BundlePackage>();

		public List<BundlePackage> BundlePackages => _bundlePackages;

		public AppManifestAppxBundle(string packageBasePath, string manifestBasePath)
			: base(packageBasePath, manifestBasePath)
		{
			_manifestType = AppxManifestType.Bundle;
			_isBundle = true;
		}

		public override void ReadManifest()
		{
			StringBuilder stringBuilder = new StringBuilder();
			LogUtil.Message("Parsing BundleManifest: {0}", _packageBasePath);
			try
			{
				IStream inputStream = StreamFactory.CreateFileStream(_packageBasePath);
				IAppxBundleReader appxBundleReader = ((IAppxBundleFactory)new AppxBundleFactory()).CreateBundleReader(inputStream);
				IAppxBundleManifestReader manifest = appxBundleReader.GetManifest();
				IAppxManifestPackageId appxManifestPackageId = null;
				appxManifestPackageId = manifest.GetPackageId();
				_title = appxManifestPackageId.GetName();
				_publisher = appxManifestPackageId.GetPublisher();
				_version = appxManifestPackageId.GetVersion().ToString();
				_packageFullName = appxManifestPackageId.GetPackageFullName();
				appxBundleReader.GetPayloadPackages();
				IAppxBundleManifestPackageInfoEnumerator packageInfoItems = manifest.GetPackageInfoItems();
				while (packageInfoItems.GetHasCurrent())
				{
					IAppxBundleManifestPackageInfo current = packageInfoItems.GetCurrent();
					IAppxManifestPackageId packageId = current.GetPackageId();
					uint num = 0u;
					string empty = string.Empty;
					DX_FEATURE_LEVEL dX_FEATURE_LEVEL = DX_FEATURE_LEVEL.DX_FEATURE_LEVEL_UNSPECIFIED;
					BundlePackage bundlePackage = new BundlePackage(current.GetPackageType());
					bundlePackage.FileName = current.GetFileName();
					bundlePackage.ProcessorArchitecture = packageId.GetArchitecture().ToString();
					bundlePackage.Version = packageId.GetVersion().ToString();
					bundlePackage.ResourceID = packageId.GetResourceId();
					IAppxManifestQualifiedResourcesEnumerator resources = current.GetResources();
					while (resources.GetHasCurrent())
					{
						IAppxManifestQualifiedResource current2 = resources.GetCurrent();
						dX_FEATURE_LEVEL = current2.GetDXFeatureLevel();
						num = current2.GetScale();
						empty = current2.GetLanguage();
						if (num != 0)
						{
							bundlePackage.Resources.Add(new Resource("Scale", num.ToString()));
							num = 0u;
						}
						else if (!string.IsNullOrEmpty(current2.GetLanguage()))
						{
							bundlePackage.Resources.Add(new Resource("Language", empty));
							empty = string.Empty;
						}
						else if (dX_FEATURE_LEVEL != 0)
						{
							bundlePackage.Resources.Add(new Resource("DXFeatureLevel", dX_FEATURE_LEVEL.ToString()));
							dX_FEATURE_LEVEL = DX_FEATURE_LEVEL.DX_FEATURE_LEVEL_UNSPECIFIED;
						}
						resources.MoveNext();
					}
					_bundlePackages.Add(bundlePackage);
					packageInfoItems.MoveNext();
				}
			}
			catch (COMException ex)
			{
				if (ex.ErrorCode == -2147221164)
				{
					throw new ArgumentException("The appxbundle provided has an invalid manifest.");
				}
				throw;
			}
			if (string.IsNullOrEmpty(_title))
			{
				stringBuilder.AppendLine("Title is not defined in the manifest");
			}
			if (string.IsNullOrEmpty(_version))
			{
				stringBuilder.AppendLine("Version is not defined in the manifest");
			}
			if (string.IsNullOrEmpty(_publisher))
			{
				stringBuilder.AppendLine("Publisher is not defined in the manifest");
			}
			if (!string.IsNullOrEmpty(stringBuilder.ToString()))
			{
				LogUtil.Error(stringBuilder.ToString());
				throw new InvalidDataException(stringBuilder.ToString());
			}
		}
	}
}
