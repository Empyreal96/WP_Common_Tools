using System.Runtime.InteropServices;

namespace FFUComponents
{
	[Guid("4EE1152F-246E-4BA3-84D1-2B6C96170E18")]
	[ComVisible(true)]
	public interface IFlashableDevice
	{
		[DispId(1)]
		string GetFriendlyName();

		[DispId(2)]
		string GetUniqueIDStr();

		[DispId(3)]
		string GetSerialNumberStr();

		[DispId(4)]
		bool FlashFFU(string filePath);
	}
}
