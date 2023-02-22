using System;
using System.IO;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.WindowsPhone.WPImage
{
	public class SigningWrapper : IPayloadWrapper
	{
		private IPayloadWrapper innerWrapper;

		private FullFlashUpdateImage ffuImage;

		public SigningWrapper(FullFlashUpdateImage ffuImage, IPayloadWrapper innerWrapper)
		{
			this.ffuImage = ffuImage;
			this.innerWrapper = innerWrapper;
		}

		public void InitializeWrapper(long payloadSize)
		{
			innerWrapper.InitializeWrapper(payloadSize);
		}

		public void ResetPosition()
		{
			innerWrapper.ResetPosition();
		}

		public void Write(byte[] data)
		{
			innerWrapper.Write(data);
		}

		public void FinalizeWrapper()
		{
			byte[] securityHeader = ffuImage.GetSecurityHeader(ffuImage.CatalogData, ffuImage.HashTableData);
			string tempFileName = Path.GetTempFileName();
			File.WriteAllBytes(tempFileName, ffuImage.CatalogData);
			SignFile(tempFileName);
			ffuImage.CatalogData = File.ReadAllBytes(tempFileName);
			byte[] securityHeader2 = ffuImage.GetSecurityHeader(ffuImage.CatalogData, ffuImage.HashTableData);
			if (securityHeader2.Length != securityHeader.Length)
			{
				throw new ImageStorageException("Signed catalog too large to fit in image.  Dismount without signing and use imagesigner.");
			}
			innerWrapper.ResetPosition();
			innerWrapper.Write(securityHeader2);
			innerWrapper.FinalizeWrapper();
		}

		private static void SignFile(string file)
		{
			int num = 0;
			try
			{
				num = CommonUtils.RunProcess("%COMSPEC%", $"/C sign.cmd \"{file}\"");
			}
			catch (Exception innerException)
			{
				throw new ImageStorageException($"Failed to sign the file {file}", innerException);
			}
			if (num != 0)
			{
				throw new ImageStorageException($"Failed to sign file {file}, exit code {num}");
			}
		}
	}
}
