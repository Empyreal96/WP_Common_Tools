using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	public class AppManifestAppx : AppManifestAppxBase
	{
		public AppManifestAppx(string packageBasePath, string manifestBasePath)
			: base(packageBasePath, manifestBasePath)
		{
			_manifestType = AppxManifestType.MainPackage;
		}

		public override void ReadManifest()
		{
			StringBuilder stringBuilder = new StringBuilder();
			LogUtil.Message("Parsing AppxManifest: {0}", _packageBasePath);
			IAppxFactory appxFactory = (IAppxFactory)new AppxFactory();
			IAppxManifestPackageId appxManifestPackageId = null;
			IAppxManifestReader appxManifestReader = null;
			IAppxManifestProperties appxManifestProperties = null;
			try
			{
				if (!string.IsNullOrEmpty(_packageBasePath))
				{
					IStream inputStream = StreamFactory.CreateFileStream(_packageBasePath);
					appxManifestReader = appxFactory.CreatePackageReader(inputStream).GetManifest();
				}
				else
				{
					IStream inputStream = StreamFactory.CreateFileStream(_manifestBasePath);
					appxManifestReader = appxFactory.CreateManifestReader(inputStream);
				}
				appxManifestProperties = appxManifestReader.GetProperties();
				appxManifestPackageId = appxManifestReader.GetPackageId();
				PopulateDefaultPackageProperties(appxManifestPackageId);
			}
			catch
			{
				LogUtil.Message("An exception occured while trying to read the manifest.");
				throw;
			}
			try
			{
				IAppxManifestQualifiedResourcesEnumerator qualifiedResources = ((IAppxManifestReader2)appxManifestReader).GetQualifiedResources();
				uint num = 0u;
				string empty = string.Empty;
				DX_FEATURE_LEVEL dX_FEATURE_LEVEL = DX_FEATURE_LEVEL.DX_FEATURE_LEVEL_UNSPECIFIED;
				while (qualifiedResources.GetHasCurrent())
				{
					IAppxManifestQualifiedResource current = qualifiedResources.GetCurrent();
					dX_FEATURE_LEVEL = current.GetDXFeatureLevel();
					num = current.GetScale();
					empty = current.GetLanguage();
					if (num != 0)
					{
						_resources.Add(new Resource("Scale", num.ToString()));
						num = 0u;
					}
					else if (!string.IsNullOrEmpty(current.GetLanguage()))
					{
						_resources.Add(new Resource("Language", empty));
						empty = string.Empty;
					}
					else if (dX_FEATURE_LEVEL != 0)
					{
						_resources.Add(new Resource("DXFeatureLevel", dX_FEATURE_LEVEL.ToString()));
						dX_FEATURE_LEVEL = DX_FEATURE_LEVEL.DX_FEATURE_LEVEL_UNSPECIFIED;
					}
					qualifiedResources.MoveNext();
				}
				_isFramework = appxManifestProperties.GetBoolValue("Framework");
				_isResource = appxManifestProperties.GetBoolValue("ResourcePackage");
				IAppxManifestPackageDependenciesEnumerator packageDependencies = appxManifestReader.GetPackageDependencies();
				PackageDependency packageDependency = new PackageDependency();
				while (packageDependencies.GetHasCurrent())
				{
					IAppxManifestPackageDependency current2 = packageDependencies.GetCurrent();
					packageDependency.Name = current2.GetName();
					packageDependency.MinVersion = current2.GetMinVersion().ToString();
					_packageDependencies.Add(packageDependency);
					packageDependencies.MoveNext();
				}
			}
			catch (COMException ex)
			{
				if (ex.ErrorCode == -2147221164)
				{
					throw new ArgumentException("The system failed to find the appropriate AppxPackaging class registration when trying to parse appx or appxbundle.");
				}
				LogUtil.Error("An exception occured while trying to extract properties from the AppxPackage file.");
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
