using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.WindowsPhone.ImageUpdate.Tools;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public class PkgManifest
	{
		public SortedDictionary<string, FileEntry> m_files = new SortedDictionary<string, FileEntry>(StringComparer.InvariantCultureIgnoreCase);

		public ReleaseType ReleaseType { get; set; }

		public PackageStyle PackageStyle { get; set; }

		public OwnerType OwnerType { get; set; }

		public BuildType BuildType { get; set; }

		public CpuId CpuType { get; set; }

		public string FeatureType { get; set; }

		public string Culture { get; set; }

		public string Resolution { get; set; }

		public string PublicKey { get; set; }

		public string Name
		{
			get
			{
				if (PackageStyle == PackageStyle.CBS)
				{
					return PackageName;
				}
				if (string.IsNullOrEmpty(Owner) || string.IsNullOrEmpty(Component))
				{
					return string.Empty;
				}
				return PackageTools.BuildPackageName(Owner, Component, SubComponent, Culture, Resolution);
			}
		}

		public string PackageName { get; set; }

		public string Owner { get; set; }

		public string Component { get; set; }

		public string SubComponent { get; set; }

		public VersionInfo Version { get; set; }

		public string Partition { get; set; }

		public string Platform { get; set; }

		public bool IsRemoval { get; set; }

		public string GroupingKey { get; set; }

		public string[] TargetGroups { get; set; }

		public string BuildString { get; set; }

		public bool IsBinaryPartition { get; private set; }

		public string Keyform { get; private set; }

		public FileEntry[] Files
		{
			get
			{
				return m_files.Values.ToArray();
			}
			set
			{
				m_files.Clear();
				Array.ForEach(value, delegate(FileEntry x)
				{
					m_files.Add(x.DevicePath, x);
				});
			}
		}

		public PkgManifest()
		{
			Owner = string.Empty;
			Component = string.Empty;
			SubComponent = string.Empty;
			Version = VersionInfo.Empty;
			Culture = string.Empty;
			Resolution = string.Empty;
			Partition = string.Empty;
			Platform = string.Empty;
			GroupingKey = string.Empty;
			FeatureType = string.Empty;
			BuildString = string.Empty;
			TargetGroups = new string[0];
			IsBinaryPartition = false;
		}

		public void BuildSourcePaths(string rootDir, BuildPathOption option)
		{
			BuildSourcePaths(rootDir, option, false);
		}

		public void BuildSourcePaths(string rootDir, BuildPathOption option, bool installed)
		{
			if (m_files == null)
			{
				return;
			}
			foreach (FileEntry value in m_files.Values)
			{
				value.BuildSourcePath(rootDir, option, installed);
			}
		}

		public void Save(string dsmPath)
		{
			IntPtr intPtr = NativeMethods.DeviceSideManifest_Create();
			if (intPtr == IntPtr.Zero)
			{
				throw new PackageException("Failed to create DSM object");
			}
			try
			{
				ExportToNativeObject(intPtr);
				NativeMethods.CheckHResult(NativeMethods.DeviceSideManifest_Save(intPtr, dsmPath), "DeviceSideManifest_Save");
			}
			catch (Exception innerException)
			{
				throw new PackageException(innerException, "Unable to save package manifest from file '{0}'", dsmPath);
			}
			finally
			{
				NativeMethods.DeviceSideManifest_Free(intPtr);
			}
		}

		public string CpuString()
		{
			switch (CpuType)
			{
			case CpuId.AMD64_X86:
				return "wow64";
			case CpuId.ARM64_ARM:
				return "arm64.arm";
			case CpuId.ARM64_X86:
				return "arm64.x86";
			default:
				return CpuType.ToString().ToLower(CultureInfo.InvariantCulture);
			}
		}

		public static PkgManifest Load(string dsmPath)
		{
			IntPtr intPtr = NativeMethods.DeviceSideManifest_Create();
			try
			{
				NativeMethods.CheckHResult(NativeMethods.DeviceSideManifest_Load(intPtr, dsmPath), "DeviceSideManifest_LoadFromXml");
				PkgManifest pkgManifest = new PkgManifest();
				pkgManifest.PackageStyle = PackageStyle.SPKG;
				pkgManifest.ImportFromNativeObject(intPtr);
				return pkgManifest;
			}
			catch (Exception innerException)
			{
				throw new PackageException(innerException, "Unable to load package manifest from file '{0}'", dsmPath);
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					NativeMethods.DeviceSideManifest_Free(intPtr);
				}
			}
		}

		public static PkgManifest Load_CBS(string pkgpath)
		{
			IntPtr intPtr = NativeMethods.DeviceSideManifest_Create();
			string dirPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			try
			{
				NativeMethods.CheckHResult(NativeMethods.DeviceSideManifest_Load_CBS(intPtr, LongPath.GetFullPathUNC(pkgpath)), "DeviceSideManifest_Load_CBS");
				PkgManifest pkgManifest = new PkgManifest();
				pkgManifest.PackageStyle = PackageStyle.CBS;
				pkgManifest.ImportFromNativeObject(intPtr);
				return pkgManifest;
			}
			finally
			{
				FileUtils.DeleteTree(dirPath);
				if (intPtr != IntPtr.Zero)
				{
					NativeMethods.DeviceSideManifest_Free(intPtr);
				}
			}
		}

		public static PkgManifest Load(string pkgPath, string pathInCab)
		{
			string tempDirectory = FileUtils.GetTempDirectory();
			try
			{
				ulong[] fileSizes;
				if (CabApiWrapper.GetFileList(pkgPath, out fileSizes).Contains("update.mum"))
				{
					return Load_CBS(pkgPath);
				}
				string text = Path.Combine(tempDirectory, pathInCab);
				try
				{
					CabApiWrapper.ExtractOne(pkgPath, tempDirectory, pathInCab);
				}
				catch (Exception innerException)
				{
					throw new PackageException(innerException, "Failed to extract man.dsm.xml from package '{0}'", pkgPath);
				}
				if (!LongPathFile.Exists(text))
				{
					throw new PackageException("Failed to extract the package manifest file from package '{0}'", pkgPath);
				}
				return Load(text);
			}
			finally
			{
				FileUtils.DeleteTree(tempDirectory);
			}
		}

		public void ImportFromNativeObject(IntPtr dsmObjPtr)
		{
			OwnerType = NativeMethods.PackageDescriptor_Get_OwnerType(dsmObjPtr);
			ReleaseType = NativeMethods.PackageDescriptor_Get_ReleaseType(dsmObjPtr);
			BuildType = NativeMethods.PackageDescriptor_Get_BuildType(dsmObjPtr);
			CpuType = NativeMethods.PackageDescriptor_Get_CpuType(dsmObjPtr);
			Keyform = NativeMethods.PackageDescriptor_Get_Keyform(dsmObjPtr);
			PackageName = NativeMethods.PackageDescriptor_Get_Name(dsmObjPtr);
			Owner = NativeMethods.PackageDescriptor_Get_Owner(dsmObjPtr);
			Component = NativeMethods.PackageDescriptor_Get_Component(dsmObjPtr);
			SubComponent = NativeMethods.PackageDescriptor_Get_SubComponent(dsmObjPtr);
			VersionInfo version = default(VersionInfo);
			NativeMethods.PackageDescriptor_Get_Version(dsmObjPtr, ref version);
			Version = version;
			Culture = NativeMethods.PackageDescriptor_Get_Culture(dsmObjPtr);
			Resolution = NativeMethods.PackageDescriptor_Get_Resolution(dsmObjPtr);
			Partition = NativeMethods.PackageDescriptor_Get_Partition(dsmObjPtr);
			Platform = NativeMethods.PackageDescriptor_Get_Platform(dsmObjPtr);
			IsRemoval = NativeMethods.PackageDescriptor_Get_IsRemoval(dsmObjPtr);
			GroupingKey = NativeMethods.PackageDescriptor_Get_GroupingKey(dsmObjPtr);
			BuildString = NativeMethods.PackageDescriptor_Get_BuildString(dsmObjPtr);
			PublicKey = NativeMethods.PackageDescriptor_Get_PublicKey(dsmObjPtr);
			IsBinaryPartition = NativeMethods.PackageDescriptor_Get_IsBinaryPartition(dsmObjPtr);
			int cGroups = 0;
			IntPtr intPtr = NativeMethods.PackageDescriptor_Get_TargetGroups(dsmObjPtr, ref cGroups);
			List<string> list = new List<string>();
			if (cGroups != 0)
			{
				IntPtr[] array = new IntPtr[cGroups];
				Marshal.Copy(intPtr, array, 0, cGroups);
				IntPtr[] array2 = array;
				foreach (IntPtr ptr in array2)
				{
					list.Add(Marshal.PtrToStringUni(ptr));
				}
				NativeMethods.Helper_Free_Array(intPtr);
			}
			TargetGroups = list.ToArray();
			IntPtr filesObjPtr = IntPtr.Zero;
			int num = NativeMethods.DeviceSideManifest_Get_Files(dsmObjPtr, ref filesObjPtr);
			if (num > 0)
			{
				IntPtr[] array3 = new IntPtr[num];
				Marshal.Copy(filesObjPtr, array3, 0, num);
				IntPtr[] array2 = array3;
				for (int i = 0; i < array2.Length; i++)
				{
					FileEntry fileEntry = new FileEntry(array2[i]);
					if (m_files.ContainsKey(fileEntry.DevicePath))
					{
						throw new PackageException("File collision in package '{0}' for file '{1}'", Name, fileEntry.DevicePath);
					}
					m_files.Add(fileEntry.DevicePath, fileEntry);
				}
			}
			ValidatePackage();
		}

		private void ValidatePackage()
		{
			if (PackageStyle == PackageStyle.CBS && Version.Equals(VersionInfo.Parse("0.0.0.0")))
			{
				throw new PackageException("Version for package '{0}' cannot be 0.0.0.0", Name);
			}
			if (OwnerType == OwnerType.Invalid)
			{
				throw new PackageException("OwnerType is Invalid for package '{0}'", Name);
			}
			if (ReleaseType == ReleaseType.Invalid)
			{
				throw new PackageException("ReleaseType is Invalid for package '{0}'", Name);
			}
			if (BuildType == BuildType.Invalid)
			{
				throw new PackageException("BuildType is Invalid for package '{0}'", Name);
			}
			if (CpuType == CpuId.Invalid)
			{
				throw new PackageException("CpuType is Invalid for package '{0}'", Name);
			}
		}

		private void ExportToNativeObject(IntPtr dsmObjPtr)
		{
			NativeMethods.CheckHResult(NativeMethods.PackageDescriptor_Set_OwnerType(dsmObjPtr, OwnerType), "PackageDescriptor_Set_OwnerType");
			NativeMethods.CheckHResult(NativeMethods.PackageDescriptor_Set_OwnerType(dsmObjPtr, OwnerType), "PackageDescriptor_Set_OwnerType");
			NativeMethods.CheckHResult(NativeMethods.PackageDescriptor_Set_ReleaseType(dsmObjPtr, ReleaseType), "PackageDescriptor_Set_ReleaseType");
			NativeMethods.CheckHResult(NativeMethods.PackageDescriptor_Set_BuildType(dsmObjPtr, BuildType), "PackageDescriptor_Set_BuildType");
			NativeMethods.CheckHResult(NativeMethods.PackageDescriptor_Set_CpuType(dsmObjPtr, CpuType), "PackageDescriptor_Set_CpuType");
			NativeMethods.CheckHResult(NativeMethods.PackageDescriptor_Set_Owner(dsmObjPtr, Owner), "PackageDescriptor_Set_Owner");
			NativeMethods.CheckHResult(NativeMethods.PackageDescriptor_Set_Component(dsmObjPtr, Component), "PackageDescriptor_Set_Component");
			NativeMethods.CheckHResult(NativeMethods.PackageDescriptor_Set_SubComponent(dsmObjPtr, SubComponent), "PackageDescriptor_Set_SubComponent");
			VersionInfo version = Version;
			NativeMethods.CheckHResult(NativeMethods.PackageDescriptor_Set_Version(dsmObjPtr, ref version), "PackageDescriptor_Set_Version");
			NativeMethods.CheckHResult(NativeMethods.PackageDescriptor_Set_Culture(dsmObjPtr, Culture), "PackageDescriptor_Set_Culture");
			NativeMethods.CheckHResult(NativeMethods.PackageDescriptor_Set_Resolution(dsmObjPtr, Resolution), "PackageDescriptor_Set_Resolution");
			NativeMethods.CheckHResult(NativeMethods.PackageDescriptor_Set_Partition(dsmObjPtr, Partition), "PackageDescriptor_Set_Partition");
			NativeMethods.CheckHResult(NativeMethods.PackageDescriptor_Set_Platform(dsmObjPtr, Platform), "PackageDescriptor_Set_Platform");
			NativeMethods.CheckHResult(NativeMethods.PackageDescriptor_Set_IsRemoval(dsmObjPtr, IsRemoval), "PackageDescriptor_Set_IsRemoval");
			NativeMethods.CheckHResult(NativeMethods.PackageDescriptor_Set_GroupingKey(dsmObjPtr, GroupingKey), "PackageDescriptor_Set_GroupingKey");
			NativeMethods.CheckHResult(NativeMethods.PackageDescriptor_Set_BuildString(dsmObjPtr, BuildString), "PackageDescriptor_Set_BuildString");
			if (PublicKey != null)
			{
				NativeMethods.CheckHResult(NativeMethods.PackageDescriptor_Set_PublicKey(dsmObjPtr, PublicKey), "PackageDescriptor_Set_PublicKey");
			}
			NativeMethods.CheckHResult(NativeMethods.PackageDescriptor_Set_TargetGroups(dsmObjPtr, TargetGroups, TargetGroups.Length), "PackageDescriptor_Set_TargetGroups");
			foreach (FileEntry value in m_files.Values)
			{
				NativeMethods.CheckHResult(NativeMethods.DeviceSideManifest_Add_File(dsmObjPtr, value.FileType, value.DevicePath, value.CabPath, value.Attributes, value.SourcePackage, value.EmbeddedSigningCategory, value.Size, value.CompressedSize, value.StagedSize, value.FileHash, value.SignInfoRequired), "DeviceSideManifest_Add_File");
			}
		}
	}
}
