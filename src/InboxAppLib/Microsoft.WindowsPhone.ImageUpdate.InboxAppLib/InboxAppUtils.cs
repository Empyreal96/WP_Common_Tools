using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	public sealed class InboxAppUtils
	{
		private InboxAppUtils()
		{
			throw new NotSupportedException("The 'InboxAppUtils' class should never be constructed on its own. Please use only the static methods.");
		}

		public static bool ExtensionMatches(string filename, string extension)
		{
			return string.Compare(Path.GetExtension(filename), extension, StringComparison.OrdinalIgnoreCase) == 0;
		}

		public static void Unzip(string zipFile, string destinationDirectory)
		{
			ZipFile.ExtractToDirectory(zipFile, destinationDirectory);
		}

		public static string ValidateFileOrDir(string filepathOrDirpath, bool isDir)
		{
			if (string.IsNullOrWhiteSpace(filepathOrDirpath))
			{
				throw new ArgumentNullException("filepathOrDirpath", "The specified path is null or empty.");
			}
			if (isDir)
			{
				if (Directory.Exists(filepathOrDirpath))
				{
					Directory.CreateDirectory(filepathOrDirpath);
					if (!Directory.Exists(filepathOrDirpath))
					{
						throw new DirectoryNotFoundException(string.Format(CultureInfo.InvariantCulture, "The directory \"{0}\" cannot be used as a working directory.", new object[1] { filepathOrDirpath }));
					}
				}
			}
			else if (!File.Exists(filepathOrDirpath))
			{
				throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, "The file \"{0}\" cannot be used a parameter for InboxApp.", new object[1] { filepathOrDirpath }));
			}
			return filepathOrDirpath;
		}

		public static string ConvertTemporaryAppXProductIDFormatToGuid(string appXAppID)
		{
			string empty = string.Empty;
			string text = appXAppID.Replace("y", "-");
			text = string.Format(CultureInfo.InvariantCulture, "{{{0}}}", new object[1] { text.Trim('x') });
			Guid result = default(Guid);
			if (!Guid.TryParse(text, out result))
			{
				return appXAppID;
			}
			return string.Format(CultureInfo.InvariantCulture, "{{{0}}}", new object[1] { result.ToString() });
		}

		public static string MakePackageFullName(string title, string version, string processorArchitecture, string resourceId, string publisher)
		{
			uint packageFullNameLength = 256u;
			StringBuilder stringBuilder = new StringBuilder((int)packageFullNameLength);
			string empty = string.Empty;
			title.Replace(" ", string.Empty);
			string text = resourceId.Replace(" ", string.Empty);
			if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 2)
			{
				LogUtil.Diagnostic("Calling kernel32!PackageFullNameFromId to get PackageFullName");
				NativeMethods.PACKAGE_ID packageId = default(NativeMethods.PACKAGE_ID);
				packageId.name = title;
				switch (processorArchitecture.ToLowerInvariant())
				{
				case "x86":
					packageId.processorArchitecture = 0u;
					break;
				case "arm":
					packageId.processorArchitecture = 5u;
					break;
				case "x64":
					packageId.processorArchitecture = 9u;
					LogUtil.Warning("AppxManifest indicates processorArchitecture=\"x64\", which is not supported on Windows Phone. This application may not function correctly when installed.");
					break;
				case "neutral":
					packageId.processorArchitecture = 11u;
					break;
				default:
					LogUtil.Warning("AppxManifest indicates an unknown processorArchitecture=\"{0}\". Defaulting to \"neutral\".", processorArchitecture);
					packageId.processorArchitecture = 11u;
					break;
				}
				packageId.resourceId = resourceId;
				packageId.publisher = publisher;
				short result = 0;
				short result2 = 0;
				short result3 = 0;
				short result4 = 0;
				string[] array = version.Split('.');
				int num = array.Length;
				packageId.Major = (short)((num > 0 && short.TryParse(array[0], out result)) ? result : 0);
				packageId.Minor = (short)((num > 1 && short.TryParse(array[1], out result2)) ? result2 : 0);
				packageId.Build = (short)((num > 2 && short.TryParse(array[2], out result3)) ? result3 : 0);
				packageId.Revision = (short)((num >= 3 && short.TryParse(array[3], out result4)) ? result4 : 0);
				int num2 = NativeMethods.PackageFullNameFromId(ref packageId, ref packageFullNameLength, stringBuilder);
				if (num2 != 0)
				{
					throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "PackageFullNameFromId returned error {5}. One of the following fields may be empty or have an invalid value:\n(title)=\"{0}\" (version)=\"{1}\" (processorArchitecture)=\"{2}\" (resourceId)=\"{3}\" (publisher)=\"{4}\"", title, version, processorArchitecture, resourceId, publisher, num2), "PackageFullNameFromId(packageID)");
				}
				if (string.IsNullOrWhiteSpace(stringBuilder.ToString()))
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "INTERNAL ERROR: PackageFullNameFromId returned a blank PackageFullName!\n(title)=\"{0}\" (version)=\"{1}\" (processorArchitecture)=\"{2}\" (resourceId)=\"{3}\" (publisher)=\"{4}\"", title, version, processorArchitecture, resourceId, publisher));
				}
				empty = stringBuilder.ToString();
				LogUtil.Diagnostic(string.Format(CultureInfo.InvariantCulture, "Got PackageFullName: \"{0}\"", new object[1] { empty }));
			}
			else
			{
				string text2 = publisher.GetHashCode().ToString("x");
				empty = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}_{3}_{4}", title, version, processorArchitecture, text, text2);
				LogUtil.Warning(string.Format(CultureInfo.InvariantCulture, "Since pkggen is not running on Windows 8 or greater, the package full name \"{0}\" is a placeholder and may not be correct. Please run pkggen on Windows 8 or greater.", new object[1] { empty }));
			}
			return empty;
		}

		public static string ResolveDestinationPath(string someDestinationPath, bool isOnDataPartition, IPkgProject packageGenerator)
		{
			string text = someDestinationPath;
			if (isOnDataPartition && "$(runtime.data)" != someDestinationPath.Substring(0, "$(runtime.data)".Length))
			{
				text = "$(runtime.data)" + someDestinationPath;
			}
			LogUtil.Diagnostic(string.Format(CultureInfo.InvariantCulture, "DestinationPathToResolve = \"{0}\"", new object[1] { text }));
			string text2 = packageGenerator.MacroResolver.Resolve(text, MacroResolveOptions.ErrorOnUnknownMacro);
			LogUtil.Diagnostic(string.Format(CultureInfo.InvariantCulture, "resolvedDestinationPath = \"{0}\"", new object[1] { text2 }));
			return text2;
		}

		public static string MapNeutralToSpecificCulture(string neutralCulture)
		{
			string nlsForm = string.Empty;
			int num = NativeMethods.Bcp47GetNlsForm(neutralCulture, ref nlsForm);
			if (num != 0)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Bcp47GetNlsForm returned error {2}. One of the following fields may be empty or have an invalid value:\n(languageTag)=\"{0}\" (nlsForm)=\"{1}\"", new object[3] { neutralCulture, nlsForm, num }));
			}
			if (!string.IsNullOrEmpty(nlsForm))
			{
				return nlsForm.ToLowerInvariant();
			}
			return string.Empty;
		}

		public static string CalcHash(string filePath)
		{
			string result = string.Empty;
			if (File.Exists(filePath))
			{
				SHA256 sHA = SHA256.Create();
				FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				byte[] array = sHA.ComputeHash(fileStream);
				fileStream.Close();
				result = BitConverter.ToString(array);
			}
			return result;
		}
	}
}
