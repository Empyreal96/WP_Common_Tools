using System.IO;

namespace Microsoft.Phone.MIT.Common.UtilityLibrary
{
	public static class FileStreamExtensions
	{
		public static int ReadReally(this FileStream fs, byte[] destBuffer, int bytesToRead)
		{
			int num = 0;
			int num2;
			do
			{
				num2 = fs.Read(destBuffer, num, bytesToRead - num);
				num += num2;
			}
			while (num2 != 0 && num != bytesToRead);
			return num;
		}
	}
}
