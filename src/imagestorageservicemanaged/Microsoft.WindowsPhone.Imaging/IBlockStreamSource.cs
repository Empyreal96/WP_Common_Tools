namespace Microsoft.WindowsPhone.Imaging
{
	internal interface IBlockStreamSource
	{
		long Length { get; }

		void ReadBlock(uint blockIndex, byte[] buffer, int bufferIndex);
	}
}
