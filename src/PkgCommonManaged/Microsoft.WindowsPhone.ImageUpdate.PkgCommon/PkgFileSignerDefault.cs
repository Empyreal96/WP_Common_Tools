namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	internal class PkgFileSignerDefault : IPkgFileSigner
	{
		public void SignFile(string fileToSign)
		{
			PackageTools.SignFile(fileToSign);
		}

		public void SignFileWithOptions(string fileToSign, string options)
		{
			PackageTools.SignFileWithOptions(fileToSign, options);
		}
	}
}
