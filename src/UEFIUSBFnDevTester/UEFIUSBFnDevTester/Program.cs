using System;
using System.Windows.Forms;
using DeviceHealth;

namespace UEFIUSBFnDevTester
{
	internal static class Program
	{
		private static Random random = new Random();

		private const int MTU = 16376;

		private const int USB_FS_MAX_PACKET_SIZE = 64;

		private const int USB_HS_MAX_PACKET_SIZE = 512;

		[STAThread]
		private static void Main()
		{
			Log.Name = "UEFI";
			Log.Source = "USBSample";
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}
	}
}
