using System;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public static class HashCalculator
	{
		private enum HashType
		{
			Sha256,
			Sha1
		}

		private static SHA256 Sha256Hasher = SHA256.Create();

		private static SHA1 Sha1Hasher = SHA1.Create();

		public static string CalculateSha1Hash(string value)
		{
			return CalculateHash(value, HashType.Sha1);
		}

		public static string CalculateSha256Hash(string value, bool forceNormalize)
		{
			if (forceNormalize)
			{
				return CalculateHash(NormalizedString.Get(value), HashType.Sha256);
			}
			return CalculateHash(value, HashType.Sha256);
		}

		private static string CalculateHash(string value, HashType hashType)
		{
			byte[] array = null;
			StringBuilder stringBuilder = new StringBuilder(null);
			try
			{
				if (hashType == HashType.Sha256)
				{
					array = Sha256Hasher.ComputeHash(Encoding.Unicode.GetBytes(value));
				}
				if (hashType == HashType.Sha1)
				{
					array = Sha1Hasher.ComputeHash(Encoding.Unicode.GetBytes(value));
				}
			}
			catch (ArgumentNullException originalException)
			{
				throw new PolicyCompilerInternalException("SecurityPolicyCompiler Internal Error: Calculating Hash for: " + value, originalException);
			}
			catch (ObjectDisposedException originalException2)
			{
				throw new PolicyCompilerInternalException("SecurityPolicyCompiler Internal Error: Calculating Hash for: " + value, originalException2);
			}
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("X2", GlobalVariables.Culture));
			}
			return stringBuilder.ToString();
		}
	}
}
