using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class RemovableDiskIdentifier : BaseIdentifier, IBlockIoIdentifier, IDeviceIdentifier
	{
		[CLSCompliant(false)]
		public uint Size => 0u;

		[CLSCompliant(false)]
		public BlockIoType BlockType => BlockIoType.HardDisk;

		[CLSCompliant(false)]
		public uint DriveNumber { get; set; }

		public void ReadFromStream(BinaryReader reader)
		{
			DriveNumber = reader.ReadUInt32();
		}

		public void WriteToStream(BinaryWriter writer)
		{
			throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: This function isn't implemented.");
		}

		[CLSCompliant(false)]
		public void LogInfo(IULogger logger, int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(text + "Identifier: Removable Disk");
			logger.LogInfo(text + "Drive Number:  {0}", DriveNumber);
		}
	}
}
