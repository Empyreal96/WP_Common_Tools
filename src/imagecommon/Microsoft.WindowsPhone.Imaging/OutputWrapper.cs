using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.WindowsPhone.Imaging
{
	public class OutputWrapper : IPayloadWrapper
	{
		private string path;

		private FileStream fileStream;

		private Queue<IAsyncResult> writes;

		public OutputWrapper(string path)
		{
			this.path = path;
			writes = new Queue<IAsyncResult>();
		}

		public void InitializeWrapper(long payloadSize)
		{
			fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 1048576, true);
			fileStream.SetLength(payloadSize);
		}

		public void ResetPosition()
		{
			fileStream.Seek(0L, SeekOrigin.Begin);
		}

		public void Write(byte[] data)
		{
			while (writes.Count > 0 && writes.Peek().IsCompleted)
			{
				fileStream.EndWrite(writes.Dequeue());
			}
			IAsyncResult item = fileStream.BeginWrite(data, 0, data.Length, null, null);
			writes.Enqueue(item);
		}

		public void FinalizeWrapper()
		{
			while (writes.Count > 0)
			{
				fileStream.EndWrite(writes.Dequeue());
			}
			if (fileStream != null)
			{
				fileStream.Close();
				fileStream = null;
			}
		}
	}
}
