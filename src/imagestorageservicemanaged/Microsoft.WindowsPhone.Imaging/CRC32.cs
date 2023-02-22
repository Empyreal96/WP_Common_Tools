using System;
using System.Security.Cryptography;

namespace Microsoft.WindowsPhone.Imaging
{
	public class CRC32 : HashAlgorithm
	{
		private static readonly uint[] _crc32Table;

		private uint _crc32Value;

		private bool _hashCoreCalled;

		private bool _hashFinalCalled;

		public override byte[] Hash
		{
			get
			{
				if (!_hashCoreCalled)
				{
					throw new NullReferenceException();
				}
				if (!_hashFinalCalled)
				{
					throw new CryptographicException("Hash must be finalized before the hash value is retrieved.");
				}
				byte[] bytes = BitConverter.GetBytes(~_crc32Value);
				Array.Reverse(bytes);
				return bytes;
			}
		}

		public override int HashSize => 32;

		static CRC32()
		{
			_crc32Table = new uint[256];
			for (uint num = 0u; num < 256; num++)
			{
				uint num2 = num;
				for (int i = 0; i < 8; i++)
				{
					num2 = (((num2 & 1) == 0) ? (num2 >> 1) : (0xEDB88320u ^ (num2 >> 1)));
				}
				_crc32Table[num] = num2;
			}
		}

		public CRC32()
		{
			InitializeVariables();
		}

		public override void Initialize()
		{
			InitializeVariables();
		}

		private void InitializeVariables()
		{
			_crc32Value = uint.MaxValue;
			_hashCoreCalled = false;
			_hashFinalCalled = false;
		}

		protected override void HashCore(byte[] array, int ibStart, int cbSize)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (_hashFinalCalled)
			{
				throw new CryptographicException("Hash not valid for use in specified state.");
			}
			_hashCoreCalled = true;
			for (int i = ibStart; i < ibStart + cbSize; i++)
			{
				byte b = (byte)(_crc32Value ^ array[i]);
				_crc32Value = _crc32Table[b] ^ ((_crc32Value >> 8) & 0xFFFFFFu);
			}
		}

		protected override byte[] HashFinal()
		{
			_hashFinalCalled = true;
			return Hash;
		}
	}
}
