using System;

namespace Microsoft.WindowsPhone.Imaging
{
	[CLSCompliant(false)]
	public interface IBlockIoIdentifier : IDeviceIdentifier
	{
		[CLSCompliant(false)]
		BlockIoType BlockType { get; }
	}
}
