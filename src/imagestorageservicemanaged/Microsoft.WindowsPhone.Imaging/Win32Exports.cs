using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.WindowsPhone.Imaging
{
	public sealed class Win32Exports
	{
		private struct FILETIME
		{
			public uint DateTimeLow;

			public uint DateTimeHigh;
		}

		[CLSCompliant(false)]
		public enum MoveMethod : uint
		{
			FILE_BEGIN,
			FILE_CURRENT,
			FILE_END
		}

		[Flags]
		[CLSCompliant(false)]
		public enum DesiredAccess : uint
		{
			GENERIC_READ = 0x80000000u,
			GENERIC_WRITE = 0x40000000u
		}

		[Flags]
		[CLSCompliant(false)]
		public enum ShareMode : uint
		{
			FILE_SHARE_NONE = 0u,
			FILE_SHARE_READ = 1u,
			FILE_SHARE_WRITE = 2u,
			FILE_SHARE_DELETE = 4u
		}

		[Flags]
		[CLSCompliant(false)]
		public enum FlagsAndAttributes : uint
		{
			FILE_ATTRIBUTES_ARCHIVE = 0x20u,
			FILE_ATTRIBUTE_HIDDEN = 2u,
			FILE_ATTRIBUTE_NORMAL = 0x80u,
			FILE_ATTRIBUTE_OFFLINE = 0x1000u,
			FILE_ATTRIBUTE_READONLY = 1u,
			FILE_ATTRIBUTE_SYSTEM = 4u,
			FILE_ATTRIBUTE_TEMPORARY = 0x100u,
			FILE_FLAG_WRITE_THROUGH = 0x80000000u,
			FILE_FLAG_OVERLAPPED = 0x40000000u,
			FILE_FLAG_NO_BUFFERING = 0x20000000u,
			FILE_FLAG_RANDOM_ACCESS = 0x10000000u,
			FILE_FLAG_SEQUENTIAL_SCAN = 0x8000000u,
			FILE_FLAG_DELETE_ON = 0x4000000u,
			FILE_FLAG_POSIX_SEMANTICS = 0x1000000u,
			FILE_FLAG_OPEN_REPARSE_POINT = 0x200000u,
			FILE_FLAG_OPEN_NO_CALL = 0x100000u
		}

		[CLSCompliant(false)]
		public enum CreationDisposition : uint
		{
			CREATE_NEW = 1u,
			CREATE_ALWAYS,
			OPEN_EXISTING,
			OPEN_ALWAYS,
			TRUNCATE_EXSTING
		}

		[Flags]
		[CLSCompliant(false)]
		public enum AllocationType : uint
		{
			MEM_COMMIT = 0x1000u,
			MEM_RESERVE = 0x2000u,
			MEM_RESET = 0x80000u,
			MEM_LARGE_PAGES = 0x20000000u,
			MEM_PHYSICAL = 0x400000u,
			MEM_TOP_DOWN = 0x100000u,
			MEM_WRITE_WATCH = 0x200000u
		}

		[Flags]
		[CLSCompliant(false)]
		public enum MemoryProtection : uint
		{
			PAGE_EXECUTE = 0x10u,
			PAGE_EXECUTE_READ = 0x20u,
			PAGE_EXECUTE_READWRITE = 0x40u,
			PAGE_EXECUTE_WRITECOPY = 0x80u,
			PAGE_NOACCESS = 1u,
			PAGE_READONLY = 2u,
			PAGE_READWRITE = 4u,
			PAGE_WRITECOPY = 8u,
			PAGE_GUARD = 0x100u,
			PAGE_NOCACHE = 0x200u,
			PAGE_WRITECOMBINE = 0x400u
		}

		[Flags]
		[CLSCompliant(false)]
		public enum FreeType : uint
		{
			MEM_DECOMMIT = 0x4000u,
			MEM_RELEASE = 0x8000u
		}

		[Flags]
		[CLSCompliant(false)]
		public enum PartitionAttributes : ulong
		{
			GPT_ATTRIBUTE_PLATFORM_REQUIRED = 1uL,
			GPT_BASIC_DATA_ATTRIBUTE_NO_DRIVE_LETTER = 9223372036854775808uL,
			GPT_BASIC_DATA_ATTRIBUTE_HIDDEN = 0x4000000000000000uL,
			GPT_BASIC_DATA_ATTRIBUTE_SHADOW_COPY = 0x2000000000000000uL,
			GPT_BASIC_DATA_ATTRIBUTE_READ_ONLY = 0x1000000000000000uL
		}

		[CLSCompliant(false)]
		public enum IoctlControlCode : uint
		{
			IoctlMountManagerScrubRegistry = 7192632u,
			IoctlDiskGetDriveLayoutEx = 458832u
		}

		public struct LUID
		{
			[CLSCompliant(false)]
			public uint LowPart;

			public int HighPart;
		}

		public struct LUID_AND_ATTRIBUTES
		{
			public LUID Luid;

			[CLSCompliant(false)]
			public uint Attributes;
		}

		public struct TOKEN_PRIVILEGES
		{
			[CLSCompliant(false)]
			public uint PrivilegeCount;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
			public LUID_AND_ATTRIBUTES[] Privileges;
		}

		public enum PARTITION_STYLE
		{
			MasterBootRecord,
			GuidPartitionTable,
			Raw
		}

		[CLSCompliant(false)]
		public struct DRIVE_LAYOUT_INFORMATION_MBR
		{
			public uint DiskSignature;
		}

		[CLSCompliant(false)]
		public struct DRIVE_LAYOUT_INFORMATION_GPT
		{
			public Guid DiskId;

			public ulong StartingUsableOffset;

			public ulong UsableLength;

			public uint MaxPartitionCount;
		}

		[StructLayout(LayoutKind.Explicit)]
		[CLSCompliant(false)]
		public struct DRIVE_LAYOUT_INFORMATION_UNION
		{
			[FieldOffset(0)]
			public DRIVE_LAYOUT_INFORMATION_MBR Mbr;

			[FieldOffset(0)]
			public DRIVE_LAYOUT_INFORMATION_GPT Gpt;
		}

		[CLSCompliant(false)]
		public struct PARTITION_INFORMATION_MBR
		{
			public byte PartitionType;

			[MarshalAs(UnmanagedType.Bool)]
			public bool BootIndicator;

			[MarshalAs(UnmanagedType.Bool)]
			public bool RecognizedPartition;

			public uint HiddenSectors;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		[CLSCompliant(false)]
		public struct PARTITION_INFORMATION_GPT
		{
			public Guid PartitionType;

			public Guid PartitionId;

			public ulong Attributes;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 36)]
			public string PartitionName;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct PARTITION_INFORMATION_UNION
		{
			[FieldOffset(0)]
			[CLSCompliant(false)]
			public PARTITION_INFORMATION_MBR Mbr;

			[FieldOffset(0)]
			[CLSCompliant(false)]
			public PARTITION_INFORMATION_GPT Gpt;
		}

		public struct PARTITION_INFORMATION_EX
		{
			public PARTITION_STYLE PartitionStyle;

			public long StartingOffset;

			public long PartitionLength;

			public int PartitionNumber;

			public bool RewritePartition;

			public PARTITION_INFORMATION_UNION DriveLayoutInformaiton;
		}

		[CLSCompliant(false)]
		public struct DRIVE_LAYOUT_INFORMATION_EX
		{
			public PARTITION_STYLE PartitionStyle;

			public int PartitionCount;

			[CLSCompliant(false)]
			public DRIVE_LAYOUT_INFORMATION_UNION DriveLayoutInformation;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128, ArraySubType = UnmanagedType.Struct)]
			public PARTITION_INFORMATION_EX[] PartitionEntry;
		}

		public const int S_OK = 0;

		public static int ERROR_SUCCESS = 0;

		public static int ERROR_NO_MORE_ITEMS = 259;

		public const int INFINITE = -1;

		public static string MountManagerPath = "\\\\.\\MountPointManager";

		private const uint MountManagerControlType = 109u;

		private const uint IoctlDiskBase = 7u;

		private const uint MethodBuffered = 0u;

		private const uint FileReadAccess = 1u;

		private const uint FileReadData = 1u;

		private const uint FileWriteAccess = 2u;

		private const uint FileAnyAccess = 0u;

		private const uint DeviceDisk = 7u;

		private const uint DiskBase = 7u;

		private const uint DiskUserStart = 2016u;

		public const int ANYSIZE_ARRAY = 1;

		public const int INVALID_HANDLE_VALUE = -1;

		[CLSCompliant(false)]
		public const uint SE_PRIVILEGE_ENABLED_BY_DEFAULT = 1u;

		[CLSCompliant(false)]
		public const uint SE_PRIVILEGE_ENABLED = 2u;

		[CLSCompliant(false)]
		public const uint SE_PRIVILEGE_REMOVED = 4u;

		[CLSCompliant(false)]
		public const uint SE_PRIVILEGE_USED_FOR_ACCESS = 2147483648u;

		[CLSCompliant(false)]
		public const uint STANDARD_RIGHTS_REQUIRED = 983040u;

		[CLSCompliant(false)]
		public const uint STANDARD_RIGHTS_READ = 131072u;

		[CLSCompliant(false)]
		public const uint TOKEN_ASSIGN_PRIMARY = 1u;

		[CLSCompliant(false)]
		public const uint TOKEN_DUPLICATE = 2u;

		[CLSCompliant(false)]
		public const uint TOKEN_IMPERSONATE = 4u;

		[CLSCompliant(false)]
		public const uint TOKEN_QUERY = 8u;

		[CLSCompliant(false)]
		public const uint TOKEN_QUERY_SOURCE = 16u;

		[CLSCompliant(false)]
		public const uint TOKEN_ADJUST_PRIVILEGES = 32u;

		[CLSCompliant(false)]
		public const uint TOKEN_ADJUST_GROUPS = 64u;

		[CLSCompliant(false)]
		public const uint TOKEN_ADJUST_DEFAULT = 128u;

		[CLSCompliant(false)]
		public const uint TOKEN_ADJUST_SESSIONID = 256u;

		[CLSCompliant(false)]
		public const uint TOKEN_READ = 131080u;

		[CLSCompliant(false)]
		public const uint TOKEN_ALL_ACCESS = 983551u;

		[CLSCompliant(false)]
		public const uint REG_NONE = 0u;

		[CLSCompliant(false)]
		public const uint REG_SZ = 1u;

		[CLSCompliant(false)]
		public const uint REG_EXPAND_SZ = 2u;

		[CLSCompliant(false)]
		public const uint REG_BINARY = 3u;

		[CLSCompliant(false)]
		public const uint REG_DWORD = 4u;

		[CLSCompliant(false)]
		public const uint REG_DWORD_BIG_ENDIAN = 5u;

		[CLSCompliant(false)]
		public const uint REG_LINK = 6u;

		[CLSCompliant(false)]
		public const uint REG_MULTI_SZ = 7u;

		[CLSCompliant(false)]
		public const uint REG_RESOURCE_LIST = 8u;

		[CLSCompliant(false)]
		public const uint REG_FULL_RESOURCE_DESCRIPTOR = 9u;

		[CLSCompliant(false)]
		public const uint REG_RESOURCE_REQUIREMENTS_LIST = 10u;

		[CLSCompliant(false)]
		public const uint REG_QWORD = 11u;

		public static bool FAILED(int hr)
		{
			return hr < 0;
		}

		public static bool SUCCEEDED(int hr)
		{
			return hr >= 0;
		}

		[DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CloseHandle_Native(IntPtr handle);

		public static void CloseHandle(IntPtr handle)
		{
			if (!CloseHandle_Native(handle))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name} failed with error {lastWin32Error}");
			}
		}

		[DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, EntryPoint = "CreateFile", SetLastError = true)]
		private static extern SafeFileHandle CreateFile_Native(string fileName, DesiredAccess desiredAccess, ShareMode shareMode, IntPtr securityAttributes, CreationDisposition creationDisposition, FlagsAndAttributes flagsAndAttributes, IntPtr templateFileHandle);

		[CLSCompliant(false)]
		public static SafeFileHandle CreateFile(string fileName, DesiredAccess desiredAccess, ShareMode shareMode, CreationDisposition creationDisposition, FlagsAndAttributes flagsAndAttributes)
		{
			SafeFileHandle safeFileHandle = CreateFile_Native(fileName, desiredAccess, shareMode, IntPtr.Zero, creationDisposition, flagsAndAttributes, IntPtr.Zero);
			if (safeFileHandle.IsInvalid)
			{
				throw new Win32ExportException(string.Format(arg2: Marshal.GetLastWin32Error(), format: "{0}({1}) failed with error {2}", arg0: MethodBase.GetCurrentMethod().Name, arg1: string.IsNullOrEmpty(fileName) ? "" : fileName));
			}
			return safeFileHandle;
		}

		[DllImport("kernel32.dll", EntryPoint = "ReadFile", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool ReadFile_Native(SafeFileHandle fileHandle, out byte[] buffer, uint bytesToRead, out uint bytesRead, IntPtr overlapped);

		[CLSCompliant(false)]
		public static void ReadFile(SafeFileHandle fileHandle, out byte[] buffer, uint bytesToRead, out uint bytesRead)
		{
			if (!ReadFile_Native(fileHandle, out buffer, bytesToRead, out bytesRead, IntPtr.Zero))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				throw new Win32ExportException($"ReadFile failed with error: {lastWin32Error}");
			}
		}

		[DllImport("kernel32.dll", EntryPoint = "ReadFile", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool ReadFile_Native(SafeFileHandle fileHandle, IntPtr buffer, uint bytesToRead, out uint bytesRead, IntPtr overlapped);

		[CLSCompliant(false)]
		public static void ReadFile(SafeFileHandle fileHandle, IntPtr buffer, uint bytesToRead, out uint bytesRead)
		{
			if (!ReadFile_Native(fileHandle, buffer, bytesToRead, out bytesRead, IntPtr.Zero))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				throw new Win32ExportException($"ReadFile failed with error: {lastWin32Error}");
			}
		}

		[DllImport("kernel32.dll", EntryPoint = "WriteFile", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool WriteFile_Native(SafeFileHandle handle, IntPtr buffer, uint numBytesToWrite, out uint numBytesWritten, IntPtr overlapped);

		[CLSCompliant(false)]
		public static void WriteFile(SafeFileHandle fileHandle, IntPtr buffer, uint bytesToWrite, out uint bytesWritten)
		{
			if (!WriteFile_Native(fileHandle, buffer, bytesToWrite, out bytesWritten, IntPtr.Zero))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				throw new Win32ExportException($"WriteFile failed with error: {lastWin32Error}");
			}
		}

		[DllImport("kernel32.dll", EntryPoint = "SetFilePointerEx", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetFilePointerEx_Native(SafeFileHandle fileHandle, long distanceToMove, out long newFilePointer, MoveMethod moveMethod);

		[CLSCompliant(false)]
		public static void SetFilePointerEx(SafeFileHandle fileHandle, long distanceToMove, out long newFileLocation, MoveMethod moveMethod)
		{
			if (!SetFilePointerEx_Native(fileHandle, distanceToMove, out newFileLocation, moveMethod))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				throw new Win32ExportException($"SetFilePointerEx failed with error: {lastWin32Error}");
			}
		}

		[DllImport("kernel32.dll", EntryPoint = "VirtualAlloc", SetLastError = true)]
		private static extern IntPtr VirtualAlloc_Native(IntPtr lpAddress, UIntPtr sizeInBytes, AllocationType allocationType, MemoryProtection memoryProtection);

		[CLSCompliant(false)]
		public static IntPtr VirtualAlloc(UIntPtr sizeInBytes, AllocationType allocationType, MemoryProtection memoryProtection)
		{
			IntPtr intPtr = VirtualAlloc_Native(IntPtr.Zero, sizeInBytes, allocationType, memoryProtection);
			if (intPtr == IntPtr.Zero)
			{
				throw new Win32ExportException(string.Format(arg1: Marshal.GetLastWin32Error(), format: "{0} failed with error {1}", arg0: MethodBase.GetCurrentMethod().Name));
			}
			return intPtr;
		}

		[DllImport("kernel32.dll", EntryPoint = "VirtualFree", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool VirtualFree_Native(IntPtr address, UIntPtr sizeInBytes, FreeType freeType);

		[CLSCompliant(false)]
		public static void VirtualFree(IntPtr address, FreeType freeType)
		{
			UIntPtr zero = UIntPtr.Zero;
			if (!VirtualFree_Native(address, zero, freeType))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name} failed with error {lastWin32Error}");
			}
		}

		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
		[CLSCompliant(false)]
		public static extern int memcmp(byte[] buffer1, IntPtr buffer2, UIntPtr count);

		[DllImport("kernel32.dll", EntryPoint = "FlushFileBuffers", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool FlushFileBuffers_Native(SafeFileHandle fileHandle);

		[CLSCompliant(false)]
		public static void FlushFileBuffers(SafeFileHandle fileHandle)
		{
			if (!FlushFileBuffers_Native(fileHandle))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name} failed: {lastWin32Error}");
			}
		}

		[DllImport("Kernel32.dll", CharSet = CharSet.Auto, EntryPoint = "DeviceIoControl", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool DeviceIoControl_Native(IntPtr hDevice, uint controlCode, byte[] inBuffer, int inBufferSize, byte[] outBuffer, int outBufferSize, out int bytesReturned, IntPtr lpOverlapped);

		[CLSCompliant(false)]
		public static void DeviceIoControl(IntPtr handle, uint controlCode, byte[] inBuffer, int inBufferSize, byte[] outBuffer, int outBufferSize, out int bytesReturned)
		{
			if (!DeviceIoControl_Native(handle, controlCode, inBuffer, inBufferSize, outBuffer, outBufferSize, out bytesReturned, IntPtr.Zero))
			{
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name}: Control code {controlCode:x} failed with error code {Marshal.GetHRForLastWin32Error():x}.");
			}
		}

		[DllImport("Kernel32.dll", CharSet = CharSet.Auto, EntryPoint = "DeviceIoControl", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool DeviceIoControl_Native(IntPtr hDevice, uint controlCode, IntPtr inBuffer, int inBufferSize, IntPtr outBuffer, int outBufferSize, out int bytesReturned, IntPtr lpOverlapped);

		[CLSCompliant(false)]
		public static void DeviceIoControl(IntPtr handle, uint controlCode, IntPtr inBuffer, int inBufferSize, IntPtr outBuffer, int outBufferSize, out int bytesReturned)
		{
			if (!DeviceIoControl_Native(handle, controlCode, inBuffer, inBufferSize, outBuffer, outBufferSize, out bytesReturned, IntPtr.Zero))
			{
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name}: Control code {controlCode:x} failed with error code {Marshal.GetHRForLastWin32Error():x}.");
			}
		}

		[DllImport("kernel32.dll", EntryPoint = "GetCurrentProcess")]
		private static extern IntPtr GetCurrentProcess_Native();

		public static IntPtr GetCurrentProcess()
		{
			IntPtr currentProcess_Native = GetCurrentProcess_Native();
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (currentProcess_Native.ToInt32() != -1)
			{
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name}: This function failed with error 0x{lastWin32Error:x}.");
			}
			return currentProcess_Native;
		}

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool LookupPrivilegeValue(string systemName, string name, out LUID luid);

		public static LUID LookupPrivilegeValue(string privilegeName)
		{
			LUID luid = default(LUID);
			if (!LookupPrivilegeValue(null, privilegeName, out luid))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name}: This function failed with error 0x{lastWin32Error:x}.");
			}
			return luid;
		}

		[DllImport("advapi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, [MarshalAs(UnmanagedType.Bool)] bool disableAllPrivileges, ref TOKEN_PRIVILEGES newState, uint bufferLengthInBytes, ref TOKEN_PRIVILEGES previousState, out uint returnLengthInBytes);

		public static TOKEN_PRIVILEGES AdjustTokenPrivileges(IntPtr tokenHandle, TOKEN_PRIVILEGES privileges)
		{
			TOKEN_PRIVILEGES previousState = default(TOKEN_PRIVILEGES);
			uint returnLengthInBytes = 0u;
			if (!AdjustTokenPrivileges(tokenHandle, false, ref privileges, (uint)Marshal.SizeOf((object)privileges), ref previousState, out returnLengthInBytes))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name}: This function failed with error 0x{lastWin32Error:x}.");
			}
			return previousState;
		}

		[DllImport("advapi32.dll", EntryPoint = "OpenProcessToken", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool OpenProcessToken_Native(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

		[CLSCompliant(false)]
		public static IntPtr OpenProcessToken(IntPtr processHandle, uint desiredAccess)
		{
			IntPtr tokenHandle;
			if (!OpenProcessToken_Native(processHandle, desiredAccess, out tokenHandle))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name}: This function failed with error 0x{lastWin32Error:x}.");
			}
			return tokenHandle;
		}

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegLoadKey", SetLastError = true)]
		private static extern uint RegLoadKey_Native(SafeRegistryHandle registryKey, string subKeyName, string fileName);

		public static void RegLoadKey(SafeRegistryHandle registryKey, string subKeyName, string fileName)
		{
			uint num = RegLoadKey_Native(registryKey, subKeyName, fileName);
			if (num != ERROR_SUCCESS)
			{
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name}: This function failed with error 0x{num:x}.");
			}
		}

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegUnLoadKey", SetLastError = true)]
		private static extern uint RegUnLoadKey_Native(SafeRegistryHandle registryKey, string subKeyName);

		public static void RegUnloadKey(SafeRegistryHandle registryKey, string subKeyName)
		{
			uint num = RegUnLoadKey_Native(registryKey, subKeyName);
			if (num != ERROR_SUCCESS)
			{
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name}: This function failed with error 0x{num:x}.");
			}
		}

		[DllImport("offreg.dll", CharSet = CharSet.Unicode)]
		private static extern uint OROpenHive(string hivePath, out IntPtr rootKey);

		public static IntPtr OfflineRegistryOpenHive(string hivePath)
		{
			IntPtr rootKey = IntPtr.Zero;
			uint num = OROpenHive(hivePath, out rootKey);
			if (num != ERROR_SUCCESS)
			{
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name}: This function failed with error 0x{num:x} for path: '{hivePath}.");
			}
			return rootKey;
		}

		[DllImport("offreg.dll", CharSet = CharSet.Unicode)]
		private static extern uint ORSaveHive(IntPtr hiveHandle, string hivePath, uint majorOsVersion, uint minorOSVersion);

		public static void OfflineRegistrySaveHive(IntPtr hiveHandle, string hivePath)
		{
			uint num = ORSaveHive(hiveHandle, hivePath, 6u, 1u);
			if (num != ERROR_SUCCESS)
			{
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name}: This function failed with error 0x{num:x} for path: '{hivePath}.");
			}
		}

		[DllImport("offreg.dll", CharSet = CharSet.Unicode)]
		private static extern uint ORCloseHive(IntPtr rootKey);

		public static void OfflineRegistryCloseHive(IntPtr registryKey)
		{
			uint num = ORCloseHive(registryKey);
			if (num != ERROR_SUCCESS)
			{
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name}: This function failed with error 0x{num:x}.");
			}
		}

		[DllImport("offreg.dll", CharSet = CharSet.Unicode)]
		private static extern uint OROpenKey(IntPtr keyHandle, string subKeyName, out IntPtr subKeyHandle);

		public static IntPtr OfflineRegistryOpenSubKey(IntPtr keyHandle, string subKeyName)
		{
			IntPtr subKeyHandle = IntPtr.Zero;
			uint num = OROpenKey(keyHandle, subKeyName, out subKeyHandle);
			if (num != ERROR_SUCCESS)
			{
				if (num == 2)
				{
					return IntPtr.Zero;
				}
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name}: This function failed with error 0x{num:x}.");
			}
			return subKeyHandle;
		}

		[DllImport("offreg.dll", CharSet = CharSet.Unicode)]
		private static extern uint ORCloseKey(IntPtr keyHandle);

		public static void OfflineRegistryCloseSubKey(IntPtr keyHandle)
		{
			uint num = ORCloseKey(keyHandle);
			if (num != ERROR_SUCCESS)
			{
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name}: This function failed with error 0x{num:x}.");
			}
		}

		[DllImport("offreg.dll", CharSet = CharSet.Unicode, EntryPoint = "OREnumKey")]
		private static extern uint OREnumKeySimple(IntPtr rootKey, uint index, StringBuilder subKeyName, ref uint subKeyCharacterCount, IntPtr subKeyClass, IntPtr subKeyClassCharacterCount, IntPtr fileTime);

		[CLSCompliant(false)]
		public static string OfflineRegistryEnumKey(IntPtr registryKey, uint index)
		{
			StringBuilder stringBuilder = new StringBuilder("keyName", (int)ImageConstants.RegistryKeyMaxNameSize);
			uint subKeyCharacterCount = ImageConstants.RegistryKeyMaxNameSize;
			uint num = OREnumKeySimple(registryKey, index, stringBuilder, ref subKeyCharacterCount, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
			if (num != ERROR_SUCCESS)
			{
				if (num == ERROR_NO_MORE_ITEMS)
				{
					return null;
				}
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name}: This function failed with error 0x{num:x}.");
			}
			return stringBuilder.ToString();
		}

		[DllImport("offreg.dll", CharSet = CharSet.Unicode, EntryPoint = "OREnumValue")]
		private static extern uint OREnumValueSimple(IntPtr rootKey, uint index, StringBuilder valueName, ref uint valueCharacterCount, IntPtr valueType, IntPtr data, IntPtr dataSize);

		[CLSCompliant(false)]
		public static string OfflineRegistryEnumValue(IntPtr registryKey, uint index)
		{
			StringBuilder stringBuilder = new StringBuilder("valueName", (int)ImageConstants.RegistryValueMaxNameSize);
			uint valueCharacterCount = ImageConstants.RegistryValueMaxNameSize;
			uint num = OREnumValueSimple(registryKey, index, stringBuilder, ref valueCharacterCount, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
			if (num != ERROR_SUCCESS)
			{
				if (num == ERROR_NO_MORE_ITEMS)
				{
					return null;
				}
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name}: This function failed with error 0x{num:x}.");
			}
			return stringBuilder.ToString();
		}

		[DllImport("offreg.dll", CharSet = CharSet.Unicode, EntryPoint = "ORGetValue")]
		private static extern uint ORGetValueKind(IntPtr keyHandle, IntPtr subKey, string valueName, out uint valueType, IntPtr data, IntPtr dataLength);

		[CLSCompliant(false)]
		public static uint OfflineRegistryGetValueKind(IntPtr keyHandle, string valueName)
		{
			uint valueType = 0u;
			uint num = ORGetValueKind(keyHandle, IntPtr.Zero, valueName, out valueType, IntPtr.Zero, IntPtr.Zero);
			if (num != ERROR_SUCCESS)
			{
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name}: This function failed with error 0x{num:x}.");
			}
			return valueType;
		}

		[DllImport("offreg.dll", CharSet = CharSet.Unicode, EntryPoint = "ORGetValue")]
		private static extern uint ORGetValueSize(IntPtr keyHandle, IntPtr subKey, string valueName, IntPtr valueType, IntPtr data, ref uint dataLength);

		[CLSCompliant(false)]
		public static uint OfflineRegistryGetValueSize(IntPtr keyHandle, string valueName)
		{
			uint dataLength = 0u;
			uint num = ORGetValueSize(keyHandle, IntPtr.Zero, valueName, IntPtr.Zero, IntPtr.Zero, ref dataLength);
			if (num != ERROR_SUCCESS)
			{
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name}: This function failed with error 0x{num:x}.");
			}
			return dataLength;
		}

		[DllImport("offreg.dll", CharSet = CharSet.Unicode)]
		private static extern uint ORGetValue(IntPtr keyHandle, IntPtr subKey, string valueName, out uint valueType, byte[] data, ref uint dataLength);

		public static object OfflineRegistryGetValue(IntPtr keyHandle, string valueName)
		{
			uint dataLength = 0u;
			uint num = ORGetValueSize(keyHandle, IntPtr.Zero, valueName, IntPtr.Zero, IntPtr.Zero, ref dataLength);
			if (num != ERROR_SUCCESS)
			{
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name}: This function failed with error 0x{num:x}.");
			}
			byte[] array = new byte[dataLength];
			uint valueType;
			num = ORGetValue(keyHandle, IntPtr.Zero, valueName, out valueType, array, ref dataLength);
			if (num != ERROR_SUCCESS)
			{
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name}: This function failed with error 0x{num:x}.");
			}
			switch (valueType)
			{
			case 3u:
				return array;
			case 4u:
				return (uint)((array[3] << 24) | (array[2] << 16) | (array[1] << 8) | array[0]);
			case 5u:
				return (uint)((array[0] << 24) | (array[1] << 16) | (array[2] << 8) | array[3]);
			case 2u:
				return Environment.ExpandEnvironmentVariables(Encoding.Unicode.GetString(array)).Split(default(char))[0];
			case 7u:
			{
				List<string> list3 = new List<string>(Encoding.Unicode.GetString(array).Split(default(char)));
				for (int k = 0; k < list3.Count; k++)
				{
					if (string.IsNullOrEmpty(list3[k]))
					{
						list3.RemoveAt(k--);
					}
					else if (string.IsNullOrWhiteSpace(list3[k]))
					{
						list3.RemoveAt(k--);
					}
				}
				return list3.ToArray();
			}
			case 1u:
				return Encoding.Unicode.GetString(array).Split(default(char))[0];
			case 9u:
				return Encoding.Unicode.GetString(array).Split(default(char))[0];
			case 6u:
				return Encoding.Unicode.GetString(array).Split(default(char))[0];
			case 8u:
			{
				List<string> list2 = new List<string>(Encoding.Unicode.GetString(array).Split(default(char)));
				for (int j = 0; j < list2.Count; j++)
				{
					if (string.IsNullOrEmpty(list2[j]))
					{
						list2.RemoveAt(j--);
					}
					else if (string.IsNullOrWhiteSpace(list2[j]))
					{
						list2.RemoveAt(j--);
					}
				}
				return list2.ToArray();
			}
			case 10u:
			{
				List<string> list = new List<string>(Encoding.Unicode.GetString(array).Split(default(char)));
				for (int i = 0; i < list.Count; i++)
				{
					if (string.IsNullOrEmpty(list[i]))
					{
						list.RemoveAt(i--);
					}
					else if (string.IsNullOrWhiteSpace(list[i]))
					{
						list.RemoveAt(i--);
					}
				}
				return list.ToArray();
			}
			default:
				return array;
			}
		}

		[DllImport("offreg.dll", CharSet = CharSet.Unicode)]
		private static extern uint ORSetValue(IntPtr keyHandle, string valueName, uint valueType, byte[] data, uint dataLength);

		[CLSCompliant(false)]
		public static void OfflineRegistrySetValue(IntPtr keyHandle, string valueName, uint valueType, byte[] data)
		{
			uint num = ORSetValue(keyHandle, valueName, valueType, data, (uint)data.Length);
			if (num != ERROR_SUCCESS)
			{
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name}: This function failed with error 0x{num:x}.");
			}
		}

		[DllImport("offreg.dll", CharSet = CharSet.Unicode)]
		private static extern uint ORCreateKey(IntPtr keyHandle, string subKeyName, string className, uint options, IntPtr securityDescriptor, out IntPtr newKeyHandle, out uint creationDisposition);

		[CLSCompliant(false)]
		public static IntPtr OfflineRegistryCreateKey(IntPtr keyHandle, string subKeyName)
		{
			IntPtr newKeyHandle = IntPtr.Zero;
			uint creationDisposition = 0u;
			uint num = ORCreateKey(keyHandle, subKeyName, null, 0u, IntPtr.Zero, out newKeyHandle, out creationDisposition);
			if (num != ERROR_SUCCESS)
			{
				throw new Win32ExportException($"{MethodBase.GetCurrentMethod().Name}: This function failed with error 0x{num:x}.");
			}
			if (creationDisposition != 1)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The key '{subKeyName}' already exists.");
			}
			return newKeyHandle;
		}
	}
}
