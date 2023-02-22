using System.Runtime.InteropServices;

namespace FFUComponents
{
	[ComVisible(false)]
	public delegate void ProgressHandler(long position, long length);
}
