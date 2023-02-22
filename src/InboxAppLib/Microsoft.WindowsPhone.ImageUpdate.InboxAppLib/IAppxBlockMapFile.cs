using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	[CLSCompliant(false)]
	[Guid("277672ac-4f63-42c1-8abc-beae3600eb59")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAppxBlockMapFile
	{
		IAppxBlockMapBlocksEnumerator GetBlocks();

		uint GetLocalFileHeaderSize();

		[return: MarshalAs(UnmanagedType.LPWStr)]
		string GetName();

		ulong GetUncompressedSize();

		bool ValidateFileHash([In] IStream fileStream);
	}
}
