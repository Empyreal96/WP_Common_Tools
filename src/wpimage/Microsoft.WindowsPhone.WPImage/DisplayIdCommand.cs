using System;
using System.IO;
using System.Text;
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.WindowsPhone.WPImage
{
	internal class DisplayIdCommand : IWPImageCommand
	{
		private string _path;

		public string Name => "DisplayId";

		public bool ParseArgs(string[] args)
		{
			if (args.Length < 1)
			{
				return false;
			}
			_path = args[0];
			if (!File.Exists(_path))
			{
				Console.Error.WriteLine("The file {0} does not exist.", _path);
				return false;
			}
			return true;
		}

		public void PrintUsage()
		{
			Console.WriteLine("{0} {1} image_path", "WPImage.exe", Name);
			Console.WriteLine("  image_path should point to a Windows Phone 8 FFU");
		}

		public void Run()
		{
			FullFlashUpdateImage fullFlashUpdateImage = new FullFlashUpdateImage();
			try
			{
				fullFlashUpdateImage.Initialize(_path);
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("Unable to initialize full flash update object.");
				Console.Error.WriteLine("{0}", ex.Message);
				return;
			}
			ImageStoreHeader imageStoreHeader = null;
			using (FileStream stream = fullFlashUpdateImage.GetImageStream())
			{
				imageStoreHeader = ImageStoreHeader.ReadFromStream(stream);
			}
			string arg = "<empty string>";
			byte[] platformIdentifier = imageStoreHeader.PlatformIdentifier;
			for (int i = 0; i < platformIdentifier.Length; i++)
			{
				if (platformIdentifier[i] != 0)
				{
					arg = Encoding.ASCII.GetString(imageStoreHeader.PlatformIdentifier);
					break;
				}
			}
			Console.WriteLine("Platform ID: {0}", arg);
		}
	}
}
