using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	internal static class MergeWorker
	{
		private static readonly string[] _settingsFiles = new string[1] { "Microsoft.WifiOnlyFeaturePackOverrides.reg" };

		private static bool IsTargetUpToDate(IEnumerable<string> inputFiles, string output)
		{
			if (!LongPathFile.Exists(output))
			{
				return false;
			}
			DateTime lastWriteTimeUtc = new FileInfo(output).LastWriteTimeUtc;
			foreach (string inputFile in inputFiles)
			{
				if (new FileInfo(inputFile).LastWriteTimeUtc > lastWriteTimeUtc)
				{
					return false;
				}
			}
			return true;
		}

		public static void Merge(IPkgBuilder pkgBuilder, List<string> pkgs, string outputPkg, bool compress, bool incremental)
		{
			if (incremental && IsTargetUpToDate(pkgs, outputPkg))
			{
				LogUtil.Message("Skipping package '{0}' because all of its source packages are not changed", outputPkg);
				return;
			}
			string tempDirectory = FileUtils.GetTempDirectory();
			try
			{
				StringBuilder stringBuilder = new StringBuilder();
				StringBuilder stringBuilder2 = new StringBuilder();
				int num = 0;
				foreach (string pkg in pkgs)
				{
					string outputDir = Path.Combine(tempDirectory, "PkgMerge" + num);
					WPExtractedPackage wPExtractedPackage = WPCanonicalPackage.ExtractAndLoad(pkg, outputDir);
					stringBuilder.AppendFormat("{0},{1}", pkg, wPExtractedPackage.Version);
					stringBuilder.AppendLine();
					foreach (FileEntry file in wPExtractedPackage.Files)
					{
						if (file.FileType == FileType.Manifest || file.FileType == FileType.Catalog)
						{
							continue;
						}
						IFileEntry fileEntry2 = pkgBuilder.FindFile(file.DevicePath);
						if (fileEntry2 != null)
						{
							MergeErrors.Instance.Add("Package '{0}' and package with name '{1}' both contain file with same device path '{2}'", pkg, fileEntry2.SourcePackage, file.DevicePath);
							continue;
						}
						FileType fileType = file.FileType;
						if (fileType == FileType.Registry)
						{
							string fileName = Path.GetFileName(file.DevicePath);
							if (_settingsFiles.Contains(fileName, StringComparer.InvariantCultureIgnoreCase))
							{
								string destination = file.DevicePath.Replace(fileName, "MicrosoftSettings.reg");
								pkgBuilder.AddFile(file.FileType, file.SourcePath, destination, file.Attributes, wPExtractedPackage.Name);
							}
							else
							{
								stringBuilder2.AppendLine("; RegistrySource=" + Path.GetFileName(file.DevicePath));
								stringBuilder2.AppendLine(File.ReadAllText(file.SourcePath));
							}
						}
						else
						{
							pkgBuilder.AddFile(file.FileType, file.SourcePath, file.DevicePath, file.Attributes, wPExtractedPackage.Name);
						}
					}
					num++;
				}
				string text = Path.Combine(tempDirectory, "combined.reg");
				if (stringBuilder2.Length != 0)
				{
					string text2 = (pkgBuilder.Name.StartsWith("MSAsOEM", StringComparison.CurrentCultureIgnoreCase) ? "Microsoft." : string.Empty);
					string destination2 = PkgConstants.c_strRguDeviceFolder + "\\" + text2 + pkgBuilder.Name + PkgConstants.c_strRguExtension;
					stringBuilder2.Replace("Windows Registry Editor Version 5.00", string.Empty);
					stringBuilder2.Insert(0, "Windows Registry Editor Version 5.00" + Environment.NewLine);
					File.WriteAllText(text, stringBuilder2.ToString(), Encoding.Unicode);
					pkgBuilder.AddFile(FileType.Registry, text, destination2, PkgConstants.c_defaultAttributes, null);
				}
				File.WriteAllText(Path.ChangeExtension(outputPkg, ".merged.txt"), stringBuilder.ToString());
				if (!MergeErrors.Instance.HasError)
				{
					pkgBuilder.SaveCab(outputPkg, compress);
				}
			}
			finally
			{
				FileUtils.DeleteTree(tempDirectory);
			}
		}
	}
}
