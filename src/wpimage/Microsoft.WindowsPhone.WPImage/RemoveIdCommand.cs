using System;
using System.IO;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.WindowsPhone.WPImage
{
	internal class RemoveIdCommand : IWPImageCommand
	{
		private string _path;

		private IULogger _logger = new IULogger();

		public string Name => "RemoveId";

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
			long position;
			using (FileStream fileStream = fullFlashUpdateImage.GetImageStream())
			{
				position = fileStream.Position;
			}
			fullFlashUpdateImage = null;
			using (FileStream fileStream2 = new FileStream(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				fileStream2.Position = position;
				ImageStoreHeader imageStoreHeader = ImageStoreHeader.ReadFromStream(fileStream2);
				for (int i = 0; i < imageStoreHeader.PlatformIdentifier.Length; i++)
				{
					imageStoreHeader.PlatformIdentifier[i] = 0;
				}
				fileStream2.Position = position;
				imageStoreHeader.WriteToStream(fileStream2);
			}
			fullFlashUpdateImage = new FullFlashUpdateImage();
			try
			{
				fullFlashUpdateImage.Initialize(_path);
			}
			catch (Exception ex2)
			{
				Console.Error.WriteLine("Unable to initialize full flash update object.");
				Console.Error.WriteLine("{0}", ex2.Message);
				return;
			}
			string text = _path + ".tmp";
			IPayloadWrapper payloadWrapper = DismountCommand.GetPayloadWrapper(fullFlashUpdateImage, text, true);
			using (FileStream fileStream3 = fullFlashUpdateImage.GetImageStream())
			{
				payloadWrapper.InitializeWrapper(fileStream3.Length - fileStream3.Position);
				while (fileStream3.Position < fileStream3.Length)
				{
					byte[] array = new byte[fullFlashUpdateImage.ChunkSizeInBytes];
					fileStream3.Read(array, 0, array.Length);
					payloadWrapper.Write(array);
				}
			}
			payloadWrapper.FinalizeWrapper();
			payloadWrapper = null;
			File.Replace(text, _path, null);
		}
	}
}
