using System;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.MCSF.Offline
{
	[Serializable]
	public class MCSFOfflineException : IUException
	{
		public MCSFOfflineException()
		{
		}

		public MCSFOfflineException(string message)
			: base(message)
		{
		}

		public MCSFOfflineException(Exception innerException, string message)
			: base(innerException, message)
		{
		}
	}
}
