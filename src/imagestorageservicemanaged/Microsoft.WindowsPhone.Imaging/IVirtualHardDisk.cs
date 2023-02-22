using System;

namespace Microsoft.WindowsPhone.Imaging
{
	[CLSCompliant(false)]
	public interface IVirtualHardDisk : IDisposable
	{
		uint SectorSize { get; }

		ulong SectorCount { get; }

		void FlushFile();

		[CLSCompliant(false)]
		void ReadSector(ulong sector, byte[] buffer, uint offset);

		[CLSCompliant(false)]
		void WriteSector(ulong sector, byte[] buffer, uint offset);
	}
}
