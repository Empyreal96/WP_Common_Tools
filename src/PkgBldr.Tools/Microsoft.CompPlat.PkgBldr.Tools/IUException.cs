using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public class IUException : Exception
	{
		public string MessageTrace
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (Exception ex = this; ex != null; ex = ex.InnerException)
				{
					if (!string.IsNullOrEmpty(ex.Message))
					{
						stringBuilder.AppendLine(ex.Message);
					}
				}
				return stringBuilder.ToString();
			}
		}

		public IUException()
		{
		}

		public IUException(string message)
			: base(message)
		{
		}

		public IUException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public IUException(string message, params object[] args)
			: this(string.Format(CultureInfo.InvariantCulture, message, args))
		{
		}

		protected IUException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public IUException(Exception innerException, string message)
			: base(message, innerException)
		{
		}

		public IUException(Exception innerException, string message, params object[] args)
			: this(innerException, string.Format(CultureInfo.InvariantCulture, message, args))
		{
		}
	}
}
