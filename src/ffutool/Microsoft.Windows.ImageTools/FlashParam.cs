using System.Threading;
using FFUComponents;

namespace Microsoft.Windows.ImageTools
{
	internal class FlashParam
	{
		public IFFUDevice Device;

		public string LogFolderPath;

		public string FfuFilePath;

		public string WimPath;

		public AutoResetEvent WaitHandle;

		public int Result;

		public bool FastFlash;
	}
}
