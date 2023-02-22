namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public class MergeResult
	{
		public IPkgInfo PkgInfo { get; set; }

		public string FilePath { get; set; }

		public string[] Languages { get; set; }

		public string[] Resolutions { get; set; }

		public bool FeatureIdentifierPackage { get; set; }
	}
}
