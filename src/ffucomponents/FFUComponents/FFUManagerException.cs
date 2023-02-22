using System;
using System.Runtime.Serialization;

namespace FFUComponents
{
	[Serializable]
	public class FFUManagerException : Exception
	{
		public FFUManagerException()
		{
		}

		public FFUManagerException(string message)
			: base(message)
		{
		}

		public FFUManagerException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected FFUManagerException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
