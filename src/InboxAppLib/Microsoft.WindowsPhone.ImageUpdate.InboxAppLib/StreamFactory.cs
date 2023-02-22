using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	[CLSCompliant(false)]
	public class StreamFactory
	{
		[Flags]
		private enum STGM
		{
			STGM_READ = 0,
			STGM_WRITE = 1,
			STGM_READWRITE = 2
		}

		public static IStream CreateFileStream(string fileName)
		{
			return SHCreateStreamOnFileEx(fileName, STGM.STGM_READ, 1u, false, null);
		}

		[DllImport("shlwapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
		private static extern IStream SHCreateStreamOnFileEx([In] string fileName, [In] STGM mode, [In] uint attributes, [In] bool create, [In] IStream template);
	}
}
