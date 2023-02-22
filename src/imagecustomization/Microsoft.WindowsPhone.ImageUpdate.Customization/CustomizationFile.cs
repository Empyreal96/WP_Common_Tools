using System.IO;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization
{
	internal class CustomizationFile
	{
		public FileType FileType { get; private set; }

		public string Source { get; private set; }

		public string Destination { get; private set; }

		public CustomizationFile(string source, string destination)
			: this(FileType.Regular, source, destination)
		{
		}

		public CustomizationFile(FileType type, string source, string destination)
		{
			FileType = type;
			Source = source;
			Destination = RemoveDriveLetter(destination);
		}

		private static string RemoveDriveLetter(string path)
		{
			string pathRoot = Path.GetPathRoot(path);
			return "\\" + path.Substring(pathRoot.Length);
		}
	}
}
