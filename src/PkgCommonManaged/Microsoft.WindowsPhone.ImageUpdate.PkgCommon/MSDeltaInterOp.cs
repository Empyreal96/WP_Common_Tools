using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	internal static class MSDeltaInterOp
	{
		[DllImport("msdelta", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CreateDeltaW([MarshalAs(UnmanagedType.I8)] DELTA_FILE_TYPE FileTypeSet, [MarshalAs(UnmanagedType.I8)] DELTA_FLAG_TYPE SetFlags, [MarshalAs(UnmanagedType.I8)] DELTA_FLAG_TYPE ResetFlags, string sourcePath, string targetPath, string sourceOption, string targetOption, DELTA_INPUT GlobalOptions, ref ulong lpTargetFileTime, uint HashAlgId, string DeltaName);
	}
}
