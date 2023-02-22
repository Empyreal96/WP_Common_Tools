using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class ImageSigner
	{
		[CLSCompliant(false)]
		public struct CRYPT_ATTR_BLOB
		{
			public uint cbData;

			public IntPtr pbData;
		}

		[CLSCompliant(false)]
		public struct CRYPTCATMEMBER
		{
			private uint cbStruct;

			[MarshalAs(UnmanagedType.LPWStr)]
			private string pwszReferenceTag;

			[MarshalAs(UnmanagedType.LPWStr)]
			private string pwszFileName;

			private Guid gSubjectType;

			private uint fdwMemberFlags;

			private IntPtr pIndirectData;

			private uint dwCertVersion;

			private uint dwReserved;

			private IntPtr hReserved;

			public CRYPT_ATTR_BLOB sEncodedIndirectData;

			private CRYPT_ATTR_BLOB sEncodedMemberInfo;
		}

		private FullFlashUpdateImage _ffuImage;

		private string _catalogFileName;

		private IULogger _logger;

		private SHA256 _sha256;

		private static Dictionary<string, bool> certPublicKeys = new Dictionary<string, bool>();

		private const string ProdCertRootThumbprint = "3B1EFD3A66EA28B16697394703A72CA340A05BD5";

		private const string TestCertRootThumbprint = "8A334AA8052DD244A647306A76B8178FA215F344";

		private const string FlightCertPCAThumbprint = "9E594333273339A97051B0F82E86F266B917EDB3";

		private const string FlightCertWindowsThumbprint = "5f444a6740b7ca2434c7a5925222c2339ee0f1b7";

		public ImageSigner()
		{
			_sha256 = new SHA256CryptoServiceProvider();
		}

		public void Initialize(FullFlashUpdateImage ffuImage, string catalogFile, IULogger logger)
		{
			_logger = logger;
			if (logger == null)
			{
				_logger = new IULogger();
			}
			_ffuImage = ffuImage;
			_catalogFileName = catalogFile;
		}

		public void SignFFUImage()
		{
			if (_ffuImage == null)
			{
				throw new ImageCommonException("ImageCommon!ImageSigner::SignFFUImage: ImageSigner has not been initialized.");
			}
			if (!File.Exists(Environment.ExpandEnvironmentVariables(_catalogFileName)))
			{
				throw new ImageCommonException("ImageCommon!ImageSigner::SignFFUImage: Unable to generate signed image - missing Catalog file: " + _catalogFileName);
			}
			if (!IsCatalogFile(IntPtr.Zero, _catalogFileName))
			{
				throw new ImageCommonException("ImageCommon!ImageSigner::SignFFUImage: The file '" + _catalogFileName + "' is not a catalog file.");
			}
			if (!HasSignature(_catalogFileName, true))
			{
				throw new ImageCommonException("ImageCommon!ImageSigner::SignFFUImage:  The file '" + _catalogFileName + "' is not signed.");
			}
			try
			{
				if (!VerifyCatalogData(_catalogFileName, _ffuImage.HashTableData))
				{
					throw new ImageCommonException("ImageCommon!ImageSigner::SignFFUImage: The catalog provided does not match the image.");
				}
				_ffuImage.CatalogData = File.ReadAllBytes(_catalogFileName);
			}
			catch (ImageCommonException)
			{
				throw;
			}
			catch (Exception ex2)
			{
				_logger.LogError("ImageCommon!ImageSigner::SignFFUImage: Error while signing FFU image: {0}", ex2.Message);
				throw new ImageCommonException("ImageCommon!ImageSigner::SignFFUImage: Exception occurred.", ex2);
			}
		}

		public void VerifyCatalog()
		{
			if (_ffuImage == null)
			{
				throw new ImageCommonException("ImageCommon!ImageSigner::VerifyCatalog: ImageSigner has not been initialized.");
			}
			if (_ffuImage.CatalogData == null || _ffuImage.CatalogData.Length == 0)
			{
				throw new ImageCommonException("ImageCommon!ImageSigner::VerifyCatalog: The FFU does not contain a catalog.");
			}
			if (!VerifyCatalogData(_ffuImage.CatalogData, _ffuImage.HashTableData))
			{
				throw new ImageCommonException("ImageCommon!ImageSigner::VerifyCatalog: The Catalog in the image does not match the Hash Table in the image.  The image appears to be corrupt or modified outside ImageApp.");
			}
			if (!VerifyHashTable())
			{
				throw new ImageCommonException("ImageCommon!ImageSigner::VerifyCatalog: The Hash Table in the image does not match the payload.  The image appears to be corrupt or modified outside ImageApp.");
			}
		}

		private bool VerifyCatalogData(byte[] catalogData, byte[] hashTableData)
		{
			_logger.LogInfo("ImageCommon: Verfiying Hash Table against catalog...");
			string tempFileName = Path.GetTempFileName();
			File.WriteAllBytes(tempFileName, catalogData);
			bool result = VerifyCatalogData(tempFileName, hashTableData);
			File.Delete(tempFileName);
			return result;
		}

		public static bool VerifyCatalogData(string catalogFile, byte[] hashTableData)
		{
			SHA1Managed sHA1Managed = new SHA1Managed();
			byte[] catalogHash = GetCatalogHash(catalogFile);
			byte[] array = sHA1Managed.ComputeHash(hashTableData);
			if (catalogHash.Length != array.Length)
			{
				return false;
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (catalogHash[i] != array[i])
				{
					return false;
				}
			}
			return true;
		}

		internal bool VerifyHashTable()
		{
			int num = 0;
			byte[] array = null;
			int num2 = 0;
			if (_ffuImage == null)
			{
				throw new ImageCommonException("ImageCommon!ImageSigner::VerifyHashTable: ImageSigner has not been initialized.");
			}
			_logger.LogInfo("ImageCommon: Verfiying Hash Table entries...");
			_logger.LogInfo("ImageCommon: Using Chunksize: {0}KB", _ffuImage.ChunkSize);
			try
			{
				byte[] hashTableData = _ffuImage.HashTableData;
				using (FileStream fileStream = _ffuImage.GetImageStream())
				{
					fileStream.Position = _ffuImage.StartOfImageHeader;
					array = GetFirstChunkHash(fileStream);
					num2++;
					while (array != null)
					{
						for (int i = 0; i < array.Length; i++)
						{
							if (num > hashTableData.Length)
							{
								throw new ImageCommonException("ImageCommon!ImageSigner::VerifyHashTable: Hash Table too small for this FFU.");
							}
							if (array[i] != hashTableData[num])
							{
								_logger.LogInfo("ImageCommon!ImageSigner::VerifyHashTable: Failed to match Chunk {0} Hash value [{1}]: {2} with {3}", num2, i, array[i].ToString("X2"), hashTableData[num].ToString("X2"));
								throw new ImageCommonException("ImageCommon!ImageSigner::VerifyHashTable: Hash Table entry does not match hash of FFU.");
							}
							num++;
						}
						array = GetNextChunkHash(fileStream);
						num2++;
					}
				}
				_logger.LogInfo("ImageCommon: The Hash Table has been sucessfully verified..");
			}
			catch (Exception innerException)
			{
				throw new ImageCommonException("ImageCommon!ImageSigner::VerifyHashTable: Error while retrieving Hash Table from FFU", innerException);
			}
			return true;
		}

		public static byte[] GenerateCatalogFile(byte[] hashData)
		{
			string tempFileName = Path.GetTempFileName();
			string tempFileName2 = Path.GetTempFileName();
			string tempFileName3 = Path.GetTempFileName();
			File.WriteAllBytes(tempFileName3, hashData);
			using (StreamWriter streamWriter = new StreamWriter(tempFileName2))
			{
				streamWriter.WriteLine("[CatalogHeader]");
				streamWriter.WriteLine("Name={0}", tempFileName);
				streamWriter.WriteLine("[CatalogFiles]");
				streamWriter.WriteLine("{0}={1}", "HashTable.blob", tempFileName3);
			}
			using (Process process = new Process())
			{
				process.StartInfo.FileName = "MakeCat.exe";
				process.StartInfo.Arguments = $"\"{tempFileName2}\"";
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.RedirectStandardOutput = true;
				try
				{
					process.Start();
					process.WaitForExit();
				}
				catch (Exception innerException)
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendFormat("CDF File: {0}\n", tempFileName2);
					if (!File.Exists(tempFileName2))
					{
						stringBuilder.AppendFormat("CDF File could not be found.\n");
					}
					try
					{
						stringBuilder.AppendFormat("Arguments : {0}\n", process.StartInfo.Arguments);
					}
					catch
					{
					}
					try
					{
						stringBuilder.AppendFormat("StandardError : {0}\n", process.StandardError);
					}
					catch
					{
					}
					try
					{
						stringBuilder.AppendFormat("StandardOutput : {0}\n", process.StandardOutput);
					}
					catch
					{
					}
					throw new ImageCommonException(stringBuilder.ToString(), innerException);
				}
				if (process.ExitCode != 0)
				{
					throw new ImageCommonException("ImageCommon!ImageSigner::GenerateCatalogFile: Failed call to MakeCat.");
				}
			}
			byte[] result = File.ReadAllBytes(tempFileName);
			File.Delete(tempFileName);
			File.Delete(tempFileName3);
			File.Delete(tempFileName2);
			return result;
		}

		private uint GetSecurityDataSize()
		{
			return _ffuImage.GetSecureHeader.ByteCount + _ffuImage.GetSecureHeader.CatalogSize + _ffuImage.GetSecureHeader.HashTableSize + _ffuImage.SecurityPadding;
		}

		private byte[] GetFirstChunkHash(Stream stream)
		{
			stream.Position = GetSecurityDataSize();
			return GetNextChunkHash(stream);
		}

		private byte[] GetNextChunkHash(Stream stream)
		{
			byte[] array = new byte[_ffuImage.ChunkSizeInBytes];
			if (stream.Position == stream.Length)
			{
				return null;
			}
			stream.Read(array, 0, array.Length);
			return _sha256.ComputeHash(array);
		}

		public static bool HasSignature(string filename, bool EnsureMicrosoftIssuer)
		{
			X509Certificate2 x509Certificate = null;
			bool value = false;
			try
			{
				x509Certificate = new X509Certificate2(filename);
				if (EnsureMicrosoftIssuer)
				{
					if (!certPublicKeys.TryGetValue(x509Certificate.Thumbprint, out value))
					{
						X509Chain x509Chain = new X509Chain(true);
						x509Chain.Build(x509Certificate);
						bool ignoreCase = true;
						X509ChainElementEnumerator enumerator = x509Chain.ChainElements.GetEnumerator();
						while (enumerator.MoveNext())
						{
							X509ChainElement current = enumerator.Current;
							if (string.Compare("3B1EFD3A66EA28B16697394703A72CA340A05BD5", current.Certificate.Thumbprint, ignoreCase, CultureInfo.InvariantCulture) == 0 || string.Compare("9E594333273339A97051B0F82E86F266B917EDB3", current.Certificate.Thumbprint, ignoreCase, CultureInfo.InvariantCulture) == 0 || string.Compare("5f444a6740b7ca2434c7a5925222c2339ee0f1b7", current.Certificate.Thumbprint, ignoreCase, CultureInfo.InvariantCulture) == 0 || string.Compare("8A334AA8052DD244A647306A76B8178FA215F344", current.Certificate.Thumbprint, ignoreCase, CultureInfo.InvariantCulture) == 0)
							{
								value = true;
								break;
							}
						}
						enumerator = x509Chain.ChainElements.GetEnumerator();
						while (enumerator.MoveNext())
						{
							X509ChainElement current2 = enumerator.Current;
							certPublicKeys[current2.Certificate.Thumbprint] = value;
						}
						return value;
					}
					return value;
				}
				return x509Certificate != null && !string.IsNullOrEmpty(x509Certificate.Subject);
			}
			catch
			{
				return false;
			}
		}

		[DllImport("WinTrust.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CryptCATOpen(string pwszFileName, uint fdwOpenFlags, IntPtr hProv, uint dwPublicVersion, uint dwEncodingType);

		[DllImport("WinTrust.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool CryptCATClose(IntPtr hCatalog);

		[DllImport("WinTrust.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CryptCATEnumerateMember(IntPtr hCatalog, IntPtr pPrevMember);

		[DllImport("WinTrust.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool IsCatalogFile(IntPtr hFile, string pwszFileName);

		internal static byte[] GetCatalogHash(string catalogFile)
		{
			IntPtr intPtr = new IntPtr(-1);
			IntPtr intPtr2 = intPtr;
			IntPtr zero = IntPtr.Zero;
			byte[] array = null;
			try
			{
				intPtr2 = CryptCATOpen(catalogFile, 2u, IntPtr.Zero, 0u, 0u);
				IntPtr intPtr3 = CryptCATEnumerateMember(intPtr2, IntPtr.Zero);
				if (intPtr3 == IntPtr.Zero)
				{
					throw new ImageCommonException("ImageCommon!ImageSigner::GetCatalogHash: Failed to get the Hash Table Hash from the Catalog '" + catalogFile + "'.  The catalog appears to be corrupt.");
				}
				CRYPTCATMEMBER obj = (CRYPTCATMEMBER)Marshal.PtrToStructure(intPtr3, typeof(CRYPTCATMEMBER));
				array = new byte[20];
				Marshal.Copy(IntPtr.Add(offset: (int)obj.sEncodedIndirectData.cbData - array.Length, pointer: obj.sEncodedIndirectData.pbData), array, 0, array.Length);
				return array;
			}
			catch (Exception ex)
			{
				throw new ImageCommonException("ImageCommon!ImageSigner::GetCatalogHash: Failed to get the Hash Table Hash from the Catalog: " + ex.Message);
			}
			finally
			{
				if (intPtr2 != intPtr)
				{
					CryptCATClose(intPtr2);
					intPtr2 = intPtr;
				}
			}
		}
	}
}
