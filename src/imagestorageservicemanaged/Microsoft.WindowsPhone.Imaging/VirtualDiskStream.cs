using System;
using System.IO;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class VirtualDiskStream : Stream
	{
		private uint _sectorBufferIndex = uint.MaxValue;

		private byte[] _sectorBuffer;

		private long _position;

		private DynamicHardDisk VirtualDisk { get; set; }

		public override bool CanRead => true;

		public override bool CanWrite => true;

		public override bool CanSeek => true;

		public override bool CanTimeout => false;

		public override long Length => (long)(VirtualDisk.SectorCount * VirtualDisk.SectorSize);

		public override long Position
		{
			get
			{
				return _position;
			}
			set
			{
				if (value > Length)
				{
					throw new ImageStorageException("The given position is beyond the end of the image payload.");
				}
				_position = value;
			}
		}

		public VirtualDiskStream(DynamicHardDisk virtualDisk)
		{
			VirtualDisk = virtualDisk;
			_sectorBuffer = new byte[virtualDisk.SectorSize];
		}

		public override void Flush()
		{
			VirtualDisk.FlushFile();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			if (offset > Length)
			{
				throw new ImageStorageException("The  offset is beyond the end of the image.");
			}
			switch (origin)
			{
			case SeekOrigin.Begin:
				_position = offset;
				return _position;
			case SeekOrigin.Current:
				if (offset == 0L)
				{
					return _position;
				}
				if (offset < 0)
				{
					throw new ImageStorageException("Negative offsets are not implemented.");
				}
				if (_position >= Length)
				{
					throw new ImageStorageException("The offset is beyond the end of the image.");
				}
				if (Length - _position < offset)
				{
					throw new ImageStorageException("The offset is beyond the end of the image.");
				}
				_position = offset;
				return _position;
			case SeekOrigin.End:
				if (offset > 0)
				{
					throw new ImageStorageException("The offset is beyond the end of the image.");
				}
				if (Length + offset < 0)
				{
					throw new ImageStorageException("The offset is invalid.");
				}
				_position = Length + offset;
				return _position;
			default:
				throw new ImageStorageException("The origin parameter is invalid.");
			}
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int num = 0;
			while (count > 0)
			{
				uint num2 = (uint)(Position / (long)VirtualDisk.SectorSize);
				uint num3 = (uint)(Position % (long)VirtualDisk.SectorSize);
				int num4 = Math.Min(count, (int)(VirtualDisk.SectorSize - num3));
				if (_sectorBufferIndex != num2)
				{
					if (VirtualDisk.SectorIsAllocated(num2))
					{
						if (num4 == VirtualDisk.SectorSize)
						{
							VirtualDisk.ReadSector(num2, buffer, (uint)offset);
						}
						else
						{
							VirtualDisk.ReadSector(num2, _sectorBuffer, 0u);
							_sectorBufferIndex = num2;
							for (int i = 0; i < num4; i++)
							{
								buffer[offset + i] = _sectorBuffer[num3 + i];
							}
						}
					}
					else
					{
						for (int j = 0; j < num4; j++)
						{
							buffer[offset + j] = 0;
						}
					}
				}
				else
				{
					for (int k = 0; k < num4; k++)
					{
						buffer[offset + k] = _sectorBuffer[num3 + k];
					}
				}
				offset += num4;
				count -= num4;
				num += num4;
				Position += num4;
			}
			return num;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (offset + Position > Length)
			{
				throw new EndOfStreamException("Cannot write past the end of the stream.");
			}
			while (count > 0)
			{
				uint num = (uint)(Position / (long)VirtualDisk.SectorSize);
				uint num2 = (uint)(Position % (long)VirtualDisk.SectorSize);
				int num3 = Math.Min(count, (int)(VirtualDisk.SectorSize - num2));
				if (!VirtualDisk.SectorIsAllocated(num))
				{
					throw new ImageStorageException("Writing to an unallocated virtual disk location is not supported.");
				}
				if (num2 == 0 && num3 == VirtualDisk.SectorSize)
				{
					VirtualDisk.WriteSector(num, buffer, (uint)offset);
				}
				else
				{
					if (_sectorBufferIndex != num)
					{
						VirtualDisk.ReadSector(num, _sectorBuffer, 0u);
						_sectorBufferIndex = num;
					}
					for (int i = 0; i < num3; i++)
					{
						_sectorBuffer[num2 + i] = buffer[offset + i];
					}
					VirtualDisk.WriteSector(num, _sectorBuffer, 0u);
				}
				offset += num3;
				count -= num3;
				Position += num3;
			}
		}
	}
}
