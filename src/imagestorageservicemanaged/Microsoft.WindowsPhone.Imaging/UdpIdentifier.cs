using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class UdpIdentifier : BaseIdentifier, IDeviceIdentifier
	{
		private List<byte> _hardwareAddress;

		[CLSCompliant(false)]
		public uint Size => 0u;

		[CLSCompliant(false)]
		public uint HardwareType { get; set; }

		public List<byte> HardwareAddress => _hardwareAddress;

		public void ReadFromStream(BinaryReader reader)
		{
			HardwareType = reader.ReadUInt32();
			_hardwareAddress = new List<byte>(reader.ReadBytes(16));
		}

		public void WriteToStream(BinaryWriter writer)
		{
			throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: This function isn't implemented.");
		}

		[CLSCompliant(false)]
		public void LogInfo(IULogger logger, int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(text + "Identifier: UDP");
		}
	}
}
