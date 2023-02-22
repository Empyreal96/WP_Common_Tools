using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.WindowsPhone.CompDB
{
	public class CompDBPayloadInfo
	{
		public enum CompDBPayloadInfoComparison
		{
			Standard,
			IgnorePayloadHash
		}

		public enum SatelliteTypes
		{
			Base,
			Language,
			Resolution,
			LangModel
		}

		public enum PayloadTypes
		{
			Canonical,
			Diff,
			ExpressPSF,
			ExpressCab
		}

		private static string c_ProductPhone = "Phone";

		private static string c_ProductDesktop = "Desktop";

		private static string c_PhoneChunkName = "MobileDeviceCritBinaries";

		private static string c_OnecoreChunkName = "SPKG_POSTBUILD";

		private static string c_Prebuilt = "Prebuilt";

		private CompDBPackageInfo _parentPkg;

		[XmlAttribute]
		public string PayloadHash;

		[DefaultValue(0)]
		[XmlAttribute]
		public long PayloadSize;

		[XmlAttribute]
		public string Path;

		[XmlAttribute]
		public string PreviousPath;

		[XmlAttribute]
		public PayloadTypes PayloadType;

		private CompDBChunkMapItem _chunkMapping;

		private string _chunkName;

		private string _chunkPath;

		[XmlIgnore]
		public string ChunkName
		{
			get
			{
				if (string.IsNullOrEmpty(_chunkName))
				{
					if (BuildCompDB.ChunkMapping != null)
					{
						if (_chunkMapping == null)
						{
							_chunkMapping = BuildCompDB.ChunkMapping.FindChunk(Path);
						}
						if (_chunkMapping != null)
						{
							_chunkName = _chunkMapping.ChunkName;
						}
					}
					else if (_isPhone)
					{
						_chunkName = c_PhoneChunkName;
					}
					else if (!_isDesktop)
					{
						_chunkName = c_OnecoreChunkName;
					}
				}
				return _chunkName;
			}
		}

		private bool _isPhone
		{
			get
			{
				if (_parentPkg != null && _parentPkg.ParentDB != null && _parentPkg.ParentDB.Product.Equals(c_ProductPhone, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				return false;
			}
		}

		private bool _isDesktop
		{
			get
			{
				if (_parentPkg != null && _parentPkg.ParentDB != null && _parentPkg.ParentDB.Product.Equals(c_ProductDesktop, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				return false;
			}
		}

		[XmlIgnore]
		public string ChunkPath
		{
			get
			{
				if (string.IsNullOrEmpty(_chunkPath))
				{
					if (BuildCompDB.ChunkMapping != null)
					{
						if (_chunkMapping == null)
						{
							_chunkMapping = BuildCompDB.ChunkMapping.FindChunk(Path);
						}
						if (_chunkMapping != null && string.IsNullOrEmpty(_chunkPath))
						{
							_chunkPath = Path.Substring(_chunkMapping.Path.Length).TrimStart('\\');
						}
					}
					else if (_isPhone)
					{
						_chunkPath = c_Prebuilt + "\\" + Path;
					}
					else if (!_isDesktop)
					{
						return Path;
					}
				}
				return _chunkPath;
			}
		}

		public CompDBPayloadInfo()
		{
		}

		public CompDBPayloadInfo(CompDBPayloadInfo payload)
		{
			Path = payload.Path;
			PreviousPath = payload.PreviousPath;
			PayloadHash = payload.PayloadHash;
			PayloadSize = payload.PayloadSize;
			PayloadType = payload.PayloadType;
		}

		public CompDBPayloadInfo(IPkgInfo pkgInfo, string payloadPath, string msPackageRoot, CompDBPackageInfo parentPkg, bool generateHash)
		{
			SetValues(pkgInfo, payloadPath, msPackageRoot, parentPkg, generateHash);
		}

		public CompDBPayloadInfo(FeatureManifest.FMPkgInfo pkgInfo, string msPackageRoot, CompDBPackageInfo parentPkg, bool generateHash)
		{
			SetValues(pkgInfo, msPackageRoot, parentPkg, generateHash);
		}

		public CompDBPayloadInfo(string payloadPath, string msPackageRoot, CompDBPackageInfo parentPkg, bool generateHash)
		{
			SetValues(payloadPath, msPackageRoot, parentPkg, generateHash);
		}

		public CompDBPayloadInfo SetParentPkg(CompDBPackageInfo parentPkg)
		{
			_parentPkg = parentPkg;
			return this;
		}

		public void SetPayloadHash(string payloadFile)
		{
			PayloadHash = GetPayloadHash(payloadFile);
			PayloadSize = GetPayloadSize(payloadFile);
		}

		public static string GetPayloadHash(string payloadFile)
		{
			return Convert.ToBase64String(PackageTools.CalculateFileHash(payloadFile));
		}

		public static long GetPayloadSize(string payloadFile)
		{
			long num = 0L;
			try
			{
				return new FileInfo(payloadFile).Length;
			}
			catch
			{
				return LongPathFile.ReadAllBytes(payloadFile).Length;
			}
		}

		public bool Equals(CompDBPayloadInfo pkg, CompDBPayloadInfoComparison compareType)
		{
			if (PayloadType != pkg.PayloadType)
			{
				return false;
			}
			if (!string.Equals(Path, pkg.Path, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			if (compareType == CompDBPayloadInfoComparison.IgnorePayloadHash)
			{
				return true;
			}
			if (string.IsNullOrEmpty(PayloadHash) != string.IsNullOrEmpty(pkg.PayloadHash) || (!string.IsNullOrEmpty(pkg.PayloadHash) && !string.Equals(PayloadHash, pkg.PayloadHash)))
			{
				return false;
			}
			return true;
		}

		public int GetHashCode(CompDBPayloadInfoComparison compareType)
		{
			int num = Path.ToUpper(CultureInfo.InvariantCulture).GetHashCode();
			if (compareType != CompDBPayloadInfoComparison.IgnorePayloadHash && !string.IsNullOrEmpty(PayloadHash))
			{
				num ^= PayloadHash.ToUpper(CultureInfo.InvariantCulture).GetHashCode();
			}
			return num;
		}

		public CompDBPayloadInfo ClearPayloadHash()
		{
			PayloadHash = null;
			PayloadSize = 0L;
			return this;
		}

		public CompDBPayloadInfo SetPath(string path)
		{
			Path = path;
			return this;
		}

		public CompDBPayloadInfo SetPreviousPath(string path)
		{
			PreviousPath = path;
			return this;
		}

		public override string ToString()
		{
			return Path + " (" + PayloadType.ToString() + ")";
		}

		private void SetValues(IPkgInfo pkgInfo, string payloadPath, string msPackageRoot, CompDBPackageInfo parentPkg, bool generateHash = false)
		{
			SetValues(payloadPath, msPackageRoot, parentPkg, generateHash);
		}

		private void SetValues(FeatureManifest.FMPkgInfo pkgInfo, string msPackageRoot, CompDBPackageInfo parentPkg, bool generateHash = false)
		{
			string text = System.IO.Path.ChangeExtension(pkgInfo.PackagePath, PkgConstants.c_strCBSPackageExtension);
			if (LongPathFile.Exists(text))
			{
				Package.LoadFromCab(text);
			}
			else
			{
				if (!LongPathFile.Exists(pkgInfo.PackagePath))
				{
					throw new ImageCommonException($"ImageCommon::CompDBPayloadInfo!SetValues: Payload file '{pkgInfo.PackagePath}' could not be found.");
				}
				Package.LoadFromCab(pkgInfo.PackagePath);
			}
			SetValues(pkgInfo.PackagePath, msPackageRoot, parentPkg, generateHash);
		}

		private void SetValues(string payloadPath, string msPackageRoot, CompDBPackageInfo parentPkg, bool generateHash = false)
		{
			char[] trimChars = new char[1] { '\\' };
			if (generateHash)
			{
				SetPayloadHash(payloadPath);
			}
			if (!string.IsNullOrEmpty(payloadPath))
			{
				Path = payloadPath.Replace(msPackageRoot, "", StringComparison.OrdinalIgnoreCase).Trim(trimChars);
			}
			_parentPkg = parentPkg;
		}
	}
}
