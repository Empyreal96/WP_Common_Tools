using System;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdElementBoolean : BcdElement
	{
		public bool Value
		{
			get
			{
				return GetBinaryData()[0] != 0;
			}
			set
			{
				if (value)
				{
					GetBinaryData()[0] = 1;
				}
				else
				{
					GetBinaryData()[0] = 0;
				}
			}
		}

		public BcdElementBoolean(byte[] binaryData, BcdElementDataType dataType)
			: base(dataType)
		{
			SetBinaryData(binaryData);
		}

		[CLSCompliant(false)]
		public override void LogInfo(IULogger logger, int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			base.LogInfo(logger, indentLevel);
			logger.LogInfo(text + "Value: {0}", Value);
		}
	}
}
