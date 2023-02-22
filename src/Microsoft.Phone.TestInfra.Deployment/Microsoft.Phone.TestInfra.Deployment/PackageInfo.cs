using System;
using System.IO;
using System.Threading;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class PackageInfo
	{
		private int count;

		public string RootPath { get; private set; }

		public string RelativePath { get; private set; }

		public string AbsolutePath { get; private set; }

		public string PackageName { get; private set; }

		public int Count
		{
			get
			{
				return count;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException("value", value, "Count is negative");
				}
				Interlocked.Exchange(ref count, value);
			}
		}

		public PackageInfo(string rootPath, string relativePath)
		{
			if (string.IsNullOrEmpty(rootPath))
			{
				throw new ArgumentNullException("rootPath");
			}
			if (string.IsNullOrEmpty(relativePath))
			{
				throw new ArgumentNullException("relativePath");
			}
			count = 1;
			RootPath = PathHelper.EndWithDirectorySeparator(rootPath);
			RelativePath = relativePath.TrimStart(Path.DirectorySeparatorChar);
			AbsolutePath = PathHelper.Combine(RootPath, RelativePath);
			PackageName = PathHelper.GetPackageNameWithoutExtension(AbsolutePath);
		}

		public override int GetHashCode()
		{
			return (AbsolutePath != null) ? AbsolutePath.GetHashCode() : 0;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (this == obj)
			{
				return true;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((PackageInfo)obj);
		}

		protected bool Equals(PackageInfo other)
		{
			return string.Equals(AbsolutePath, other.AbsolutePath, StringComparison.OrdinalIgnoreCase);
		}
	}
}
