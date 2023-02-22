using System;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdElementInteger : BcdElement
	{
		[CLSCompliant(false)]
		public ulong Value
		{
			get
			{
				byte[] binaryData = GetBinaryData();
				uint num = 0u;
				for (int i = 4; i < Math.Min(binaryData.Length, 8); i++)
				{
					num |= (uint)(binaryData[i] << (i - 4) * 8);
				}
				uint num2 = 0u;
				for (int j = 0; j < Math.Min(binaryData.Length, 4); j++)
				{
					num2 |= (uint)(binaryData[j] << j * 8);
				}
				return ((ulong)num << 32) | num2;
			}
		}

		public BcdElementInteger(byte[] binaryData, BcdElementDataType dataType)
			: base(dataType)
		{
			SetBinaryData(binaryData);
		}

		[CLSCompliant(false)]
		public override void LogInfo(IULogger logger, int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			base.LogInfo(logger, indentLevel);
			try
			{
				logger.LogInfo(text + "Value: 0x{0:x}", Value);
			}
			catch (ImageStorageException)
			{
				logger.LogInfo(text + "Value: <invalid data>");
			}
		}
	}
}
