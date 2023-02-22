using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public class NativeSecurityMethods
	{
		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[CLSCompliant(false)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ConvertSecurityDescriptorToStringSecurityDescriptor([In] byte[] pBinarySecurityDescriptor, int RequestedStringSDRevision, SecurityInformationFlags SecurityInformation, out IntPtr StringSecurityDescriptor, out int StringSecurityDescriptorLen);

		[DllImport("AdvAPI32.DLL", CharSet = CharSet.Auto, SetLastError = true)]
		[CLSCompliant(false)]
		public static extern bool GetFileSecurity(string lpFileName, SecurityInformationFlags RequestedInformation, IntPtr pSecurityDescriptor, int nLength, ref int lpnLengthNeeded);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int IU_AdjustProcessPrivilege(string strPrivilegeName, bool fEnabled);
	}
}
