using System;
using System.IO;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	internal class WPInstalledPackage : WPCanonicalPackage
	{
		protected WPInstalledPackage(string installationRoot, PkgManifest pkgManifest)
			: base(pkgManifest)
		{
			pkgManifest.BuildSourcePaths(installationRoot, BuildPathOption.UseDevicePath, true);
		}

		protected override void ExtractFiles(FileEntryBase[] files, string[] targetPaths)
		{
			for (int i = 0; i < files.Length; i++)
			{
				LongPathFile.Copy(files[i].SourcePath, targetPaths[i], true);
			}
		}

		public static WPInstalledPackage Load(string installationDir, string manifestPath)
		{
			PkgManifest pkgManifest = null;
			pkgManifest = ((!Path.GetExtension(manifestPath).Equals(PkgConstants.c_strMumExtension, StringComparison.OrdinalIgnoreCase)) ? PkgManifest.Load(manifestPath) : PkgManifest.Load_CBS(manifestPath));
			return new WPInstalledPackage(installationDir, pkgManifest);
		}
	}
}
