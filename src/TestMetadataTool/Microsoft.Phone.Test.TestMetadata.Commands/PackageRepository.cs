using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Phone.Test.TestMetadata.ObjectModel;
using Microsoft.Tools.IO;
//using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.Phone.Test.TestMetadata.Commands
{
	internal class PackageRepository
	{
		private readonly PackageFileRepository _packageFileRepository;

		private object lockAddingToPackageFileRepository = new object();

		private static Queue queue;

		private static Action QueueSub;

		private int NumberOfLoaders { get; set; }

		public IEnumerable<string> TargetProjects { get; private set; }

		public string PackageRoot { get; private set; }

		public string ExtractRoot { get; private set; }

		internal bool IgnoreLocaleSpecificPackages { get; set; }

		internal bool IgnoreResourceSpecificPackages { get; set; }

		internal bool DepXmlToOutputFolder { get; set; }

		internal HashSet<string> RazzlePkgPaths { get; set; }

		internal string[] PackageRootPaths { get; set; }

		public PackageRepository(string packageRoot, string extractRoot, IEnumerable<string> targetProjects, string supressionFile, string testRoot, bool ignoreLocaleSpecificPackages, bool ignoreResourceSpecificPackages, int numberOfLoaders, bool depXmlToOutputFolder, string razzlePkgPath)
		{
			char[] separator = new char[1] { ';' };
			PackageRootPaths = packageRoot.Split(separator);
			PackageRoot = PackageRootPaths[0].ToLowerInvariant();
			ExtractRoot = extractRoot.ToLowerInvariant();
			TargetProjects = targetProjects.Concat(targetProjects.Select((string x) => LongPathPath.Combine("wow", x)));
			_packageFileRepository = new PackageFileRepository(testRoot);
			if (supressionFile != null)
			{
				_packageFileRepository.LoadSupressionFile(supressionFile);
			}
			IgnoreLocaleSpecificPackages = ignoreLocaleSpecificPackages;
			IgnoreResourceSpecificPackages = ignoreResourceSpecificPackages;
			NumberOfLoaders = numberOfLoaders;
			DepXmlToOutputFolder = depXmlToOutputFolder;
			if (!string.IsNullOrEmpty(razzlePkgPath))
			{
				RazzlePkgPaths = new HashSet<string>(razzlePkgPath.Split(separator));
				return;
			}
			string[] packageRootPaths = PackageRootPaths;
			foreach (string rootPath in packageRootPaths)
			{
				LoadPackages(rootPath);
			}
		}

		private void LoadPackages(string rootPath)
		{
			List<string> list = LongPathDirectory.EnumerateFiles(rootPath, "*.cab", SearchOption.AllDirectories).ToList();
			foreach (string item in LongPathDirectory.EnumerateFiles(rootPath, "*.spkg", SearchOption.AllDirectories))
			{
				string value = LongPathPath.ChangeExtension(item, ".cab");
				if (!list.Contains(value, StringComparer.OrdinalIgnoreCase))
				{
					list.Add(item);
				}
			}
			Log.Message("PERF: NumberOfLoaders @ {0}", NumberOfLoaders);
			Log.Message("PERF: Loading Start @ {0}", DateTime.Now);
			Log.Message("Root Path =  {0}", rootPath);
			QueueSub = delegate
			{
				while (queue.Count > 0)
				{
					try
					{
						string text = (string)queue.Dequeue();
						if (!IsIgnoreProjectProject(text, rootPath) && (!IgnoreLocaleSpecificPackages || !IsLocaleSpecificPackage(text)) && (!IgnoreResourceSpecificPackages || !IsResourceSpecificPackage(text)))
						{
							Log.Message("Tid{1}: Loading Package {0}", text, Thread.CurrentThread.ManagedThreadId.ToString("D2"));
							bool isProjectPackage = IsTargetProject(text, rootPath);
							Package package = new Package(rootPath, ExtractRoot, text, isProjectPackage, _packageFileRepository);
							if (!package.IsSupressed)
							{
								lock (lockAddingToPackageFileRepository)
								{
									if (_packageFileRepository.ContainsPackage(package.Partition, package.Name))
									{
										Log.Error("Tid{1}: Package with same name {0} exists at following locations:", package.Name, Thread.CurrentThread.ManagedThreadId.ToString("D2"));
										Package package2 = _packageFileRepository.GetPackage(package.Partition, package.Name);
										Log.Error("Tid{1}: \t{0}", package2.PackageFilePath, Thread.CurrentThread.ManagedThreadId.ToString("D2"));
										Log.Error("Tid{1}: \t{0}", package.PackageFilePath, Thread.CurrentThread.ManagedThreadId.ToString("D2"));
									}
									_packageFileRepository.AddPackage(package);
								}
							}
							else
							{
								Log.Warning("Tid{1}: Skipping Supressed Package {0}.", package.Name, Thread.CurrentThread.ManagedThreadId.ToString("D2"));
							}
						}
					}
					catch (Exception ex2)
					{
						Log.Error("Tid{1}: {0}", ex2.ToString(), Thread.CurrentThread.ManagedThreadId.ToString("D2"));
					}
				}
			};
			queue = Queue.Synchronized(new Queue());
			foreach (string item2 in list)
			{
				queue.Enqueue(item2);
			}
			List<IAsyncResult> list2 = new List<IAsyncResult>();
			for (int i = 0; i < NumberOfLoaders; i++)
			{
				list2.Add(QueueSub.BeginInvoke(null, null));
			}
			foreach (IAsyncResult item3 in list2)
			{
				try
				{
					QueueSub.EndInvoke(item3);
				}
				catch (Exception ex)
				{
					Log.Error(ex.ToString());
				}
			}
			Log.Message("PERF: Loading Finish @ {0}", DateTime.Now);
		}

		private bool IsTargetProject(string path, string rooPath)
		{
			return TargetProjects.Any((string project) => path.StartsWith(LongPathPath.Combine(rooPath, project), StringComparison.OrdinalIgnoreCase));
		}

		private bool IsIgnoreProjectProject(string path, string rooPath)
		{
			return path.StartsWith(LongPathPath.Combine(rooPath, "merged"), StringComparison.OrdinalIgnoreCase);
		}

		private bool IsLocaleSpecificPackage(string path)
		{
			return path.IndexOf("_Lang_", StringComparison.OrdinalIgnoreCase) != -1;
		}

		private bool IsResourceSpecificPackage(string path)
		{
			return path.IndexOf("_RES_", StringComparison.OrdinalIgnoreCase) != -1;
		}

		public void ResolveDependency(bool allDependencies, string csvFolder)
		{
			foreach (KeyValuePair<string, Dictionary<string, Package>> package in _packageFileRepository.Packages)
			{
				HashSet<Package> hashSet = new HashSet<Package>();
				HashSet<string> hashSet2 = new HashSet<string>();
				foreach (KeyValuePair<string, Package> item in package.Value.Where((KeyValuePair<string, Package> packageItem) => packageItem.Value.IsProjectPackage))
				{
					if (!item.Value.IsProceessed && !item.Value.PackageFilePath.EndsWith(".guest.cab", StringComparison.OrdinalIgnoreCase))
					{
						Log.Message("Processing package {0}.", item.Value.Name);
						item.Value.ResolveDependency(item.Value.ResolvedDependencies, hashSet, allDependencies, hashSet2);
						SavePackageDependency(item.Value);
						SavePackageAnalysisCsv(item.Value, hashSet2, csvFolder);
						item.Value.IsProceessed = true;
						hashSet.Clear();
						hashSet2.Clear();
					}
				}
			}
		}

		public void ResolveDependency(string packageName, bool allDependencies, string csvFolder)
		{
			foreach (KeyValuePair<string, Dictionary<string, Package>> package in _packageFileRepository.Packages)
			{
				HashSet<Package> hashSet = new HashSet<Package>();
				HashSet<string> hashSet2 = new HashSet<string>();
				foreach (KeyValuePair<string, Package> item in package.Value.Where((KeyValuePair<string, Package> packageItem) => packageItem.Value.IsProjectPackage))
				{
					if (!item.Value.IsProceessed && item.Value.Name.Equals(packageName, StringComparison.OrdinalIgnoreCase))
					{
						Log.Message("Processing package {0}.", item.Value.Name);
						item.Value.Validate();
						item.Value.ResolveDependency(item.Value.ResolvedDependencies, hashSet, allDependencies, hashSet2);
						SavePackageDependency(item.Value);
						SavePackageAnalysisCsv(item.Value, hashSet2, csvFolder);
						item.Value.IsProceessed = true;
						hashSet.Clear();
						hashSet2.Clear();
						return;
					}
				}
			}
			Log.Error("Package {0} not found.", packageName);
		}

		public void UpdateDependency(string packageName, bool allDependencies, string csvFolder)
		{
			if (string.IsNullOrEmpty(packageName))
			{
				throw new ArgumentNullException("PackageName");
			}
			IEnumerable<string> source = LongPathDirectory.EnumerateFiles(PackageRoot, packageName + ".spkg", SearchOption.AllDirectories);
			if (!source.Any())
			{
				throw new FileNotFoundException(packageName);
			}
			if (source.Count() > 1)
			{
				throw new InvalidOperationException($"More than one spkg named {packageName} found under the package root.");
			}
			string text = source.ToArray().ElementAt(0);
			Package package = new Package(PackageRoot, ExtractRoot, text, IsTargetProject(text, PackageRoot), _packageFileRepository);
			_packageFileRepository.AddPackage(package);
			string empty = string.Empty;
			foreach (Dependency item in package.GetDependenciesFromDepXml())
			{
				if (item is PackageDependency)
				{
					text = LongPathPath.Combine(PackageRoot, ((PackageDependency)item).Name);
					if (string.Compare(LongPathPath.GetFileNameWithoutExtension(text), packageName, StringComparison.InvariantCultureIgnoreCase) != 0)
					{
						Package package2 = new Package(PackageRoot, ExtractRoot, text, IsTargetProject(text, PackageRoot), _packageFileRepository);
						_packageFileRepository.AddPackage(package2);
					}
				}
			}
			foreach (string razzlePkgPath in RazzlePkgPaths)
			{
				LoadPackages(razzlePkgPath);
			}
			ResolveDependency(packageName, allDependencies, csvFolder);
		}

		private void SavePackageAnalysisCsv(Package package, HashSet<string> packageAnalysisCSV, string csvFolder)
		{
			if (csvFolder != null)
			{
				string path = LongPathPath.Combine(csvFolder, $"{package.Name}.csv");
				IEnumerable<string> contents = packageAnalysisCSV.Where((string line) => line.StartsWith($"{package.Name},") || line.StartsWith("Package,", StringComparison.InvariantCultureIgnoreCase));
				File.AppendAllLines(path, contents);
				string path2 = LongPathPath.Combine(csvFolder, "MasterToc.txt");
				IEnumerable<string> contents2 = package.PackageFiles.Select((PackageFile pkgFile) => $"{package.Name},{pkgFile.Name}");
				File.AppendAllLines(path2, contents2);
			}
		}

		private void SavePackageDependency(Package package)
		{
			string directoryName = LongPathPath.GetDirectoryName(package.PackageFilePath);
			string text = LongPathPath.GetFileNameWithoutExtension(package.PackageFilePath) + ".dep.xml";
			text = ((!DepXmlToOutputFolder) ? LongPathPath.Combine(directoryName, text) : LongPathPath.Combine(ExtractRoot, text));
			ResolvedDependency.Save(text, package.ResolvedDependencies);
		}
	}
}
