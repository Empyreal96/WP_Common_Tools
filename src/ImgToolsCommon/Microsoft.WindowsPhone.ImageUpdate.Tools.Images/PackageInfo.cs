using System;
using System.IO;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Images
{
	public class PackageInfo
	{
		private IPkgInfo _package;

		private IULogger _logger;

		private string _PackageFile;

		private static string _dwordFormatString = "{0} (0x{0:X})";

		private ASCIIEncoding _enc = new ASCIIEncoding();

		private string _tempDirectoryPath = string.Empty;

		private bool _cleanUpTempDir;

		public PackageInfo(string tempFileDirectory)
		{
			SetTempDirectoryPath(tempFileDirectory);
		}

		public PackageInfo(IPkgInfo package, string tempFileDirectory)
		{
			_package = package;
			SetTempDirectoryPath(tempFileDirectory);
		}

		public PackageInfo(string fileName, string tempFileDirectory)
		{
			if (tempFileDirectory != null)
			{
				SetTempDirectoryPath(tempFileDirectory);
			}
			else
			{
				SetTempDirectoryPath(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
				_cleanUpTempDir = true;
			}
			_logger = new IULogger();
			_PackageFile = fileName;
			try
			{
				_package = Package.LoadFromCab(_PackageFile);
			}
			catch (Exception ex)
			{
				throw new ImagesException("Tools.ImgCommon!PackageInfo: Failed to load Package file '" + fileName + "' : " + ex.Message, ex);
			}
		}

		public void Dispose()
		{
			if (_cleanUpTempDir)
			{
				FileUtils.DeleteTree(_tempDirectoryPath);
				_tempDirectoryPath = string.Empty;
			}
		}

		private void SetTempDirectoryPath(string tempFileDirectory)
		{
			_tempDirectoryPath = tempFileDirectory;
			if (!Directory.Exists(_tempDirectoryPath))
			{
				Directory.CreateDirectory(_tempDirectoryPath);
			}
			_tempDirectoryPath = FileUtils.GetShortPathName(_tempDirectoryPath);
		}

		public string DisplayFileName()
		{
			return DisplayFileName(_PackageFile);
		}

		public string DisplayFileName(string packageFile)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("****************************************************************************");
			stringBuilder.AppendLine("****************************************************************************");
			stringBuilder.AppendLine("File: " + packageFile);
			stringBuilder.AppendLine("****************************************************************************");
			stringBuilder.AppendLine("****************************************************************************");
			stringBuilder.AppendLine();
			return stringBuilder.ToString();
		}

		public string DisplayPackage(bool fSummary, bool fDisplayHeader)
		{
			return DisplayPackage(_package, fSummary, fDisplayHeader);
		}

		public static string DisplayPackage(IPkgInfo package, bool fSummary, bool fDisplayHeader)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (!fSummary || (fSummary && fDisplayHeader))
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("{0,-50:G}\t", "Package Name");
				stringBuilder.AppendFormat("{0,-15:G}\t", "Version");
				stringBuilder.AppendFormat("{0,-15:G}\t", "Canonical\\Diff");
				stringBuilder.AppendFormat("{0,-20:G}\t", "Build");
				stringBuilder.AppendFormat("{0,-10:G}\t", "File Count");
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("__________________________________________________\t_______________\t_______________\t____________________\t__________");
			}
			stringBuilder.AppendFormat("{0,-50:G}\t", package.Name);
			stringBuilder.AppendFormat("{0,-15:G}\t", package.Version);
			stringBuilder.AppendFormat("{0,-15:G}\t", package.Type);
			stringBuilder.AppendFormat("{0,-20:G}\t", package.BuildString);
			stringBuilder.AppendFormat("{0,-10:G}\t", package.FileCount);
			stringBuilder.AppendLine();
			if (!fSummary)
			{
				stringBuilder.AppendLine("[Package Details]");
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("\t[Package]");
				stringBuilder.AppendFormat("\t\tName: {0}", package.Name);
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("\t\tOwner: {0}", package.Owner);
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("\t\tComponent: {0}", package.Component);
				stringBuilder.AppendLine();
				if (!string.IsNullOrEmpty(package.SubComponent))
				{
					stringBuilder.AppendFormat("\t\tSubComponent: {0}", package.SubComponent);
					stringBuilder.AppendLine();
				}
				stringBuilder.AppendFormat("\t\tPartition: {0}", package.Partition);
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("\t\tVersion: ");
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("\t\t\tMajor: {0}", package.Version.Major);
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("\t\t\tMinor: {0}", package.Version.Minor);
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("\t\t\tQFE: {0}", package.Version.QFE);
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("\t\t\tBuild: {0}", package.Version.Build);
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("\t\tOwner Type: {0}", package.OwnerType);
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("\t\tRelease Type: {0}", package.ReleaseType);
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("\t\tBuild Type: {0}", package.BuildType);
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("\t\tCPU: {0}", package.CpuType);
				stringBuilder.AppendLine();
				if (!string.IsNullOrEmpty(package.Culture))
				{
					stringBuilder.AppendFormat("\t\tCulture: {0}", package.Culture);
					stringBuilder.AppendLine();
				}
				if (!string.IsNullOrEmpty(package.Resolution))
				{
					stringBuilder.AppendFormat("\t\tResolution: {0}", package.Resolution);
					stringBuilder.AppendLine();
				}
				stringBuilder.AppendFormat("\t\tPackage Type: {0}", package.Type);
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("\t\tBuild: {0}", package.BuildString);
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("\t\tGroup Key: {0}", package.GroupingKey);
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("\t\tFile Count: {0}", package.FileCount);
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("\t\t[File Summary for package {0}]", package.Name);
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.AppendFormat("\t\t{0,-50:G}\t", "Filename");
				stringBuilder.AppendFormat("{0,-15:G}\t", "Size (bytes)");
				stringBuilder.AppendFormat("{0,-25:G}\t", "Attributes");
				stringBuilder.AppendFormat("{0,-15:G}\t", "Type");
				stringBuilder.AppendFormat("{0,-35:G}\t", "Device Path");
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("\t\t__________________________________________________\t_______________\t_________________________\t_______________\t___________________________________");
				foreach (IFileEntry file in package.Files)
				{
					string fileName = Path.GetFileName(file.DevicePath);
					string arg = file.DevicePath.Replace(fileName, "", StringComparison.OrdinalIgnoreCase);
					stringBuilder.AppendFormat("\t\t{0,-50:G}\t", fileName);
					stringBuilder.AppendFormat("{0,-15:G}\t", file.Size);
					stringBuilder.AppendFormat("{0,-25:G}\t", file.Attributes);
					stringBuilder.AppendFormat("{0,-15:G}\t", file.FileType);
					stringBuilder.AppendFormat("{0,-35:G}\t", arg);
					stringBuilder.AppendLine();
				}
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("\t\t[Files]");
				foreach (IFileEntry file2 in package.Files)
				{
					stringBuilder.AppendLine("\t\t\t[File]");
					string fileName = Path.GetFileName(file2.DevicePath);
					string arg = file2.DevicePath.Replace(fileName, "", StringComparison.OrdinalIgnoreCase);
					stringBuilder.AppendFormat("\t\t\t\tName: {0}", fileName);
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("\t\t\t\tSize: " + _dwordFormatString + " bytes", file2.Size, file2.Size);
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("\t\t\t\tAttributes: {0}", file2.Attributes);
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("\t\t\t\tPath: {0}", arg);
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("\t\t\t\tPackage: {0}", package.Name);
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("\t\t\t\tFileType: {0}", file2.FileType);
					stringBuilder.AppendLine();
					stringBuilder.AppendFormat("\t\t\t\tSource Package: {0}", file2.SourcePackage);
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
					stringBuilder.AppendLine();
				}
			}
			return stringBuilder.ToString();
		}

		public string DisplayPackages(string packagesInfo)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("[Packages]");
			stringBuilder.AppendLine();
			stringBuilder.Append(packagesInfo);
			return stringBuilder.ToString();
		}
	}
}
