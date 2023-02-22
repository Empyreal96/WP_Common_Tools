using System;
using System.IO;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BootIdentifier : BaseIdentifier, IDeviceIdentifier
	{
		[CLSCompliant(false)]
		public uint Size => BcdElementBootDevice.BaseBootDeviceSizeInBytes - BcdElementBootDevice.BaseSize;

		public void ReadFromStream(BinaryReader reader)
		{
			reader.ReadBytes((int)Size);
		}

		public void WriteToStream(BinaryWriter writer)
		{
			byte[] array = new byte[Size];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = 0;
			}
			writer.Write(array, 0, array.Length);
		}

		[CLSCompliant(false)]
		public void LogInfo(IULogger logger, int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(text + "Boot Identifier");
		}
	}
}
