using System;
using System.Threading;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public class MobileCoreWIM : MobileCoreImage
	{
		private bool _commitChanges;

		private string _tmpDir;

		private string _mountPoint;

		private static readonly object _lockObj = new object();

		private const int SLEEP_1000 = 1000;

		private const int MAX_RETRY = 3;

		internal MobileCoreWIM(string path)
			: base(path)
		{
		}

		private void MountWithRetry(bool readOnly)
		{
			if (base.IsMounted)
			{
				return;
			}
			int num = 0;
			int num2 = 3;
			bool flag = false;
			do
			{
				flag = false;
				try
				{
					MountWIM(readOnly);
				}
				catch (Exception)
				{
					num++;
					flag = num < num2;
					if (!flag)
					{
						throw;
					}
					Thread.Sleep(1000);
				}
			}
			while (flag);
			base.IsMounted = true;
		}

		public override void MountReadOnly()
		{
			bool readOnly = true;
			MountWithRetry(readOnly);
		}

		public override void Mount()
		{
			bool readOnly = false;
			MountWithRetry(readOnly);
		}

		private void MountWIM(bool readOnly)
		{
			lock (_lockObj)
			{
				m_partitions.Clear();
				string text = string.Empty;
				if (!readOnly)
				{
					text = FileUtils.GetTempDirectory();
				}
				string tempDirectory = FileUtils.GetTempDirectory();
				if (CommonUtils.MountWIM(m_mobileCoreImagePath, tempDirectory, text))
				{
					m_partitions.Add(new ImagePartition("WIM", tempDirectory));
					_tmpDir = text;
					_mountPoint = tempDirectory;
					_commitChanges = !readOnly;
				}
				else
				{
					FileUtils.DeleteTree(tempDirectory);
					if (!string.IsNullOrEmpty(text))
					{
						FileUtils.DeleteTree(text);
					}
				}
			}
		}

		public override void Unmount()
		{
			lock (_lockObj)
			{
				CommonUtils.DismountWIM(m_mobileCoreImagePath, _mountPoint, _commitChanges);
				m_partitions.Clear();
				base.IsMounted = false;
				if (!string.IsNullOrEmpty(_tmpDir))
				{
					FileUtils.DeleteTree(_tmpDir);
				}
				_tmpDir = null;
				_mountPoint = null;
			}
		}
	}
}
