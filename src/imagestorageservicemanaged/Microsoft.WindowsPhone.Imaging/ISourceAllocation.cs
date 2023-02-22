namespace Microsoft.WindowsPhone.Imaging
{
	internal interface ISourceAllocation
	{
		uint GetAllocationSize();

		bool BlockIsAllocated(ulong diskByteOffset);
	}
}
