using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class CdRomIdentifier : BaseIdentifier, IBlockIoIdentifier, IDeviceIdentifier
	{
		[CLSCompliant(false)]
		public uint Size => 0u;

		[CLSCompliant(false)]
		public BlockIoType BlockType => BlockIoType.HardDisk;

		[CLSCompliant(false)]
		public uint CdRomNumber { get; set; }

		public void ReadFromStream(BinaryReader reader)
		{
			CdRomNumber = reader.ReadUInt32();
		}

		public void WriteToStream(BinaryWriter writer)
		{
			throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: This function isn't implemented.");
		}

		[CLSCompliant(false)]
		public void LogInfo(IULogger logger, int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(text + "Identifier: CdRom");
			logger.LogInfo(text + "CdRom Number:  {0}", CdRomNumber);
		}
	}
}
