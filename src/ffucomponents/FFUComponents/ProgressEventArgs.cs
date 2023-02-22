using System;

namespace FFUComponents
{
	public class ProgressEventArgs : EventArgs
	{
		public IFFUDevice Device { get; private set; }

		public long Position { get; private set; }

		public long Length { get; private set; }

		public ProgressEventArgs(IFFUDevice device, long pos, long len)
		{
			Device = device;
			Position = pos;
			Length = len;
		}
	}
}
