using System;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdElementString : BcdElement
	{
		public string Value
		{
			get
			{
				return base.StringData;
			}
			set
			{
				base.StringData = value;
			}
		}

		public BcdElementString(string stringData, BcdElementDataType dataType)
			: base(dataType)
		{
			base.StringData = stringData;
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
