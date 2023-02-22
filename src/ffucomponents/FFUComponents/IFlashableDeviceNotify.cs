using System.Runtime.InteropServices;

namespace FFUComponents
{
	[Guid("323459AA-B365-44FE-A763-AEACCBCA8880")]
	[ComVisible(true)]
	public interface IFlashableDeviceNotify
	{
		[DispId(1)]
		void Progress(long position, long length);
	}
}
