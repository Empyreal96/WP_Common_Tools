using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class VirtualDiskIdentifier : BaseIdentifier, IBlockIoIdentifier, IDeviceIdentifier
	{
		[CLSCompliant(false)]
		public BlockIoType BlockType => BlockIoType.VirtualHardDisk;

		[CLSCompliant(false)]
		public uint Size => InternalIdentifer.Size + FileDevice.Size;

		public HardDiskIdentifier InternalIdentifer { get; set; }

		public BcdElementBootDevice FileDevice { get; set; }

		public void ReadFromStream(BinaryReader reader)
		{
			InternalIdentifer = new HardDiskIdentifier();
			InternalIdentifer.ReadFromStream(reader);
			InternalIdentifer.AsVirtualDisk = true;
			FileDevice = new BcdElementBootDevice();
			FileDevice.ReadFromStream(reader);
		}

		public void WriteToStream(BinaryWriter writer)
		{
			throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: This function isn't implemented.");
		}

		[CLSCompliant(false)]
		public void LogInfo(IULogger logger, int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(text + "Identifier: Virtual Hard Disk");
			checked
			{
				InternalIdentifer.LogInfo(logger, indentLevel + 2);
				logger.LogInfo("");
				FileDevice.LogInfo(logger, indentLevel + 2);
			}
		}
	}
}
