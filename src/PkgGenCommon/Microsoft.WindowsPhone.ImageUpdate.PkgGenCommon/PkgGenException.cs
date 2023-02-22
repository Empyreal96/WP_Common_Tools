using System;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon
{
	public class PkgGenException : IUException
	{
		public PkgGenException(string msg, params object[] args)
			: base(msg, args)
		{
		}

		public PkgGenException(Exception innerException, string msg, params object[] args)
			: base(innerException, msg, args)
		{
		}
	}
}
