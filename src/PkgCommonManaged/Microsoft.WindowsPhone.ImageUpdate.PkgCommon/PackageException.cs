using System;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public class PackageException : IUException
	{
		public PackageException(string msg)
			: base(msg)
		{
		}

		public PackageException(string message, params object[] args)
			: base(message, args)
		{
		}

		public PackageException(Exception inner, string msg)
			: base(inner, msg)
		{
		}

		public PackageException(Exception innerException, string message, params object[] args)
			: base(innerException, message, args)
		{
		}
	}
}
