using System.IO;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public interface IFileEntry
	{
		FileType FileType { get; }

		string DevicePath { get; }

		string CabPath { get; }

		FileAttributes Attributes { get; }

		ulong Size { get; }

		ulong CompressedSize { get; }

		string SourcePackage { get; }

		string FileHash { get; }

		bool SignInfoRequired { get; }

		string FileArch { get; }
	}
}
