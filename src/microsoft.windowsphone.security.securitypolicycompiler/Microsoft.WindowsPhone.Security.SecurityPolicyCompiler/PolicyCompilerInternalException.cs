using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	[Serializable]
	public class PolicyCompilerInternalException : Exception
	{
		public PolicyCompilerInternalException()
		{
		}

		public PolicyCompilerInternalException(string errMsg)
			: base(errMsg)
		{
		}

		protected PolicyCompilerInternalException(SerializationInfo serializationInfo, StreamingContext streamingContext)
			: base(serializationInfo, streamingContext)
		{
		}

		public PolicyCompilerInternalException(string errMsg, Exception originalException)
			: base(errMsg, originalException)
		{
		}
	}
}
