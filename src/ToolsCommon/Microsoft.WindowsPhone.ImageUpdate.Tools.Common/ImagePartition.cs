using System.IO;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public class ImagePartition
	{
		private FileInfo[] _files;

		public string PhysicalDeviceId { get; protected set; }

		public string Name { get; protected set; }

		public string Root { get; protected set; }

		public DriveInfo MountedDriveInfo { get; protected set; }

		public FileInfo[] Files
		{
			get
			{
				if (_files == null && !string.IsNullOrEmpty(Root))
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(Root);
					_files = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);
				}
				return _files;
			}
		}

		protected ImagePartition()
		{
		}

		public ImagePartition(string name, string root)
		{
			Name = name;
			Root = root;
		}
	}
}
