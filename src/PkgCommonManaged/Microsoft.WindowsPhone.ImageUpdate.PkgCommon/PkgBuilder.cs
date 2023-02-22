using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.Tools;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	internal class PkgBuilder : WPPackageBase, IPkgBuilder, IDisposable
	{
		private string _tmpDir;

		private FileEntry _manifestEntry = new FileEntry
		{
			FileType = FileType.Manifest
		};

		private FileEntry _catalogEntry = new FileEntry
		{
			FileType = FileType.Catalog
		};

		private List<FileEntry> _allFiles = new List<FileEntry>();

		private IPkgFileSigner _signer = new PkgFileSignerDefault();

		private static bool IsGeneratedFileType(FileType type)
		{
			if (type != FileType.Manifest)
			{
				return type == FileType.Catalog;
			}
			return true;
		}

		private static bool IsGeneratedFileType(FileType type, PackageStyle style, string name)
		{
			if (style == PackageStyle.SPKG)
			{
				return IsGeneratedFileType(type);
			}
			if (!name.Equals(PkgConstants.c_strCBSCatalogFile, StringComparison.OrdinalIgnoreCase))
			{
				return name.Equals(PkgConstants.c_strMumFile, StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}

		private string GetTempFile()
		{
			if (_tmpDir == null)
			{
				_tmpDir = FileUtils.GetTempDirectory();
			}
			return FileUtils.GetTempFile(_tmpDir);
		}

		private void ValidateRegistry()
		{
			string tempDirectory = FileUtils.GetTempDirectory();
			try
			{
				List<string> list = new List<string>();
				foreach (FileEntry item in _allFiles.Where((FileEntry x) => x.FileType == FileType.Registry))
				{
					string text = Path.Combine(tempDirectory, Path.GetFileName(item.DevicePath));
					LongPathFile.Copy(item.SourcePath, text);
					list.Add(text);
				}
				List<string> list2 = new List<string>();
				foreach (FileEntry item2 in _allFiles.Where((FileEntry x) => x.FileType == FileType.RegistryMultiStringAppend))
				{
					string text2 = Path.Combine(tempDirectory, Path.GetFileName(item2.DevicePath));
					LongPathFile.Copy(item2.SourcePath, text2);
					list2.Add(text2);
				}
				RegValidator.Validate(list, list2);
			}
			catch (IUException innerException)
			{
				throw new PackageException(innerException, "Registry validation failed for package '{0}'", base.Name);
			}
			finally
			{
				FileUtils.DeleteTree(tempDirectory);
			}
		}

		private void BuildCabPaths()
		{
			for (int i = 0; i < _allFiles.Count; i++)
			{
				if (_allFiles[i].CabPath == null)
				{
					_allFiles[i].CabPath = PackageTools.MakeShortPath(_allFiles[i].DevicePath, i.ToString());
				}
			}
		}

		private void SaveManifest()
		{
			m_pkgManifest.Files = _allFiles.ToArray();
			if (base.ReleaseType == ReleaseType.Production)
			{
				PackageTools.CheckCrossPartitionFiles(base.Name, base.Partition, _allFiles.Select((FileEntry x) => x.DevicePath));
			}
			m_pkgManifest.Save(_manifestEntry.SourcePath);
		}

		private void Cleanup()
		{
			if (_tmpDir != null)
			{
				try
				{
					FileUtils.DeleteTree(_tmpDir);
				}
				catch (IOException)
				{
				}
				catch (UnauthorizedAccessException)
				{
				}
				_tmpDir = null;
			}
		}

		public void AddFile(IFileEntry file, string source, string embedSignCategory = "None")
		{
			AddFile(file.FileType, source, file.DevicePath, file.Attributes, file.SourcePackage, (file.CabPath == null) ? "" : file.CabPath, embedSignCategory);
		}

		public void AddFile(FileType type, string source, string destination, FileAttributes attributes, string sourcePackage, string embedSignCategory = "None")
		{
			if (!LongPathFile.Exists(source))
			{
				throw new PackageException("File {0} doesn't exist", source);
			}
			if (string.IsNullOrEmpty(destination))
			{
				throw new PackageException("DevicePath can't be empty");
			}
			if (destination.Contains("\\.\\") || destination.Contains("\\..\\") || destination.Contains("\\\\"))
			{
				throw new PackageException("DevicePath can't contain  \\.\\ or \\..\\ or \\\\");
			}
			if (destination.Length > PkgConstants.c_iMaxDevicePath)
			{
				throw new PackageException("DevicePath can't exceed {0} characters", PkgConstants.c_iMaxDevicePath);
			}
			if (!Path.IsPathRooted(destination))
			{
				throw new PackageException("Only absolute path is allowed in DevicePath: '{0}'", destination);
			}
			if (!string.IsNullOrEmpty(sourcePackage) && sourcePackage.Length > PkgConstants.c_iMaxPackageName)
			{
				throw new PackageException("SourcePackage can't be longer than {0} characters", PkgConstants.c_iMaxPackageName);
			}
			if (base.IsBinaryPartition)
			{
				throw new PackageException("File with device path '{0}' can not be added since the package contains a BinaryPartition file ", destination);
			}
			switch (type)
			{
			case FileType.Regular:
			{
				string text = Array.Find(PkgConstants.c_strSpecialFolders, (string x) => destination.StartsWith(x, StringComparison.InvariantCultureIgnoreCase));
				if (text != null)
				{
					throw new PackageException("Failed to add file (Type: '{0}', SourcePath: '{1}', DevicePath: '{2}'): regular files are not allowed in folder '{3}' on device", type, source, destination, text);
				}
				break;
			}
			case FileType.Registry:
			case FileType.RegistryMultiStringAppend:
				if (!PkgConstants.c_strHivePartitions.Contains(base.Partition, StringComparer.InvariantCultureIgnoreCase))
				{
					throw new PackageException("Failed to add registry file '{0}' to partition '{1}': only the listed partitions can have registry hives: {2}", destination, base.Partition, string.Join(",", PkgConstants.c_strHivePartitions));
				}
				break;
			}
			FileAttributes fileAttributes = attributes & ~PkgConstants.c_validAttributes;
			if (fileAttributes != 0)
			{
				throw new PackageException("Failed to add file (Type: '{0}', SourcePath: '{1}', DevicePath: '{2}'): invalid attributes '{3}' specified", type, source, destination, fileAttributes);
			}
			FileEntry fileEntry = _allFiles.Find((FileEntry x) => string.Compare(x.DevicePath, destination, StringComparison.OrdinalIgnoreCase) == 0);
			if (fileEntry != null)
			{
				if (!source.Equals(fileEntry.SourcePath, StringComparison.InvariantCultureIgnoreCase) && !File.ReadAllBytes(source).SequenceEqual(File.ReadAllBytes(fileEntry.SourcePath)))
				{
					throw new PackageException("Failed to add file (Type: '{0}', SourcePath: '{1}', DevicePath: '{2}'): there is already a '{3}' file with same DevicePath ('{4}') in the package. However, their source paths point to different files.", type, source, destination, fileEntry.SourcePath, fileEntry.FileType);
				}
				if (!embedSignCategory.Equals(fileEntry.EmbeddedSigningCategory, StringComparison.OrdinalIgnoreCase))
				{
					throw new PackageException("Failed to add file (Type: '{0}', SourcePath: '{1}', DevicePath: '{2}'): there is already a '{3}' file with same DevicePath ('{4}') in the package. However, their signing options are different.", type, source, destination, fileEntry.SourcePath, fileEntry.FileType);
				}
				if (attributes != fileEntry.Attributes)
				{
					throw new PackageException("Failed to add file (Type: '{0}', SourcePath: '{1}', DevicePath: '{2}'): there is already a '{3}' file with same DevicePath ('{4}') in the package. However, their attributes are different.", type, source, destination, fileEntry.SourcePath, fileEntry.FileType);
				}
				LogUtil.Warning("Ignoring addition of duplicate file '{0}'. Please make sure that this is expected.", destination);
			}
			if (fileEntry == null)
			{
				FileEntry item = new FileEntry(type, destination, attributes, source, sourcePackage, embedSignCategory);
				_allFiles.Add(item);
			}
		}

		public void AddFile(FileType type, string source, string destination, FileAttributes attributes, string sourcePackage, string cabPath, string embedSignCategory)
		{
			if (!LongPathFile.Exists(source))
			{
				throw new PackageException("File {0} doesn't exist", source);
			}
			if (string.IsNullOrEmpty(destination))
			{
				throw new PackageException("DevicePath can't be empty");
			}
			if (destination.Contains("\\.\\") || destination.Contains("\\..\\") || destination.Contains("\\\\"))
			{
				throw new PackageException("DevicePath can't contain  \\.\\ or \\..\\ or \\\\");
			}
			if (destination.Length > PkgConstants.c_iMaxDevicePath)
			{
				throw new PackageException("DevicePath can't exceed {0} characters", PkgConstants.c_iMaxDevicePath);
			}
			if (!Path.IsPathRooted(destination))
			{
				throw new PackageException("Only absolute path is allowed in DevicePath: '{0}'", destination);
			}
			if (!string.IsNullOrEmpty(sourcePackage) && sourcePackage.Length > PkgConstants.c_iMaxPackageName)
			{
				throw new PackageException("SourcePackage can't be longer than {0} characters", PkgConstants.c_iMaxPackageName);
			}
			if (base.IsBinaryPartition)
			{
				throw new PackageException("File with device path '{0}' can not be added since the package contains a BinaryPartition file ", destination);
			}
			switch (type)
			{
			case FileType.Regular:
			{
				string text = Array.Find(PkgConstants.c_strSpecialFolders, (string x) => destination.StartsWith(x, StringComparison.InvariantCultureIgnoreCase));
				if (text != null)
				{
					throw new PackageException("Failed to add file (Type: '{0}', SourcePath: '{1}', DevicePath: '{2}'): regular files are not allowed in folder '{3}' on device", type, source, destination, text);
				}
				break;
			}
			case FileType.Registry:
			case FileType.RegistryMultiStringAppend:
				if (!PkgConstants.c_strHivePartitions.Contains(base.Partition, StringComparer.InvariantCultureIgnoreCase))
				{
					throw new PackageException("Failed to add registry file '{0}' to partition '{1}': only the listed partitions can have registry hives: {2}", destination, base.Partition, string.Join(",", PkgConstants.c_strHivePartitions));
				}
				break;
			}
			FileAttributes fileAttributes = attributes & ~PkgConstants.c_validAttributes;
			if (fileAttributes != 0)
			{
				throw new PackageException("Failed to add file (Type: '{0}', SourcePath: '{1}', DevicePath: '{2}'): invalid attributes '{3}' specified", type, source, destination, fileAttributes);
			}
			FileEntry fileEntry = _allFiles.Find((FileEntry x) => string.Compare(x.DevicePath, destination, StringComparison.OrdinalIgnoreCase) == 0);
			if (fileEntry != null)
			{
				if (!source.Equals(fileEntry.SourcePath, StringComparison.InvariantCultureIgnoreCase) && !File.ReadAllBytes(source).SequenceEqual(File.ReadAllBytes(fileEntry.SourcePath)))
				{
					throw new PackageException("Failed to add file (Type: '{0}', SourcePath: '{1}', DevicePath: '{2}'): there is already a '{3}' file with same DevicePath ('{4}') in the package. However, their source paths point to different files.", type, source, destination, fileEntry.SourcePath, fileEntry.FileType);
				}
				if (!embedSignCategory.Equals(fileEntry.EmbeddedSigningCategory))
				{
					throw new PackageException("Failed to add file (Type: '{0}', SourcePath: '{1}', DevicePath: '{2}'): there is already a '{3}' file with same DevicePath ('{4}') in the package. However, their signing options are different.", type, source, destination, fileEntry.SourcePath, fileEntry.FileType);
				}
				if (attributes != fileEntry.Attributes)
				{
					throw new PackageException("Failed to add file (Type: '{0}', SourcePath: '{1}', DevicePath: '{2}'): there is already a '{3}' file with same DevicePath ('{4}') in the package. However, their attributes are different.", type, source, destination, fileEntry.SourcePath, fileEntry.FileType);
				}
				LogUtil.Warning("Ignoring addition of duplicate file '{0}'. Please make sure that this is expected.", destination);
			}
			if (fileEntry == null)
			{
				FileEntry fileEntry2 = new FileEntry(type, destination, attributes, source, sourcePackage, embedSignCategory);
				fileEntry2.CabPath = cabPath;
				_allFiles.Add(fileEntry2);
			}
		}

		public void RemoveFile(string destination)
		{
			FileEntry fileEntry = _allFiles.Find((FileEntry x) => string.Compare(x.DevicePath, destination, StringComparison.OrdinalIgnoreCase) == 0);
			if (fileEntry == null)
			{
				throw new PackageException("Failed to remove file with device path '{0}': file is not in the package yet", destination);
			}
			if (fileEntry.FileType == FileType.Manifest || fileEntry.FileType == FileType.Catalog)
			{
				throw new PackageException("Failed to remove file with device path '{0}': file wtih type '{0}' cannot be removed", destination, fileEntry.FileType);
			}
			_allFiles.Remove(fileEntry);
		}

		public void RemoveAllFiles()
		{
			_allFiles.RemoveAll((FileEntry x) => !IsGeneratedFileType(x.FileType));
		}

		public new IFileEntry FindFile(string devicePath)
		{
			return _allFiles.Find((FileEntry x) => string.Compare(x.DevicePath, devicePath, StringComparison.OrdinalIgnoreCase) == 0);
		}

		public void SetIsRemoval(bool isRemoval)
		{
			m_pkgManifest.IsRemoval = isRemoval;
		}

		public void SetPkgFileSigner(IPkgFileSigner signer)
		{
			_signer = signer;
		}

		public void SaveCab(string cabPath)
		{
			SaveCab(cabPath, true, PackageStyle.SPKG);
		}

		public void SaveCab(string cabPath, PackageStyle outputStyle)
		{
			SaveCab(cabPath, true, outputStyle);
		}

		public void SaveCab(string cabPath, bool compress)
		{
			SaveCab(cabPath, compress ? Package.DefaultCompressionType : CompressionType.None, PackageStyle.SPKG);
		}

		public void SaveCab(string cabPath, bool compress, PackageStyle outputStyle)
		{
			SaveCab(cabPath, compress ? Package.DefaultCompressionType : CompressionType.None, outputStyle);
		}

		public void SaveCab(string cabPath, CompressionType compressionType)
		{
			SaveCab(cabPath, compressionType, PackageStyle.SPKG);
		}

		public void SaveCab(string cabPath, CompressionType compressionType, PackageStyle outputStyle)
		{
			if (outputStyle == PackageStyle.CBS)
			{
				SaveCBS(cabPath, compressionType);
				_manifestEntry.DevicePath = Path.Combine("\\", PkgConstants.c_strMumFile);
				_manifestEntry.CabPath = PkgConstants.c_strMumFile;
				_manifestEntry.Attributes = PkgConstants.c_defaultAttributes & ~FileAttributes.Compressed;
				_catalogEntry.DevicePath = "\\Windows\\System32\\catroot\\{F750E6C3-38EE-11D1-85E5-00C04FC295EE}\\update.cat";
				_catalogEntry.CabPath = PkgConstants.c_strCBSCatalogFile;
				_catalogEntry.SourcePath = GetTempFile() + "_" + PkgConstants.c_strCatalogFile;
				_allFiles.ForEach(delegate(FileEntry x)
				{
					x.CabPath = x.CabPath.TrimStart('\\');
				});
			}
			else
			{
				ValidateRegistry();
				_allFiles.ForEach(delegate(FileEntry x)
				{
					x.CabPath = null;
				});
				_manifestEntry.DevicePath = Path.Combine(PkgConstants.c_strDsmDeviceFolder, base.Name + PkgConstants.c_strDsmExtension);
				_manifestEntry.CabPath = PkgConstants.c_strDsmFile;
				_manifestEntry.Attributes = PkgConstants.c_defaultAttributes & ~FileAttributes.Compressed;
				if (_manifestEntry.SourcePath == null)
				{
					_manifestEntry.SourcePath = GetTempFile() + "_" + PkgConstants.c_strDsmFile;
				}
				_catalogEntry.DevicePath = Path.Combine(PkgConstants.c_strCatalogDeviceFolder, base.Name + PkgConstants.c_strCatalogFileExtension);
				_catalogEntry.CabPath = PkgConstants.c_strCatalogFile;
				if (_catalogEntry.SourcePath == null)
				{
					_catalogEntry.SourcePath = GetTempFile() + "_" + PkgConstants.c_strCatalogFile;
				}
				BuildCabPaths();
				SaveManifest();
			}
			IEnumerable<FileEntry> source = _allFiles.Where((FileEntry x) => x.FileType != FileType.Catalog);
			string[] sourcePaths = source.Select((FileEntry x) => x.SourcePath).ToArray();
			string[] devicePaths = source.Select((FileEntry x) => x.DevicePath).ToArray();
			try
			{
				PackageTools.CreateCatalog(sourcePaths, devicePaths, base.Name, _catalogEntry.SourcePath);
			}
			catch
			{
				LogUtil.Error("Failed to create catalog file for package {0}", base.Name);
				throw;
			}
			if (outputStyle == PackageStyle.SPKG)
			{
				_catalogEntry.CalculateFileSizes();
				_manifestEntry.CalculateFileSizes();
				SaveManifest();
			}
			List<string> list = new List<string>();
			foreach (FileEntry item in _allFiles.Where((FileEntry x) => !x.EmbeddedSigningCategory.Equals("None", StringComparison.OrdinalIgnoreCase)))
			{
				string tempFile = GetTempFile();
				File.Copy(item.SourcePath, tempFile);
				list.Add(tempFile);
				try
				{
					_signer.SignFileWithOptions(tempFile, item.EmbeddedSigningCategory);
				}
				catch (PackageException innerException)
				{
					throw new PackageException(innerException, "Unable to sign file {0} with options {1}", item.SourcePath, item.EmbeddedSigningCategory);
				}
				item.SourcePath = tempFile;
			}
			try
			{
				PackageTools.CreateCatalog(sourcePaths, devicePaths, base.Name, _signer, _catalogEntry.SourcePath);
			}
			catch
			{
				LogUtil.Error("Failed to create catalog file for package {0}", base.Name);
				throw;
			}
			CabArchiver cab = new CabArchiver();
			_allFiles.ForEach(delegate(FileEntry x)
			{
				cab.AddFile(x.CabPath, x.SourcePath);
			});
			cab.Save(cabPath, compressionType);
			try
			{
				_signer.SignFile(cabPath);
			}
			catch (Exception innerException2)
			{
				throw new PackageException(innerException2, "Failed to sign generated package: {0}", cabPath);
			}
			finally
			{
				list.ForEach(delegate(string x)
				{
					File.Delete(x);
				});
			}
			GC.KeepAlive(this);
		}

		public void SaveCBS(string cabPath, CompressionType compressionType)
		{
			string value = (string.IsNullOrEmpty(m_pkgManifest.Culture) ? "neutral" : m_pkgManifest.Culture);
			string value2 = (string.IsNullOrEmpty(m_pkgManifest.PublicKey) ? PkgConstants.c_strCBSPublicKey : m_pkgManifest.PublicKey);
			XNamespace xNamespace = "urn:schemas-microsoft-com:asm.v3";
			XElement xElement = new XElement(xNamespace + "assembly", new XAttribute("manifestVersion", "1.0"), new XAttribute("description", m_pkgManifest.Name), new XAttribute("displayName", m_pkgManifest.Name), new XAttribute("company", "Microsoft Corporation"), new XAttribute("copyright", "Microsoft Corporation"));
			XElement content = new XElement(xNamespace + "assemblyIdentity", new XAttribute("name", m_pkgManifest.Name + "-Package"), new XAttribute("version", m_pkgManifest.Version), new XAttribute("processorArchitecture", m_pkgManifest.CpuString()), new XAttribute("language", value), new XAttribute("buildType", m_pkgManifest.BuildType), new XAttribute("publicKeyToken", value2), new XAttribute("versionScope", "nonSxS"));
			XElement xElement2 = new XElement(xNamespace + "package", new XAttribute("identifier", m_pkgManifest.Name), new XAttribute("releaseType", "Feature Pack"));
			xElement.Add(content);
			XElement xElement3 = new XElement(xNamespace + "customInformation");
			XElement content2 = new XElement(xNamespace + "phoneInformation", new XAttribute("phoneRelease", m_pkgManifest.ReleaseType), new XAttribute("phoneOwner", m_pkgManifest.Owner), new XAttribute("phoneOwnerType", m_pkgManifest.OwnerType), new XAttribute("phoneComponent", m_pkgManifest.Component), new XAttribute("phoneSubComponent", m_pkgManifest.SubComponent), new XAttribute("phoneGroupingKey", m_pkgManifest.GroupingKey));
			xElement3.Add(content2);
			XElement content3 = new XElement(xNamespace + "file", new XAttribute("name", "\\" + PkgConstants.c_strMumFile), new XAttribute("size", 0), new XAttribute("staged", 0), new XAttribute("compressed", 0), new XAttribute("sourcePackage", m_pkgManifest.Name), new XAttribute("embeddedSign", ""), new XAttribute("keyform", ""), new XAttribute("cabpath", PkgConstants.c_strMumFile));
			xElement3.Add(content3);
			content3 = new XElement(xNamespace + "file", new XAttribute("name", "\\" + PkgConstants.c_strCBSCatalogFile), new XAttribute("size", 0), new XAttribute("staged", 0), new XAttribute("compressed", 0), new XAttribute("sourcePackage", m_pkgManifest.Name), new XAttribute("embeddedSign", ""), new XAttribute("keyform", ""), new XAttribute("cabpath", PkgConstants.c_strCBSCatalogFile));
			xElement3.Add(content3);
			foreach (FileEntry allFile in _allFiles)
			{
				if (allFile.DevicePath == null)
				{
					continue;
				}
				if (allFile.FileType == FileType.Manifest)
				{
					if (Path.GetExtension(allFile.DevicePath) == PkgConstants.c_strMumExtension)
					{
						continue;
					}
					XDocument xDocument = XDocument.Load(allFile.SourcePath);
					if (xDocument.Root.Descendants(xDocument.Root.Name.Namespace + "deployment").Count() != 0)
					{
						XElement xElement4 = xDocument.Root.Element(xDocument.Root.Name.Namespace + "assemblyIdentity");
						string text = xElement4.Attribute("name").Value;
						if (text.LastIndexOf("-Deployment", StringComparison.OrdinalIgnoreCase) != -1)
						{
							string text2 = text;
							text = text2.Remove(text2.LastIndexOf("-Deployment", StringComparison.OrdinalIgnoreCase));
						}
						else if (text.LastIndexOf(".Deployment", StringComparison.OrdinalIgnoreCase) != -1)
						{
							string text3 = text;
							text = text3.Remove(text3.LastIndexOf(".Deployment", StringComparison.OrdinalIgnoreCase));
						}
						XElement xElement5 = new XElement(xNamespace + "update", new XAttribute("name", text));
						XElement xElement6 = new XElement(xNamespace + "package", new XAttribute("contained", "false"), new XAttribute("integrate", "hidden"));
						XElement content4 = new XElement(xNamespace + "assemblyIdentity", new XAttribute("buildType", xElement4.Attribute("buildType").Value), new XAttribute("name", text + "-Package"), new XAttribute("version", xElement4.Attribute("version").Value), new XAttribute("language", xElement4.Attribute("language").Value), new XAttribute("processorArchitecture", xElement4.Attribute("processorArchitecture").Value), new XAttribute("publicKeyToken", xElement4.Attribute("publicKeyToken").Value));
						xElement6.Add(content4);
						xElement5.Add(xElement6);
						xElement2.Add(xElement5);
					}
				}
				else if (allFile.FileType == FileType.Catalog && Path.GetFileName(allFile.DevicePath).Equals("update.cat", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				content3 = new XElement(xNamespace + "file", new XAttribute("name", allFile.DevicePath), new XAttribute("size", allFile.Size), new XAttribute("staged", allFile.StagedSize), new XAttribute("compressed", allFile.CompressedSize), new XAttribute("sourcePackage", allFile.SourcePackage), new XAttribute("embeddedSign", allFile.EmbeddedSigningCategory), new XAttribute("keyform", Path.GetDirectoryName(allFile.CabPath).TrimStart('\\')), new XAttribute("cabpath", allFile.CabPath.TrimStart('\\')));
				xElement3.Add(content3);
			}
			xElement2.AddFirst(xElement3);
			xElement.Add(xElement2);
			XDocument xDocument2 = new XDocument(xElement);
			string text4 = GetTempFile() + "_" + PkgConstants.c_strDsmFile;
			xDocument2.Save(text4);
			_manifestEntry.SourcePath = text4;
		}

		public void SaveCBSR(string cabPath, CompressionType compressionType)
		{
			if (m_pkgManifest.IsBinaryPartition)
			{
				throw new PackageException("Cannot remove binary partitions. Tried to remove: " + m_pkgManifest.Partition);
			}
			string value = (string.IsNullOrEmpty(m_pkgManifest.PublicKey) ? PkgConstants.c_strCBSPublicKey : m_pkgManifest.PublicKey);
			_manifestEntry.DevicePath = Path.Combine(PkgConstants.c_strDsmDeviceFolder, base.Name + PkgConstants.c_strMumExtension);
			_manifestEntry.CabPath = PkgConstants.c_strMumFile;
			_manifestEntry.Attributes = PkgConstants.c_defaultAttributes & ~FileAttributes.Compressed;
			_allFiles.Clear();
			_allFiles.Add(_manifestEntry);
			if (_manifestEntry.SourcePath == null)
			{
				_manifestEntry.SourcePath = GetTempFile() + "_" + PkgConstants.c_strDsmFile;
			}
			_catalogEntry.DevicePath = Path.Combine(PkgConstants.c_strCatalogDeviceFolder, base.Name + PkgConstants.c_strCatalogFileExtension);
			_catalogEntry.CabPath = PkgConstants.c_strCBSCatalogFile;
			if (_catalogEntry.SourcePath == null)
			{
				_catalogEntry.SourcePath = GetTempFile() + "_" + PkgConstants.c_strCBSCatalogFile;
			}
			string value2 = m_pkgManifest.Culture;
			if (string.IsNullOrEmpty(value2))
			{
				value2 = "neutral";
			}
			VersionInfo version = m_pkgManifest.Version;
			XNamespace xNamespace = "urn:schemas-microsoft-com:cbs";
			XElement xElement = new XElement(xNamespace + "assembly", new XAttribute("manifestVersion", "1.0"), new XAttribute("description", m_pkgManifest.Name + ".Recall"), new XAttribute("displayName", m_pkgManifest.Name + " Recall"), new XAttribute("company", "Microsoft Corporation"), new XAttribute("copyright", "Microsoft Corporation"));
			XElement content = new XElement(xNamespace + "assemblyIdentity", new XAttribute("name", m_pkgManifest.Name + ".Recall"), new XAttribute("version", version), new XAttribute("language", value2), new XAttribute("processorArchitecture", m_pkgManifest.CpuString()), new XAttribute("buildType", "release"), new XAttribute("publicKeyToken", value));
			XElement xElement2 = new XElement(xNamespace + "package", new XAttribute("identifier", m_pkgManifest.Name), new XAttribute("releaseType", "Hotfix"), new XAttribute("restart", "possible"), new XAttribute("targetPartition", m_pkgManifest.Partition), new XAttribute("binaryPartition", "false"));
			XElement xElement3 = new XElement(xNamespace + "recall");
			XElement content2 = new XElement(xNamespace + "assemblyIdentity", new XAttribute("name", m_pkgManifest.Name), new XAttribute("version", version), new XAttribute("language", value2), new XAttribute("processorArchitecture", m_pkgManifest.CpuString()), new XAttribute("publicKeyToken", value));
			XElement xElement4 = new XElement(xNamespace + "customInformation");
			XElement content3 = new XElement(xNamespace + "phoneInformation", new XAttribute("phoneRelease", m_pkgManifest.ReleaseType), new XAttribute("phoneOwner", m_pkgManifest.Owner), new XAttribute("phoneOwnerType", m_pkgManifest.OwnerType), new XAttribute("phoneComponent", m_pkgManifest.Component), new XAttribute("phoneSubComponent", m_pkgManifest.SubComponent), new XAttribute("phoneGroupingKey", m_pkgManifest.GroupingKey));
			xElement4.AddFirst(new XElement(xNamespace + "file", new XAttribute("name", "\\" + PkgConstants.c_strMumFile), new XAttribute("size", 0), new XAttribute("staged", 0), new XAttribute("compressed", 0), new XAttribute("sourcePackage", m_pkgManifest.Name), new XAttribute("embeddedSign", "none"), new XAttribute("cabPath", PkgConstants.c_strMumFile)));
			xElement4.AddFirst(new XElement(xNamespace + "file", new XAttribute("name", "\\Windows\\System32\\catroot\\{F750E6C3-38EE-11D1-85E5-00C04FC295EE}\\update.cat"), new XAttribute("size", 0), new XAttribute("staged", 0), new XAttribute("compressed", 0), new XAttribute("sourcePackage", m_pkgManifest.Name), new XAttribute("embeddedSign", "none"), new XAttribute("cabPath", PkgConstants.c_strCBSCatalogFile)));
			xElement4.AddFirst(content3);
			xElement3.Add(content2);
			xElement2.Add(xElement4);
			xElement2.Add(xElement3);
			xElement.Add(content);
			xElement.Add(xElement2);
			XDocument xDocument = new XDocument(xElement);
			SaveManifest();
			xDocument.Save(_manifestEntry.SourcePath, SaveOptions.None);
			FileEntry[] source = new FileEntry[1] { _manifestEntry };
			string[] sourcePaths = source.Select((FileEntry x) => x.SourcePath).ToArray();
			string[] devicePaths = source.Select((FileEntry x) => x.DevicePath).ToArray();
			try
			{
				PackageTools.CreateCatalog(sourcePaths, devicePaths, base.Name, _catalogEntry.SourcePath);
			}
			catch
			{
				LogUtil.Error("Failed to create catalog file for package {0}", base.Name);
				throw;
			}
			_catalogEntry.CalculateFileSizes();
			_manifestEntry.CalculateFileSizes();
			SaveManifest();
			xDocument.Save(_manifestEntry.SourcePath, SaveOptions.None);
			try
			{
				PackageTools.CreateCatalog(sourcePaths, devicePaths, base.Name, _catalogEntry.SourcePath);
			}
			catch
			{
				LogUtil.Error("Failed to create catalog file for package {0}", base.Name);
				throw;
			}
			CabArchiver cabArchiver = new CabArchiver();
			cabArchiver.AddFile(_manifestEntry.CabPath, _manifestEntry.SourcePath);
			cabArchiver.AddFile(_catalogEntry.CabPath, _catalogEntry.SourcePath);
			cabArchiver.Save(cabPath, compressionType);
			try
			{
				_signer.SignFile(cabPath);
			}
			catch (Exception innerException)
			{
				throw new PackageException(innerException, "Failed to sign generated package: {0}", cabPath);
			}
		}

		~PkgBuilder()
		{
			Cleanup();
		}

		public void Dispose()
		{
			Cleanup();
			GC.SuppressFinalize(this);
		}

		internal PkgBuilder()
			: this(new PkgManifest())
		{
		}

		internal PkgBuilder(PkgManifest pkgManifest)
			: this(pkgManifest, null)
		{
		}

		internal PkgBuilder(PkgManifest pkgManifest, string tmpDir)
			: base(pkgManifest)
		{
			_tmpDir = tmpDir;
			_allFiles.Insert(0, _manifestEntry);
			_allFiles.Insert(0, _catalogEntry);
		}

		internal static PkgBuilder Load(string cabPath)
		{
			string tempDirectory = FileUtils.GetTempDirectory();
			WPCanonicalPackage wPCanonicalPackage = WPCanonicalPackage.ExtractAndLoad(cabPath, tempDirectory);
			PkgBuilder pkgBuilder = new PkgBuilder(wPCanonicalPackage.Manifest, tempDirectory);
			pkgBuilder._allFiles.AddRange(wPCanonicalPackage.Files.Where((IFileEntry x) => !IsGeneratedFileType(x.FileType, pkgBuilder.PackageStyle, Path.GetFileName(x.CabPath))).Cast<FileEntry>());
			return pkgBuilder;
		}
	}
}
