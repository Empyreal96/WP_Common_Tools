using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.Imaging.WimInterop
{
	public class ProcessFileEventArgs : EventArgs
	{
		private string _filePath;

		private bool _abort;

		private IntPtr _skipFileFlag;

		public string FilePath
		{
			get
			{
				string result = "";
				if (_filePath != null)
				{
					result = _filePath;
				}
				return result;
			}
		}

		public bool Abort
		{
			get
			{
				return _abort;
			}
			set
			{
				_abort = value;
			}
		}

		public ProcessFileEventArgs(string file, IntPtr skipFileFlag)
		{
			_filePath = file;
			_skipFileFlag = skipFileFlag;
		}

		public void SkipFile()
		{
			byte[] array = new byte[1];
			Marshal.Copy(length: array.Length, source: array, startIndex: 0, destination: _skipFileFlag);
		}
	}
}
