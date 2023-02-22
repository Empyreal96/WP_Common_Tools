using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.WindowsPhone.ImageUpdate.Tools;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	internal class DiffPkg : WPPackageBase, IDiffPkg, IPkgInfo
	{
		private string m_strCabPath;

		private DiffPkgManifest m_diffManifest;

		public PackageType Type => PackageType.Diff;

		public PackageStyle Style => m_pkgManifest.PackageStyle;

		public VersionInfo PrevVersion => m_diffManifest.SourceVersion;

		public byte[] PrevDsmHash => m_diffManifest.SourceHash;

		public IEnumerable<IDiffEntry> DiffFiles => m_diffManifest.m_files.Values.Cast<IDiffEntry>();

		public DiffPkg(DiffPkgManifest diffPkgManifest, PkgManifest pkgManifest, string strCabPath)
			: base(pkgManifest)
		{
			m_strCabPath = strCabPath;
			m_diffManifest = diffPkgManifest;
		}

		public void ExtractFile(string devicePath, string destPath, bool overwriteExistingFiles)
		{
			DiffFileEntry value = null;
			if (!m_diffManifest.m_files.TryGetValue(devicePath, out value))
			{
				throw new PackageException("File '{0}' doesn't exist", devicePath);
			}
			if (value.DiffType == DiffType.Remove)
			{
				throw new PackageException("File '{0}' is removed, nothing to extract", devicePath);
			}
			if (LongPathFile.Exists(destPath) && !overwriteExistingFiles)
			{
				throw new PackageException("Failed to extract file '{0}', file already exists and overwriteExistingFiles not set", destPath);
			}
			LongPathFile.Delete(destPath);
			CabApiWrapper.ExtractSelected(m_strCabPath, new string[1] { value.CabPath }, new string[1] { destPath });
		}

		public void ExtractAll(string outputDir, bool overwriteExistingFiles)
		{
			IEnumerable<DiffFileEntry> source = m_diffManifest.m_files.Values.Where((DiffFileEntry x) => x.DiffType != DiffType.Remove);
			string[] filesToExtract = source.Select((DiffFileEntry x) => x.CabPath).ToArray();
			string[] array = source.Select((DiffFileEntry x) => outputDir + x.DevicePath).ToArray();
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (LongPathFile.Exists(text) && !overwriteExistingFiles)
				{
					throw new PackageException("Failed to extract file '{0}', file already exists and overwriteExistingFiles not set", text);
				}
				LongPathFile.Delete(text);
			}
			CabApiWrapper.ExtractSelected(m_strCabPath, filesToExtract, array);
		}

		internal static DiffPkg LoadFromCab(string cabPath)
		{
			if (string.IsNullOrEmpty(cabPath))
			{
				throw new PackageException("Empty CabPath specified for IDiffPkg.LoadFromCab");
			}
			string tempDirectory = FileUtils.GetTempDirectory();
			try
			{
				DiffPkgManifest diffMan;
				PkgManifest pkgMan;
				DiffPkgManifest.Load_Diff_CBS(cabPath, out diffMan, out pkgMan);
				DiffPkg result = new DiffPkg(diffMan, pkgMan, cabPath);
				pkgMan.PackageStyle = PackageStyle.CBS;
				return result;
			}
			catch (Exception)
			{
				try
				{
					CabApiWrapper.ExtractSelected(cabPath, tempDirectory, new string[2]
					{
						PkgConstants.c_strDsmFile,
						PkgConstants.c_strDiffDsmFile
					});
				}
				catch (Exception innerException)
				{
					throw new PackageException(innerException, "Internal exception when extracting package '{0}'", cabPath);
				}
				string text = Path.Combine(tempDirectory, PkgConstants.c_strDiffDsmFile);
				if (!LongPathFile.Exists(text))
				{
					throw new PackageException("No Diff manifest file found when loading package '{0}'", cabPath);
				}
				string text2 = Path.Combine(tempDirectory, PkgConstants.c_strDsmFile);
				if (!LongPathFile.Exists(text2))
				{
					throw new PackageException("No package manifest file found when loading package '{0}'", cabPath);
				}
				PkgManifest pkgManifest = PkgManifest.Load(text2);
				DiffPkg diffPkg = new DiffPkg(DiffPkgManifest.Load(text), pkgManifest, cabPath);
				diffPkg.Validate();
				pkgManifest.PackageStyle = PackageStyle.SPKG;
				return diffPkg;
			}
			finally
			{
				FileUtils.DeleteTree(tempDirectory);
			}
		}

		protected void Validate()
		{
			if (!string.Equals(base.Name, m_diffManifest.Name, StringComparison.InvariantCultureIgnoreCase))
			{
				throw new PackageException("The package name in DSM ('{0}') doesn't match the name in DDSM ('{1}')", base.Name, m_diffManifest.Name);
			}
			if (base.Version != m_diffManifest.TargetVersion)
			{
				throw new PackageException("DSM version ('{0}') and DDSM's Target Version ('{1}') are different", base.Version, m_diffManifest.TargetVersion);
			}
			bool flag = false;
			foreach (DiffFileEntry diffFile in DiffFiles)
			{
				IFileEntry fileEntry = FindFile(diffFile.DevicePath);
				if (diffFile.DiffType == DiffType.Remove)
				{
					if (fileEntry != null)
					{
						throw new PackageException("File '{0}' is marked as removed in diff manifest but still shows up in target manifest", diffFile.DevicePath);
					}
					continue;
				}
				if (fileEntry == null)
				{
					throw new PackageException("File '{0}' is in diff manifest but not found in target manifest", diffFile.DevicePath);
				}
				if (diffFile.DiffType == DiffType.TargetDSM)
				{
					if (fileEntry.FileType != FileType.Manifest)
					{
						throw new PackageException("Incorrect DSM DevicePath '{0}' in diff manifest", diffFile.DevicePath);
					}
					flag = true;
				}
			}
			if (!flag)
			{
				throw new PackageException("No DSM file is found in DiffPackage '{0}'", base.Name);
			}
		}
	}
}
