using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	[Serializable]
	public class FeatureAPIException : Exception
	{
		public FeatureAPIException()
		{
		}

		public FeatureAPIException(string message)
			: base(message)
		{
		}

		public FeatureAPIException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected FeatureAPIException(SerializationInfo info, StreamingContext context)
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
