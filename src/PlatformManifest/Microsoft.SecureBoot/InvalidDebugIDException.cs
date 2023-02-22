using System;

namespace Microsoft.SecureBoot
{
	public class InvalidDebugIDException : Exception
	{
		public InvalidDebugIDException(string msg)
			: base(msg)
		{
		}
	}
}
