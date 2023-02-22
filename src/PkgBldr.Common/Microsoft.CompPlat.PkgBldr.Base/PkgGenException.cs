using System;
using Microsoft.CompPlat.PkgBldr.Tools;

namespace Microsoft.CompPlat.PkgBldr.Base
{
	public class PkgGenException : IUException
	{
		public PkgGenException(string msg)
			: base(msg)
		{
		}

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
