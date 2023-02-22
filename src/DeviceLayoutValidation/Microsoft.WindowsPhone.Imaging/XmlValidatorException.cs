using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	[Serializable]
	public class XmlValidatorException : Exception
	{
		public XmlValidatorException()
		{
		}

		public XmlValidatorException(string message)
			: base(message)
		{
		}

		public XmlValidatorException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected XmlValidatorException(SerializationInfo info, StreamingContext context)
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
