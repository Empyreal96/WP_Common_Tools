using System.Collections;
using System.Runtime.InteropServices;

namespace FFUComponents
{
	[Guid("FAD741FC-3AEA-4FAB-9B8D-CBBF5E265D1B")]
	[ComVisible(true)]
	public interface IFlashingManager
	{
		[DispId(1)]
		bool Start();

		[DispId(2)]
		bool Stop();

		[DispId(3)]
		bool GetFlashableDevices(ref IEnumerator result);

		[DispId(4)]
		IFlashableDevice GetFlashableDevice(string instancePath, bool enableFallback);
	}
}
