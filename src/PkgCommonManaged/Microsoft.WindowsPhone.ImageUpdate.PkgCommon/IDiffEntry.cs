namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public interface IDiffEntry
	{
		FileType FileType { get; }

		DiffType DiffType { get; }

		string DevicePath { get; }
	}
}
