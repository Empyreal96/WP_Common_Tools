using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32.SafeHandles;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public static class PackageTools
	{
		private const uint FILE_ATTRIBUTE_NORMAL = 128u;

		private const short INVALID_HANDLE_VALUE = -1;

		private const uint GENERIC_READ = 2147483648u;

		private const uint GENERIC_WRITE = 1073741824u;

		private const uint CREATE_NEW = 1u;

		private const uint CREATE_ALWAYS = 2u;

		private const uint OPEN_EXISTING = 3u;

		private const uint FILE_SHARE_READ = 1u;

		public static byte[] CalculateFileHash(string file)
		{
			using (FileStream inputStream = LongPathFile.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (HashAlgorithm hashAlgorithm = HashAlgorithm.Create(PkgConstants.c_strHashAlgorithm))
				{
					return hashAlgorithm.ComputeHash(inputStream);
				}
			}
		}

		public static string MakeShortPath(string path, string prefix)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
			string extension = Path.GetExtension(path);
			string text = $"{prefix}_{((fileNameWithoutExtension.Length >= 8) ? fileNameWithoutExtension.Substring(0, 7) : fileNameWithoutExtension)}";
			if (!string.IsNullOrEmpty(extension) && extension.Length > 1)
			{
				text += extension;
			}
			return text.Replace(" ", "_");
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool GetFileSizeEx(SafeFileHandle hFile, out long lpFileSize);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern SafeFileHandle CreateFile(string fileName, uint desiredAccess, uint shareMode, IntPtr securityAttributes, uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile);

		private static long GetFileSize(string fileName)
		{
			using (SafeFileHandle safeFileHandle = CreateFile(fileName, 2147483648u, 1u, IntPtr.Zero, 3u, 128u, IntPtr.Zero))
			{
				if (safeFileHandle.IsInvalid)
				{
					throw new Exception("CreateFile() failed while calling GetFileSize().  GetLastError() = " + Marshal.GetLastWin32Error().ToString(CultureInfo.InvariantCulture) + "\nMake sure the file '" + fileName + "' exist and has sharing access.");
				}
				long lpFileSize;
				if (!GetFileSizeEx(safeFileHandle, out lpFileSize))
				{
					throw new Exception("GetFileSizeEx() failed while calling GetFileSize().  GetLastError() = " + Marshal.GetLastWin32Error().ToString(CultureInfo.InvariantCulture) + "\nMake sure the file '" + fileName + "' exist and has sharing access.");
				}
				return lpFileSize;
			}
		}

		private static bool IsFileEmpty(string fileName)
		{
			long fileSize = GetFileSize(fileName);
			return fileSize == 0;
		}

		public static void CreateCDF(string[] filePaths, string[] fileTags, string catalogPath, string packageName, string cdfFile)
		{
			using (TextWriter textWriter = new StreamWriter(LongPathFile.OpenWrite(cdfFile)))
			{
				textWriter.WriteLine("[CatalogHeader]");
				textWriter.WriteLine("Name={0}", Path.GetFileName(catalogPath));
				textWriter.WriteLine("ResultDir={0}", Path.GetDirectoryName(catalogPath));
				textWriter.WriteLine("PublicVersion=0x00000001");
				textWriter.WriteLine("EncodingType=0x00010001");
				textWriter.WriteLine("PageHashes=false");
				textWriter.WriteLine("CatalogVersion=2");
				textWriter.WriteLine("HashAlgorithms=SHA256");
				textWriter.WriteLine("CATATTR1=0x00010001:OSAttr:2:6.1,2:6.0,2:5.2,2:5.1");
				textWriter.WriteLine("CATATTR2=0x00010001:PackageName:{0}\r\n", packageName);
				textWriter.WriteLine();
				textWriter.WriteLine("[CatalogFiles]");
				for (int i = 0; i < filePaths.Length; i++)
				{
					string fullPathUNC = LongPath.GetFullPathUNC(filePaths[i]);
					if (!IsFileEmpty(fullPathUNC))
					{
						string arg = $"<HASH>{fileTags[i].Replace('=', '~')}";
						textWriter.WriteLine("{0}={1}", arg, fullPathUNC);
					}
				}
			}
		}

		public static void CreateCatalog(string[] sourcePaths, string[] devicePaths, string packageName, IPkgFileSigner signer, string catalogPath)
		{
			string tempFile = FileUtils.GetTempFile();
			try
			{
				if (sourcePaths.Length != devicePaths.Length)
				{
					throw new PackageException("sourcePaths and devicePaths must be of the same length");
				}
				CreateCDF(sourcePaths, devicePaths, catalogPath, packageName, tempFile);
				int num = 0;
				try
				{
					num = CommonUtils.RunProcess("makecat.exe", $"/v \"{tempFile}\"");
				}
				catch (Exception inner)
				{
					throw new PackageException(inner, "makecat.exe failed unexpectedly");
				}
				if (num != 0)
				{
					throw new PackageException("makecat.exe finished with non-zero exit code {0}", num);
				}
				if (!File.Exists(catalogPath))
				{
					throw new PackageException("Failed to create package integrity catalog file");
				}
				try
				{
					signer?.SignFile(catalogPath);
				}
				catch (Exception inner2)
				{
					throw new PackageException(inner2, "Failed to sign package integrity catalog file");
				}
			}
			finally
			{
				FileUtils.DeleteFile(tempFile);
			}
		}

		public static void CreateCatalog(string[] sourcePaths, string[] devicePaths, string packageName, string catalogPath)
		{
			IPkgFileSigner signer = new PkgFileSignerDefault();
			CreateCatalog(sourcePaths, devicePaths, packageName, signer, catalogPath);
		}

		public static void CheckCrossPartitionFiles(string pkgName, string partition, IEnumerable<string> devicePaths)
		{
			CheckCrossPartitionFiles(pkgName, partition, devicePaths, true);
		}

		public static void CheckCrossPartitionFiles(string pkgName, string partition, IEnumerable<string> devicePaths, bool logCrossers)
		{
			if (!partition.Equals(PkgConstants.c_strMainOsPartition, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			bool flag = false;
			foreach (string devicePath in devicePaths)
			{
				string[] c_strJunctionPaths = PkgConstants.c_strJunctionPaths;
				foreach (string value in c_strJunctionPaths)
				{
					if (devicePath.StartsWith(value, StringComparison.OrdinalIgnoreCase))
					{
						if (logCrossers)
						{
							LogUtil.Error("Package '{0}', File '{1}': Referencing files from '{2}' partition using junctions ({2}) is not allowed in production package. Changing ReleaseType to Test or set Partition attribute with the right partition, such as, Partition=\"Data\", and using the right path in DestinationDir", pkgName, devicePath, partition, string.Join(",", PkgConstants.c_strJunctionPaths));
						}
						flag = true;
					}
				}
			}
			if (!flag)
			{
				return;
			}
			throw new PackageException("Cross partition files found in production package '{0}'", pkgName);
		}

		public static void SignFileWithOptions(string file, string options)
		{
			int num = 0;
			if (options == null)
			{
				options = string.Empty;
			}
			try
			{
				CommonUtils.FindInPath("sign.cmd");
				num = CommonUtils.RunProcess("%COMSPEC%", $"/C sign.cmd {options} \"{file}\"");
			}
			catch (Exception innerException)
			{
				throw new PackageException(innerException, "Failed to sign the file {0} with options {1}", file, options);
			}
			if (num != 0)
			{
				throw new PackageException("Failed to sign file {0} with options {1}, exit code {2}", file, options, num);
			}
		}

		public static bool FileHasPageHashes(string fileName)
		{
			string fileName2 = CommonUtils.FindInPath("signtool.exe");
			Process process = new Process();
			process.StartInfo.FileName = fileName2;
			process.StartInfo.Arguments = $"verify /v /pa \"{fileName}\"";
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.Start();
			string text = process.StandardOutput.ReadToEnd();
			string text2 = process.StandardError.ReadToEnd();
			process.WaitForExit();
			if (text2.Contains("SignTool Error"))
			{
				throw new PackageException(text2);
			}
			return text.Contains("File has page hashes");
		}

		public static void SignFile(string file)
		{
			SignFileWithOptions(file, null);
		}

		public static string BuildPackageName(string owner, string component)
		{
			return BuildPackageName(owner, component, null, null, null);
		}

		public static string BuildPackageName(string owner, string component, string subComponent)
		{
			return BuildPackageName(owner, component, subComponent, null, null);
		}

		public static string BuildPackageName(string owner, string component, string subComponent, string culture)
		{
			return BuildPackageName(owner, component, subComponent, culture, null);
		}

		public static string BuildPackageName(string owner, string component, string subComponent, string culture, string resolution)
		{
			if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(component))
			{
				throw new PackageException("Owner and Component can't be null");
			}
			if (!string.IsNullOrEmpty(culture) && !string.IsNullOrEmpty(resolution))
			{
				throw new PackageException("Culture and Resolution can not be set in the same time");
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("{0}.{1}", owner, component);
			if (!string.IsNullOrEmpty(subComponent))
			{
				stringBuilder.AppendFormat(".{0}", subComponent);
			}
			if (!string.IsNullOrEmpty(culture))
			{
				stringBuilder.AppendFormat("_Lang_{0}", culture);
			}
			if (!string.IsNullOrEmpty(resolution))
			{
				stringBuilder.AppendFormat("_RES_{0}", resolution);
			}
			return stringBuilder.ToString();
		}

		public static string GetDefaultDriveLetter(string partitionName)
		{
			if (PkgConstants.c_strUpdateOsPartition.Equals(partitionName, StringComparison.OrdinalIgnoreCase))
			{
				return PkgConstants.c_strUpdateOSDrive;
			}
			return PkgConstants.c_strDefaultDrive;
		}
	}
}
