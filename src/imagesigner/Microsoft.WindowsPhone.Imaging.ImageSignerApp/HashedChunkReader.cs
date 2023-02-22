using System.IO;
using System.Security.Cryptography;

namespace Microsoft.WindowsPhone.Imaging.ImageSignerApp
{
	internal class HashedChunkReader
	{
		private byte[] hashData;

		private FileStream imageData;

		private int curOffset;

		private int chunkSize;

		private SHA256 hasher;

		public HashedChunkReader(byte[] hashData, FileStream imageData, uint chunkSize, long firstChunkOffset)
		{
			imageData.Seek(firstChunkOffset, SeekOrigin.Begin);
			this.hashData = hashData;
			this.chunkSize = (int)chunkSize;
			this.imageData = imageData;
			hasher = new SHA256Cng();
			curOffset = 0;
		}

		public byte[] GetNextChunk()
		{
			byte[] array = new byte[chunkSize];
			if (imageData.Length - imageData.Position < array.LongLength)
			{
				throw new HashedChunkReaderException("Unabled to read next chunk: insufficient image data remaining.  Image may be truncated.");
			}
			imageData.Read(array, 0, array.Length);
			byte[] array2 = hasher.ComputeHash(array);
			for (int i = 0; i < array2.Length; i++)
			{
				if (array2[i] != hashData[curOffset])
				{
					throw new HashedChunkReaderException($"Hash data mismatch at table offset 0x{curOffset:x}.  The hash data does not match the image data, indicating corruption.");
				}
				curOffset++;
			}
			return array;
		}
	}
}
