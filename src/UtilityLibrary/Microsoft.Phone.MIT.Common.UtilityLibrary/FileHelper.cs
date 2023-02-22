using System;
using System.IO;

namespace Microsoft.Phone.MIT.Common.UtilityLibrary
{
	public class FileHelper
	{
		public static bool FileCompare(string file1, string file2)
		{
			byte[] array = new byte[64];
			byte[] array2 = new byte[64];
			if (string.Compare(Path.GetFullPath(file1), Path.GetFullPath(file2), StringComparison.OrdinalIgnoreCase) == 0)
			{
				return true;
			}
			using (FileStream fileStream = new FileStream(file1, FileMode.Open, FileAccess.Read, FileShare.Read, 65536))
			{
				using (FileStream fileStream2 = new FileStream(file2, FileMode.Open, FileAccess.Read, FileShare.Read, 65536))
				{
					if (fileStream.Length != fileStream2.Length)
					{
						return false;
					}
					while (true)
					{
						bool flag = true;
						int num = fileStream.ReadReally(array, 64);
						num = fileStream2.ReadReally(array2, 64);
						if (num == 0)
						{
							break;
						}
						for (int i = 0; i < num; i++)
						{
							if (array[i] != array2[i])
							{
								return false;
							}
						}
					}
				}
			}
			return true;
		}

		public static bool FileCompareByLine(string file1, string file2, Func<int, string, string, bool> comparer = null)
		{
			int num = 0;
			using (StreamReader streamReader = new StreamReader(new FileStream(file1, FileMode.Open, FileAccess.Read, FileShare.Read, 65536)))
			{
				using (StreamReader streamReader2 = new StreamReader(new FileStream(file2, FileMode.Open, FileAccess.Read, FileShare.Read, 65536)))
				{
					string text;
					string text2;
					Int32 test;
					do
					{
						bool flag = true;
						text = (streamReader.EndOfStream ? null : streamReader.ReadLine());
						text2 = (streamReader2.EndOfStream ? null : streamReader2.ReadLine());
						num++;
						if (text == null && text2 == null)
						{
							return true;
						}

						test = string.Compare(text, text2, StringComparison.OrdinalIgnoreCase);
					}
					
					
					while (test == 0);
					return false;
				}
			}
		}

		public static bool CopyFileIfNecessary(string source, string destination)
		{
			FileInfo fileInfo = new FileInfo(source);
			FileInfo fileInfo2 = new FileInfo(destination);
			if (fileInfo2.Exists && fileInfo.Length == fileInfo2.Length && fileInfo.LastWriteTime == fileInfo2.LastWriteTime)
			{
				return false;
			}
			string directoryName = Path.GetDirectoryName(destination);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			File.Copy(source, destination, true);
			return true;
		}
	}
}
