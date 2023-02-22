using System.Collections.Generic;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public interface IPkgInfo
	{
		PackageType Type { get; }

		PackageStyle Style { get; }

		string Name { get; }

		string PackageName { get; set; }

		string Owner { get; }

		string Component { get; }

		string SubComponent { get; }

		string Partition { get; }

		string Platform { get; }

		string PublicKey { get; }

		VersionInfo Version { get; }

		OwnerType OwnerType { get; }

		ReleaseType ReleaseType { get; }

		BuildType BuildType { get; }

		CpuId CpuType { get; }

		CpuId ComplexCpuType { get; }

		string Culture { get; }

		string Resolution { get; }

		bool IsBinaryPartition { get; }

		bool IsWow { get; }

		VersionInfo PrevVersion { get; }

		byte[] PrevDsmHash { get; }

		string BuildString { get; }

		string GroupingKey { get; }

		string[] TargetGroups { get; }

		int FileCount { get; }

		IEnumerable<IFileEntry> Files { get; }

		string Keyform { get; }

		IFileEntry FindFile(string devicePath);

		IFileEntry GetDsmFile();

		void ExtractFile(string devicePath, string destPath, bool overwriteExistingFiles);

		void ExtractAll(string rootDir, bool overwriteExistingFiles);
	}
}
