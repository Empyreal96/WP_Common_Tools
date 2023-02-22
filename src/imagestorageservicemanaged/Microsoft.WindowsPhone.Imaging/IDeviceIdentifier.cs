using System;
using System.IO;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	[CLSCompliant(false)]
	public interface IDeviceIdentifier
	{
		[CLSCompliant(false)]
		uint Size { get; }

		BcdElementBootDevice Parent { get; set; }

		void ReadFromStream(BinaryReader reader);

		void WriteToStream(BinaryWriter writer);

		void LogInfo(IULogger logger, int indentLevel);
	}
}
