using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsPhone.Imaging.ImageSignerApp
{
	[Serializable]
	public class ImageSignerException : Exception
	{
		public ImageSignerException()
		{
		}

		public ImageSignerException(string message)
			: base(message)
		{
		}

		public ImageSignerException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected ImageSignerException(SerializationInfo info, StreamingContext context)
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
