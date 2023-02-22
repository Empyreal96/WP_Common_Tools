using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class GeneralCacheGenerator
	{
		public static void DoWork(string outputPath, string rootPath)
		{
			if (PathHelper.GetPathType(rootPath) != PathType.PhoneBuildPath)
			{
				throw new InvalidDataException($"{rootPath} is not a phone build path.");
			}
			if (!PathHelper.IsPrebuiltPath(rootPath))
			{
				throw new InvalidDataException($"{rootPath} is not a prebuilt path.");
			}
			string winBuildPath = PathHelper.GetWinBuildPath(rootPath);
			if (string.IsNullOrEmpty(winBuildPath))
			{
				throw new InvalidDataException($"{rootPath} does not have a corresponding Windows build path.");
			}
			HashSet<string> prebuiltPaths = PathHelper.GetPrebuiltPaths(winBuildPath);
			if (prebuiltPaths.Count < 2)
			{
				throw new InvalidDataException($"{winBuildPath} does not have a prebuilt folder under the bin chunk.");
			}
			GenerateGeneralBinaryLocationCache(outputPath, winBuildPath);
			GenerateGeneralPackageLocationCache(outputPath, rootPath);
		}

		private static void GenerateGeneralPackageLocationCache(string outputPath, string rootPath)
		{
			if (string.IsNullOrWhiteSpace(rootPath))
			{
				throw new ArgumentException("cannot be null or empty", "rootPath");
			}
			if (!ReliableDirectory.Exists(rootPath, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs)))
			{
				throw new DirectoryNotFoundException(rootPath);
			}
			SerializableDictionary<string, HashSet<string>> serializableDictionary = new SerializableDictionary<string, HashSet<string>>();
			List<string> list = new List<string>();
			list.AddRange(PathHelper.GetPrebuiltPaths(PathHelper.GetWinBuildPath(rootPath)).ToArray());
			list.AddRange(PathHelper.GetPrebuiltPaths(rootPath).ToArray());
			foreach (string item2 in list)
			{
				IEnumerable<string> prebuiltPathForAllArchitectures = PathHelper.GetPrebuiltPathForAllArchitectures(item2);
				foreach (string item3 in prebuiltPathForAllArchitectures)
				{
					if (!ReliableDirectory.Exists(item3, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs)))
					{
						Logger.Info("Path {0} is inaccessible, ignored.", item3);
						continue;
					}
					Logger.Info("Scanning {0}...", item3);
					IEnumerable<string> packageFilesUnderPath = PathHelper.GetPackageFilesUnderPath(item3);
					foreach (string item4 in packageFilesUnderPath)
					{
						int num = item4.LastIndexOf('\\');
						string path = PathHelper.GetPackageNameWithoutExtension(item4.Substring(num + 1)).ToLowerInvariant();
						path = PathHelper.GetFileNameWithoutExtension(path, Constants.CabFileExtension);
						string item = PathHelper.ChangeParent(item4.Substring(0, num), item3, string.Empty).ToLowerInvariant();
						if (serializableDictionary.ContainsKey(path))
						{
							serializableDictionary[path].Add(item);
							continue;
						}
						serializableDictionary[path] = new HashSet<string> { item };
					}
				}
			}
			serializableDictionary.SerializeToFile(Path.Combine(outputPath, Constants.GeneralPackageLocationCacheFileName));
		}

		private static void GenerateGeneralBinaryLocationCache(string outputPath, string rootPath)
		{
			if (string.IsNullOrWhiteSpace(rootPath))
			{
				throw new ArgumentException("cannot be null or empty", "rootPath");
			}
			if (!ReliableDirectory.Exists(rootPath, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs)))
			{
				throw new DirectoryNotFoundException(rootPath);
			}
			Dictionary<string, PackageDescription> dictionary = null;
			foreach (string prebuiltPath in PathHelper.GetPrebuiltPaths(rootPath))
			{
				Dictionary<string, PackageDescription> dictionary2 = BinaryLocator.ScanBuild(prebuiltPath);
				if (dictionary == null || !dictionary.Any())
				{
					dictionary = dictionary2;
					continue;
				}
				foreach (KeyValuePair<string, PackageDescription> item in dictionary2)
				{
					if (dictionary.ContainsKey(item.Key))
					{
						Logger.Error("Found more than one packages named {0}, ignore the one uncer {1}", item.Key, prebuiltPath);
					}
					else
					{
						dictionary.Add(item.Key, item.Value);
					}
				}
			}
			SerializableDictionary<string, HashSet<string>> serializableDictionary = BinaryLocator.ComputeBinaryPackageMapping(dictionary);
			serializableDictionary.SerializeToFile(Path.Combine(outputPath, Constants.GeneralBinaryPackageMappingCacheFileName));
			HashSet<string> hashSet = new HashSet<string>();
			string text = Path.Combine(Constants.AssemblyDirectory, Constants.SupressionFileName);
			if (!File.Exists(text))
			{
				throw new FileNotFoundException(text);
			}
			IEnumerable<string> source = from x in File.ReadAllLines(text)
				select x.Trim();
			string suppressBinaryPrefix = "BIN,*,*,";
			foreach (KeyValuePair<string, PackageDescription> item2 in dictionary)
			{
				IEnumerable<string> enumerable = from x in item2.Value.Dependencies
					where x is BinaryDependency
					select (x as BinaryDependency).FileName;
				foreach (string binaryName in enumerable)
				{
					if (!serializableDictionary.ContainsKey(binaryName) && !source.Any((string x) => string.Compare(x, suppressBinaryPrefix + binaryName, true) == 0))
					{
						hashSet.Add(binaryName.ToLowerInvariant());
					}
				}
			}
			string path = "binarySuppressToAppend.txt";
			string path2 = Path.Combine(outputPath, path);
			File.WriteAllLines(path2, hashSet.Select((string x) => suppressBinaryPrefix + x));
		}
	}
}
