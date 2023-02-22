using System;
using System.IO;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public sealed class FileEntry : FileEntryBase, IFileEntry
	{
		public FileAttributes Attributes { get; set; }

		public string SourcePackage { get; private set; }

		public string EmbeddedSigningCategory { get; private set; }

		public FileEntry()
		{
			base.FileType = FileType.Invalid;
			Attributes = PkgConstants.c_defaultAttributes;
			SourcePackage = string.Empty;
			EmbeddedSigningCategory = "None";
		}

		public FileEntry(IntPtr filePtr)
			: base(filePtr)
		{
			Attributes = NativeMethods.DSMFileEntry_Get_Attributes(filePtr);
			SourcePackage = NativeMethods.DSMFileEntry_Get_SourcePackage(filePtr);
			EmbeddedSigningCategory = NativeMethods.DSMFileEntry_Get_EmbeddedSigningCategory(filePtr);
			base.Size = NativeMethods.DSMFileEntry_Get_FileSize(filePtr);
			base.CompressedSize = NativeMethods.DSMFileEntry_Get_CompressedFileSize(filePtr);
			base.StagedSize = NativeMethods.DSMFileEntry_Get_StagedFileSize(filePtr);
		}

		public FileEntry(FileType type, string destination, string source)
			: this()
		{
			base.FileType = type;
			base.SourcePath = source;
			base.DevicePath = destination;
		}

		public FileEntry(FileType type, string destination, FileAttributes attributes, string source, string sourcePackage, string embedSignCategory)
			: this(type, destination, source)
		{
			Attributes = attributes;
			SourcePackage = ((sourcePackage == null) ? string.Empty : sourcePackage);
			EmbeddedSigningCategory = embedSignCategory;
		}
	}
}
