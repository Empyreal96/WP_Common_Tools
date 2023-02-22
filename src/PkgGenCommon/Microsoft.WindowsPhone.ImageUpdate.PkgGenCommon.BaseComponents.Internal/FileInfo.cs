using System.IO;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	public sealed class FileInfo
	{
		public FileType Type;

		public string SourcePath;

		public string DevicePath;

		public FileAttributes Attributes;

		public string EmbeddedSigningCategory = "None";
	}
}
