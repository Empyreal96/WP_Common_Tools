namespace Microsoft.WindowsPhone.Multivariant.Offline
{
	public class RegFilePartitionInfo
	{
		public string regFilename;

		public string partition;

		public RegFilePartitionInfo(string filename, string partionString)
		{
			regFilename = filename;
			partition = partionString;
		}
	}
}
