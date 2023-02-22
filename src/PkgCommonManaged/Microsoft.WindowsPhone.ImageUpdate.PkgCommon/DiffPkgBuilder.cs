using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	internal static class DiffPkgBuilder
	{
		private class Pair<T1, T2>
		{
			public T1 First { get; set; }

			public T2 Second { get; set; }

			public Pair(T1 first, T2 second)
			{
				First = first;
				Second = second;
			}
		}

		private static readonly DELTA_INPUT dummyInput = new DELTA_INPUT(IntPtr.Zero, UIntPtr.Zero, false);

		private static DiffFileEntry CreateDiffEntry(FileEntry source, FileEntry target, string outputDir, CpuId cpuType)
		{
			DiffFileEntry diffFileEntry = null;
			if (target == null)
			{
				return CreateRemoveEntry(source);
			}
			if (source == null)
			{
				return CreateCanonicalEntry(target, outputDir);
			}
			if (target.FileType == FileType.Manifest)
			{
				return CreateDsmEntry(target, outputDir);
			}
			return CreateDeltaEntry(source, target, outputDir, cpuType);
		}

		private static DiffFileEntry CreateDsmEntry(FileEntry target, string outputDir)
		{
			return new DiffFileEntry(FileType.Manifest, DiffType.TargetDSM, target.DevicePath, target.SourcePath);
		}

		private static DiffFileEntry CreateRemoveEntry(IFileEntry source)
		{
			return new DiffFileEntry(source.FileType, DiffType.Remove, source.DevicePath, null);
		}

		private static DiffFileEntry CreateCanonicalEntry(FileEntry target, string outputDir)
		{
			return new DiffFileEntry(target.FileType, DiffType.Canonical, target.DevicePath, target.SourcePath);
		}

		private static DiffFileEntry CreateDeltaEntry(FileEntry source, FileEntry target, string outputDir, CpuId cpuType)
		{
			if (source == null)
			{
				throw new PackageException("Can not create a Delta type DiffFileEntry with null source file entry");
			}
			if (target == null)
			{
				throw new PackageException("Can not create a Delta type DiffFileEntry with null target file entry");
			}
			if (string.Compare(source.DevicePath, target.DevicePath, StringComparison.InvariantCultureIgnoreCase) != 0)
			{
				throw new PackageException("To create a Delta type DiffFileEntry the source and target file must have same DevicePath");
			}
			VerifyFileForDiff(source, target);
			if (string.IsNullOrEmpty(source.SourcePath))
			{
				throw new PackageException("SoucePath of the source file entry is empty");
			}
			if (string.IsNullOrEmpty(target.SourcePath))
			{
				throw new PackageException("SourcePath of the target file entry is empty");
			}
			if (!LongPathFile.Exists(source.SourcePath))
			{
				throw new PackageException("Source file '{0}' doesn't exist", source.SourcePath);
			}
			if (!LongPathFile.Exists(target.SourcePath))
			{
				throw new PackageException("Target file '{0}' doesn't exist", target.SourcePath);
			}
			if (File.ReadAllBytes(source.SourcePath).SequenceEqual(File.ReadAllBytes(target.SourcePath)))
			{
				if (source.Attributes != target.Attributes)
				{
					return CreateCanonicalEntry(target, outputDir);
				}
				return null;
			}
			if (IsCanonicalNeeded(target))
			{
				return CreateCanonicalEntry(target, outputDir);
			}
			string text = Path.Combine(outputDir, target.CabPath);
			CreateDelta(source, target, text, cpuType);
			return new DiffFileEntry(target.FileType, DiffType.Delta, target.DevicePath, text);
		}

		private static void CreateDelta(FileEntry source, FileEntry target, string deltaPath, CpuId cpuType)
		{
			ulong lpTargetFileTime = (ulong)File.GetCreationTimeUtc(source.SourcePath).ToFileTimeUtc();
			if (!MSDeltaInterOp.CreateDeltaW(DELTA_FILE_TYPE.DELTA_FILE_TYPE_RAW, DELTA_FLAG_TYPE.DELTA_FLAG_NONE, DELTA_FLAG_TYPE.DELTA_FLAG_NONE, source.SourcePath, target.SourcePath, null, null, dummyInput, ref lpTargetFileTime, 32u, deltaPath))
			{
				throw new PackageException("MSDeltaInterOp.CreateDelta failed with error code {0} when creating delta from '{1}' to '{2}'", Marshal.GetLastWin32Error(), source.SourcePath, target.SourcePath);
			}
		}

		private static SortedDictionary<string, Pair<FileEntry, FileEntry>> BuildFileDictionary(WPExtractedPackage source, WPExtractedPackage target)
		{
			SortedDictionary<string, Pair<FileEntry, FileEntry>> sortedDictionary = new SortedDictionary<string, Pair<FileEntry, FileEntry>>(StringComparer.InvariantCultureIgnoreCase);
			foreach (FileEntry file in source.Files)
			{
				sortedDictionary.Add(file.DevicePath, new Pair<FileEntry, FileEntry>(file, null));
			}
			foreach (FileEntry file2 in target.Files)
			{
				Pair<FileEntry, FileEntry> value = null;
				if (!sortedDictionary.TryGetValue(file2.DevicePath, out value))
				{
					sortedDictionary.Add(file2.DevicePath, new Pair<FileEntry, FileEntry>(null, file2));
				}
				else
				{
					value.Second = file2;
				}
			}
			return sortedDictionary;
		}

		private static void VerifyPkgForDiff(WPExtractedPackage source, WPExtractedPackage target)
		{
			if (!string.Equals(source.Name, target.Name, StringComparison.InvariantCultureIgnoreCase))
			{
				throw new PackageException("Source and Target have different name. Source:'{0}' Target:'{1}'", source.Name, target.Name);
			}
			if (!(source.Version < target.Version))
			{
				throw new PackageException("Target version '{1}' must be higher than Source's '{0}'", source.Version, target.Version);
			}
			if (source.CpuType != target.CpuType)
			{
				throw new PackageException("Source '{0}' and Target '{1}' have different CPU", source.CpuType, target.CpuType);
			}
			if (!string.Equals(source.Partition, target.Partition, StringComparison.InvariantCultureIgnoreCase))
			{
				throw new PackageException("Source '{0}'and Target '{1}' have different Partition", source.Partition, target.Partition);
			}
			if (!string.Equals(source.Platform, target.Platform, StringComparison.InvariantCultureIgnoreCase))
			{
				throw new PackageException("Source '{0}' and Target '{1}' have different Platform", source.Platform, target.Platform);
			}
			if (source.PackageStyle != target.PackageStyle)
			{
				throw new PackageException("Source '{0}' and Target '{1}' have different Package Style", source.PackageStyle, target.PackageStyle);
			}
		}

		private static void VerifyFileForDiff(IFileEntry source, IFileEntry target)
		{
			if (source.FileType == FileType.BinaryPartition && target.FileType != FileType.BinaryPartition)
			{
				throw new PackageException("File '{0}': changing file type from BinaryParitition to type '{1}' is not allowed", source.DevicePath, target.FileType);
			}
			if (source.FileType != FileType.BinaryPartition && target.FileType == FileType.BinaryPartition)
			{
				throw new PackageException("File '{0}': changing file type from type '{1}' to BinaryParitition is not allowed", source.DevicePath, source.FileType);
			}
		}

		private static bool IsCanonicalNeeded(IFileEntry target)
		{
			List<string> source = new List<string> { "\\Windows\\System32\\config\\FP", "\\Windows\\System32\\config\\BBI", "\\Programs\\MobileUI\\SendToMediaLib.exe", "\\Windows\\ImageUpdate\\OEMDevicePlatform.xml", "\\Windows\\System32\\Tasks\\Microsoft\\Windows\\Clip\\License Validation" };
			if (target.FileType != FileType.BinaryPartition && target.Size < 8388608)
			{
				return source.Contains(target.DevicePath, StringComparer.InvariantCultureIgnoreCase);
			}
			return true;
		}

		public static DiffPkgManifest CreateDiff(WPExtractedPackage source, WPExtractedPackage target, string outputDir)
		{
			VerifyPkgForDiff(source, target);
			SortedDictionary<string, Pair<FileEntry, FileEntry>> sortedDictionary = BuildFileDictionary(source, target);
			DiffPkgManifest diffPkgManifest = new DiffPkgManifest
			{
				Name = target.Name,
				SourceVersion = source.Version,
				TargetVersion = target.Version
			};
			foreach (KeyValuePair<string, Pair<FileEntry, FileEntry>> item in sortedDictionary)
			{
				DiffFileEntry diffFileEntry = CreateDiffEntry(item.Value.First, item.Value.Second, outputDir, target.CpuType);
				if (diffFileEntry != null)
				{
					diffPkgManifest.AddFileEntry(diffFileEntry);
				}
			}
			FileEntry fileEntry = source.GetDsmFile() as FileEntry;
			diffPkgManifest.SourceHash = PackageTools.CalculateFileHash(fileEntry.SourcePath);
			return diffPkgManifest;
		}

		private static void SaveDiffPkg(DiffPkgManifest diffPkgManifest, string cabPath)
		{
			if (string.IsNullOrEmpty(cabPath))
			{
				throw new ArgumentNullException("cabPath", "The path for cabinet file is null or empty");
			}
			LongPathFile.Delete(cabPath);
			string tempFile = FileUtils.GetTempFile();
			try
			{
				CabArchiver cabArchiver = new CabArchiver();
				diffPkgManifest.Save(tempFile);
				DiffFileEntry[] files = diffPkgManifest.Files;
				foreach (DiffFileEntry diffFileEntry in files)
				{
					if (diffFileEntry.DiffType != DiffType.Remove && diffFileEntry.DiffType != DiffType.TargetDSM)
					{
						cabArchiver.AddFile(diffFileEntry.CabPath, diffFileEntry.SourcePath);
					}
				}
				cabArchiver.AddFileToFront(PkgConstants.c_strDiffDsmFile, tempFile);
				cabArchiver.AddFileToFront(PkgConstants.c_strDsmFile, diffPkgManifest.TargetDsmFile.SourcePath);
				cabArchiver.Save(cabPath, Package.DefaultCompressionType);
				try
				{
					PackageTools.SignFile(cabPath);
				}
				catch (Exception innerException)
				{
					throw new PackageException(innerException, "Failed to sign generated package: {0}", cabPath);
				}
			}
			finally
			{
				LongPathFile.Delete(tempFile);
			}
		}

		[DllImport("CbsXpdGen.dll")]
		public static extern int GenerateCbs2CbsXpdEx([MarshalAs(UnmanagedType.LPWStr)] string Source, [MarshalAs(UnmanagedType.LPWStr)] string Target, [MarshalAs(UnmanagedType.LPWStr)] string SourcePdbPath, [MarshalAs(UnmanagedType.LPWStr)] string TargetPdbPath, [MarshalAs(UnmanagedType.U4)] uint deltaThresholdMB, [MarshalAs(UnmanagedType.LPWStr)] string PackageName, [MarshalAs(UnmanagedType.LPWStr)] string OutputFolder);

		private static void CallCbsXpdGen(string sourceDir, string targetDir, uint deltaThresholdMB, string outputDir, string outputCab)
		{
			if (string.IsNullOrEmpty(sourceDir))
			{
				throw new ArgumentNullException("sourceDir", "The path for source is null or empty");
			}
			if (string.IsNullOrEmpty(targetDir))
			{
				throw new ArgumentNullException("targetDir", "The path for target is null or empty");
			}
			if (string.IsNullOrEmpty(outputDir))
			{
				throw new ArgumentNullException("outputDir", "The path for output is null or empty");
			}
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(outputCab);
			NativeMethods.CheckHResult(GenerateCbs2CbsXpdEx(sourceDir, targetDir, ".", ".", deltaThresholdMB, fileNameWithoutExtension, outputDir), "GenerateCbs2CbsXpdEx");
		}

		internal static DiffError CreateDiff(string sourceCab, string targetCab, DiffOptions options, Dictionary<DiffOptions, object> optionValues, string outputCab)
		{
			string tempDirectory = FileUtils.GetTempDirectory();
			try
			{
				string text = Path.Combine(tempDirectory, "source");
				LongPathDirectory.CreateDirectory(text);
				WPExtractedPackage wPExtractedPackage = WPCanonicalPackage.ExtractAndLoad(sourceCab, text);
				string text2 = Path.Combine(tempDirectory, "target");
				LongPathDirectory.CreateDirectory(text2);
				WPExtractedPackage wPExtractedPackage2 = WPCanonicalPackage.ExtractAndLoad(targetCab, text2);
				bool flag = false;
				if (wPExtractedPackage.Version == wPExtractedPackage2.Version)
				{
					return DiffError.SameVersion;
				}
				string text3 = Path.Combine(tempDirectory, "output");
				LongPathDirectory.CreateDirectory(text3);
				try
				{
					PackageTools.CheckCrossPartitionFiles(wPExtractedPackage2.Name, wPExtractedPackage2.Partition, wPExtractedPackage2.Files.Select((IFileEntry x) => x.DevicePath), false);
				}
				catch (PackageException)
				{
					flag = true;
				}
				if (flag)
				{
					LongPathFile.Copy(targetCab, outputCab);
				}
				else if (PackageStyle.CBS == wPExtractedPackage.PackageStyle && PackageStyle.CBS == wPExtractedPackage2.PackageStyle)
				{
					string text4 = Path.Combine(text3, "output", Path.GetFileName(outputCab));
					uint deltaThresholdMB = uint.MaxValue;
					if (options.HasFlag(DiffOptions.DeltaThresholdMB))
					{
						deltaThresholdMB = (uint)optionValues[DiffOptions.DeltaThresholdMB];
					}
					CallCbsXpdGen(text, text2, deltaThresholdMB, text3, Path.GetFileName(outputCab));
					PackageTools.SignFile(text4);
					LongPathFile.Copy(text4, outputCab);
				}
				else
				{
					if (PackageStyle.SPKG != wPExtractedPackage.PackageStyle || PackageStyle.SPKG != wPExtractedPackage2.PackageStyle)
					{
						throw new PackageException("Trying to create diff between invalid source and target package styles.  Source: {0}, Target: {1}", wPExtractedPackage.PackageStyle, wPExtractedPackage2.PackageStyle);
					}
					SaveDiffPkg(CreateDiff(wPExtractedPackage, wPExtractedPackage2, text3), outputCab);
				}
				return DiffError.OK;
			}
			finally
			{
				FileUtils.DeleteTree(tempDirectory);
			}
		}

		[DllImport("ParseManifest.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int CacheManifests(string[] manifests, [Out][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] names, [Out][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] versions, [Out][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] uint[] sizes, [Out][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U8)] ulong[] offsets, uint count, string cacheFile, out string cmsVersion);
	}
}
