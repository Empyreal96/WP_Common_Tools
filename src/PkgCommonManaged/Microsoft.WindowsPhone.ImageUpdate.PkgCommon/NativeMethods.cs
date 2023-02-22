using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	internal static class NativeMethods
	{
		private const string PKGCOMMON_DLL = "UpdateDLL.dll";

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern FileType FileEntryBase_Get_FileType(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.LPWStr)]
		public static extern string FileEntryBase_Get_DevicePath(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.LPWStr)]
		public static extern string FileEntryBase_Get_CabPath(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.LPWStr)]
		public static extern string FileEntryBase_Get_FileHash(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.U1)]
		public static extern bool FileEntryBase_Get_SignInfo(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern FileAttributes DSMFileEntry_Get_Attributes(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.LPWStr)]
		public static extern string DSMFileEntry_Get_SourcePackage(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.LPWStr)]
		public static extern string DSMFileEntry_Get_EmbeddedSigningCategory(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.U8)]
		public static extern ulong DSMFileEntry_Get_FileSize(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.U8)]
		public static extern ulong DSMFileEntry_Get_CompressedFileSize(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.U8)]
		public static extern ulong DSMFileEntry_Get_StagedFileSize(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern DiffType DiffFileEntry_Get_DiffType(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.LPWStr)]
		public static extern string PackageDescriptor_Get_Keyform(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.LPWStr)]
		public static extern string PackageDescriptor_Get_Name(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.LPWStr)]
		public static extern string PackageDescriptor_Get_Owner(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int PackageDescriptor_Set_Owner(IntPtr objPtr, string owner);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.LPWStr)]
		public static extern string PackageDescriptor_Get_Component(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int PackageDescriptor_Set_Component(IntPtr objPtr, string component);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.LPWStr)]
		public static extern string PackageDescriptor_Get_SubComponent(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int PackageDescriptor_Set_SubComponent(IntPtr objPtr, string subComponent);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.LPWStr)]
		public static extern string PackageDescriptor_Get_BuildString(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int PackageDescriptor_Set_BuildString(IntPtr objPtr, string buildString);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern string PackageDescriptor_Get_PublicKey(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int PackageDescriptor_Set_PublicKey(IntPtr objPtr, string publicKeyString);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern OwnerType PackageDescriptor_Get_OwnerType(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int PackageDescriptor_Set_OwnerType(IntPtr objPtr, OwnerType ownerType);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern ReleaseType PackageDescriptor_Get_ReleaseType(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int PackageDescriptor_Set_ReleaseType(IntPtr objPtr, ReleaseType releaseType);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern BuildType PackageDescriptor_Get_BuildType(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int PackageDescriptor_Set_BuildType(IntPtr objPtr, BuildType buildType);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern CpuId PackageDescriptor_Get_CpuType(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int PackageDescriptor_Set_CpuType(IntPtr objPtr, CpuId cpuType);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.LPWStr)]
		public static extern string PackageDescriptor_Get_Culture(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int PackageDescriptor_Set_Culture(IntPtr objPtr, string culture);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.LPWStr)]
		public static extern string PackageDescriptor_Get_Resolution(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int PackageDescriptor_Set_Resolution(IntPtr objPtr, string Resolution);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.LPWStr)]
		public static extern string PackageDescriptor_Get_Description(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int PackageDescriptor_Set_Description(IntPtr objPtr, string description);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.LPWStr)]
		public static extern string PackageDescriptor_Get_GroupingKey(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int PackageDescriptor_Set_GroupingKey(IntPtr objPtr, string groupingKey);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.U1)]
		public static extern bool PackageDescriptor_Get_IsBinaryPartition(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.U1)]
		public static extern bool PackageDescriptor_Get_IsRemoval(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int PackageDescriptor_Set_IsRemoval(IntPtr objPtr, [MarshalAs(UnmanagedType.U1)] bool isRemoval);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.LPWStr)]
		public static extern string PackageDescriptor_Get_Partition(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int PackageDescriptor_Set_Partition(IntPtr objPtr, string partition);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.LPWStr)]
		public static extern string PackageDescriptor_Get_Platform(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int PackageDescriptor_Set_Platform(IntPtr objPtr, string platform);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern IntPtr PackageDescriptor_Get_TargetGroups(IntPtr objPtr, ref int cGroups);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int PackageDescriptor_Set_TargetGroups(IntPtr objPtr, string[] targetGroups, int cGroups);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern void PackageDescriptor_Get_Version(IntPtr objPtr, [In][Out] ref VersionInfo version);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int PackageDescriptor_Set_Version(IntPtr objPtr, [In] ref VersionInfo version);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern void DeviceSideManifest_Clear_Files(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int DeviceSideManifest_Add_File(IntPtr objPtr, FileType fileType, string devicePath, string cabPath, FileAttributes attributes, string sourcePackage, string embedSignCategory, ulong FileSize, ulong CompressedFileSize, ulong StagedFileSize, string fileHash, [MarshalAs(UnmanagedType.U1)] bool signFile);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int DeviceSideManifest_Get_Files(IntPtr objPtr, ref IntPtr filesObjPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int DeviceSideManifest_Load(IntPtr objPtr, string xmlPath);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int DeviceSideManifest_Load_CBS(IntPtr objPtr, string cabPath);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int DeviceSideManifest_Save(IntPtr objPtr, string xmlPath);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern IntPtr DeviceSideManifest_Create();

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern IntPtr DeviceSideManifest_Free(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.LPWStr)]
		public static extern string DiffManifest_Get_Name(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int DiffManifest_Set_Name(IntPtr objPtr, string name);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern IntPtr DiffManifest_Get_SourceHash(IntPtr objPtr, out int cbHash);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int DiffManifest_Set_SourceHash(IntPtr objPtr, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] hash, int cbHash);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern void DiffManifest_Get_TargetVersion(IntPtr objPtr, [In][Out] ref VersionInfo version);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int DiffManifest_Set_TargetVersion(IntPtr objPtr, [In] ref VersionInfo version);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern void DiffManifest_Get_SourceVersion(IntPtr objPtr, [In][Out] ref VersionInfo version);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int DiffManifest_Set_SourceVersion(IntPtr objPtr, [In] ref VersionInfo version);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int DiffManifest_Add_File(IntPtr objPtr, FileType fileType, DiffType diffType, string devicePath, string cabPath);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int DiffManifest_Get_Files(IntPtr objPtr, ref IntPtr filesObjPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int DiffManifest_Load(IntPtr objPtr, string xmlPath);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int DiffManifest_Load_XPD(IntPtr dmsPtr, IntPtr diffPtr, string xmlPath);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int DiffManifest_Save(IntPtr objPtr, string xmlPath);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern IntPtr DiffManifest_Create();

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern void DiffManifest_Clear_Files(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern IntPtr DiffManifest_Free(IntPtr objPtr);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern void Helper_Free_Array(IntPtr arrayPtr);

		public static void CheckHResult(int hr, string function)
		{
			if (hr != 0)
			{
				throw new PackageException("Unexpected hr value ({0:X8}) from function '{1}'", hr, function);
			}
		}

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern uint IU_GetStagedAndCompressedSize(string file, out ulong fileSize, out ulong stagedSize, out ulong compressedSize);
	}
}
