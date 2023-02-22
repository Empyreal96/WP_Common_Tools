using System;

namespace Microsoft.WindowsPhone.Imaging.WimInterop
{
	public class DefaultImageEventArgs : EventArgs
	{
		private IntPtr _wParam;

		private IntPtr _lParam;

		private IntPtr _userData;

		public IntPtr WideParameter => _wParam;

		public IntPtr LeftParameter => _lParam;

		public IntPtr UserData => _userData;

		public DefaultImageEventArgs(IntPtr wideParameter, IntPtr leftParameter, IntPtr userData)
		{
			_wParam = wideParameter;
			_lParam = leftParameter;
			_userData = userData;
		}
	}
}
