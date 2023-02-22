using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Images
{
	public class ImageInfo
	{
		private WPImage _wpImage;

		private IULogger _logger;

		private string _imageFile;

		private string _dwordFormatString = "{0} (0x{0:X})";

		private ASCIIEncoding _enc = new ASCIIEncoding();

		private string _tempDirectoryPath = string.Empty;

		private bool _cleanUpTempDir;

		private bool _cleanUpWPImage;

		public ImageInfo(string tempFileDirectory)
		{
			SetTempDirectoryPath(tempFileDirectory);
		}

		public ImageInfo(WPImage wpImage, string tempFileDirectory)
		{
			_wpImage = wpImage;
			SetTempDirectoryPath(tempFileDirectory);
		}

		public ImageInfo(string fileName, string tempFileDirectory)
		{
			if (tempFileDirectory != null)
			{
				SetTempDirectoryPath(tempFileDirectory);
			}
			else
			{
				SetTempDirectoryPath(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
				_cleanUpTempDir = true;
			}
			_logger = new IULogger();
			_imageFile = fileName;
			try
			{
				_wpImage = new WPImage(_logger);
				_cleanUpWPImage = true;
				_wpImage.LoadImage(fileName);
			}
			catch (ImagesException)
			{
				throw;
			}
			catch (Exception ex2)
			{
				throw new ImagesException("Tools.ImgCommon!ImageInfo: Failed to load image file '" + fileName + "' : " + ex2.Message, ex2);
			}
		}

		public void Dispose()
		{
			if (_wpImage != null)
			{
				if (_cleanUpWPImage)
				{
					_wpImage.Dispose();
				}
				_wpImage = null;
			}
			if (_cleanUpTempDir)
			{
				FileUtils.DeleteTree(_tempDirectoryPath);
				_tempDirectoryPath = string.Empty;
			}
		}

		private void SetTempDirectoryPath(string tempFileDirectory)
		{
			_tempDirectoryPath = tempFileDirectory;
			if (!Directory.Exists(_tempDirectoryPath))
			{
				Directory.CreateDirectory(_tempDirectoryPath);
			}
			_tempDirectoryPath = FileUtils.GetShortPathName(_tempDirectoryPath);
		}

		public string DisplayFileName()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("****************************************************************************");
			stringBuilder.AppendLine("****************************************************************************");
			stringBuilder.AppendLine("File: " + _imageFile);
			stringBuilder.AppendLine("****************************************************************************");
			stringBuilder.AppendLine("****************************************************************************");
			stringBuilder.AppendLine();
			return stringBuilder.ToString();
		}

		public string DisplayMetadata()
		{
			StringBuilder stringBuilder = new StringBuilder();
			char[] array = new char[1];
			if (_wpImage == null)
			{
				return "";
			}
			stringBuilder.AppendLine("[Metadata]");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Device Platform IDs:");
			foreach (string devicePlatformID in _wpImage.DevicePlatformIDs)
			{
				stringBuilder.AppendFormat("\tID: {0}", devicePlatformID);
				stringBuilder.AppendLine();
			}
			if (!_wpImage.IsFFU)
			{
				return stringBuilder.ToString();
			}
			if (_wpImage.Metadata == null)
			{
				return stringBuilder.ToString();
			}
			stringBuilder.AppendLine("[Secure Header]");
			stringBuilder.AppendFormat("\tcbSize: " + _dwordFormatString + "  bytes", _wpImage.Metadata.GetSecureHeader.ByteCount, _wpImage.Metadata.GetSecureHeader.ByteCount);
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("\tSignature: {0}", _enc.GetString(FullFlashUpdateHeaders.GetSecuritySignature()));
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("\tChunk Size: " + _dwordFormatString + " KB", _wpImage.Metadata.GetSecureHeader.ChunkSize, _wpImage.Metadata.GetSecureHeader.ChunkSize);
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("\tHash Algorithm ID: " + _dwordFormatString, _wpImage.Metadata.GetSecureHeader.HashAlgorithmID, _wpImage.Metadata.GetSecureHeader.HashAlgorithmID);
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("\tCatalog Size: " + _dwordFormatString + " bytes", _wpImage.Metadata.GetSecureHeader.CatalogSize, _wpImage.Metadata.GetSecureHeader.CatalogSize);
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("\tHash Table Size: " + _dwordFormatString + " bytes", _wpImage.Metadata.GetSecureHeader.HashTableSize, _wpImage.Metadata.GetSecureHeader.HashTableSize);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("[Catalog File]");
			string text = _tempDirectoryPath + "\\WMImage.cat";
			X509Certificate x509Certificate = null;
			File.WriteAllBytes(text, _wpImage.Metadata.CatalogData);
			x509Certificate = GetSigningCert(text);
			if (x509Certificate == null)
			{
				stringBuilder.AppendLine("\tNot Signed");
			}
			else
			{
				stringBuilder.AppendLine("\tSigned with: ");
				stringBuilder.AppendFormat("\t\t\tSubject: {0}", x509Certificate.Subject);
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("\t\t\t\tIssuer: {0}", x509Certificate.Issuer);
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine("[Image Header]");
			stringBuilder.AppendFormat("\tcbSize: " + _dwordFormatString + "  bytes", _wpImage.Metadata.GetImageHeader.ByteCount, _wpImage.Metadata.GetImageHeader.ByteCount);
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("\tSignature: {0}", _enc.GetString(FullFlashUpdateHeaders.GetImageSignature()));
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("\tManifest Length: " + _dwordFormatString + " bytes", _wpImage.Metadata.GetImageHeader.ManifestLength, _wpImage.Metadata.GetImageHeader.ManifestLength);
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("\tChunk Size: " + _dwordFormatString + " KB", _wpImage.Metadata.GetImageHeader.ChunkSize, _wpImage.Metadata.GetImageHeader.ChunkSize);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("[Manifest]");
			stringBuilder.AppendLine();
			string text2 = "";
			MemoryStream memoryStream = new MemoryStream();
			_wpImage.Metadata.GetManifest.WriteToStream(memoryStream);
			text2 = _enc.GetString(memoryStream.GetBuffer());
			text2 = text2.Substring(0, (int)memoryStream.Length);
			stringBuilder.AppendLine(text2);
			return stringBuilder.ToString();
		}

		public string DisplayStore()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (_wpImage == null)
			{
				return "";
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("****************************************************************************");
			stringBuilder.AppendLine("****************************************************************************");
			stringBuilder.AppendLine("\t\t\tStore");
			stringBuilder.AppendLine("****************************************************************************");
			stringBuilder.AppendLine("****************************************************************************");
			stringBuilder.AppendLine("[Store]");
			stringBuilder.AppendLine();
			using (WPStore wPStore = _wpImage.Store)
			{
				int num = 0;
				foreach (WPPartition partition in wPStore.Partitions)
				{
					if (!partition.IsWim)
					{
						num++;
					}
				}
				stringBuilder.AppendFormat("Sector Size: " + _dwordFormatString + "  bytes", wPStore.SectorSize, wPStore.SectorSize);
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("Min Sector Count: " + _dwordFormatString + "  sectors", wPStore.MinSectorCount, wPStore.MinSectorCount);
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("PartitionCount: {0}", num);
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString();
		}

		public string DisplayPartitions()
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			if (_wpImage == null)
			{
				return "";
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("****************************************************************************");
			stringBuilder.AppendLine("****************************************************************************");
			stringBuilder.AppendLine("\t\t\tPartitions");
			stringBuilder.AppendLine("****************************************************************************");
			stringBuilder.AppendLine("****************************************************************************");
			stringBuilder.AppendLine("[Partitions]");
			stringBuilder.AppendLine();
			stringBuilder2.AppendLine();
			stringBuilder2.AppendLine();
			stringBuilder2.AppendLine("****************************************************************************");
			stringBuilder2.AppendLine("****************************************************************************");
			stringBuilder2.AppendLine("\t\t\tWIMs");
			stringBuilder2.AppendLine("****************************************************************************");
			stringBuilder2.AppendLine("****************************************************************************");
			stringBuilder2.AppendLine("[WIMs]");
			stringBuilder2.AppendLine();
			foreach (WPPartition partition in _wpImage.Partitions)
			{
				if (partition.IsWim)
				{
					stringBuilder2.Append(DisplayWIM(partition));
				}
				else
				{
					stringBuilder.Append(DisplayPartition(partition));
				}
			}
			return stringBuilder.ToString() + stringBuilder2.ToString();
		}

		public string DisplayPartition(WPPartition partition)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("\t[Partition]");
			stringBuilder.AppendFormat("\t\tName: {0}", partition.Name);
			stringBuilder.AppendLine();
			if (!string.IsNullOrEmpty(partition.FileSystem))
			{
				stringBuilder.AppendFormat("\t\tFile System: {0}", partition.FileSystem);
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendFormat("\t\tType: {0}", partition.PartitionType);
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("\t\tTotal Sectors: " + _dwordFormatString + " sectors", partition.TotalSectors, partition.TotalSectors);
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("\t\tUse All Space: {0}", partition.UseAllSpace.ToString());
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("\t\tSectors In Use: " + _dwordFormatString + " sectors", partition.SectorsInUse, partition.SectorsInUse);
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("\t\tRequiredToFlash: {0}", partition.RequiredToFlash.ToString());
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("\t\tBootable: {0}", partition.Bootable.ToString());
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("\t\tReadOnly: {0}", partition.ReadOnly.ToString());
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("\t\tHidden: {0}", partition.Hidden.ToString());
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("\t\tPrimary Partition: {0}", partition.PrimaryPartition);
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("\t\tAttach Drive Letter: {0}", partition.AttachDriveLetter.ToString());
			stringBuilder.AppendLine();
			if (partition.ByteAlignment != 0)
			{
				stringBuilder.AppendFormat("\t\tByte Alignment: " + _dwordFormatString + " bytes", partition.ByteAlignment, partition.ByteAlignment);
				stringBuilder.AppendLine();
			}
			if (partition.SectorAlignment != 0)
			{
				stringBuilder.AppendFormat("\t\tSector Alignment: " + _dwordFormatString + " sectors", partition.SectorAlignment, partition.SectorAlignment);
				stringBuilder.AppendLine();
			}
			if (partition.InvalidPartition)
			{
				stringBuilder.AppendLine("\t\tPartition is inaccessible.");
			}
			return stringBuilder.ToString();
		}

		public string DisplayWIM(WPPartition partition)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("\t[WIM]");
			stringBuilder.AppendFormat("\t\tFile Name: {0}", partition.WimFile);
			stringBuilder.AppendLine();
			if (!string.IsNullOrEmpty(partition.FileSystem))
			{
				stringBuilder.AppendFormat("\t\tFile System: {0}", partition.FileSystem);
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendFormat("\t\tWim File Size: " + _dwordFormatString + " bytes", partition.WimFileSize, partition.WimFileSize);
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("\t\tWim File Content Size: " + _dwordFormatString + " bytes", partition.WimFileContentSize, partition.WimFileContentSize);
			stringBuilder.AppendLine();
			return stringBuilder.ToString();
		}

		public string DisplayPackageList(WPPartition partition)
		{
			StringBuilder stringBuilder = new StringBuilder();
			SortedList<string, string> sortedList = new SortedList<string, string>();
			stringBuilder.AppendFormat("Packages for Partition {0}\n", partition.Name);
			foreach (IPkgInfo package in partition.Packages)
			{
				string text = package.Name.ToUpper(CultureInfo.InvariantCulture);
				sortedList.Add(text, text);
			}
			foreach (string key in sortedList.Keys)
			{
				stringBuilder.AppendLine("\t" + key);
			}
			return stringBuilder.ToString();
		}

		public string DisplayRegistry()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (_wpImage == null)
			{
				return "";
			}
			foreach (WPPartition partition in _wpImage.Partitions)
			{
				stringBuilder.Append(DisplayRegistry(partition));
			}
			return stringBuilder.ToString();
		}

		public string DisplayRegistry(WPPartition partition)
		{
			StringBuilder outputStr = new StringBuilder();
			if (partition.IsBinaryPartition)
			{
				return "";
			}
			outputStr.AppendLine();
			outputStr.AppendLine();
			outputStr.AppendLine("****************************************************************************");
			outputStr.AppendLine("****************************************************************************");
			outputStr.AppendLine("\t\t\tRegistry for " + partition.PartitionTypeLabel + " '" + partition.Name + "'");
			outputStr.AppendLine("****************************************************************************");
			outputStr.AppendLine("****************************************************************************");
			if (partition.HasRegistryHive(SystemRegistryHiveFiles.DEFAULT))
			{
				DisplayPartitionRegistryHive(partition, SystemRegistryHiveFiles.DEFAULT, ref outputStr);
			}
			if (partition.HasRegistryHive(SystemRegistryHiveFiles.DRIVERS))
			{
				DisplayPartitionRegistryHive(partition, SystemRegistryHiveFiles.DRIVERS, ref outputStr);
			}
			if (partition.HasRegistryHive(SystemRegistryHiveFiles.SAM))
			{
				DisplayPartitionRegistryHive(partition, SystemRegistryHiveFiles.SAM, ref outputStr);
			}
			if (partition.HasRegistryHive(SystemRegistryHiveFiles.SECURITY))
			{
				DisplayPartitionRegistryHive(partition, SystemRegistryHiveFiles.SECURITY, ref outputStr);
			}
			if (partition.HasRegistryHive(SystemRegistryHiveFiles.SOFTWARE))
			{
				DisplayPartitionRegistryHive(partition, SystemRegistryHiveFiles.SOFTWARE, ref outputStr);
			}
			if (partition.HasRegistryHive(SystemRegistryHiveFiles.SYSTEM))
			{
				DisplayPartitionRegistryHive(partition, SystemRegistryHiveFiles.SYSTEM, ref outputStr);
			}
			if (partition.HasRegistryHive(SystemRegistryHiveFiles.BCD))
			{
				DisplayPartitionRegistryHive(partition, SystemRegistryHiveFiles.BCD, ref outputStr);
			}
			return outputStr.ToString();
		}

		private void DisplayPartitionRegistryHive(WPPartition partition, SystemRegistryHiveFiles hiveType, ref StringBuilder outputStr)
		{
			string keyPrefix = RegistryUtils.MapHiveToMountPoint(hiveType);
			HiveToRegConverter hiveToRegConverter = new HiveToRegConverter(partition.GetRegistryHivePath(hiveType), keyPrefix);
			outputStr.AppendLine();
			outputStr.AppendFormat("[{2} {0}: {1} ]", partition.Name, partition.GetRegistryHiveDevicePath(hiveType), partition.PartitionTypeLabel);
			outputStr.AppendLine();
			hiveToRegConverter.ConvertToReg(ref outputStr);
		}

		public string DisplayPackages(bool fSummary)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (_wpImage == null)
			{
				return "";
			}
			stringBuilder.AppendLine("[Packages]");
			stringBuilder.AppendLine();
			foreach (WPPartition partition in _wpImage.Partitions)
			{
				stringBuilder.Append(DisplayPackages(partition, fSummary));
			}
			return stringBuilder.ToString();
		}

		public string DisplayDefaultCerts()
		{
			StringBuilder stringBuilder = new StringBuilder(null);
			foreach (WPPartition partition in _wpImage.Partitions)
			{
				stringBuilder.AppendLine(DisplayDefaultCert(partition));
			}
			return stringBuilder.ToString();
		}

		public string DisplayDefaultCert(WPPartition partition)
		{
			StringBuilder stringBuilder = new StringBuilder(null);
			byte[] array = null;
			foreach (string item in Directory.EnumerateFiles(Path.Combine(partition.PartitionPath, PkgConstants.c_strDsmDeviceFolder), "*.dat"))
			{
				stringBuilder.AppendLine("Certs file " + item + " info for partition " + partition.Name);
				array = File.ReadAllBytes(item);
				if (array != null)
				{
					stringBuilder.AppendLine(DisplayDefaultCerts(array));
				}
				else
				{
					stringBuilder.AppendLine("\tUnable to extract '" + item + "' file.");
				}
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString();
		}

		public string DisplayPackages(WPPartition partition, bool fSummary)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("****************************************************************************");
			stringBuilder.AppendLine("****************************************************************************");
			stringBuilder.AppendLine("\t\t\tPackages for " + partition.PartitionTypeLabel + " '" + partition.Name + "'");
			stringBuilder.AppendLine("****************************************************************************");
			stringBuilder.AppendLine("****************************************************************************");
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("[Packages Summary for " + partition.PartitionTypeLabel + " {0}]", partition.Name);
			stringBuilder.AppendLine();
			if (partition.Packages.Count == 0)
			{
				stringBuilder.AppendLine("None");
			}
			else
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("{0,-25:G}\t", "Package Name");
				stringBuilder.AppendFormat("{0,-10:G}\t", "Version");
				stringBuilder.AppendFormat("{0,-15:G}\t", "Canonical\\Diff");
				stringBuilder.AppendFormat("{0,-20:G}\t", "Build");
				stringBuilder.AppendFormat("{0,-10:G}\t", "File Count");
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("_________________________\t__________\t_______________\t____________________\t__________");
				foreach (IPkgInfo package in partition.Packages)
				{
					if (!package.IsBinaryPartition)
					{
						stringBuilder.AppendFormat("{0,-25:G}\t", package.Name);
						stringBuilder.AppendFormat("{0,-10:G}\t", package.Version);
						stringBuilder.AppendFormat("{0,-15:G}\t", package.Type);
						stringBuilder.AppendFormat("{0,-20:G}\t", package.BuildString);
						stringBuilder.AppendFormat("{0,-10:G}\t", package.FileCount);
						stringBuilder.AppendLine();
					}
				}
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("[Package Details]");
				stringBuilder.AppendLine();
				foreach (IPkgInfo package2 in partition.Packages)
				{
					if (package2.IsBinaryPartition)
					{
						continue;
					}
					stringBuilder.AppendLine("\t[Package]");
					stringBuilder.AppendFormat("\t\tName: {0}", package2.Name);
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("\t\tOwner: {0}", package2.Owner);
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("\t\tComponent: {0}", package2.Component);
					stringBuilder.AppendLine();
					if (!string.IsNullOrEmpty(package2.SubComponent))
					{
						stringBuilder.AppendFormat("\t\tSubComponent: {0}", package2.SubComponent);
						stringBuilder.AppendLine();
					}
					stringBuilder.AppendFormat("\t\t" + partition.PartitionTypeLabel + ": {0}", package2.Partition);
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("\t\tVersion: ");
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("\t\t\tMajor: {0}", package2.Version.Major);
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("\t\t\tMinor: {0}", package2.Version.Minor);
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("\t\t\tQFE: {0}", package2.Version.QFE);
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("\t\t\tBuild: {0}", package2.Version.Build);
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("\t\tOwner Type: {0}", package2.OwnerType);
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("\t\tRelease Type: {0}", package2.ReleaseType);
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("\t\tBuild Type: {0}", package2.BuildType);
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("\t\tCPU: {0}", package2.CpuType);
					stringBuilder.AppendLine();
					if (!string.IsNullOrEmpty(package2.Culture))
					{
						stringBuilder.AppendFormat("\t\tCulture: {0}", package2.Culture);
						stringBuilder.AppendLine();
					}
					if (!string.IsNullOrEmpty(package2.Resolution))
					{
						stringBuilder.AppendFormat("\t\tResolution: {0}", package2.Resolution);
						stringBuilder.AppendLine();
					}
					stringBuilder.AppendFormat("\t\tPackage Type: {0}", package2.Type);
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("\t\tBuild: {0}", package2.BuildString);
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("\t\tGroup Key: {0}", package2.GroupingKey);
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("\t\tFile Count: {0}", package2.FileCount);
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("\t\t[File Summary for package {0}]", package2.Name);
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("\t\t{0,-35:G}\t", "Filename");
					stringBuilder.AppendFormat("{0,-15:G}\t", "Size (bytes)");
					stringBuilder.AppendFormat("{0,-25:G}\t", "Attributes");
					stringBuilder.AppendFormat("{0,-10:G}\t", "Type");
					stringBuilder.AppendFormat("{0,-35:G}\t", "Device Path");
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("\t\t___________________________________\t_______________\t_________________________\t__________\t___________________________________");
					foreach (IFileEntry file in package2.Files)
					{
						string fileName = Path.GetFileName(file.DevicePath);
						string arg = file.DevicePath.Replace(fileName, "", StringComparison.OrdinalIgnoreCase);
						string path = partition.PartitionPath + file.DevicePath;
						stringBuilder.AppendFormat("\t\t{0,-35:G}\t", fileName);
						if (!File.Exists(path))
						{
							stringBuilder.AppendFormat("{0,-15:G}\t", "File not found");
						}
						else
						{
							FileInfo fileInfo = new FileInfo(partition.PartitionPath + file.DevicePath);
							stringBuilder.AppendFormat("{0,-15:G}\t", fileInfo.Length);
						}
						stringBuilder.AppendFormat("{0,-25:G}\t", file.Attributes);
						stringBuilder.AppendFormat("{0,-10:G}\t", file.FileType);
						stringBuilder.AppendFormat("{0,-35:G}\t", arg);
						stringBuilder.AppendLine();
					}
					stringBuilder.AppendLine();
					if (fSummary)
					{
						continue;
					}
					stringBuilder.AppendLine("\t\t[Files]");
					foreach (IFileEntry file2 in package2.Files)
					{
						stringBuilder.AppendLine("\t\t\t[File]");
						string fileName = Path.GetFileName(file2.DevicePath);
						string arg = file2.DevicePath.Replace(fileName, "", StringComparison.OrdinalIgnoreCase);
						string text = partition.PartitionPath + file2.DevicePath;
						string empty = string.Empty;
						long num = 0L;
						string text2 = string.Empty;
						bool flag = false;
						if (File.Exists(text))
						{
							flag = true;
							FileInfo fileInfo2 = new FileInfo(text);
							FileVersionInfo.GetVersionInfo(text);
							num = fileInfo2.Length;
							text2 = fileInfo2.CreationTime.ToString();
						}
						stringBuilder.AppendFormat("\t\t\t\tName: {0} {1}", fileName, flag ? "" : "(File not Found)");
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\t\t\t\tCreation Date\\Time: " + (flag ? text2 : "Unknown"));
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\t\t\t\tVersion Number: " + (flag ? empty : "Unknown"));
						stringBuilder.AppendLine();
						if (flag)
						{
							stringBuilder.AppendFormat("\t\t\t\tSize: " + _dwordFormatString + " bytes", num, num);
						}
						else
						{
							stringBuilder.AppendFormat("\t\t\t\tSize: Unknown");
						}
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\t\t\t\tAttributes: {0}", file2.Attributes);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\t\t\t\tPath: {0}", arg);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\t\t\t\tPackage: {0}", package2.Name);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\t\t\t\tFileType: {0}", file2.FileType);
						stringBuilder.AppendLine();
						stringBuilder.AppendFormat("\t\t\t\tSource Package: {0}", file2.SourcePackage);
						stringBuilder.AppendLine();
					}
				}
			}
			return stringBuilder.ToString();
		}

		private X509Certificate GetSigningCert(string fileName)
		{
			X509Certificate x509Certificate = null;
			try
			{
				return new X509Certificate2(fileName);
			}
			catch
			{
				return null;
			}
		}

		public string DisplayPackages(string packagesInfo)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("[Packages]");
			stringBuilder.AppendLine();
			stringBuilder.Append(packagesInfo);
			return stringBuilder.ToString();
		}

		public string DisplayPartitions(string partitionsInfo)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("[Partitions]");
			stringBuilder.AppendLine();
			stringBuilder.Append(partitionsInfo);
			return stringBuilder.ToString();
		}

		public string DisplayWIMs(string wimsInfo)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("[WIMs]");
			stringBuilder.AppendLine();
			stringBuilder.Append(wimsInfo);
			return stringBuilder.ToString();
		}

		public string DisplayDefaultCerts(byte[] defaultCerts)
		{
			StringBuilder stringBuilder = new StringBuilder();
			try
			{
				if (defaultCerts != null && defaultCerts.Length != 0)
				{
					X509Certificate2 x509Certificate;
					for (int i = 0; i < defaultCerts.Length - 1; i += x509Certificate.RawData.Length)
					{
						byte[] array = new byte[defaultCerts.Length];
						for (int j = 0; j < defaultCerts.Length - i; j++)
						{
							array[j] = defaultCerts[i + j];
						}
						x509Certificate = new X509Certificate2(array);
						stringBuilder.AppendLine("\t[Cert]");
						stringBuilder.AppendLine("\t\tSubject: " + x509Certificate.Subject);
						stringBuilder.AppendLine("\t\tIssuer: " + x509Certificate.Issuer);
						stringBuilder.AppendLine("\t\tSerial Number: " + x509Certificate.SerialNumber);
						stringBuilder.AppendLine("\t\tSignature Algorithm: " + x509Certificate.SignatureAlgorithm.FriendlyName);
						stringBuilder.AppendLine("\t\tVersion: " + x509Certificate.Version);
					}
				}
				else
				{
					stringBuilder.AppendLine("Empty file");
				}
			}
			catch (Exception ex)
			{
				stringBuilder.AppendLine("Error parsing file: " + ex.Message);
			}
			return stringBuilder.ToString();
		}
	}
}
