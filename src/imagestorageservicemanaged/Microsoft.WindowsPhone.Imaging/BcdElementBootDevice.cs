using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdElementBootDevice
	{
		[CLSCompliant(false)]
		public enum DeviceType : uint
		{
			BlockIo = 0u,
			Unused = 1u,
			Partition = 2u,
			SerialPort = 3u,
			Udp = 4u,
			Boot = 5u,
			PartitionEx = 6u,
			Locate = 8u
		}

		[CLSCompliant(false)]
		public static readonly uint BaseBootDeviceSizeInBytes = 72u;

		private IDeviceIdentifier _identifier;

		[CLSCompliant(false)]
		public DeviceType Type { get; set; }

		[CLSCompliant(false)]
		public uint Flags { get; set; }

		internal static uint BaseSize => 16u;

		[CLSCompliant(false)]
		public uint Size { get; set; }

		[CLSCompliant(false)]
		public uint CalculatedSize
		{
			get
			{
				uint num = BaseSize;
				if (Identifier != null)
				{
					num += Identifier.Size;
				}
				return num;
			}
		}

		[CLSCompliant(false)]
		public IDeviceIdentifier Identifier
		{
			get
			{
				return _identifier;
			}
			protected set
			{
				_identifier = value;
			}
		}

		internal long OriginalStreamPosition { get; set; }

		public static BcdElementBootDevice CreateBaseBootDevice()
		{
			return new BcdElementBootDevice
			{
				Type = DeviceType.Boot,
				Size = BaseBootDeviceSizeInBytes,
				Flags = 0u,
				Identifier = new BootIdentifier()
			};
		}

		public static BcdElementBootDevice CreateBaseRamdiskDevice(string filePath, BcdElementBootDevice parentDevice)
		{
			return new BcdElementBootDevice
			{
				Type = DeviceType.BlockIo,
				Size = 33u,
				Flags = 1u,
				Identifier = new RamDiskIdentifier(filePath, parentDevice)
			};
		}

		[CLSCompliant(false)]
		public void ReplaceIdentifier(IDeviceIdentifier identifier)
		{
			Identifier = identifier;
			if (_identifier.GetType() == typeof(PartitionIdentifierEx))
			{
				Type = DeviceType.PartitionEx;
			}
			Size = BaseSize + identifier.Size;
		}

		public void ReadFromStream(BinaryReader reader)
		{
			OriginalStreamPosition = reader.BaseStream.Position;
			Type = (DeviceType)reader.ReadUInt32();
			Flags = reader.ReadUInt32();
			Size = reader.ReadUInt32();
			reader.ReadUInt32();
			switch (Type)
			{
			case DeviceType.BlockIo:
				Identifier = BlockIoIdentifierFactory.CreateFromStream(reader);
				break;
			case DeviceType.Locate:
				Identifier = new LocateIdentifier();
				break;
			case DeviceType.Partition:
				Identifier = new PartitionIdentifier();
				break;
			case DeviceType.PartitionEx:
				Identifier = new PartitionIdentifierEx();
				break;
			case DeviceType.SerialPort:
				Identifier = new SerialPortIdentifier();
				break;
			case DeviceType.Udp:
				Identifier = new UdpIdentifier();
				break;
			default:
				throw new ImageStorageException("Unknown Device Identifier type.");
			case DeviceType.Boot:
				break;
			}
			if (Identifier == null)
			{
				return;
			}
			Identifier.Parent = this;
			Identifier.ReadFromStream(reader);
			if (reader.BaseStream.Position - OriginalStreamPosition >= Size)
			{
				return;
			}
			uint count = Size - (uint)(int)(reader.BaseStream.Position - OriginalStreamPosition);
			byte[] array = reader.ReadBytes((int)count);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != 0)
				{
					throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Non-zero data was found at the end of a boot device object.");
				}
			}
		}

		public void WriteToStream(BinaryWriter writer)
		{
			writer.Write((uint)Type);
			writer.Write(Flags);
			writer.Write(BaseSize + Identifier.Size);
			writer.Write(0u);
			Identifier.WriteToStream(writer);
		}

		[CLSCompliant(false)]
		public void LogInfo(IULogger logger, int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(text + $"Boot Device:  {Type}");
			logger.LogInfo(text + $"Device Flags: 0x{Flags:x}");
			logger.LogInfo(text + $"Device Size:  0x{Size:x}");
			if (Identifier != null)
			{
				logger.LogInfo("");
				Identifier.LogInfo(logger, checked(indentLevel + 2));
			}
		}
	}
}
