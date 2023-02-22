using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Tools.IO;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public static class PathHelper
	{
		private static readonly string DirectorySeparator = new string(new char[1] { Path.DirectorySeparatorChar });

		private static string[] winBuildArchitectures = new string[8] { "armfre", "armchk", "arm64fre", "arm64chk", "amd64fre", "amd64chk", "x86fre", "x86chk" };

		private static string[] phoneBuildArchitectures = new string[10] { "MC.amd64chk", "MC.amd64fre", "MC.arm64chk", "MC.arm64fre", "MC.armchk", "MC.armfre", "MC.x86chk", "MC.x86fre", "NT.amd64fre", "NT.x86fre" };

		public static string PrebuiltFolderName => "Prebuilt";

		public static string NonCritPrebuiltFolderName => "Prebuilt_noncrit";

		public static string GetFileNameWithoutExtension(string path, string extension)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException("path");
			}
			if (string.IsNullOrEmpty(extension))
			{
				throw new ArgumentNullException("extension");
			}
			if (!extension.StartsWith(".", StringComparison.OrdinalIgnoreCase))
			{
				extension = "." + extension;
			}
			string fileName = Path.GetFileName(path);
			if (fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
			{
				return fileName.Substring(0, fileName.Length - extension.Length);
			}
			return fileName;
		}

		public static string GetPackageNameWithoutExtension(string package)
		{
			string result = LongPathPath.GetFileName(package);
			string[] array = new string[2]
			{
				Constants.CabFileExtension,
				Constants.SpkgFileExtension
			};
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (package.EndsWith(text, StringComparison.OrdinalIgnoreCase))
				{
					result = GetFileNameWithoutExtension(package.Trim().ToLowerInvariant(), text);
					break;
				}
			}
			return result;
		}

		public static string EndWithDirectorySeparator(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException("path");
			}
			return path.EndsWith(DirectorySeparator, StringComparison.OrdinalIgnoreCase) ? path : (path + DirectorySeparator);
		}

		public static string ChangeParent(string path, string oldParent, string newParent)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException("path");
			}
			if (string.IsNullOrEmpty(oldParent))
			{
				throw new ArgumentNullException("oldParent");
			}
			if (newParent == null)
			{
				throw new ArgumentNullException("newParent");
			}
			if (string.Compare(path, oldParent, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return newParent;
			}
			oldParent = EndWithDirectorySeparator(oldParent);
			newParent = (string.IsNullOrEmpty(newParent) ? string.Empty : EndWithDirectorySeparator(newParent));
			if (!path.StartsWith(oldParent, StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Path '{0}' does not start with '{1}'", new object[2] { path, oldParent }));
			}
			return newParent + path.Substring(oldParent.Length);
		}

		public static string Combine(string path1, string path2)
		{
			if (path1 == null)
			{
				throw new ArgumentNullException("path1");
			}
			if (path2 == null)
			{
				throw new ArgumentNullException("path2");
			}
			return Path.Combine(path1, path2.TrimStart(Path.DirectorySeparatorChar));
		}

		public static string GetWinBuildPath(string path)
		{
			if (path.StartsWith("\\\\"))
			{
				return GetWinBuildPathForPublicBuild(path);
			}
			return GetWinBuildPathForLocalPath(path);
		}

		public static string GetContainingFolderPath(string path, string containingFolder)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException("path");
			}
			DirectoryInfo directoryInfo = new DirectoryInfo(path);
			while (directoryInfo != null && string.Compare(directoryInfo.Name, containingFolder, true) != 0)
			{
				directoryInfo = directoryInfo.Parent;
			}
			return (directoryInfo == null) ? string.Empty : directoryInfo.FullName;
		}

		public static PathType GetPathType(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException("path");
			}
			path = path.ToLowerInvariant();
			if (path.StartsWith(Settings.Default.WinBuildSharePrefix, StringComparison.OrdinalIgnoreCase))
			{
				return PathType.WinbBuildPath;
			}
			if (path.StartsWith(Settings.Default.PhoneBuildSharePrefix, StringComparison.OrdinalIgnoreCase))
			{
				return PathType.PhoneBuildPath;
			}
			if (path.StartsWith("\\\\"))
			{
				return PathType.NetworkPath;
			}
			return PathType.LocalPath;
		}

		public static IOrderedEnumerable<string> SortPathSetOnPathType(HashSet<string> paths)
		{
			return paths.OrderBy((string x) => GetPathType(x));
		}

		public static HashSet<string> AddRelatedPathsToSet(HashSet<string> paths)
		{
			HashSet<string> hashSet = new HashSet<string>();
			HashSet<string> hashSet2 = new HashSet<string>();
			string empty = string.Empty;
			foreach (string path in paths)
			{
				hashSet2 = GetPrebuiltPaths(path);
				if (hashSet2.Count() == 0)
				{
					hashSet.Add(path);
				}
				else
				{
					hashSet.UnionWith(hashSet2);
				}
				empty = GetWinBuildPath(path);
				if (!string.IsNullOrEmpty(empty))
				{
					hashSet.UnionWith(GetPrebuiltPaths(empty));
				}
			}
			return hashSet;
		}

		public static HashSet<string> GetPrebuiltPaths(string path, bool throwException = false)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException("path");
			}
			HashSet<string> hashSet = new HashSet<string>();
			if (!Directory.Exists(path))
			{
				if (throwException)
				{
					throw new InvalidDataException($"Path {path} does not exist or is not accessible.");
				}
				Logger.Info($"Path {path} does not exist or is not accessible.");
				return hashSet;
			}
			string containingFolderPath = GetContainingFolderPath(path, PrebuiltFolderName);
			string containingFolderPath2 = GetContainingFolderPath(path, NonCritPrebuiltFolderName);
			string text = string.Empty;
			if (!string.IsNullOrEmpty(containingFolderPath))
			{
				hashSet.Add(containingFolderPath.ToLowerInvariant());
				DirectoryInfo parent = new DirectoryInfo(containingFolderPath).Parent;
				if (parent != null)
				{
					if (string.Compare(parent.Name, "bin", true) == 0)
					{
						if (parent.Parent != null)
						{
							text = parent.Parent.FullName;
						}
					}
					else
					{
						text = parent.FullName;
					}
				}
			}
			else
			{
				if (string.IsNullOrEmpty(containingFolderPath2))
				{
					return hashSet;
				}
				hashSet.Add(containingFolderPath2.ToLowerInvariant());
				DirectoryInfo parent = new DirectoryInfo(containingFolderPath2).Parent;
				if (parent != null)
				{
					text = parent.FullName;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				return hashSet;
			}
			string[] array = new string[4]
			{
				PrebuiltFolderName,
				"bin\\" + PrebuiltFolderName,
				NonCritPrebuiltFolderName,
				"..\\" + NonCritPrebuiltFolderName
			};
			string[] array2 = array;
			foreach (string path2 in array2)
			{
				string fullPath = Path.GetFullPath(Path.Combine(text, path2));
				if (ReliableDirectory.Exists(fullPath, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs)))
				{
					hashSet.Add(fullPath.ToLowerInvariant());
				}
			}
			return hashSet;
		}

		public static string GetPrebuiltPath(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException("cannot be null or empty.", "path");
			}
			if (!ReliableDirectory.Exists(path, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs)))
			{
				throw new DirectoryNotFoundException(path);
			}
			try
			{
				string containingFolderPath = GetContainingFolderPath(path, Constants.PrebuiltFolderName);
				if (IsPrebuiltPath(containingFolderPath))
				{
					return containingFolderPath;
				}
				containingFolderPath = Path.Combine(path, Constants.PrebuiltFolderName);
				if (ReliableDirectory.Exists(containingFolderPath, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs)) && IsPrebuiltPath(containingFolderPath))
				{
					return containingFolderPath;
				}
				if (IsPrebuiltPath(path))
				{
					return path;
				}
				return string.Empty;
			}
			catch
			{
				return string.Empty;
			}
		}

		public static IEnumerable<string> GetPackageFilesUnderPath(string path)
		{
			IEnumerable<string> enumerable;
			List<string> list;
			if (IsPrebuiltPath(path))
			{
				enumerable = from x in GetFilesUnderPrebuiltPath(path, "*" + Constants.SpkgFileExtension)
					select x.ToLowerInvariant();
				list = (from x in GetFilesUnderPrebuiltPath(path, "*" + Constants.CabFileExtension)
					select x.ToLowerInvariant()).ToList();
			}
			else
			{
				enumerable = from x in ReliableDirectory.GetFiles(path, "*" + Constants.SpkgFileExtension, SearchOption.AllDirectories, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs))
					select x.ToLowerInvariant();
				list = (from x in ReliableDirectory.GetFiles(path, "*" + Constants.CabFileExtension, SearchOption.AllDirectories, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs))
					select x.ToLowerInvariant()).ToList();
			}
			foreach (string item2 in enumerable)
			{
				string item = Path.ChangeExtension(item2, Constants.CabFileExtension);
				if (list.Contains(item))
				{
					list.Remove(item);
				}
				list.Add(item2);
			}
			return list;
		}

		public static bool IsPrebuiltPath(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return false;
			}
			if (!ReliableDirectory.Exists(path, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs)))
			{
				throw new DirectoryNotFoundException(path);
			}
			string[] source = new string[1] { "test" };
			string[] source2 = new string[1] { "windows" };
			bool flag = !source.Any((string x) => !ReliableDirectory.Exists(Path.Combine(path, x), Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs)));
			bool flag2 = !source2.Any((string x) => !ReliableDirectory.Exists(Path.Combine(path, x), Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs)));
			return flag || flag2;
		}

		public static IEnumerable<string> GetPrebuiltPathForAllArchitectures(string prebuiltPath)
		{
			if (string.IsNullOrWhiteSpace(prebuiltPath))
			{
				throw new ArgumentException("cannot be null or empty", "prebuiltPath");
			}
			prebuiltPath = prebuiltPath.ToLowerInvariant();
			string phoneBuildArchitecture = phoneBuildArchitectures.FirstOrDefault((string x) => prebuiltPath.IndexOf(x.ToLowerInvariant()) >= 0);
			if (!string.IsNullOrEmpty(phoneBuildArchitecture))
			{
				return phoneBuildArchitectures.Select((string x) => prebuiltPath.Replace(phoneBuildArchitecture.ToLowerInvariant(), x.ToLowerInvariant()));
			}
			string winBuildArchitecture = winBuildArchitectures.FirstOrDefault((string x) => prebuiltPath.IndexOf(x.ToLowerInvariant()) >= 0);
			if (string.IsNullOrEmpty(winBuildArchitecture))
			{
				throw new InvalidDataException($"Cannot decide the build architecture of path {prebuiltPath}");
			}
			return winBuildArchitectures.Select((string x) => prebuiltPath.Replace(winBuildArchitecture.ToLowerInvariant(), x.ToLowerInvariant().Replace("arm", "woa")));
		}

		private static IEnumerable<string> GetFilesUnderPrebuiltPath(string prebuiltPath, string searchPattern)
		{
			if (!ReliableDirectory.Exists(prebuiltPath, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs)))
			{
				throw new DirectoryNotFoundException(prebuiltPath);
			}
			List<string> list = new List<string>();
			string excludedSubDirsUnderPrebuiltPath = Settings.Default.ExcludedSubDirsUnderPrebuiltPath;
			IEnumerable<string> subDirectoriesToExclude = from x in excludedSubDirsUnderPrebuiltPath.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries)
				select x.ToLowerInvariant();
			IEnumerable<string> enumerable = from x in ReliableDirectory.GetDirectories(prebuiltPath, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs))
				where !subDirectoriesToExclude.Contains(Path.GetFileName(x).ToLowerInvariant())
				select x;
			list.AddRange(ReliableDirectory.GetFiles(prebuiltPath, searchPattern, SearchOption.TopDirectoryOnly, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs)).ToList());
			foreach (string item in enumerable)
			{
				list.AddRange(ReliableDirectory.GetFiles(item, searchPattern, SearchOption.AllDirectories, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs)).ToList());
			}
			return list;
		}

		private static string GetWinBuildPathForPublicBuild(string path)
		{
			string text = path;
			string path2 = "windows.ini";
			string winBuildSharePrefix = Settings.Default.WinBuildSharePrefix;
			string text2 = "WINDOWS_BUILD=";
			string empty = string.Empty;
			string empty2 = string.Empty;
			string text3 = string.Empty;
			string empty3 = string.Empty;
			string empty4 = string.Empty;
			string empty5 = string.Empty;
			DirectoryInfo directoryInfo = new DirectoryInfo(text);
			try
			{
				bool flag = false;
				while (!string.IsNullOrEmpty(text) && directoryInfo.Parent != null)
				{
					if (File.Exists(Path.Combine(text, path2)))
					{
						flag = true;
						break;
					}
					directoryInfo = directoryInfo.Parent;
					text = directoryInfo.FullName;
				}
				if (!flag)
				{
					Logger.Info("Did not find Windows.ini in the Path and its parent directories.");
					return string.Empty;
				}
				empty = Path.Combine(text, path2);
				DirectoryInfo directoryInfo2 = new DirectoryInfo(text);
				empty2 = directoryInfo2.Parent.Name;
				StreamReader streamReader = new StreamReader(empty);
				while ((empty3 = streamReader.ReadLine()) != null)
				{
					if (empty3.StartsWith(text2, StringComparison.OrdinalIgnoreCase))
					{
						text3 = Regex.Replace(empty3, text2, string.Empty, RegexOptions.IgnoreCase);
						text3 = Regex.Replace(text3, empty2 + ".", string.Empty, RegexOptions.IgnoreCase);
						break;
					}
				}
				if (string.IsNullOrEmpty(text3))
				{
					Logger.Info("Could not get the Windows_build info from the windows.ini.");
					return string.Empty;
				}
				empty4 = winBuildArchitectures.First((string x) => path.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0);
				if (string.IsNullOrEmpty(empty4))
				{
					Logger.Info("Could not decide the build architecture from the path.");
					return string.Empty;
				}
				if (string.Compare(empty4, "armfre", true) == 0 || string.Compare(empty4, "armchk", true) == 0)
				{
					empty4 = Regex.Replace(empty4, "arm", "woa", RegexOptions.IgnoreCase);
				}
				empty5 = Path.Combine(winBuildSharePrefix, empty2, text3, empty4, "Build", PrebuiltFolderName);
				if (!Directory.Exists(empty5))
				{
					Logger.Info($"WinBuildPath {empty5} is not available.");
					return string.Empty;
				}
				Logger.Info($"Found WinBuildPath {empty5}.");
				return empty5;
			}
			catch (Exception ex)
			{
				Logger.Info($"Error in GetWinBuildPathForPublicBuild. Error {ex.Message}");
			}
			return string.Empty;
		}

		private static string GetWinBuildPathForLocalPath(string path)
		{
			string fullPath = Path.GetFullPath(path);
			string containingFolderPath = GetContainingFolderPath(fullPath, PrebuiltFolderName);
			string text = string.Empty;
			string empty = string.Empty;
			try
			{
				if (string.IsNullOrEmpty(containingFolderPath))
				{
					if (Directory.Exists(Path.Combine(fullPath, PrebuiltFolderName)))
					{
						text = path;
					}
				}
				else
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(containingFolderPath);
					if (directoryInfo.Parent != null)
					{
						text = directoryInfo.Parent.FullName;
					}
				}
				if (string.IsNullOrEmpty(text))
				{
					Logger.Info("Path {0} does not appear like a local phone build location. Stop searching for corresponding Windows build path.", text);
					return string.Empty;
				}
				string fileName = Path.GetFileName(text);
				string pattern = ".+\\.binaries\\.[arm|amd64|x86|arm64]+[chk|fre]+\\..+\\.[MC|NT]+";
				if (!Regex.IsMatch(fileName, pattern, RegexOptions.IgnoreCase))
				{
					Logger.Info("Path {0} does not appear like a local phone build location. Stop searching for corresponding Windows build path.", text);
					return string.Empty;
				}
				DirectoryInfo directoryInfo2 = new DirectoryInfo(text);
				if (directoryInfo2.Parent == null)
				{
					Logger.Info("Path {0} does not have a parent directory. Stop searching for corresponding Windows build path.", text);
					return string.Empty;
				}
				string[] array = fileName.Split('.');
				string path2 = array[0] + '.' + array[1] + '.' + array[2];
				empty = Path.Combine(directoryInfo2.Parent.FullName, path2);
				empty = Path.Combine(empty, PrebuiltFolderName);
				if (!Directory.Exists(empty))
				{
					Logger.Info($"WinBuildPath {empty} is not available.");
					return string.Empty;
				}
				Logger.Info($"Found WinBuildPath {empty}.");
				return empty;
			}
			catch (IOException ex)
			{
				Logger.Info($"Error in GetWinBuildPathForLocalPath. Error {ex.Message}");
			}
			return string.Empty;
		}
	}
}
