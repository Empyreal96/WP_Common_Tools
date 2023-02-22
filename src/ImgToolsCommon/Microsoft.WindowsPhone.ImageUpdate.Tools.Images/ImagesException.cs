using System;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Images
{
	public class ImagesException : IUException
	{
		public ImagesException(string msg)
			: base(msg)
		{
		}

		public ImagesException(string message, params object[] args)
			: base(message, args)
		{
		}

		public ImagesException(string msg, Exception inner)
			: base(msg, inner)
		{
		}
	}
}
