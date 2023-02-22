namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public interface IPkgFileSigner
	{
		void SignFile(string fileToSign);

		void SignFileWithOptions(string fileToSign, string options);
	}
}
