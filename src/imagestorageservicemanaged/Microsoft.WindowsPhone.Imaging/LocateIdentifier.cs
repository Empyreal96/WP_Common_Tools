using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class LocateIdentifier : BaseIdentifier, IDeviceIdentifier
	{
		[CLSCompliant(false)]
		public enum LocateType : uint
		{
			BootElement,
			String
		}

		[CLSCompliant(false)]
		public uint Size => 0u;

		[CLSCompliant(false)]
		public LocateType Type { get; set; }

		[CLSCompliant(false)]
		public uint ElementType { get; set; }

		[CLSCompliant(false)]
		public uint ParentOffset { get; set; }

		public string Path { get; set; }

		public void ReadFromStream(BinaryReader reader)
		{
			Type = (LocateType)reader.ReadUInt32();
			ElementType = reader.ReadUInt32();
			ParentOffset = reader.ReadUInt32();
			Path = reader.ReadString();
			if (Type == LocateType.BootElement)
			{
				throw new ImageStorageException("Not supported.");
			}
		}

		public void WriteToStream(BinaryWriter writer)
		{
			throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: This function isn't implemented.");
		}

		[CLSCompliant(false)]
		public void LogInfo(IULogger logger, int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(text + "Identifier: Locate");
		}
	}
}
