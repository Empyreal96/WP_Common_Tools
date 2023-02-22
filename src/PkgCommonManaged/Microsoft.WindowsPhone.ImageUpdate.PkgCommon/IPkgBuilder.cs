using System;
using System.IO;
using Microsoft.WindowsPhone.ImageUpdate.Tools;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public interface IPkgBuilder : IDisposable
	{
		string Name { get; }

		string PackageName { get; set; }

		string Owner { get; set; }

		string Component { get; set; }

		string SubComponent { get; set; }

		string Partition { get; set; }

		string Platform { get; set; }

		VersionInfo Version { get; set; }

		OwnerType OwnerType { get; set; }

		ReleaseType ReleaseType { get; set; }

		PackageStyle PackageStyle { get; set; }

		BuildType BuildType { get; set; }

		CpuId CpuType { get; set; }

		CpuId ComplexCpuType { get; }

		bool IsWow { get; }

		string Culture { get; set; }

		string PublicKey { get; set; }

		string Resolution { get; set; }

		string BuildString { get; set; }

		string GroupingKey { get; set; }

		string[] TargetGroups { get; set; }

		IFileEntry FindFile(string destination);

		void AddFile(IFileEntry file, string source, string embedSignCategory = "None");

		void AddFile(FileType type, string source, string destination, FileAttributes attributes, string srcPkg, string embedSignCategory = "None");

		void AddFile(FileType type, string source, string destination, FileAttributes attributes, string srcPkg, string cabPath, string embedSignCategory);

		void RemoveFile(string destination);

		void RemoveAllFiles();

		void SetIsRemoval(bool isRemoval);

		void SetPkgFileSigner(IPkgFileSigner signer);

		void SaveCab(string cabPath);

		void SaveCab(string cabPath, PackageStyle outputStyle);

		void SaveCab(string cabPath, bool compress);

		void SaveCab(string cabPath, bool compress, PackageStyle outputStyle);

		void SaveCab(string cabPath, CompressionType compressionType);

		void SaveCab(string cabPath, CompressionType compressionType, PackageStyle outputStyle);

		void SaveCBSR(string cabPath, CompressionType compressionType);
	}
}
