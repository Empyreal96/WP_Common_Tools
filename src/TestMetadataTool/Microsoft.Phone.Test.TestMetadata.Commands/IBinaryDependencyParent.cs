namespace Microsoft.Phone.Test.TestMetadata.Commands
{
	internal interface IBinaryDependencyParent
	{
		string Name { get; }

		string Partition { get; }

		PackageFileRepository PackageFileRepository { get; }
	}
}
