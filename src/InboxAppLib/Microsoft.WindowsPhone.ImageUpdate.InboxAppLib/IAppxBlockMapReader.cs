using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	[CLSCompliant(false)]
	[Guid("5efec991-bca3-42d1-9ec2-e92d609ec22a")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAppxBlockMapReader
	{
		IAppxBlockMapFile GetFile([In][MarshalAs(UnmanagedType.LPWStr)] string filename);

		IAppxBlockMapFilesEnumerator GetFiles();

		IUri GetHashMethod();

		IStream GetStream();
	}
}
