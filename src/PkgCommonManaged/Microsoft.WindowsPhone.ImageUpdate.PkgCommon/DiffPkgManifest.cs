using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public sealed class DiffPkgManifest
	{
		[XmlIgnore]
		public SortedDictionary<string, DiffFileEntry> m_files;

		public string Name { get; set; }

		public VersionInfo SourceVersion { get; set; }

		public VersionInfo TargetVersion { get; set; }

		public byte[] SourceHash { get; set; }

		public DiffFileEntry[] Files
		{
			get
			{
				return m_files.Values.ToArray();
			}
			set
			{
				m_files.Clear();
				foreach (DiffFileEntry fe in value)
				{
					AddFileEntry(fe);
				}
			}
		}

		public DiffFileEntry TargetDsmFile
		{
			get
			{
				DiffFileEntry result = null;
				try
				{
					result = m_files.Values.First((DiffFileEntry dfe) => dfe.DiffType == DiffType.TargetDSM);
					return result;
				}
				catch (InvalidOperationException)
				{
					return result;
				}
			}
		}

		public DiffPkgManifest()
		{
			Name = string.Empty;
			SourceHash = new byte[0];
			SourceVersion = VersionInfo.Empty;
			TargetVersion = VersionInfo.Empty;
			m_files = new SortedDictionary<string, DiffFileEntry>(StringComparer.InvariantCultureIgnoreCase);
		}

		public void AddFileEntry(DiffFileEntry fe)
		{
			if (fe.DiffType == DiffType.Invalid)
			{
				throw new PackageException("Trying to add uninitialized diff file entry");
			}
			if (m_files.ContainsKey(fe.DevicePath))
			{
				throw new PackageException("Multiple file entries with same device path are not allowed in DDSM");
			}
			m_files.Add(fe.DevicePath, fe);
		}

		public void Save(string diffDsmPath)
		{
			if (string.IsNullOrEmpty(diffDsmPath))
			{
				throw new ArgumentNullException("diffDsmPath", "the path to diff DSM is null or empty");
			}
			MakeShortPaths();
			Validate();
			IntPtr intPtr = NativeMethods.DiffManifest_Create();
			try
			{
				ExportToNativeObject(intPtr);
				NativeMethods.CheckHResult(NativeMethods.DiffManifest_Save(intPtr, diffDsmPath), "DiffManifest_Save");
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					NativeMethods.DiffManifest_Free(intPtr);
				}
			}
		}

		public static DiffPkgManifest Load(string diffDsmPath)
		{
			if (string.IsNullOrEmpty(diffDsmPath))
			{
				throw new PackageException("The path to diff DSM is null or empty");
			}
			IntPtr intPtr = NativeMethods.DiffManifest_Create();
			try
			{
				NativeMethods.CheckHResult(NativeMethods.DiffManifest_Load(intPtr, diffDsmPath), "DiffManifest_Load");
				DiffPkgManifest diffPkgManifest = new DiffPkgManifest();
				diffPkgManifest.ImportFromNativeObject(intPtr);
				diffPkgManifest.Validate();
				return diffPkgManifest;
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					NativeMethods.DiffManifest_Free(intPtr);
				}
			}
		}

		public static void Load_Diff_CBS(string diffDsmPath, out DiffPkgManifest diffMan, out PkgManifest pkgMan)
		{
			if (string.IsNullOrEmpty(diffDsmPath))
			{
				throw new PackageException("The path to diff DSM is null or empty");
			}
			IntPtr intPtr = NativeMethods.DiffManifest_Create();
			IntPtr intPtr2 = NativeMethods.DeviceSideManifest_Create();
			try
			{
				NativeMethods.CheckHResult(NativeMethods.DiffManifest_Load_XPD(intPtr2, intPtr, diffDsmPath), "DiffManifest_Load_XPD");
				diffMan = new DiffPkgManifest();
				diffMan.ImportFromNativeObject(intPtr);
				pkgMan = new PkgManifest();
				pkgMan.ImportFromNativeObject(intPtr2);
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					NativeMethods.DiffManifest_Free(intPtr);
				}
				if (intPtr2 != IntPtr.Zero)
				{
					NativeMethods.DeviceSideManifest_Free(intPtr2);
				}
			}
		}

		public void BuildSourcePath(string rootDir, BuildPathOption option)
		{
			foreach (DiffFileEntry value in m_files.Values)
			{
				if (value.DiffType != DiffType.Remove)
				{
					value.BuildSourcePath(rootDir, option, false);
				}
			}
			Validate();
		}

		private void Validate()
		{
			if (string.IsNullOrEmpty(Name))
			{
				throw new PackageException("Empty Name is not allowed in a diff package");
			}
			if (SourceHash == null || SourceHash.Length != PkgConstants.c_iHashSize)
			{
				throw new PackageException("Invalid SourceHash size ({0}) for a diff package", (SourceHash != null) ? SourceHash.Length : 0);
			}
			if (SourceVersion == VersionInfo.Empty)
			{
				throw new PackageException("Empty SourceVerison is not allowed in a diff package");
			}
			if (!(SourceVersion < TargetVersion))
			{
				throw new PackageException("SourceVersion ({0}) must be less than TargetVersion ({1}) in a diff package", SourceVersion, TargetVersion);
			}
			DiffFileEntry value = null;
			string text = Path.Combine(PkgConstants.c_strDsmDeviceFolder, Name + PkgConstants.c_strDsmExtension);
			if (!m_files.TryGetValue(text, out value) || value.DiffType != DiffType.TargetDSM)
			{
				throw new PackageException("DSM file ({0}) not found in the diff file list", text);
			}
			foreach (DiffFileEntry value2 in m_files.Values)
			{
				value2.Validate();
			}
		}

		private void MakeShortPaths()
		{
			int num = 0;
			foreach (DiffFileEntry value in m_files.Values)
			{
				if (value.DiffType == DiffType.TargetDSM)
				{
					value.CabPath = PkgConstants.c_strDsmFile;
				}
				else
				{
					value.CabPath = PackageTools.MakeShortPath(value.DevicePath, num.ToString());
				}
				num++;
			}
		}

		private void ImportFromNativeObject(IntPtr diffDsmPtr)
		{
			Name = NativeMethods.DiffManifest_Get_Name(diffDsmPtr);
			VersionInfo version = default(VersionInfo);
			NativeMethods.DiffManifest_Get_SourceVersion(diffDsmPtr, ref version);
			SourceVersion = version;
			version = default(VersionInfo);
			NativeMethods.DiffManifest_Get_TargetVersion(diffDsmPtr, ref version);
			TargetVersion = version;
			int cbHash = 0;
			IntPtr source = NativeMethods.DiffManifest_Get_SourceHash(diffDsmPtr, out cbHash);
			if (cbHash != 0)
			{
				byte[] array = new byte[cbHash];
				Marshal.Copy(source, array, 0, cbHash);
				SourceHash = array;
			}
			IntPtr filesObjPtr = IntPtr.Zero;
			int num = NativeMethods.DiffManifest_Get_Files(diffDsmPtr, ref filesObjPtr);
			if (num > 0)
			{
				IntPtr[] array2 = new IntPtr[num];
				Marshal.Copy(filesObjPtr, array2, 0, num);
				IntPtr[] array3 = array2;
				for (int i = 0; i < array3.Length; i++)
				{
					DiffFileEntry diffFileEntry = new DiffFileEntry(array3[i]);
					m_files.Add(diffFileEntry.DevicePath, diffFileEntry);
				}
				NativeMethods.Helper_Free_Array(filesObjPtr);
			}
		}

		private void ExportToNativeObject(IntPtr diffDsmPtr)
		{
			NativeMethods.CheckHResult(NativeMethods.DiffManifest_Set_Name(diffDsmPtr, Name), "DiffManifest_Set_Name");
			VersionInfo version = SourceVersion;
			NativeMethods.CheckHResult(NativeMethods.DiffManifest_Set_SourceVersion(diffDsmPtr, ref version), "DiffManifest_Set_SourceVersion");
			version = TargetVersion;
			NativeMethods.CheckHResult(NativeMethods.DiffManifest_Set_TargetVersion(diffDsmPtr, ref version), "DiffManifest_Set_TargetVersion");
			NativeMethods.CheckHResult(NativeMethods.DiffManifest_Set_SourceHash(diffDsmPtr, SourceHash, SourceHash.Length), "DiffManifest_Set_SourceHash");
			foreach (DiffFileEntry value in m_files.Values)
			{
				NativeMethods.CheckHResult(NativeMethods.DiffManifest_Add_File(diffDsmPtr, value.FileType, value.DiffType, value.DevicePath, value.CabPath), "DiffManifest_Add_File");
			}
		}
	}
}
