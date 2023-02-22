using System;
using System.IO;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public sealed class DiffFileEntry : FileEntryBase, IDiffEntry
	{
		[XmlElement]
		public DiffType DiffType { get; set; }

		public DiffFileEntry()
		{
			DiffType = DiffType.Invalid;
		}

		public DiffFileEntry(IntPtr objPtr)
			: base(objPtr)
		{
			DiffType = NativeMethods.DiffFileEntry_Get_DiffType(objPtr);
		}

		public DiffFileEntry(FileType ft, DiffType diffType, string destination, string source)
		{
			if (diffType == DiffType.Invalid || ft == FileType.Invalid)
			{
				throw new PackageException("Invalid type should not be used for non-default constructor");
			}
			if (diffType != DiffType.Remove)
			{
				if (string.IsNullOrEmpty(source))
				{
					throw new PackageException("No source path specified for in non-default constructor with diff types other than Remove");
				}
				if (!File.Exists(source))
				{
					throw new PackageException("Source file '{0}' doesn't exist", source);
				}
				base.SourcePath = source;
			}
			base.FileType = ft;
			DiffType = diffType;
			base.DevicePath = destination;
		}

		public new void Validate()
		{
			if (DiffType == DiffType.Invalid)
			{
				throw new PackageException("Invalid DiffType");
			}
			base.Validate();
		}
	}
}
