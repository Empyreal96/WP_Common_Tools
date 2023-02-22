using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgSignTool
{
	internal class Program
	{
		private class UnpackCmdHandler : QuietCmdHandler
		{
			private const string EFI_I386_BOOT_NAME = "bootia32.efi";

			private const string EFI_AMD64_BOOT_NAME = "bootx64.efi";

			private const string EFI_ARMNT_BOOT_NAME = "bootarm.efi";

			private const string EFI_ARM64_BOOT_NAME = "bootaa64.efi";

			private const string EFI_SIGCHECK_NAME = "sigcheck.efi";

			private const string APPX_INFUSEDAPPS_PATH = "windows\\InfusedApps\\";

			public override string Command => "Unpack";

			public override string Description => "Unpacking a given package to the specified output directory";

			private static bool ShouldSign(FileEntry fe)
			{
				if (fe.FileType != FileType.Regular)
				{
					return false;
				}
				if (fe.DevicePath.Contains("windows\\InfusedApps\\"))
				{
					return false;
				}
				if (fe.DevicePath.EndsWith("bootia32.efi", StringComparison.InvariantCultureIgnoreCase) || fe.DevicePath.EndsWith("bootx64.efi", StringComparison.InvariantCultureIgnoreCase) || fe.DevicePath.EndsWith("bootarm.efi", StringComparison.InvariantCultureIgnoreCase) || fe.DevicePath.EndsWith("bootaa64.efi", StringComparison.InvariantCultureIgnoreCase) || fe.DevicePath.EndsWith("sigcheck.efi", StringComparison.InvariantCultureIgnoreCase))
				{
					return false;
				}
				return true;
			}

			private static void PkgUnpack(string package, string outputDir)
			{
				if (!LongPathFile.Exists(package))
				{
					throw new FileNotFoundException($"The specified package ('{package}') doesn't exist");
				}
				if (!LongPathDirectory.Exists(outputDir))
				{
					LongPathDirectory.CreateDirectory(outputDir);
				}
				IPkgInfo pkgInfo = Package.LoadFromCab(package);
				List<FileEntryBase> list = new List<FileEntryBase>();
				switch (pkgInfo.Type)
				{
				case PackageType.Canonical:
				case PackageType.Removal:
				{
					CabApiWrapper.Extract(package, outputDir);
					PkgManifest pkgManifest = null;
					string path = PkgConstants.c_strCatalogFile;
					if (pkgInfo.Style == PackageStyle.CBS)
					{
						Path.Combine(outputDir, PkgConstants.c_strMumFile);
						pkgManifest = PkgManifest.Load_CBS(outputDir);
						path = PkgConstants.c_strCBSCatalogFile;
					}
					else
					{
						pkgManifest = PkgManifest.Load(Path.Combine(outputDir, PkgConstants.c_strDsmFile));
					}
					pkgManifest.BuildSourcePaths(outputDir, BuildPathOption.UseCabPath);
					list.AddRange(pkgManifest.Files);
					if (pkgInfo.Style == PackageStyle.CBS)
					{
						Dictionary<string, string> fileMap = CreateFileMap(outputDir, pkgManifest);
						LongPathFile.WriteAllLines(Path.Combine(outputDir, "filemap.csv"), fileMap.Keys.Select((string x) => fileMap[x] + "," + x));
					}
					LongPathFile.WriteAllLines(Path.Combine(outputDir, "embedded_sign.csv"), from x in pkgManifest.Files
						where ShouldSign(x)
						select x.SourcePath);
					IEnumerable<FileEntry> source = pkgManifest.Files.Where((FileEntry x) => x.FileType != FileType.Catalog);
					PackageTools.CreateCDF(source.Select((FileEntry x) => x.SourcePath).ToArray(), source.Select((FileEntry x) => x.DevicePath).ToArray(), Path.Combine(outputDir, path), pkgInfo.Name, Path.Combine(outputDir, "content.cdf"));
					using (TextWriter textWriter2 = new StreamWriter(Path.Combine(outputDir, "files.txt")))
					{
						foreach (FileEntryBase item in list)
						{
							FileAttributes attributes = LongPathFile.GetAttributes(item.SourcePath);
							if (attributes.HasFlag(FileAttributes.ReadOnly))
							{
								LongPathFile.SetAttributes(item.SourcePath, attributes & ~FileAttributes.ReadOnly);
							}
							NativeMethods.SetLastWriteTimeLong(item.SourcePath, DateTime.Now);
							if (attributes.HasFlag(FileAttributes.ReadOnly))
							{
								LongPathFile.SetAttributes(item.SourcePath, attributes);
							}
							textWriter2.WriteLine("{0},{1},{2}", item.SourcePath, item.DevicePath, NativeMethods.GetCreationTimeLong(item.SourcePath));
						}
					}
					logger.LogInfo("PkgSignTool: Successfully unpacked {0} to {1}.", package, outputDir);
					break;
				}
				case PackageType.Diff:
				{
					string text = Path.Combine(outputDir, c_OpaqueContainerCookieFileName);
					string fullPathUNC = LongPath.GetFullPathUNC(package);
					logger.LogInfo("PkgSignTool: Creating cookie indicating this is an opaque file at {0}.", text);
					using (TextWriter textWriter = new StreamWriter(text))
					{
						textWriter.WriteLine(fullPathUNC);
						break;
					}
				}
				default:
					throw new PackageException("Unexpected package type '{0}'. The package may contain a corrupted {1}.", pkgInfo.Type, (pkgInfo.Style == PackageStyle.CBS) ? "update.mum" : "man.dsm.xml");
				}
			}

			protected override int DoExecution()
			{
				string parameterAsString = _cmdLineParser.GetParameterAsString("input");
				string switchAsString = _cmdLineParser.GetSwitchAsString("out");
				SetLoggingVerbosity(logger);
				PkgUnpack(parameterAsString, switchAsString);
				return 0;
			}

			public UnpackCmdHandler()
			{
				_cmdLineParser.SetRequiredParameterString("input", "path to the package to be unpacked");
				_cmdLineParser.SetOptionalSwitchString("out", "root output directory for the unpacked package", ".");
				SetQuietCommand();
			}
		}

		private class UpdateCmdHandler : QuietCmdHandler
		{
			private const string EMPTY_FILE_HASH = "00000000000000000000000000000000000000000000";

			public override string Command => "Update";

			public override string Description => "Update component manifests in an unpacked package after it has been signed";

			private static void PkgUpdate(string inputDir, bool incVersion)
			{
				Dictionary<string, string> dictionary = ReadFileMap(inputDir);
				List<string> list = new List<string>();
				PackageStyle packageStyle = PackageStyle.SPKG;
				string text = Path.Combine(inputDir, PkgConstants.c_strDsmFile);
				if (!LongPathFile.Exists(text))
				{
					text = Path.Combine(inputDir, PkgConstants.c_strMumFile);
					packageStyle = PackageStyle.CBS;
					if (!LongPathFile.Exists(text))
					{
						throw new FileNotFoundException($"The specified input directory '{inputDir}' doesn't have man.dsm.xml or update.mum file. Unable to deduce type of package provided.");
					}
				}
				if (packageStyle != PackageStyle.CBS)
				{
					return;
				}
				PkgManifest pkgManifest = PkgManifest.Load_CBS(text);
				List<FileEntryBase> list2 = new List<FileEntryBase>();
				list2.AddRange(pkgManifest.Files);
				SHA256 sHA = SHA256.Create();
				foreach (FileEntry item in list2)
				{
					if (item.FileType != FileType.Manifest)
					{
						continue;
					}
					string cabPath = item.CabPath;
					if (!cabPath.EndsWith("manifest", StringComparison.InvariantCultureIgnoreCase) && !cabPath.EndsWith("mum", StringComparison.InvariantCultureIgnoreCase))
					{
						continue;
					}
					string text2 = Path.Combine(inputDir, cabPath);
					logger.LogInfo("PkgSignTool: Updating hashes for manifest at path: {0}", text2);
					string tempFileName = Path.GetTempFileName();
					logger.LogInfo("PkgSignTool: Copying manifest to temp file: {0}", tempFileName);
					LongPathFile.Copy(text2, tempFileName, true);
					list.Add(tempFileName);
					XDocument xDocument = XDocument.Load(tempFileName);
					XNamespace @namespace = xDocument.Root.Name.Namespace;
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(cabPath);
					if (cabPath.EndsWith("mum", StringComparison.InvariantCultureIgnoreCase))
					{
						if (!incVersion)
						{
							continue;
						}
						logger.LogInfo("Increasing version number in mum file.");
						XElement xElement = xDocument.Root.Descendants(@namespace + "assemblyIdentity").First();
						VersionInfo version = pkgManifest.Version;
						version.Build++;
						xElement.Attribute("version").Value = version.ToString();
					}
					else
					{
						foreach (XElement item2 in xDocument.Root.Descendants(@namespace + "file"))
						{
							string value = item2.Attribute("name").Value;
							string path = "";
							XNamespace xNamespace = "urn:schemas-microsoft-com:asm.v2";
							XNamespace xNamespace2 = "http://www.w3.org/2000/09/xmldsig#";
							XElement xElement2 = item2.Element(xNamespace + "hash");
							if (xElement2 == null)
							{
								continue;
							}
							xElement2 = xElement2.Element(xNamespace2 + "DigestValue");
							if (xElement2 == null || xElement2.Value.Equals("00000000000000000000000000000000000000000000"))
							{
								continue;
							}
							XAttribute xAttribute = item2.Attribute("name");
							XAttribute xAttribute2 = item2.Attribute("sourcePath");
							if (xAttribute2 != null)
							{
								path = xAttribute2.Value;
							}
							string path2;
							if (xAttribute != null)
							{
								path2 = xAttribute.Value;
							}
							else
							{
								path2 = Path.GetFileName(value);
								string directoryName = Path.GetDirectoryName(value);
								if (!string.IsNullOrEmpty(directoryName))
								{
									path = directoryName;
								}
							}
							string text3 = Path.Combine(inputDir, fileNameWithoutExtension, path, path2);
							text3 = text3.Replace(".\\", "");
							string path3 = ((!dictionary.ContainsKey(text3)) ? text3 : dictionary[text3]);
							FileStream inputStream = new FileStream(path3, FileMode.Open, FileAccess.Read);
							string text4 = Convert.ToBase64String(sHA.ComputeHash(inputStream)).Replace("-", string.Empty);
							int length = text4.Length;
							xElement2.SetValue(text4);
						}
					}
					xDocument.Save(tempFileName);
					LongPathFile.Copy(tempFileName, text2, true);
				}
				foreach (string item3 in list)
				{
					logger.LogInfo("PkgSignTool: Deleting temp manifest at {0}", item3);
					File.Delete(item3);
				}
			}

			protected override int DoExecution()
			{
				string parameterAsString = _cmdLineParser.GetParameterAsString("input");
				bool switchAsBoolean = _cmdLineParser.GetSwitchAsBoolean("incVersion");
				SetLoggingVerbosity(logger);
				PkgUpdate(parameterAsString, switchAsBoolean);
				return 0;
			}

			public UpdateCmdHandler()
			{
				_cmdLineParser.SetRequiredParameterString("input", "path to the directory containing the unpacked package");
				_cmdLineParser.SetOptionalSwitchBoolean("incVersion", "increases the version number of the package (default: false)", false);
				SetQuietCommand();
			}
		}

		private class RepackCmdHandler : QuietCmdHandler
		{
			public override string Command => "Repack";

			public override string Description => "Repacking an unpacked package and save to the specified file";

			private static void PkgRepack(string inputDir, string outputCab, CompressionType compressionType)
			{
				Dictionary<string, string> dictionary = ReadFileMap(inputDir);
				if (!LongPathDirectory.Exists(inputDir))
				{
					throw new DirectoryNotFoundException($"The specified input directory '{inputDir}' doesn't exist");
				}
				string text = Path.Combine(inputDir, c_OpaqueContainerCookieFileName);
				if (LongPathFile.Exists(text))
				{
					string[] array = LongPathFile.ReadAllLines(text);
					if (array.Length != 1)
					{
						throw new PackageException("Expected one line in {0}.  Found {1} lines.", text, array.Length);
					}
					string fullPathUNC = LongPath.GetFullPathUNC(array[0]);
					string fullPathUNC2 = LongPath.GetFullPathUNC(outputCab);
					if (fullPathUNC.Equals(fullPathUNC2, StringComparison.InvariantCultureIgnoreCase))
					{
						logger.LogInfo("PkgSignTool: Source from unpack matches target for repack for an opaque container - skipping repack.");
						return;
					}
					logger.LogInfo("PkgSignTool: Copying opaque container source indicated in {0} as {1} to the repack target: {2}", text, fullPathUNC, fullPathUNC2);
					LongPathFile.Copy(fullPathUNC, fullPathUNC2, true);
					return;
				}
				PackageStyle packageStyle = PackageStyle.SPKG;
				string text2 = Path.Combine(inputDir, PkgConstants.c_strDsmFile);
				if (!LongPathFile.Exists(text2))
				{
					text2 = Path.Combine(inputDir, PkgConstants.c_strMumFile);
					packageStyle = PackageStyle.CBS;
					if (!LongPathFile.Exists(text2))
					{
						throw new PackageException("The specified input directory '{0}' doesn't have man.dsm.xml or update.mum file", inputDir);
					}
				}
				string path = Path.Combine(inputDir, "files.txt");
				if (!LongPathFile.Exists(path))
				{
					throw new PackageException("The specified input directory '{0}' doesn't have files.txt", inputDir);
				}
				string[] array2 = LongPathFile.ReadAllLines(path);
				for (int i = 0; i < array2.Length; i++)
				{
					string[] array3 = array2[i].Split(',');
					if (array3.Length != 3)
					{
						throw new PackageException("Incorrect file information '{0}' in files.txt", array3);
					}
					DateTime time = DateTime.Parse(array3[2]);
					LongPathFile.SetAttributes(array3[0], FileAttributes.Normal);
					NativeMethods.SetCreationTimeLong(array3[0], time);
				}
				foreach (string key in dictionary.Keys)
				{
					LongPathFile.Copy(dictionary[key], key, true);
				}
				CabArchiver cab = new CabArchiver();
				List<FileEntryBase> list = new List<FileEntryBase>();
				string text3 = Path.Combine(inputDir, PkgConstants.c_strDiffDsmFile);
				if (!LongPathFile.Exists(text3))
				{
					PkgManifest pkgManifest = null;
					pkgManifest = ((packageStyle != PackageStyle.CBS) ? PkgManifest.Load(text2) : PkgManifest.Load_CBS(text2));
					if (outputCab == null)
					{
						outputCab = pkgManifest.Name + (pkgManifest.IsRemoval ? PkgConstants.c_strRemovalPkgExtension : PkgConstants.c_strPackageExtension);
					}
					pkgManifest.BuildSourcePaths(inputDir, BuildPathOption.UseCabPath);
					list.AddRange(pkgManifest.Files);
				}
				else
				{
					DiffPkgManifest diffPkgManifest = DiffPkgManifest.Load(text3);
					if (outputCab == null)
					{
						outputCab = diffPkgManifest.Name + PkgConstants.c_strDiffPackageExtension;
					}
					diffPkgManifest.BuildSourcePath(inputDir, BuildPathOption.UseCabPath);
					list.AddRange(diffPkgManifest.Files.Where((DiffFileEntry x) => x.DiffType != DiffType.Remove));
					cab.AddFile(PkgConstants.c_strDiffDsmFile, text3);
				}
				list.ForEach(delegate(FileEntryBase x)
				{
					cab.AddFile(x.CabPath, x.SourcePath);
				});
				cab.Save(outputCab, compressionType);
			}

			protected override int DoExecution()
			{
				string parameterAsString = _cmdLineParser.GetParameterAsString("input");
				string outputCab = (_cmdLineParser.IsAssignedSwitch("out") ? _cmdLineParser.GetSwitchAsString("out") : null);
				CompressionType compressionType = (CompressionType)Enum.Parse(typeof(CompressionType), _cmdLineParser.GetSwitchAsString("compress"), true);
				SetLoggingVerbosity(logger);
				PkgRepack(parameterAsString, outputCab, compressionType);
				return 0;
			}

			public RepackCmdHandler()
			{
				_cmdLineParser.SetRequiredParameterString("input", "directory of the unpacked package");
				_cmdLineParser.SetOptionalSwitchString("out", "path for the result package", ".\\<name from manifest>.spkg");
				_cmdLineParser.SetOptionalSwitchString("compress", "compression type", CompressionType.LZX.ToString(), false, Enum.GetNames(typeof(CompressionType)));
				SetQuietCommand();
			}
		}

		private static string c_OpaqueContainerCookieFileName = "PkgSignToolOpaqueContainer.txt";

		private static IULogger logger = new IULogger();

		private static Dictionary<string, string> CreateFileMap(string outputDir, PkgManifest manifest)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			int num = 0;
			FileEntry[] files = manifest.Files;
			foreach (FileEntry fileEntry in files)
			{
				if (fileEntry.FileType != FileType.Regular || fileEntry.SourcePath.EndsWith("update.mum", StringComparison.InvariantCultureIgnoreCase) || fileEntry.SourcePath.EndsWith("man.dsm.xml", StringComparison.InvariantCultureIgnoreCase) || (fileEntry.SourcePath.EndsWith("cat", StringComparison.InvariantCultureIgnoreCase) && Path.GetFileName(fileEntry.SourcePath) == fileEntry.CabPath))
				{
					continue;
				}
				string text;
				while (true)
				{
					text = Path.Combine(outputDir, num.ToString(CultureInfo.InvariantCulture) + Path.GetExtension(fileEntry.SourcePath));
					if (!File.Exists(text))
					{
						break;
					}
					num++;
				}
				LongPathFile.Move(fileEntry.SourcePath, text);
				dictionary.Add(text, fileEntry.SourcePath);
				fileEntry.SourcePath = text;
				num++;
			}
			return dictionary;
		}

		private static Dictionary<string, string> ReadFileMap(string inputDir)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string path = Path.Combine(inputDir, "filemap.csv");
			if (!File.Exists(path))
			{
				return dictionary;
			}
			using (StreamReader streamReader = new StreamReader(path))
			{
				while (!streamReader.EndOfStream)
				{
					string text = streamReader.ReadLine();
					string[] array = text.Split(',');
					if (2 != array.Length)
					{
						throw new Exception("Expected two entries in filemap.csv line.  Found " + array.Length + ".\nLine: " + text);
					}
					dictionary.Add(array[0], array[1]);
				}
				return dictionary;
			}
		}

		private static int Main(string[] args)
		{
			MultiCmdHandler multiCmdHandler = new MultiCmdHandler();
			multiCmdHandler.AddCmdHandler(new UnpackCmdHandler());
			multiCmdHandler.AddCmdHandler(new UpdateCmdHandler());
			multiCmdHandler.AddCmdHandler(new RepackCmdHandler());
			ProcessPrivilege.Adjust(PrivilegeNames.BackupPrivilege, true);
			ProcessPrivilege.Adjust(PrivilegeNames.SecurityPrivilege, true);
			int num = -1;
			try
			{
				num = multiCmdHandler.Run(args);
			}
			catch (Exception ex)
			{
				logger.LogException(ex);
				num = Marshal.GetHRForException(ex);
			}
			ProcessPrivilege.Adjust(PrivilegeNames.BackupPrivilege, false);
			ProcessPrivilege.Adjust(PrivilegeNames.SecurityPrivilege, false);
			return num;
		}
	}
}
