using System;
using System.Diagnostics.Eventing;
using System.IO;

namespace FFUComponents
{
	public class FlashingDeviceLogger : IDisposable
	{
		internal DeviceEventProvider m_provider = new DeviceEventProvider(new Guid("3bbd891e-180f-4386-94b5-d71ba7ac25a9"));

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				m_provider.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public bool LogDeviceEvent(byte[] logData, Guid deviceUniqueId, string deviceFriendlyName, out string errInfo)
		{
			BinaryReader binaryReader = new BinaryReader(new MemoryStream(logData));
			binaryReader.ReadByte();
			int num = binaryReader.ReadInt16();
			byte b = binaryReader.ReadByte();
			byte b2 = binaryReader.ReadByte();
			byte b3 = binaryReader.ReadByte();
			byte b4 = binaryReader.ReadByte();
			int num2 = binaryReader.ReadInt16();
			long keywords = binaryReader.ReadInt64();
			EventDescriptor eventDescriptor = new EventDescriptor(num, b, b2, b3, b4, num2, keywords);
			string text = binaryReader.ReadString();
			if (b3 <= 2)
			{
				errInfo = $"{{ 0x{num:x}, 0x{b:x}, 0x{b2:x}, 0x{b3:x}, 0x{b4:x}, 0x{num2:x} }}";
				if (text != "")
				{
					errInfo = errInfo + " : " + text;
				}
			}
			else
			{
				errInfo = "";
			}
			return m_provider.TemplateDeviceEvent(ref eventDescriptor, deviceUniqueId, deviceFriendlyName, text);
		}
	}
}
