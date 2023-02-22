using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdElementIntegerList : BcdElement
	{
		[CLSCompliant(false)]
		public List<ulong> Value
		{
			get
			{
				byte[] binaryData = GetBinaryData();
				int num = binaryData.Length / 8;
				ulong[] array = new ulong[num];
				for (int i = 0; i < num; i++)
				{
					int num2 = i * 8;
					uint num3 = (uint)((binaryData[num2 + 7] << 24) | (binaryData[num2 + 6] << 16) | (binaryData[num2 + 5] << 8) | binaryData[num2 + 4]);
					uint num4 = (uint)((binaryData[num2 + 3] << 24) | (binaryData[num2 + 2] << 16) | (binaryData[num2 + 1] << 8) | binaryData[num2]);
					array[i] = ((ulong)num3 << 32) | num4;
				}
				return new List<ulong>(array);
			}
		}

		public BcdElementIntegerList(byte[] binaryData, BcdElementDataType dataType)
			: base(dataType)
		{
			SetBinaryData(binaryData);
		}

		[CLSCompliant(false)]
		public override void LogInfo(IULogger logger, int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			base.LogInfo(logger, indentLevel);
			foreach (ulong item in Value)
			{
				logger.LogInfo(text + "Value: 0x{0:x16}", item);
			}
		}
	}
}
