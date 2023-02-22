using System;

namespace FFUComponents
{
	public class ConnectEventArgs : EventArgs
	{
		private IFFUDevice device;

		public IFFUDevice Device => device;

		private ConnectEventArgs()
		{
		}

		public ConnectEventArgs(IFFUDevice device)
		{
			this.device = device;
		}
	}
}
