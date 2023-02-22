using System;
using System.Collections.Generic;
using Microsoft.Phone.Test.TestMetadata.Helper;
using Microsoft.Phone.Test.TestMetadata.ObjectModel;
using Microsoft.Tools.IO;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools;

namespace Microsoft.Phone.Test.TestMetadata.Commands
{
	internal class PackageFile : IBinaryDependencyParent
	{
		public string Name { get; private set; }

		public bool IsExtractable { get; private set; }

		public bool IsBinary { get; private set; }

		public bool IsProjectFile => Package.IsProjectPackage;

		public string CabPath { get; private set; }

		public string Partition => Package.Partition;

		public HashSet<string> FilePaths { get; private set; }

		public Package Package { get; private set; }

		public HashSet<Dependency> ExplicitDependencies { get; private set; }

		public PackageFileRepository PackageFileRepository => Package.PackageFileRepository;

		public PackageFile(FileEntry fileEntry, string extractRoot, Package package, bool delayLoad = false)
		{
			string path = fileEntry.DevicePath.ToLowerInvariant();
			path = RemoveLeadingSlash(path);
			Name = LongPathPath.GetFileName(path);
			string extractPath = LongPathPath.Combine(extractRoot, fileEntry.CabPath);
			Package = package;
			CabPath = fileEntry.CabPath;
			FilePaths = new HashSet<string>();
			ExplicitDependencies = new HashSet<Dependency>();
			SetFileFlags();
			if (!delayLoad)
			{
				ExtractFile(extractRoot);
			}
			if (IsBinary)
			{
				package.PackageFileRepository.AddFile(this, extractPath);
			}
		}

		public void LoadDependencyMetadata(string metadataFile)
		{
			ExplicitDependency.Load(metadataFile, ExplicitDependencies);
		}

		public void ExtractFile(string extractPath)
		{
			if (IsExtractable)
			{
				CabApiWrapper.ExtractOne(Package.PackageFilePath, extractPath, CabPath);
			}
		}

		private void SetFileFlags()
		{
			switch (GetFileExtension(Name))
			{
			case ".dll":
			case ".exe":
			case ".sys":
				IsBinary = true;
				IsExtractable = IsProjectFile;
				break;
			case ".meta.xml":
				IsExtractable = IsProjectFile;
				break;
			}
		}

		private static string RemoveLeadingSlash(string path)
		{
			if (!path.StartsWith("\\", StringComparison.OrdinalIgnoreCase))
			{
				return path;
			}
			return path.Substring(1);
		}

		private static string GetFileExtension(string fileName)
		{
			string result = LongPathPath.GetExtension(fileName);
			string extension = LongPathPath.GetExtension(LongPathPath.GetFileNameWithoutExtension(fileName));
			if (!string.IsNullOrEmpty(extension) && ".meta".Equals(extension, StringComparison.OrdinalIgnoreCase))
			{
				result = ".meta.xml";
			}
			return result;
		}

		public void ResolveDependency(HashSet<Dependency> dependencies, HashSet<Package> packageWorkingSet, HashSet<PackageFile> fileWorkingSet, bool allDependencies, HashSet<string> packageAnalysisCSV)
		{
			if (fileWorkingSet.Contains(this))
			{
				return;
			}
			fileWorkingSet.Add(this);
			foreach (string filePath in FilePaths)
			{
				foreach (PortableExecutableDependency item in BinaryFile.GetDependency(filePath))
				{
					ResolveDependency(dependencies, packageWorkingSet, item, allDependencies, packageAnalysisCSV);
				}
			}
			ResolveExplicitDependency(dependencies, packageWorkingSet, allDependencies, packageAnalysisCSV);
		}

		private void ResolveExplicitDependency(HashSet<Dependency> dependencies, HashSet<Package> packageWorkingSet, bool allDependencies, HashSet<string> packageAnalysisCSV)
		{
			foreach (PortableExecutableDependency item in ExplicitDependency.FileDependcyList(ExplicitDependencies))
			{
				packageAnalysisCSV.Add($"{Package.Name},{item.Name},,ExplicitAtFileLevel,{Name},");
				ResolveDependency(dependencies, packageWorkingSet, item, allDependencies, packageAnalysisCSV);
			}
			foreach (PackageDependency item2 in ExplicitDependency.PackageDependencyList(ExplicitDependencies))
			{
				if (Package.PackageFileRepository.ContainsPackage(Partition, item2.Name))
				{
					packageAnalysisCSV.Add($"{Package.Name},,{item2.Name},ExplicitAtFileLevel,{Name},");
					Package.PackageFileRepository.GetPackage(Partition, item2.Name).ResolveDependency(dependencies, packageWorkingSet, allDependencies, packageAnalysisCSV);
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
					packageAnalysisCSV.Add($"{Package.Name},{portableExecutableDependency.Name},{package.Name},PEDependency,{Name},");
				}
				package.ResolveDependency(dependencies, packageWorkingSet, allDependencies, packageAnalysisCSV);
			}
		}
	}
}
