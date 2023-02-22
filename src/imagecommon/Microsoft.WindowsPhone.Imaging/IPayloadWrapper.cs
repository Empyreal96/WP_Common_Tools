namespace Microsoft.WindowsPhone.Imaging
{
	public interface IPayloadWrapper
	{
		void InitializeWrapper(long payloadSize);

		void ResetPosition();

		void Write(byte[] data);

		void FinalizeWrapper();
	}
}
