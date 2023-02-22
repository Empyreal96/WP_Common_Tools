using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Images
{
	public class ImageExtractor : IDisposable
	{
		private IULogger _logger;

		private string _extractionDirectory;

		private bool _userCancelled;

		private bool _fMetadataOnly;

		private string _imageFile;

		private WPImage _wpImage;

		private ImageInfo _imgDump;

		private bool _alreadyDisposed;

		public bool MetaDataOnly
		{
			get
			{
				return _fMetadataOnly;
			}
			set
			{
				_fMetadataOnly = value;
			}
		}

		public List<WPPartition> Partitions
		{
			get
			{
				if (_wpImage == null)
				{
					return null;
				}
				return _wpImage.Partitions;
			}
		}

		public ImageExtractor(string imageFile, IULogger logger, string extractionDirectory)
		{
			_logger = logger;
			_extractionDirectory = extractionDirectory;
			_imageFile = imageFile;
			try
			{
				_wpImage = new WPImage(_logger);
				_wpImage.LoadImage(_imageFile);
			}
			catch (Exception ex)
			{
				throw new ImagesException("Tools.ImgCommon!ImageExtractor: Failed to open the Image to extract : " + ex.Message);
			}
			_imgDump = new ImageInfo(_wpImage, _extractionDirectory);
		}

		~ImageExtractor()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool isDisposing)
		{
			if (!_alreadyDisposed)
			{
				if (_imgDump != null)
				{
					_imgDump.Dispose();
					_imgDump = null;
				}
				if (_wpImage != null)
				{
					_wpImage.Dispose();
					_wpImage = null;
				}
				_alreadyDisposed = true;
			}
		}

		public bool ExtractImage()
		{
			bool result = false;
			if (_wpImage == null)
			{
				throw new ImagesException("Tools.ImgCommon!ExtractImage: Image is not loaded.");
			}
			if (!_userCancelled)
			{
				switch (Path.GetExtension(_imageFile).ToUpper(CultureInfo.InvariantCulture))
				{
				case ".FFU":
				case ".VHD":
					_logger.LogInfo("Extracting " + _imageFile);
					result = ExtractFiles();
					break;
				case ".WIM":
					_logger.LogError("Tools.ImgCommon!ExtractImage: WIM not yet supported. Unable to compare.");
					break;
				default:
					_logger.LogError("Tools.ImgCommon!ExtractImage: Unrecognized file format. Unable to compare.");
					break;
				}
			}
			return result;
		}

		private bool ExtractFiles()
		{
			bool flag = true;
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			StringBuilder stringBuilder3 = new StringBuilder();
			StringBuilder stringBuilder4 = new StringBuilder();
			StringBuilder stringBuilder5 = new StringBuilder();
			StringBuilder stringBuilder6 = new StringBuilder();
			try
			{
				if (!_userCancelled)
				{
					foreach (WPPartition partition in _wpImage.Partitions)
					{
						if (!_fMetadataOnly)
						{
							flag = ExtractPartition(partition);
						}
						if (partition.IsWim)
						{
							stringBuilder4.Append(_imgDump.DisplayWIM(partition));
						}
						else
						{
							stringBuilder3.Append(_imgDump.DisplayPartition(partition));
						}
						stringBuilder6.Append(_imgDump.DisplayPackageList(partition));
						if (!partition.IsBinaryPartition)
						{
							stringBuilder5.Append(_imgDump.DisplayPackages(partition, true));
							stringBuilder2.Append(_imgDump.DisplayRegistry(partition));
						}
						if (_userCancelled || !flag)
						{
							break;
						}
					}
				}
				if (flag)
				{
					if (!_userCancelled)
					{
						try
						{
							_logger.LogInfo("Extracting Metadata...");
							stringBuilder.Append(_imgDump.DisplayMetadata());
							stringBuilder.Append(_imgDump.DisplayStore());
							stringBuilder.Append(_imgDump.DisplayPartitions(stringBuilder3.ToString()));
							stringBuilder.Append(_imgDump.DisplayWIMs(stringBuilder4.ToString()));
							stringBuilder.Append(_imgDump.DisplayPackages(stringBuilder5.ToString()));
							stringBuilder.Append(stringBuilder2.ToString());
						}
						catch (Exception ex)
						{
							_logger.LogError("Tools.ImgCommon!ExtractFiles: Failure occurred getting metadata: " + ex.Message, ex.InnerException);
						}
						if (stringBuilder.Length > 0)
						{
							_logger.LogInfo("Writing Metadata file....");
							File.WriteAllText(Path.Combine(_extractionDirectory, "_Metadata.txt"), stringBuilder.ToString());
						}
						if (stringBuilder6.Length > 0)
						{
							_logger.LogInfo("Writing Metadata file....");
							File.WriteAllText(Path.Combine(_extractionDirectory, "_PackageList.txt"), stringBuilder6.ToString());
							return flag;
						}
						return flag;
					}
					return flag;
				}
				return flag;
			}
			catch (Exception ex2)
			{
				_logger.LogError("Tools.ImgCommon!ExtractFiles: Failure occurred getting metadata: " + ex2.Message, ex2.InnerException);
				return flag;
			}
		}

		public bool ExtractPartition(WPPartition partition)
		{
			bool result = false;
			_logger.LogInfo("Extracting {0}: {1}", partition.PartitionTypeLabel, partition.Name);
			if (partition.IsBinaryPartition)
			{
				string text = Path.Combine(_extractionDirectory, partition.Name);
				LongPathDirectory.CreateDirectory(text);
				FileUtils.CleanDirectory(text);
				string destinationFile = Path.Combine(text, partition.Name + ".bin");
				try
				{
					partition.CopyAsBinary(destinationFile);
					result = true;
					return result;
				}
				catch (Exception ex)
				{
					if (partition.InvalidPartition)
					{
						_logger.LogError("Tools.ImgCommon!ExtractPartition: Failed to extract partition '" + partition.Name + "' as binary data because the partition is inaccessible.");
						return result;
					}
					_logger.LogError("Tools.ImgCommon!ExtractPartition: Failed to extract partition '" + partition.Name + "' as binary data: '" + ex.Message + "'");
					return result;
				}
			}
			_logger.LogInfo("Extracting files... ");
			string text2 = Path.Combine(_extractionDirectory, partition.Name);
			LongPathDirectory.CreateDirectory(text2);
			FileUtils.CleanDirectory(text2);
			if (partition.Name.ToUpper(CultureInfo.InvariantCulture) == ImageConstants.MAINOS_PARTITION_NAME.ToUpper(CultureInfo.InvariantCulture))
			{
				foreach (string item in Directory.EnumerateFiles(partition.PartitionPath))
				{
					File.Copy(item, Path.Combine(text2, Path.GetFileName(item)));
				}
				foreach (string item2 in Directory.EnumerateDirectories(partition.PartitionPath))
				{
					bool flag = false;
					string text3 = Path.GetFileName(item2).ToUpper(CultureInfo.InvariantCulture);
					foreach (WPPartition partition2 in _wpImage.Partitions)
					{
						if (partition2.Name.ToUpper(CultureInfo.InvariantCulture) == text3 || text3 == "DATA")
						{
							flag = true;
							break;
						}
					}
					if (!flag && text3 != "SYSTEM VOLUME INFORMATION")
					{
						string destination = Path.Combine(text2, text3);
						FileUtils.CopyDirectory(item2, destination);
					}
				}
			}
			else
			{
				FileUtils.CopyDirectory(partition.PartitionPath, text2);
			}
			return true;
		}
	}
}
