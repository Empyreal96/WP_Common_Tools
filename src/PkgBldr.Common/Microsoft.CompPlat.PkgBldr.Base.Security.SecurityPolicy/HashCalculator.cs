using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.CompPlat.PkgBldr.Base.Security.SecurityPolicy
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

		public static string CalculateSha1Hash(string Value)
		{
			return CalculateHash(Value, HashType.Sha1);
		}

		public static string CalculateSha256Hash(string Value)
		{
			return CalculateHash(Value, HashType.Sha256);
		}

		private static string CalculateHash(string Value, HashType HashType)
		{
			byte[] array = null;
			StringBuilder stringBuilder = new StringBuilder();
			switch (HashType)
			{
			case HashType.Sha256:
				array = Sha256Hasher.ComputeHash(Encoding.Unicode.GetBytes(Value));
				break;
			case HashType.Sha1:
				array = Sha1Hasher.ComputeHash(Encoding.Unicode.GetBytes(Value));
				break;
			default:
				throw new PkgGenException("Invalid hash algorithm");
			}
			byte[] array2 = array;
			foreach (byte b in array2)
			{
				stringBuilder.Append(b.ToString("X2", CultureInfo.InvariantCulture));
			}
			return stringBuilder.ToString();
		}
	}
}
