using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Phone.Test.TestMetadata.Helper;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	public class PackageSetBuilder : PackageSetBuilderBase, IPackageSetBuilder
	{
		private bool _doCompression;

		private List<KeyValuePair<SatelliteId, RegValueInfo>> _allValues = new List<KeyValuePair<SatelliteId, RegValueInfo>>();

		private RgaBuilder _rgaBuilder = new RgaBuilder();

		private HashSet<string> binaryDependencySet = new HashSet<string>();

		private HashSet<string> environmentPathDependencySet = new HashSet<string>();

		private string spkgMetaFile = string.Empty;

		private string _spkgOutputFile;

		private bool _isRazzleEnv;

		private bool WhetherRazzleEnv()
		{
			return _isRazzleEnv;
		}

		private void SaveManifest(SatelliteId satelliteId, IEnumerable<FileInfo> files, string outputDir)
		{
			using (IPkgBuilder pkgBuilder = CreatePackage(satelliteId))
			{
				string filename = Path.Combine(outputDir, pkgBuilder.Name + PkgConstants.c_strPackageExtension);
				string path = pkgBuilder.Name + ".man.dsm.xml";
				CabApiWrapper.ExtractOne(filename, outputDir, "man.dsm.xml");
				path = Path.Combine(outputDir, path);
				if (LongPathFile.Exists(path))
				{
					LongPathFile.Delete(path);
				}
				LongPathFile.Move(Path.Combine(outputDir, "man.dsm.xml"), path);
				InsertDependencyList(path);
			}
		}

		private void InsertBinaryDependency(XmlDocument manifestXml, XmlNode depNode)
		{
			string path = "pkgdep_supress.txt";
			string text = Path.Combine(LongPath.GetDirectoryName(Assembly.GetExecutingAssembly().Location), path);
			DependencySuppression dependencySuppression = null;
			if (LongPathFile.Exists(text))
			{
				dependencySuppression = new DependencySuppression(text);
			}
			else
			{
				LogUtil.Message("pkgdep_suppress.txt is missing. Ignore it and move on.");
			}
			IEnumerable<string> source = _allFiles.Select((KeyValuePair<SatelliteId, FileInfo> x) => Path.GetFileName(x.Value.SourcePath));
			foreach (string item in binaryDependencySet)
			{
				if ((dependencySuppression == null || !dependencySuppression.IsFileSupressed(item)) && !source.Contains(item))
				{
					XmlElement xmlElement = manifestXml.CreateElement("Binary", manifestXml.DocumentElement.NamespaceURI);
					xmlElement.SetAttribute("Name", item);
					depNode.AppendChild(xmlElement);
				}
			}
		}

		private void InsertEnvrionmentDependency(XmlDocument manifestXml, XmlNode depNode)
		{
			foreach (string item in environmentPathDependencySet)
			{
				XmlElement xmlElement = manifestXml.CreateElement("EnvrionmentPath", manifestXml.DocumentElement.NamespaceURI);
				xmlElement.SetAttribute("Name", item);
				depNode.AppendChild(xmlElement);
			}
		}

		private void InsertExplicitDependency(XmlDocument manifestXml, XmlNode depNode)
		{
			if (!string.IsNullOrEmpty(spkgMetaFile))
			{
				if (!LongPathFile.Exists(spkgMetaFile))
				{
					throw new FileNotFoundException(spkgMetaFile);
				}
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(spkgMetaFile);
				XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
				xmlNamespaceManager.AddNamespace("tm", xmlDocument.DocumentElement.NamespaceURI);
				XmlNode xmlNode = xmlDocument.SelectSingleNode("/tm:Metadata/tm:Dependencies", xmlNamespaceManager);
				if (xmlNode != null)
				{
					string pattern = "xmlns=\"(.*?)\"";
					string text = Regex.Replace(xmlNode.InnerXml, pattern, "", RegexOptions.IgnoreCase);
					depNode.InnerXml += text;
				}
			}
		}

		private XmlNode CreateDependenciesNode(XmlDocument manifestXml)
		{
			XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(manifestXml.NameTable);
			xmlNamespaceManager.AddNamespace("iu", manifestXml.DocumentElement.NamespaceURI);
			string qualifiedName = "Dependencies";
			XmlNode xmlNode = manifestXml.SelectSingleNode("//iu:Package/iu:Dependencies", xmlNamespaceManager);
			if (xmlNode == null)
			{
				XmlElement newChild = manifestXml.CreateElement(qualifiedName, manifestXml.DocumentElement.NamespaceURI);
				xmlNode = manifestXml.DocumentElement.AppendChild(newChild);
			}
			else
			{
				xmlNode.RemoveAll();
			}
			return xmlNode;
		}

		private void InsertDependencyList(string manifestFileName)
		{
			if (string.IsNullOrEmpty(manifestFileName))
			{
				throw new ArgumentNullException("manifestFileName");
			}
			if (!LongPathFile.Exists(manifestFileName))
			{
				throw new InvalidDataException($"File {manifestFileName} does not exist");
			}
			LogUtil.Message("Add dependency info to the manifest file...");
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(manifestFileName);
				XmlNode depNode = CreateDependenciesNode(xmlDocument);
				InsertBinaryDependency(xmlDocument, depNode);
				InsertEnvrionmentDependency(xmlDocument, depNode);
				InsertExplicitDependency(xmlDocument, depNode);
				xmlDocument.Save(manifestFileName);
			}
			catch (Exception ex)
			{
				LogUtil.Message("Error occurred in inserting dependencies to the manifest file.");
				LogUtil.Message(ex.Message);
				throw ex;
			}
		}

		private void BuildPackage(SatelliteId satelliteId, IEnumerable<FileInfo> files, string outputDir)
		{
			using (IPkgBuilder pkgBuilder = CreatePackage(satelliteId))
			{
				bool flag = WhetherRazzleEnv();
				string text = Path.Combine(outputDir, pkgBuilder.Name + PkgConstants.c_strPackageExtension);
				string text2 = PkgConstants.c_strPackageExtension + PkgConstants.c_strCustomMetadataExtension;
				LogUtil.Message("Building package '{0}'", text);
				foreach (FileInfo file in files)
				{
					if (file.Type == FileType.Invalid)
					{
						continue;
					}
					string text3 = file.DevicePath;
					if (text3 == null)
					{
						switch (file.Type)
						{
						case FileType.BinaryPartition:
							text3 = "\\" + pkgBuilder.Partition + ".bin";
							break;
						case FileType.Registry:
							text3 = Path.Combine(PkgConstants.c_strRguDeviceFolder, pkgBuilder.Name + PkgConstants.c_strRguExtension);
							break;
						case FileType.RegistryMultiStringAppend:
							text3 = Path.Combine(PkgConstants.c_strRgaDeviceFolder, pkgBuilder.Name + PkgConstants.c_strRegAppendExtension);
							break;
						case FileType.SecurityPolicy:
							text3 = Path.Combine(PkgConstants.c_strPolicyDeviceFolder, pkgBuilder.Name + PkgConstants.c_strPolicyExtension);
							break;
						case FileType.Certificates:
							text3 = Path.Combine(PkgConstants.c_strCertStoreDeviceFolder, pkgBuilder.Name + PkgConstants.c_strCertStoreExtension);
							break;
						default:
							throw new PkgGenException("DevicePath must be specified for file type file type {0}", file.Type);
						}
					}
					LogUtil.Message("Adding file '{0}' to package '{1}' as '{2}'", file.SourcePath, text, text3);
					pkgBuilder.AddFile(file.Type, file.SourcePath, text3, file.Attributes, null, file.EmbeddedSigningCategory);
					if (!flag || file.Type != FileType.Regular)
					{
						continue;
					}
					string directoryName = LongPath.GetDirectoryName(text3);
					string fileName = Path.GetFileName(file.SourcePath);
					string strA = Path.GetExtension(fileName).ToLowerInvariant();
					string fileName2 = Path.GetFileName(text3);
					string strB = Path.GetFileNameWithoutExtension(text) + text2;
					if (string.Compare(strA, ".dll", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(strA, ".exe", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(strA, ".sys", StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						foreach (PortableExecutableDependency item in BinaryFile.GetDependency(file.SourcePath))
						{
							binaryDependencySet.Add(Path.GetFileName(item.Name));
						}
						if (!directoryName.StartsWith("\\Windows\\", StringComparison.InvariantCultureIgnoreCase))
						{
							environmentPathDependencySet.Add(directoryName);
						}
					}
					else if (string.Compare(fileName2, strB, true, CultureInfo.InvariantCulture) == 0)
					{
						LogUtil.Message("Found spkg meta file, source: '{0}', Destination: '{1}'", fileName, fileName2);
						spkgMetaFile = file.SourcePath;
					}
				}
				pkgBuilder.SaveCab(text, _doCompression);
				LogUtil.Message("Done package \"{0}\"", text);
				if (_spkgOutputFile != null)
				{
					LongPathFile.AppendAllText(_spkgOutputFile, $"{text}{Environment.NewLine}");
				}
			}
		}

		public void AddRegValue(SatelliteId satelliteId, RegValueInfo valueInfo)
		{
			_allValues.Add(new KeyValuePair<SatelliteId, RegValueInfo>(satelliteId, valueInfo));
		}

		public void AddMultiSzSegment(string keyName, string valueName, params string[] valueSegments)
		{
			_rgaBuilder.AddRgaValue(keyName, valueName, valueSegments);
		}

		public void Save(string outputDir)
		{
			string tempDirectory = FileUtils.GetTempDirectory();
			bool flag = WhetherRazzleEnv();
			try
			{
				foreach (IGrouping<SatelliteId, KeyValuePair<SatelliteId, RegValueInfo>> item in from x in _allValues
					group x by x.Key)
				{
					string text = Path.Combine(tempDirectory, "reg" + item.Key.FileSuffix + PkgConstants.c_strRguExtension);
					RegBuilder.Build(item.Select((KeyValuePair<SatelliteId, RegValueInfo> x) => x.Value), text);
					AddFile(item.Key, new FileInfo
					{
						Type = FileType.Registry,
						SourcePath = text,
						DevicePath = null,
						Attributes = (PkgConstants.c_defaultAttributes & ~FileAttributes.Compressed)
					});
				}
				if (_rgaBuilder.HasContent)
				{
					string text2 = Path.Combine(tempDirectory, "regMultiSz" + PkgConstants.c_strRegAppendExtension);
					_rgaBuilder.Save(text2);
					AddFile(SatelliteId.Neutral, new FileInfo
					{
						Type = FileType.RegistryMultiStringAppend,
						SourcePath = text2,
						DevicePath = null,
						Attributes = (PkgConstants.c_defaultAttributes & ~FileAttributes.Compressed)
					});
				}
				foreach (IGrouping<SatelliteId, KeyValuePair<SatelliteId, FileInfo>> item2 in from x in _allFiles
					group x by x.Key)
				{
					BuildPackage(item2.Key, item2.Select((KeyValuePair<SatelliteId, FileInfo> x) => x.Value), outputDir);
					if (flag)
					{
						SaveManifest(item2.Key, item2.Select((KeyValuePair<SatelliteId, FileInfo> x) => x.Value), outputDir);
					}
				}
			}
			finally
			{
				FileUtils.DeleteTree(tempDirectory);
			}
		}

		public PackageSetBuilder(CpuId cpuType, BuildType bldType, VersionInfo version, bool doCompression, bool isRazzleEnv, string spkgOutputFile)
			: base(cpuType, bldType, version)
		{
			_doCompression = doCompression;
			_isRazzleEnv = isRazzleEnv;
			_spkgOutputFile = spkgOutputFile;
		}

		public PackageSetBuilder(CpuId cpuType, BuildType bldType, VersionInfo version, bool doCompression, bool isRazzleEnv)
			: this(cpuType, bldType, version, doCompression, isRazzleEnv, null)
		{
		}
	}
}
