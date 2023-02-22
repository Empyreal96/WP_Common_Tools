using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdElementDevice : BcdElement
	{
		public Guid AdditionalFlags { get; set; }

		public BcdElementBootDevice BootDevice { get; set; }

		[CLSCompliant(false)]
		public uint BinarySize => 16 + BootDevice.Size;

		public static BcdElementDevice CreateBaseBootDevice()
		{
			return new BcdElementDevice(BcdElementBootDevice.CreateBaseBootDevice(), new BcdElementDataType(285212673u));
		}

		public static BcdElementDevice CreateBaseRamdiskDevice(string filePath, BcdElementBootDevice parentDevice)
		{
			return new BcdElementDevice(BcdElementBootDevice.CreateBaseRamdiskDevice(filePath, parentDevice), BcdElementDataTypes.OsLoaderDevice, BcdObjects.WindowsSetupRamdiskOptions);
		}

		public BcdElementDevice(byte[] binaryData, BcdElementDataType dataType)
			: base(dataType)
		{
			SetBinaryData(binaryData);
			MemoryStream stream = new MemoryStream(binaryData);
			ReadFromStream(stream);
			stream = null;
		}

		public BcdElementDevice(BcdElementBootDevice bootDevice, BcdElementDataType dataType)
			: base(dataType)
		{
			AdditionalFlags = Guid.Empty;
			BootDevice = bootDevice;
			byte[] array = new byte[BinarySize];
			using (MemoryStream stream = new MemoryStream(array))
			{
				WriteToStream(stream);
				SetBinaryData(array);
			}
		}

		public BcdElementDevice(BcdElementBootDevice bootDevice, BcdElementDataType dataType, Guid Flags)
			: base(dataType)
		{
			AdditionalFlags = Flags;
			BootDevice = bootDevice;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				WriteToStream(memoryStream);
				SetBinaryData(memoryStream.ToArray());
			}
		}

		public void ReadFromStream(Stream stream)
		{
			BinaryReader binaryReader = new BinaryReader(stream);
			byte[] b = binaryReader.ReadBytes(16);
			AdditionalFlags = new Guid(b);
			BootDevice = new BcdElementBootDevice();
			BootDevice.ReadFromStream(binaryReader);
			binaryReader = null;
		}

		public void WriteToStream(Stream stream)
		{
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			byte[] buffer = AdditionalFlags.ToByteArray();
			binaryWriter.Write(buffer);
			BootDevice.WriteToStream(binaryWriter);
			binaryWriter = null;
		}

		[CLSCompliant(false)]
		public override void LogInfo(IULogger logger, int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			base.LogInfo(logger, indentLevel);
			logger.LogInfo(text + "Additional Flags:   {{{0}}}", AdditionalFlags);
			logger.LogInfo("");
			BootDevice.LogInfo(logger, checked(indentLevel + 2));
		}

		[CLSCompliant(false)]
		public void ReplaceRamDiskDeviceIdentifier(IDeviceIdentifier identifier)
		{
			RamDiskIdentifier ramDiskIdentifier = (RamDiskIdentifier)BootDevice.Identifier;
			if (ramDiskIdentifier == null)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The device's identifier is not a ramdisk.");
			}
			ramDiskIdentifier.ReplaceParentDeviceIdentifier(identifier);
			BootDevice.ReplaceIdentifier(ramDiskIdentifier);
			byte[] array = new byte[BinarySize];
			using (MemoryStream memoryStream = new MemoryStream(array))
			{
				WriteToStream(memoryStream);
				memoryStream.Flush();
				memoryStream.Close();
			}
			SetBinaryData(array);
		}

		[CLSCompliant(false)]
		public void ReplaceBootDeviceIdentifier(IDeviceIdentifier identifier)
		{
			BootDevice.ReplaceIdentifier(identifier);
			byte[] array = new byte[BinarySize];
			MemoryStream memoryStream = new MemoryStream(array);
			WriteToStream(memoryStream);
			SetBinaryData(array);
			memoryStream.Close();
			memoryStream = null;
		}
	}
}
