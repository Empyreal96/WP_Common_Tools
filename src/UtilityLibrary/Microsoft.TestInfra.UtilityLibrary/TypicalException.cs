using System;

namespace Microsoft.TestInfra.UtilityLibrary
{
	public class TypicalException<T> : Exception
	{
		public TypicalException()
		{
		}

		public TypicalException(string message)
			: base(message)
		{
		}

		public TypicalException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
