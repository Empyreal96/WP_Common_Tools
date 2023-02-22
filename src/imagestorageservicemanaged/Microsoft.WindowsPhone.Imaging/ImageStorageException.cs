using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	[Serializable]
	public class ImageStorageException : Exception
	{
		public ImageStorageException()
		{
		}

		public ImageStorageException(string message)
			: base(message)
		{
		}

		public ImageStorageException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected ImageStorageException(SerializationInfo info, StreamingContext context)
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
