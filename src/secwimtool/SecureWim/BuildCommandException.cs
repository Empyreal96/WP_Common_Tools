using System;

namespace SecureWim
{
	internal class BuildCommandException : Exception
	{
		public int ErrorCode { get; private set; }

		public BuildCommandException(string message, int errorCode)
			: base(message)
		{
			ErrorCode = errorCode;
		}
	}
}
