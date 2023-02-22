using System;
using System.IO;
using System.Linq;
using Microsoft.WindowsPhone.ImageUpdate.Tools;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	internal class WPCanonicalPackage : WPPackageBase, IPkgInfo
	{
		public PackageType Type
		{
			get
			{
				if (!m_pkgManifest.IsRemoval)
				{
					return PackageType.Canonical;
				}
				return PackageType.Removal;
			}
		}

		public PackageStyle Style => m_pkgManifest.PackageStyle;

		public VersionInfo PrevVersion
		{
			get
			{
				throw new NotImplementedException("PreVersion is not available for canonical package");
			}
		}

		public byte[] PrevDsmHash => null;

		protected WPCanonicalPackage(PkgManifest pkgManifest)
			: base(pkgManifest)
		{
		}

		protected virtual void ExtractFiles(FileEntryBase[] files, string[] targetPaths)
		{
			throw new NotImplementedException();
		}

		public void ExtractFile(string devicePath, string destPath, bool overwriteExistingFiles)
		{
			FileEntryBase fileEntryBase = FindFile(devicePath) as FileEntryBase;
			if (fileEntryBase == null)
			{
				throw new PackageException("Failed to extract file '{0}' to '{1}': file specified doesn't exist in package", devicePath, destPath);
			}
			if (LongPathFile.Exists(destPath) && !overwriteExistingFiles)
			{
				throw new PackageException("Failed to extract file '{0}', file already exists and overwriteExistingFiles not set", destPath);
			}
			ExtractFiles(new FileEntryBase[1] { fileEntryBase }, new string[1] { destPath });
		}

		public void ExtractAll(string outputDir, bool overwriteExistingFiles)
		{
			string[] array = base.Files.Select((IFileEntry x) => outputDir + x.DevicePath).ToArray();
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (LongPathFile.Exists(text) && !overwriteExistingFiles)
				{
					throw new PackageException("Failed to extract file '{0}', file already exists and overwriteExistingFiles not set", text);
				}
			}
			ExtractFiles(base.Files.Cast<FileEntryBase>().ToArray(), array);
		}

		internal static WPExtractedPackage ExtractAndLoad(string cabPath, string outputDir)
		{
			try
			{
				CabApiWrapper.Extract(cabPath, outputDir);
			}
			catch (CabException innerException)
			{
				throw new PackageException(innerException, "ExtractAndLoad: Failed to extract contents in cab file '{0}' to '{1}'", cabPath, outputDir);
			}
			catch (IOException innerException2)
			{
				throw new PackageException(innerException2, "ExtractAndLoad: Failed to extract contents in cab file '{0}' to '{1}'", cabPath, outputDir);
			}
			return WPExtractedPackage.Load(outputDir);
		}

		internal static WPCabPackage LoadFromCab(string cabPath)
		{
			return WPCabPackage.Load(cabPath);
		}

		internal static WPInstalledPackage LoadFromInstallationDir(string manifestPath, string dirRoot)
		{
			return WPInstalledPackage.Load(dirRoot, manifestPath);
		}
	}
}
