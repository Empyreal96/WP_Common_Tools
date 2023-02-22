using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public abstract class MobileCoreImage
	{
		protected string m_mobileCoreImagePath;

		protected ImagePartitionCollection m_partitions = new ImagePartitionCollection();

		private const string EXTENSION_VHD = ".VHD";

		private const string EXTENSION_WIM = ".WIM";

		private const string ERROR_IMAGENOTFOUND = "The specified file ({0}) either does not exist or cannot be read.";

		private const string ERROR_INVALIDIMAGE = "The specified file ({0}) is not a valid VHD image.";

		private const string STR_HIVE_PATH = "Windows\\System32\\Config";

		private const string ERROR_NO_SUCH_PARTITION = "Request partition {0} cannot be found in the image";

		private const string STR_SYSTEM32_DIR = "Windows\\System32";

		public string ImagePath => m_mobileCoreImagePath;

		public bool IsMounted { get; protected set; }

		public ReadOnlyCollection<ImagePartition> Partitions
		{
			get
			{
				ReadOnlyCollection<ImagePartition> result = null;
				if (IsMounted)
				{
					result = new ReadOnlyCollection<ImagePartition>(m_partitions);
				}
				return result;
			}
		}

		protected MobileCoreImage(string path)
		{
			m_mobileCoreImagePath = path;
		}

		public static MobileCoreImage Create(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException(path);
			}
			MobileCoreImage mobileCoreImage = null;
			if (!LongPathFile.Exists(path))
			{
				throw new FileNotFoundException($"The specified file ({path}) is not a valid VHD image.");
			}
			FileInfo fileInfo = new FileInfo(path);
			if (fileInfo.Extension.Equals(".VHD", StringComparison.OrdinalIgnoreCase))
			{
				return new MobileCoreVHD(path);
			}
			if (fileInfo.Extension.Equals(".WIM", StringComparison.OrdinalIgnoreCase))
			{
				return new MobileCoreWIM(path);
			}
			throw new ArgumentException($"The specified file ({path}) is not a valid VHD image.");
		}

		public abstract void Mount();

		public abstract void MountReadOnly();

		public abstract void Unmount();

		public ImagePartition GetPartition(MobileCorePartitionType type)
		{
			ImagePartition imagePartition = null;
			if (!IsMounted)
			{
				return null;
			}
			foreach (ImagePartition partition in Partitions)
			{
				if (partition.Root != null && type == MobileCorePartitionType.System && LongPathDirectory.Exists(Path.Combine(partition.Root, "Windows\\System32")))
				{
					imagePartition = partition;
				}
			}
			if (imagePartition == null)
			{
				throw new IUException("Request partition {0} cannot be found in the image", Enum.GetName(typeof(MobileCorePartitionType), type));
			}
			return imagePartition;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (IsMounted)
			{
				foreach (ImagePartition partition in Partitions)
				{
					stringBuilder.AppendFormat("{0}, Root = {1}", partition.Name, partition.Root);
				}
			}
			else
			{
				stringBuilder.AppendLine("This image is not mounted");
			}
			return stringBuilder.ToString();
		}

		public AclCollection GetFileSystemACLs()
		{
			ImagePartition partition = GetPartition(MobileCorePartitionType.System);
			bool flag = false;
			if (!IsMounted)
			{
				Mount();
				flag = true;
			}
			AclCollection aclCollection = null;
			try
			{
				return SecurityUtils.GetFileSystemACLs(partition.Root);
			}
			finally
			{
				if (flag)
				{
					Unmount();
				}
			}
		}

		public AclCollection GetRegistryACLs()
		{
			ImagePartition partition = GetPartition(MobileCorePartitionType.System);
			bool flag = false;
			if (!IsMounted)
			{
				Mount();
				flag = true;
			}
			AclCollection aclCollection = null;
			try
			{
				aclCollection = new AclCollection();
				string hiveRoot = Path.Combine(partition.Root, "Windows\\System32\\Config");
				aclCollection.UnionWith(SecurityUtils.GetRegistryACLs(hiveRoot));
				return aclCollection;
			}
			finally
			{
				if (flag)
				{
					Unmount();
				}
			}
		}
	}
}
