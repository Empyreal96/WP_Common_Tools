using System.Collections.Generic;
using System.Linq;
using Microsoft.Phone.Test.TestMetadata.Helper;

namespace Microsoft.Phone.Test.TestMetadata.Commands
{
	internal class PackageFileRepository
	{
		private readonly Dictionary<string, Dictionary<string, Dictionary<string, PackageFile>>> _fileTable = new Dictionary<string, Dictionary<string, Dictionary<string, PackageFile>>>();

		private readonly Dictionary<string, Dictionary<string, Package>> _packageTable = new Dictionary<string, Dictionary<string, Package>>();

		private DependencySuppression depSuppress;

		public Dictionary<string, Dictionary<string, Package>> Packages
		{
			get
			{
				lock (this)
				{
					return _packageTable;
				}
			}
		}

		public string TestRoot { get; internal set; }

		public PackageFileRepository(string testRoot)
		{
			TestRoot = testRoot;
		}

		public void AddFile(PackageFile file, string extractPath)
		{
			lock (this)
			{
				if (!_fileTable.ContainsKey(file.Partition))
				{
					_fileTable.Add(file.Partition, new Dictionary<string, Dictionary<string, PackageFile>>());
				}
				Dictionary<string, Dictionary<string, PackageFile>> dictionary = _fileTable[file.Partition];
				if (!dictionary.ContainsKey(file.Name))
				{
					dictionary.Add(file.Name, new Dictionary<string, PackageFile>());
				}
				Dictionary<string, PackageFile> dictionary2 = dictionary[file.Name];
				if (!dictionary2.ContainsKey(file.Package.Name))
				{
					dictionary2.Add(file.Package.Name, file);
				}
				dictionary2[file.Package.Name].FilePaths.Add(extractPath);
			}
		}

		public bool ContainsFile(string partition, string fileName)
		{
			lock (this)
			{
				partition = partition.ToLowerInvariant();
				fileName = fileName.ToLowerInvariant();
				if (!_fileTable.ContainsKey(partition))
				{
					return false;
				}
				if (!_fileTable[partition].ContainsKey(fileName))
				{
					return false;
				}
				return true;
			}
		}

		public List<PackageFile> GetFile(string partition, string fileName)
		{
			lock (this)
			{
				partition = partition.ToLowerInvariant();
				fileName = fileName.ToLowerInvariant();
				return _fileTable[partition][fileName].Values.ToList();
			}
		}

		public bool IsFileSupressed(string partitionName, string sourceName, string targetName)
		{
			return depSuppress.IsFileSupressed(partitionName, sourceName, targetName);
		}

		public bool IsPackageSupressed(string partitionName, string packageName)
		{
			return depSuppress.IsPackageSupressed(partitionName, packageName);
		}

		public void LoadSupressionFile(string suppressionFile)
		{
			depSuppress = new DependencySuppression(suppressionFile);
		}

		public void AddPackage(Package package)
		{
			lock (this)
			{
				if (!_packageTable.ContainsKey(package.Partition))
				{
					_packageTable[package.Partition] = new Dictionary<string, Package>();
				}
				Dictionary<string, Package> dictionary = _packageTable[package.Partition];
				if (!dictionary.ContainsKey(package.Name))
				{
					dictionary[package.Name] = package;
				}
			}
		}

		public bool ContainsPackage(string partition, string packageName)
		{
			lock (this)
			{
				if (!_packageTable.ContainsKey(partition))
				{
					return false;
				}
				if (!_packageTable[partition].ContainsKey(packageName))
				{
					return false;
				}
				return true;
			}
		}

		public Package GetPackage(string partition, string packageName)
		{
			lock (this)
			{
				return _packageTable[partition][packageName];
			}
		}
	}
}
