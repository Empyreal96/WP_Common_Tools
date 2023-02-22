using System;

namespace Microsoft.WindowsPhone.Imaging.ImageSignerApp
{
	internal class HashedChunkReaderException : Exception
	{
		public HashedChunkReaderException(string message)
			: base(message)
		{
		}
	}
}
