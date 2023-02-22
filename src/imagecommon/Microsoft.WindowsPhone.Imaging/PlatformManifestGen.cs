using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SecureBoot;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class PlatformManifestGen
	{
		private IULogger _logger;

		private string _signinfoPath;

		private bool _doSignInfo;

		private static List<string> _signInfoFiles = null;

		private PlatformManifest _pmAPI;

		public static string c_strSignInfoExtension = ".signinfo";

		public static string c_strSignInfoDir = "SignInfo";

		public static string c_strPlatformManifestMainOSDevicePath = "\\Windows\\System32\\PlatformManifest\\";

		public static string c_strPlatformManifestEFIESPDevicePath = "\\EFI\\Microsoft\\Boot\\PlatformManifest\\";

		public static string c_strPlatformManifestSubcomponent = "PlatformManifest";

		public static string c_strPlatformManifestExtension = ".pm";

		public static string c_strSignInfoEnabledEnvVar = "GENERATE_SIGNINFO_FILES";

		public StringBuilder ErrorMessages = new StringBuilder();

		public bool ErrorsFound => ErrorMessages.Length > 0;

		public PlatformManifestGen(Guid featureManifestID, string buildBranchInfo, string signInfoPath, ReleaseType releaseType, IULogger logger)
		{
			_logger = logger;
			_signinfoPath = signInfoPath;
			_pmAPI = new PlatformManifest(featureManifestID, buildBranchInfo);
			_pmAPI.ImageType = ((releaseType != ReleaseType.Production) ? PlatformManifest.ImageReleaseType.Test : PlatformManifest.ImageReleaseType.Retail);
			if (!string.IsNullOrWhiteSpace(_signinfoPath) && Directory.Exists(_signinfoPath))
			{
				_doSignInfo = true;
			}
			if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(c_strSignInfoEnabledEnvVar)))
			{
				if (!_doSignInfo)
				{
					throw new ImageCommonException("ImageCommon!PlatformManifestGen::PlatformManifestGen: The SignInfo Path does not exist '" + _signinfoPath + "' but is required.");
				}
			}
			else
			{
				_doSignInfo = false;
			}
			if (_doSignInfo && _signInfoFiles == null)
			{
				_signInfoFiles = (from file in new DirectoryInfo(_signinfoPath).GetFiles("*.signinfo")
					select file.Name.ToLower(CultureInfo.InvariantCulture)).ToList();
			}
		}

		public void AddPackages(List<IPkgInfo> packages)
		{
			foreach (IPkgInfo package in packages)
			{
				AddPackage(package);
			}
		}

		public void AddPackage(IPkgInfo package)
		{
			_pmAPI.AddStringEntry(package.Name);
			if (!_doSignInfo)
			{
				return;
			}
			foreach (IFileEntry file in package.Files)
			{
				if (!file.SignInfoRequired)
				{
					continue;
				}
				byte[] inArray = HexStringToByteArray(file.FileHash);
				string fileHash = Convert.ToBase64String(inArray).Replace('/', '-').ToLower(CultureInfo.InvariantCulture);
				if (fileHash == string.Empty)
				{
					_logger.LogWarning("Warning: File '{0}' in package '{1}' requires signInfo but has empty fileHash!", file.DevicePath, package.Name);
					continue;
				}
				List<string> list = _signInfoFiles.Where((string file2) => file2.Contains(fileHash)).ToList();
				if (list.Count() != 1)
				{
					if (list.Count() == 0)
					{
						string filename = Path.GetFileName(file.CabPath);
						if (_signInfoFiles.Any((string file3) => file3.StartsWith(filename, StringComparison.OrdinalIgnoreCase)))
						{
							ErrorMessages.AppendLine("Error: File '" + file.DevicePath + "' in package '" + package.Name + "' failed to find any SignInfo files using the following pattern: " + fileHash + " (Filename found but using different hashes)");
						}
						else
						{
							ErrorMessages.AppendLine("Error: File '" + file.DevicePath + "' in package '" + package.Name + "' failed to find any SignInfo files using the following pattern: " + fileHash);
						}
						continue;
					}
					string text = "Error: File '" + file.DevicePath + "' in package '" + package.Name + "' failed as we found multiple SignInfo files using the following pattern: " + fileHash;
					ErrorMessages.AppendLine(text);
					_logger.LogError(text);
					foreach (string item in list)
					{
						_logger.LogError($"Error: \tMatching file: '{item}'");
					}
				}
				else
				{
					string signinfoFile = Path.Combine(_signinfoPath, list[0]);
					try
					{
						_pmAPI.AddBinaryFromSignInfo(signinfoFile);
					}
					catch
					{
						ErrorMessages.AppendLine("Error: File '" + file.DevicePath + "' in package '" + package.Name + "' failed to be added to the Platform Manifest");
					}
				}
			}
		}

		public static byte[] HexStringToByteArray(string hex)
		{
			return (from x in Enumerable.Range(0, hex.Length)
				where x % 2 == 0
				select Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
		}

		public void WriteToFile(string outputFile)
		{
			_pmAPI.WriteToFile(outputFile);
		}
	}
}
