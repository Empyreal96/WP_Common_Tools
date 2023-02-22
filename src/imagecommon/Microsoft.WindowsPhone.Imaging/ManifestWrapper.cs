namespace Microsoft.WindowsPhone.Imaging
{
	public class ManifestWrapper : IPayloadWrapper
	{
		private FullFlashUpdateImage ffuImage;

		private IPayloadWrapper innerWrapper;

		public ManifestWrapper(FullFlashUpdateImage ffuImage, IPayloadWrapper innerWrapper)
		{
			this.ffuImage = ffuImage;
			this.innerWrapper = innerWrapper;
		}

		public void InitializeWrapper(long payloadSize)
		{
			byte[] manifestRegion = ffuImage.GetManifestRegion();
			long payloadSize2 = payloadSize + manifestRegion.Length;
			innerWrapper.InitializeWrapper(payloadSize2);
			innerWrapper.Write(manifestRegion);
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
			innerWrapper.FinalizeWrapper();
		}
	}
}
