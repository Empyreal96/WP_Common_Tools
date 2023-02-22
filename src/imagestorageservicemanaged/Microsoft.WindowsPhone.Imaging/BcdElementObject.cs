using System;
using System.Reflection;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdElementObject : BcdElement
	{
		public Guid ElementObject
		{
			get
			{
				Guid result = Guid.Empty;
				if (!Guid.TryParse(base.StringData, out result))
				{
					throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The string data isn't a valid Guid.");
				}
				return result;
			}
			set
			{
				base.StringData = value.ToString();
			}
		}

		public BcdElementObject(string value, BcdElementDataType dataType)
			: base(dataType)
		{
			base.StringData = value;
		}

		[CLSCompliant(false)]
		public override void LogInfo(IULogger logger, int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			base.LogInfo(logger, indentLevel);
			logger.LogInfo(text + "Object ID: {{{0}}}", ElementObject);
		}
	}
}
