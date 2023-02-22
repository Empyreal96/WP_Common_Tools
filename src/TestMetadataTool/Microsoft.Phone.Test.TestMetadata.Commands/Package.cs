using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Phone.Test.TestMetadata.Helper;
using Microsoft.Phone.Test.TestMetadata.ObjectModel;
using Microsoft.Tools.IO;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools;

namespace Microsoft.Phone.Test.TestMetadata.Commands
{
	internal class Package : IBinaryDependencyParent
	{
		private PkgManifest _packageManifest;

		private string dsmFile;

		private bool dsmFileExist;

		private bool filesLoaded;

		private bool is64BitPackage;

		public PackageFileRepository PackageFileRepository { get; private set; }

		public string Name { get; private set; }

		public string Partition { get; private set; }

		public string PackageFilePath { get; private set; }

		public string ExtractPath { get; private set; }

		public string RelativePath { get; private set; }

		public bool IsProceessed { get; set; }

		public bool IsProjectPackage { get; set; }

		public bool IsSupressed { get; set; }

		public HashSet<PackageFile> PackageFiles { get; private set; }

		public HashSet<Dependency> ResolvedDependencies { get; private set; }

		private HashSet<Dependency> ExplicitDependencies { get; set; }

		public Package(string packageRoot, string extractRoot, string packageFile, bool isProjectPackage, PackageFileRepository packageFileRepository)
		{
			packageRoot = packageRoot.ToLowerInvariant();
			extractRoot = extractRoot.ToLowerInvariant();
			packageFile = packageFile.ToLowerInvariant();
			IsProjectPackage = isProjectPackage;
			PackageFilePath = packageFile;
			IsProceessed = false;
			PackageFiles = new HashSet<PackageFile>();
			ResolvedDependencies = new HashSet<Dependency>();
			ExplicitDependencies = new HashSet<Dependency>();
			PackageFileRepository = packageFileRepository;
			char[] trimChars = new char[1] { '\\' };
			packageRoot = packageRoot.TrimEnd(trimChars);
			RelativePath = packageFile.Replace(string.Format(CultureInfo.InvariantCulture, "{0}\\", new object[1] { packageRoot }), string.Empty);
			ExtractPath = LongPathPath.Combine(extractRoot, RelativePath);
			string directoryName = LongPathPath.GetDirectoryName(PackageFilePath);
			string text = LongPathPath.GetFileNameWithoutExtension(PackageFilePath) + "man.dsm.xml";
			dsmFile = LongPathPath.Combine(directoryName, text);
			dsmFileExist = LongPathFile.Exists(dsmFile);
			is64BitPackage = Is64BitCabPackage();
			LoadPackage(extractRoot);
		}

		public void ResolveDependency(HashSet<Dependency> dependencies, HashSet<Package> packageWorkingSet, bool allDependencies, HashSet<string> packageAnalysisCSV)
		{
			if (!filesLoaded)
			{
				LoadPackageFiles(ExtractPath);
			}
			if (packageWorkingSet.Contains(this) || (!allDependencies && !IsProjectPackage))
			{
				if (!packageWorkingSet.Contains(this) && !IsProjectPackage)
				{
					dependencies.UnionWith(ResolvedDependencies);
				}
				return;
			}
			packageWorkingSet.Add(this);
			dependencies.Add(new PackageDependency(RelativePath));
			AddGuestCabPackage();
			dependencies.UnionWith(ResolvedDependencies);
			if (IsProceessed || !IsProjectPackage)
			{
				return;
			}
			packageAnalysisCSV.Add($"Package,DependentBinary,DependentPackage,DependencyReasonType,DependencyReasonBinary,DependencyReasonPackage");
			HashSet<PackageFile> hashSet = new HashSet<PackageFile>();
			foreach (PackageFile packageFile in PackageFiles)
			{
				List<PackageFile> file = PackageFileRepository.GetFile(packageFile.Partition, packageFile.Name);
				if (file.Count > 1)
				{
					Log.Error("File with name [{0}] found in multiple packages:", packageFile.Name);
					foreach (PackageFile item in file)
					{
						Log.Error("\t{0}", item.Package.Name);
					}
				}
				packageFile.ResolveDependency(dependencies, packageWorkingSet, hashSet, allDependencies, packageAnalysisCSV);
			}
			ResolveExplicitDependency(dependencies, packageWorkingSet, allDependencies, packageAnalysisCSV);
			ResolveWowDependency();
			hashSet.Clear();
		}

		private void ResolveWowDependency()
		{
			if (!is64BitPackage)
			{
				ResolvedDependencies.RemoveWhere((Dependency x) => x is PackageDependency && (x as PackageDependency).Name.EndsWith(".guest.cab", StringComparison.OrdinalIgnoreCase));
				return;
			}
			IEnumerable<PackageDependency> enumerable = from x in ResolvedDependencies
				where x is PackageDependency
				select x as PackageDependency;
			foreach (PackageDependency item in enumerable)
			{
				if (item.Name.EndsWith(".guest.cab"))
				{
					string text = item.Name.Substring(0, item.Name.Length - ".guest.cab".Length);
					string spkgPackageName = text + ".spkg";
					string cabPackageName = text + ".cab";
					if (!enumerable.Any((PackageDependency x) => string.Compare(LongPathPath.GetFileName(x.Name), spkgPackageName, StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(LongPathPath.GetFileName(x.Name), cabPackageName, StringComparison.OrdinalIgnoreCase) == 0))
					{
						Log.Error("{0} was in the resolved dependency list, but its 64-bit counterpart is missing. Adding the 64-bit package.", item.Name);
						if (PackageFileRepository.ContainsPackage(Partition, cabPackageName))
						{
							ResolvedDependencies.Add(new PackageDependency(PackageFileRepository.GetPackage(Partition, cabPackageName).RelativePath));
							continue;
						}
						if (PackageFileRepository.ContainsPackage(Partition, spkgPackageName))
						{
							ResolvedDependencies.Add(new PackageDependency(PackageFileRepository.GetPackage(Partition, spkgPackageName).RelativePath));
							continue;
						}
						Log.Error("The 64-bit {0} package is missing.", item.Name);
					}
				}
				else
				{
					string packageName = LongPathPath.GetFileNameWithoutExtension(item.Name) + LongPathPath.GetFileNameWithoutExtension(".guest.cab");
					if (PackageFileRepository.ContainsPackage(Partition, packageName))
					{
						ResolvedDependencies.Add(new PackageDependency(PackageFileRepository.GetPackage(Partition, packageName).RelativePath));
					}
				}
			}
		}

		public HashSet<Dependency> GetDependenciesFromDepXml()
		{
			string fileNameWithoutExtension = LongPathPath.GetFileNameWithoutExtension(PackageFilePath);
			string text = LongPathPath.Combine(LongPathPath.GetDirectoryName(PackageFilePath), fileNameWithoutExtension + ".dep.xml");
			HashSet<Dependency> hashSet = new HashSet<Dependency>();
			if (!LongPathFile.Exists(text))
			{
				throw new FileNotFoundException(text);
			}
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(text);
			XmlNode documentElement = xmlDocument.DocumentElement;
			XmlNodeList xmlNodeList = documentElement.SelectNodes("//Required/Package");
			string empty = string.Empty;
			foreach (XmlNode item in xmlNodeList)
			{
				empty = item.Attributes["Name"].Value;
				if (!string.IsNullOrEmpty(empty))
				{
					hashSet.Add(new PackageDependency(empty));
				}
			}
			foreach (XmlNode item2 in documentElement.SelectNodes("//Required/EnvironmentPath"))
			{
				empty = item2.Attributes["Name"].Value;
				if (!string.IsNullOrEmpty(empty))
				{
					hashSet.Add(new EnvironmentPathDependnecy(empty));
				}
			}
			return hashSet;
		}

		private void ResolveExplicitDependency(HashSet<Dependency> dependencies, HashSet<Package> packageWorkingSet, bool allDependencies, HashSet<string> packageAnalysisCSV)
		{
			foreach (PortableExecutableDependency item in ExplicitDependency.FileDependcyList(ExplicitDependencies))
			{
				ResolveDependency(dependencies, packageWorkingSet, item, allDependencies, packageAnalysisCSV);
			}
			foreach (PackageDependency item2 in ExplicitDependency.PackageDependencyList(ExplicitDependencies))
			{
				if (PackageFileRepository.ContainsPackage(Partition, item2.Name))
				{
					packageAnalysisCSV.Add(string.Format("{0},,{1},ExplicitAtPackageLevel,,{0}", Name, item2.Name));
					PackageFileRepository.GetPackage(Partition, item2.Name).ResolveDependency(dependencies, packageWorkingSet, allDependencies, packageAnalysisCSV);
				}
				else
				{
					Log.Error("Package {0} [Explicit] referenced by file {1} not found", item2.Name, Name);
				}
			}
			dependencies.UnionWith(ExplicitDependency.RemoteFileDependencyList(ExplicitDependencies));
		}

		private void ResolveDependency(HashSet<Dependency> dependencies, HashSet<Package> packageWorkingSet, PortableExecutableDependency portableExecutableDependency, bool allDependencies, HashSet<string> packageAnalysisCSV)
		{
			Package package = BinaryDependencyResolver.ResolveDependency(portableExecutableDependency, this);
			if (package != null)
			{
				if (package.IsProjectPackage)
				{
					packageAnalysisCSV.Add(string.Format("{0},{1},{2},ExplicitAtPackageLevel,,{0}", Name, portableExecutableDependency.Name, package.Name));
				}
				package.ResolveDependency(dependencies, packageWorkingSet, allDependencies, packageAnalysisCSV);
			}
		}

		private static string GetVirtualGroup(string partitionName)
		{
			string text = partitionName.ToLowerInvariant();
			if (text == "data")
			{
				return "mainos";
			}
			return partitionName.ToLowerInvariant();
		}

		private void LoadPackageFiles(string extractRoot)
		{
			foreach (PackageFile packageFile in PackageFiles)
			{
				packageFile.ExtractFile(extractRoot);
			}
			filesLoaded = true;
		}

		private void LoadPackage(string extractRoot)
		{
			_packageManifest = LoadPackageManifest();
			Name = _packageManifest.Name.ToLowerInvariant();
			Partition = GetVirtualGroup(_packageManifest.Partition.ToLowerInvariant());
			if (PackageFileRepository.IsPackageSupressed(Partition, Name))
			{
				IsSupressed = true;
				return;
			}
			FileEntry[] files = _packageManifest.Files;
			foreach (FileEntry fileEntry in files)
			{
				PackageFile packageFile = new PackageFile(fileEntry, LongPathPath.Combine(extractRoot, RelativePath), this, dsmFileExist);
				filesLoaded = !dsmFileExist;
				if (packageFile.IsBinary)
				{
					PackageFiles.Add(packageFile);
					string text = LongPathPath.GetDirectoryName(fileEntry.DevicePath);
					if (string.IsNullOrEmpty(text))
					{
						text = "\\";
					}
					text = text.ToLowerInvariant();
					if (!text.StartsWith("\\windows\\"))
					{
						ResolvedDependencies.Add(new EnvironmentPathDependnecy
						{
							Name = text
						});
					}
				}
			}
			LoadPackageMetadata();
			foreach (PackageFile packageFile2 in PackageFiles)
			{
				string text2 = packageFile2.Name + ".meta.xml";
				string metadataFile = LongPathPath.Combine(PackageFileRepository.TestRoot, "metadata", text2);
				FileEntry fileEntry2 = _packageManifest.Files.FirstOrDefault((FileEntry fe) => fe.DevicePath.Equals(metadataFile, StringComparison.OrdinalIgnoreCase));
				if (fileEntry2 != null)
				{
					packageFile2.LoadDependencyMetadata(LongPathPath.Combine(ExtractPath, fileEntry2.CabPath));
				}
			}
		}

		private void AddGuestCabPackage()
		{
			if (is64BitPackage)
			{
				string fileNameWithoutExtension = LongPathPath.GetFileNameWithoutExtension(Name + ".guest.cab");
				if (PackageFileRepository.ContainsPackage(Partition, fileNameWithoutExtension))
				{
					Package package = PackageFileRepository.GetPackage(Partition, fileNameWithoutExtension);
					ResolvedDependencies.Add(new PackageDependency(package.RelativePath));
					PackageFiles.UnionWith(package.PackageFiles);
				}
			}
		}

		private void LoadPackageMetadata()
		{
			string text = Name + ".spkg.meta.xml";
			string metadataFile = LongPathPath.Combine(PackageFileRepository.TestRoot, "metadata", text);
			FileEntry fileEntry = _packageManifest.Files.FirstOrDefault((FileEntry fe) => fe.DevicePath.Equals(metadataFile, StringComparison.OrdinalIgnoreCase));
			if (fileEntry != null)
			{
				string text2 = LongPathPath.Combine(ExtractPath, fileEntry.CabPath);
				if (!LongPathFile.Exists(text2))
				{
					CabApiWrapper.ExtractOne(PackageFilePath, ExtractPath, fileEntry.CabPath);
				}
				ExplicitDependency.Load(text2, ExplicitDependencies);
			}
		}

		private PkgManifest LoadPackageManifest()
		{
			PkgManifest pkgManifest = new PkgManifest();
			if (string.Compare(LongPathPath.GetExtension(PackageFilePath), ".spkg", true) == 0)
			{
				if (!dsmFileExist)
				{
					CabApiWrapper.ExtractOne(PackageFilePath, ExtractPath, "man.dsm.xml");
					dsmFile = LongPathPath.Combine(ExtractPath, "man.dsm.xml");
				}
				pkgManifest = PkgManifest.Load(dsmFile);
				if (!dsmFileExist)
				{
					LongPathFile.Delete(dsmFile);
				}
			}
			else
			{
				if (string.Compare(LongPathPath.GetExtension(PackageFilePath), ".cab", true) != 0)
				{
					throw new InvalidDataException($"Unsupport package file extension: {PackageFilePath}");
				}
				pkgManifest = PkgManifest.Load_CBS(PackageFilePath);
			}
			return pkgManifest;
		}

		private bool Is64BitCabPackage()
		{
			try
			{
				CabApiWrapper.ExtractOne(PackageFilePath, ExtractPath, "update.mum");
				string filename = LongPathPath.Combine(ExtractPath, "update.mum");
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(filename);
				XmlElement documentElement = xmlDocument.DocumentElement;
				XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
				xmlNamespaceManager.AddNamespace("iu", xmlDocument.DocumentElement.NamespaceURI);
				XmlNode xmlNode = documentElement.SelectSingleNode("//iu:assembly/iu:assemblyIdentity", xmlNamespaceManager);
				string processorArchitectureVal = xmlNode.Attributes["processorArchitecture"].Value;
				return Constants.ArchitecturesOf64Bit.Any((string x) => string.Compare(processorArchitectureVal, x, StringComparison.OrdinalIgnoreCase) == 0);
			}
			catch (Exception)
			{
				return false;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			Package package = obj as Package;
			if (package.Name.Equals(Name, StringComparison.OrdinalIgnoreCase))
			{
				return package.Partition.Equals(Partition, StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode() ^ Partition.GetHashCode();
		}

		public void Validate()
		{
			PackageValidator.Validate(_packageManifest, PackageFileRepository.TestRoot);
		}
	}
}
