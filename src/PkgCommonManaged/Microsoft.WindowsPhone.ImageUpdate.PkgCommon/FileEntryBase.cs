using System;
using System.IO;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public class FileEntryBase
	{
		private string _sourcePath;

		private string _fileHash = string.Empty;

		private ulong _size;

		private ulong _compressedSize;

		private ulong _stagedSize;

		public FileType FileType { get; set; }

		public string SourcePath
		{
			get
			{
				return _sourcePath;
			}
			set
			{
				_sourcePath = value;
			}
		}

		public string FileHash
		{
			get
			{
				if (string.IsNullOrEmpty(_fileHash))
				{
					return "";
				}
				return _fileHash;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					_fileHash = "";
				}
				else
				{
					_fileHash = value;
				}
			}
		}

		public bool SignInfoRequired { get; set; }

		public string DevicePath { get; set; }

		public string CabPath { get; set; }

		public string FileArch { get; set; }

		public ulong Size
		{
			get
			{
				CalculateFileSizes();
				return _size;
			}
			set
			{
				_size = value;
			}
		}

		public ulong CompressedSize
		{
			get
			{
				CalculateFileSizes();
				return _compressedSize;
			}
			set
			{
				_compressedSize = value;
			}
		}

		public ulong StagedSize
		{
			get
			{
				CalculateFileSizes();
				return _stagedSize;
			}
			set
			{
				_stagedSize = value;
			}
		}

		public void Validate()
		{
			if (string.IsNullOrEmpty(DevicePath))
			{
				throw new PackageException("DevicePath not specified");
			}
			if (!Path.IsPathRooted(DevicePath))
			{
				throw new PackageException("DevicePath must be absolutepath");
			}
			if (string.IsNullOrEmpty(CabPath))
			{
				throw new PackageException("CabPath not specified");
			}
		}

		public void BuildSourcePath(string rootDir, BuildPathOption option, bool installed)
		{
			switch (option)
			{
			case BuildPathOption.UseCabPath:
				SourcePath = Path.Combine(rootDir, CabPath.TrimStart('\\'));
				break;
			case BuildPathOption.UseDevicePath:
				SourcePath = Path.Combine(rootDir, DevicePath.TrimStart('\\'));
				break;
			default:
				throw new PackageException("Unexpected option '{0}' specified in BuigldSourcePaths", option);
			}
		}

		public void CalculateFileSizes()
		{
			if (_size != 0L || _stagedSize != 0L || _compressedSize != 0L)
			{
				return;
			}
			if (!LongPathFile.Exists(SourcePath))
			{
				if (string.Equals(Path.GetExtension(SourcePath), ".bin", StringComparison.OrdinalIgnoreCase) || FileType == FileType.Manifest || FileType == FileType.Catalog)
				{
					return;
				}
				throw new PackageException($"Couldn't get file sizes for file '{SourcePath}' since it does not exist");
			}
			ulong fileSize = 0uL;
			ulong stagedSize = 0uL;
			ulong compressedSize = 0uL;
			uint num = NativeMethods.IU_GetStagedAndCompressedSize(SourcePath, out fileSize, out stagedSize, out compressedSize);
			if (num != 0)
			{
				throw new PackageException("Failed IU_GetStagedAndCompressedSize with error '{0}'", num);
			}
			_size = fileSize;
			_stagedSize = stagedSize;
			_compressedSize = compressedSize;
		}

		protected FileEntryBase()
		{
		}

		protected FileEntryBase(IntPtr filePtr)
			: this()
		{
			FileType = NativeMethods.FileEntryBase_Get_FileType(filePtr);
			DevicePath = NativeMethods.FileEntryBase_Get_DevicePath(filePtr);
			CabPath = NativeMethods.FileEntryBase_Get_CabPath(filePtr);
			FileHash = NativeMethods.FileEntryBase_Get_FileHash(filePtr);
			SignInfoRequired = NativeMethods.FileEntryBase_Get_SignInfo(filePtr);
		}
	}
}
