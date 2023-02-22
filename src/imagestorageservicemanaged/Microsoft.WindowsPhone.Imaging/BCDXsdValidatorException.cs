using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	[Serializable]
	public class BCDXsdValidatorException : Exception
	{
		public BCDXsdValidatorException()
		{
		}

		public BCDXsdValidatorException(string message)
			: base(message)
		{
		}

		public BCDXsdValidatorException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected BCDXsdValidatorException(SerializationInfo info, StreamingContext context)
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
