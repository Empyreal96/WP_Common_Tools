using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	[Serializable]
	public class ImageCommonException : Exception
	{
		public ImageCommonException()
		{
		}

		public ImageCommonException(string message)
			: base(message)
		{
		}

		public ImageCommonException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected ImageCommonException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public override string ToString()
		{
			string text = Message;
			if (base.InnerException != null)
			{
				text += base.InnerException.ToString();
			}
			return text;
		}
	}
}
