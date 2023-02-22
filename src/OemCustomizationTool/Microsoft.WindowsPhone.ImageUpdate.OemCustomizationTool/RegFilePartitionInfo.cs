namespace Microsoft.WindowsPhone.ImageUpdate.OemCustomizationTool
{
	internal class RegFilePartitionInfo
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
