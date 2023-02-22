using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	[Serializable]
	public class Win32ExportException : Exception
	{
		public Win32ExportException()
		{
		}

		public Win32ExportException(string message)
			: base(message)
		{
		}

		public Win32ExportException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected Win32ExportException(SerializationInfo info, StreamingContext context)
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
