using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	[Serializable]
	public class ImagingException : Exception
	{
		public ImagingException()
		{
		}

		public ImagingException(string message)
			: base(message)
		{
		}

		public ImagingException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected ImagingException(SerializationInfo info, StreamingContext context)
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
