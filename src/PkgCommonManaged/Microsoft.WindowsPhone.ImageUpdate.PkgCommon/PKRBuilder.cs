using System;
using System.IO;
using Microsoft.WindowsPhone.ImageUpdate.Tools;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	internal static class PKRBuilder
	{
		internal static void Create(string inputCab, string outputCab)
		{
			Console.WriteLine(Path.ChangeExtension(outputCab, PkgConstants.c_strRemovalCbsExtension));
			PkgManifest pkgManifest = PkgManifest.Load(inputCab, PkgConstants.c_strDsmFile);
			if (pkgManifest.IsRemoval)
			{
				throw new PackageException("Input package '{0}' can not be a removal package", inputCab);
			}
			if (pkgManifest.IsBinaryPartition)
			{
				throw new PackageException("Input package '{0}' can not be a binary partition package", inputCab);
			}
			pkgManifest.IsRemoval = true;
			IPkgBuilder pkgBuilder = new PkgBuilder(pkgManifest);
			if (pkgManifest.PackageStyle == PackageStyle.CBS)
			{
				pkgBuilder.SaveCBSR(outputCab, CompressionType.FastLZX);
			}
			else
			{
				pkgBuilder.SaveCab(outputCab, true);
			}
		}
	}
}
