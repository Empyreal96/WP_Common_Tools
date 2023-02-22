using System.Text;

namespace Microsoft.WindowsPhone.Imaging
{
	public static class FullFlashUpdateHeaders
	{
		public static uint SecurityHeaderSize => (uint)(FullFlashUpdateImage.SecureHeaderSize + GetSecuritySignature().Length);

		public static uint ImageHeaderSize => (uint)(FullFlashUpdateImage.ImageHeaderSize + GetImageSignature().Length);

		public static byte[] GetSecuritySignature()
		{
			return Encoding.ASCII.GetBytes("SignedImage ");
		}

		public static byte[] GetImageSignature()
		{
			return Encoding.ASCII.GetBytes("ImageFlash  ");
		}
	}
}
