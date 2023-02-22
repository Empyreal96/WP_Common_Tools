using System.IO;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	internal class WPExtractedPackage : WPCanonicalPackage
	{
		protected WPExtractedPackage(string extractedRootDir, PkgManifest pkgManifest)
			: base(pkgManifest)
		{
			pkgManifest.BuildSourcePaths(extractedRootDir, BuildPathOption.UseCabPath);
		}

		protected override void ExtractFiles(FileEntryBase[] files, string[] targetPaths)
		{
			for (int i = 0; i < files.Length; i++)
			{
				File.Copy(files[i].SourcePath, targetPaths[i], true);
			}
		}

		public static WPExtractedPackage Load(string extractedRootDir)
		{
			PkgManifest pkgManifest;
			if (LongPathFile.Exists(Path.Combine(extractedRootDir, PkgConstants.c_strMumFile)))
			{
				pkgManifest = PkgManifest.Load_CBS(Path.Combine(extractedRootDir, PkgConstants.c_strMumFile));
				pkgManifest.PackageStyle = PackageStyle.CBS;
			}
			else
			{
				pkgManifest = PkgManifest.Load(Path.Combine(extractedRootDir, PkgConstants.c_strDsmFile));
				pkgManifest.PackageStyle = PackageStyle.SPKG;
			}
			return new WPExtractedPackage(extractedRootDir, pkgManifest);
		}
	}
}
