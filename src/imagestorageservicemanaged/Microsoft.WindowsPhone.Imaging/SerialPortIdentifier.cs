using System;
using System.IO;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class SerialPortIdentifier : BaseIdentifier, IDeviceIdentifier
	{
		[CLSCompliant(false)]
		public uint Size => 0u;

		[CLSCompliant(false)]
		public uint Type { get; set; }

		[CLSCompliant(false)]
		public uint PortNumber { get; set; }

		public byte GenericAddressSpaceId { get; set; }

		public byte GenericAddressWidth { get; set; }

		public byte GenericAddressBitOffset { get; set; }

		public byte GenericAddressAccessSize { get; set; }

		[CLSCompliant(false)]
		public ulong GenericAddressPhysicalAddress { get; set; }

		public void ReadFromStream(BinaryReader reader)
		{
			Type = reader.ReadUInt32();
			byte[] array = reader.ReadBytes(12);
			PortNumber = (uint)((array[0] << 24) | (array[1] << 16) | (array[2] << 8) | array[3]);
			GenericAddressSpaceId = array[0];
			GenericAddressWidth = array[1];
			GenericAddressBitOffset = array[2];
			GenericAddressAccessSize = array[3];
			GenericAddressPhysicalAddress = (ulong)((array[4] << 24) | (array[5] << 16) | (array[6] << 8) | array[7] | (array[8] << 24) | (array[9] << 16) | (array[10] << 8) | array[11]);
		}

		public void WriteToStream(BinaryWriter writer)
		{
		}

		[CLSCompliant(false)]
		public void LogInfo(IULogger logger, int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(text + "Identifier: Serial Port");
		}
	}
}
