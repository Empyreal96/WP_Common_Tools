using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class FileIdentifier : BaseIdentifier, IBlockIoIdentifier, IDeviceIdentifier
	{
		[CLSCompliant(false)]
		public uint Size => Length;

		[CLSCompliant(false)]
		public BlockIoType BlockType => BlockIoType.File;

		[CLSCompliant(false)]
		public uint Version { get; set; }

		[CLSCompliant(false)]
		public uint Length
		{
			get
			{
				uint num = 12u;
				num += 2;
				if (Path != null)
				{
					num += (uint)(2 * Path.Length);
				}
				if (ParentDevice != null)
				{
					num += ParentDevice.CalculatedSize;
				}
				return num;
			}
		}

		[CLSCompliant(false)]
		public uint Type { get; set; }

		public string Path { get; private set; }

		public BcdElementBootDevice ParentDevice { get; set; }

		public FileIdentifier(string filePath, BcdElementBootDevice parentDevice)
		{
			Path = filePath;
			ParentDevice = parentDevice;
		}

		[CLSCompliant(false)]
		public void ReplaceParentDeviceIdentifier(IDeviceIdentifier identifier)
		{
			ParentDevice.ReplaceIdentifier(identifier);
		}

		public void ReadFromStream(BinaryReader reader)
		{
			long position = reader.BaseStream.Position;
			Version = reader.ReadUInt32();
			uint num = reader.ReadUInt32();
			Type = reader.ReadUInt32();
			ParentDevice.ReadFromStream(reader);
			if (reader.BaseStream.Position - position >= Length)
			{
				throw new ImageStorageException(string.Format("{0}: The FileIdentifier appears to be invalid at position: 0x{1:x}  AND  {2] {3} {4}", MethodBase.GetCurrentMethod().Name, position, Version, Length, Type));
			}
			byte[] bytes = reader.ReadBytes((int)num - (int)(reader.BaseStream.Position - position));
			Path = Encoding.Unicode.GetString(bytes);
		}

		public void WriteToStream(BinaryWriter writer)
		{
			writer.Write(1u);
			writer.Write(Length);
			writer.Write(5u);
			ParentDevice.WriteToStream(writer);
			string path = Path;
			foreach (char c in path)
			{
				writer.Write((short)c);
			}
			writer.Write((short)0);
		}

		[CLSCompliant(false)]
		public void LogInfo(IULogger logger, int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(text + "Block IO Type: File");
			logger.LogInfo(text + "Version:       {0}", Version);
			logger.LogInfo(text + "Length:        {0}", Length);
			logger.LogInfo(text + "Type:          {0}", Type);
			logger.LogInfo(text + "Path:          {0}", Path);
			logger.LogInfo("");
			ParentDevice.LogInfo(logger, checked(indentLevel + 2));
		}
	}
}
