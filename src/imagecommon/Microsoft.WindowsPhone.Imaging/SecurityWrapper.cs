using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Microsoft.WindowsPhone.Imaging
{
	public class SecurityWrapper : IPayloadWrapper
	{
		private IPayloadWrapper innerWrapper;

		private FullFlashUpdateImage ffuImage;

		private Task hashTask;

		private SHA256 sha;

		private byte[] hashData;

		private int bytesHashed;

		private int hashOffset;

		public byte[] CatalogData { get; private set; }

		public SecurityWrapper(FullFlashUpdateImage ffuImage, IPayloadWrapper innerWrapper)
		{
			this.ffuImage = ffuImage;
			this.innerWrapper = innerWrapper;
		}

		public void InitializeWrapper(long payloadSize)
		{
			if (payloadSize % (long)ffuImage.ChunkSizeInBytes != 0L)
			{
				throw new ImageCommonException("Data size not aligned with hash chunk size.");
			}
			sha = new SHA256CryptoServiceProvider();
			sha.Initialize();
			bytesHashed = 0;
			hashOffset = 0;
			uint num = (uint)(int)(payloadSize / (long)ffuImage.ChunkSizeInBytes) * ((uint)sha.HashSize / 8u);
			hashData = new byte[num];
			CatalogData = ImageSigner.GenerateCatalogFile(hashData);
			byte[] securityHeader = ffuImage.GetSecurityHeader(CatalogData, hashData);
			innerWrapper.InitializeWrapper(payloadSize + securityHeader.Length);
			innerWrapper.Write(securityHeader);
		}

		public void ResetPosition()
		{
			innerWrapper.ResetPosition();
		}

		public void Write(byte[] data)
		{
			HashBufferAsync(data);
			innerWrapper.Write(data);
		}

		public void FinalizeWrapper()
		{
			hashTask.Wait();
			hashTask = null;
			if (hashOffset != hashData.Length)
			{
				throw new ImageCommonException($"Failed to hash all data in the stream. hashOffset = {hashOffset}, hashData.Length = {hashData.Length}, bytesHashed = {bytesHashed}.");
			}
			CatalogData = ImageSigner.GenerateCatalogFile(hashData);
			byte[] securityHeader = ffuImage.GetSecurityHeader(CatalogData, hashData);
			innerWrapper.ResetPosition();
			innerWrapper.Write(securityHeader);
			ffuImage.CatalogData = CatalogData;
			ffuImage.HashTableData = hashData;
			innerWrapper.FinalizeWrapper();
		}

		private void HashBufferAsync(byte[] data)
		{
			if (hashTask != null)
			{
				hashTask.Wait();
			}
			hashTask = Task.Factory.StartNew(delegate
			{
				HashBuffer(data);
			});
		}

		private void HashBuffer(byte[] data)
		{
			int chunkSizeInBytes = (int)ffuImage.ChunkSizeInBytes;
			int num = chunkSizeInBytes - bytesHashed;
			for (int i = 0; i < data.Length; i += chunkSizeInBytes)
			{
				int num2 = num;
				if (data.Length - i < num)
				{
					num2 = data.Length;
				}
				byte[] hash = sha.ComputeHash(data, i, num2);
				bytesHashed += num2;
				bytesHashed %= chunkSizeInBytes;
				if (bytesHashed == 0)
				{
					CommitHashToTable(hash);
				}
				num = chunkSizeInBytes;
			}
		}

		private void CommitHashToTable(byte[] hash)
		{
			hash.CopyTo(hashData, hashOffset);
			hashOffset += hash.Length;
		}
	}
}
