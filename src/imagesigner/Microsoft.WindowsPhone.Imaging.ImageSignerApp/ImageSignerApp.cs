using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Microsoft.WindowsPhone.Imaging.ImageSignerApp
{
	public class ImageSignerApp
	{
		public static int Main(string[] args)
		{
			if (args.Length != 3)
			{
				return WriteUsage();
			}
			try
			{
				string text = args[0];
				string fullPath = Path.GetFullPath(args[1]);
				string fullPath2 = Path.GetFullPath(args[2]);
				switch (text.ToLower(CultureInfo.InvariantCulture))
				{
				case "sign":
					return SignImage(fullPath, fullPath2);
				case "getcatalog":
					return ExtractCatalog(fullPath, fullPath2);
				case "truncate":
					return TruncateImage(fullPath, fullPath2);
				default:
					Console.WriteLine("Unrecognized command: {0}", text);
					return WriteUsage();
				}
			}
			catch (FileNotFoundException ex)
			{
				Console.WriteLine("File not found: {0}", ex.FileName);
			}
			catch (FormatException ex2)
			{
				Console.WriteLine("FFU format invalid: {0}", ex2.Message);
			}
			catch (ImageCommonException ex3)
			{
				Console.WriteLine("Error occured in ImageCommon: {0}", ex3.Message);
			}
			catch (IOException ex4)
			{
				Console.WriteLine("File IO failed: {0}", ex4.ToString());
			}
			catch (UnauthorizedAccessException ex5)
			{
				Console.WriteLine("Unable to access file: {0}", ex5.Message);
			}
			return -1;
		}

		private static int WriteUsage()
		{
			Console.WriteLine("Usage:");
			Console.WriteLine("\timagesigner sign <FFU> <catalog file>");
			Console.WriteLine("\timagesigner getcatalog <FFU> <catalog file>");
			Console.WriteLine("\timagesigner truncate <FFU> <truncated FFU>");
			return -1;
		}

		private static int SignImage(string ffuPath, string catalogPath)
		{
			FileStream fileStream = File.Open(ffuPath, FileMode.Open, FileAccess.ReadWrite);
			uint num;
			using (FileStream fileStream2 = File.OpenRead(catalogPath))
			{
				num = (uint)fileStream2.Length;
			}
			uint headerSize;
			uint chunkSize;
			uint algorithmId;
			uint paddingSize;
			byte[] catalogData;
			byte[] hashData;
			ReadHeaderFromStream(fileStream, out headerSize, out chunkSize, out algorithmId, out paddingSize, out catalogData, out hashData);
			if (!ImageSigner.VerifyCatalogData(catalogPath, hashData))
			{
				Console.WriteLine("The specified catalog does not match the image, or the image has been corrupted.");
				return -1;
			}
			if (num > catalogData.Length + paddingSize)
			{
				uint bytesNeeded = (uint)((int)num - catalogData.Length) - paddingSize;
				uint dataOffset = (uint)((int)headerSize + catalogData.Length + hashData.Length) + paddingSize;
				ExtendImage(fileStream, dataOffset, bytesNeeded, chunkSize, ref paddingSize);
			}
			if (num < catalogData.Length && (uint)(catalogData.Length - (int)num + (int)paddingSize) > chunkSize)
			{
				uint excessPadding = (uint)(catalogData.Length - (int)num) + paddingSize;
				uint dataOffset2 = (uint)((int)headerSize + catalogData.Length + hashData.Length) + paddingSize;
				ShrinkImage(fileStream, dataOffset2, excessPadding, chunkSize, ref paddingSize);
			}
			fileStream.Position = headerSize - 8;
			byte[] bytes = BitConverter.GetBytes(num);
			fileStream.Write(bytes, 0, bytes.Length);
			byte[] bytes2 = BitConverter.GetBytes((uint)hashData.Length);
			fileStream.Write(bytes2, 0, bytes2.Length);
			byte[] array = File.ReadAllBytes(catalogPath);
			fileStream.Write(array, 0, array.Length);
			fileStream.Write(hashData, 0, hashData.Length);
			Console.WriteLine("Successfully signed image.");
			return 0;
		}

		private static void ShrinkImage(FileStream ffuFile, uint dataOffset, uint excessPadding, uint chunkSize, ref uint paddingSize)
		{
			long position = ffuFile.Position;
			uint num = Align(excessPadding - (chunkSize - 1), chunkSize);
			byte[] array = new byte[chunkSize];
			long num2 = dataOffset;
			long num3 = dataOffset - num;
			while (num2 < ffuFile.Length)
			{
				ffuFile.Position = num2;
				ffuFile.Read(array, 0, array.Length);
				num2 += chunkSize;
				ffuFile.Position = num3;
				ffuFile.Write(array, 0, array.Length);
				num3 += chunkSize;
			}
			ffuFile.SetLength(ffuFile.Length - num);
			paddingSize = excessPadding - num;
			ffuFile.Position = position;
		}

		private static void ExtendImage(FileStream ffuFile, uint dataOffset, uint bytesNeeded, uint chunkSize, ref uint paddingSize)
		{
			long position = ffuFile.Position;
			uint num = Align(bytesNeeded, chunkSize);
			byte[] array = new byte[chunkSize];
			ffuFile.SetLength(ffuFile.Length + num);
			long num2 = ffuFile.Length - chunkSize;
			long num3 = ffuFile.Length - num - chunkSize;
			while (num3 >= dataOffset)
			{
				ffuFile.Position = num3;
				ffuFile.Read(array, 0, array.Length);
				num3 -= chunkSize;
				ffuFile.Position = num2;
				ffuFile.Write(array, 0, array.Length);
				num2 -= chunkSize;
			}
			paddingSize += num;
			ffuFile.Position = position;
		}

		private static int ExtractCatalog(string ffuPath, string catalogPath)
		{
			byte[] catalogData;
			int num = ReadCatalogDataFromStream(File.OpenRead(ffuPath), out catalogData);
			if (num != 0)
			{
				return num;
			}
			File.WriteAllBytes(catalogPath, catalogData);
			Console.WriteLine("Successfully extracted catalog.");
			return 0;
		}

		private static int ReadCatalogDataFromStream(FileStream ffuFile, out byte[] catalogData)
		{
			uint headerSize;
			uint chunkSize;
			uint algorithmId;
			uint paddingSize;
			byte[] hashData;
			ReadHeaderFromStream(ffuFile, out headerSize, out chunkSize, out algorithmId, out paddingSize, out catalogData, out hashData);
			string text = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
			File.WriteAllBytes(text, catalogData);
			if (!ImageSigner.VerifyCatalogData(text, hashData))
			{
				Console.WriteLine("The catalog does not match the image, or the image has been corrupted.");
				return -1;
			}
			long firstChunkOffset = headerSize + catalogData.LongLength + hashData.LongLength + paddingSize;
			try
			{
				HashedChunkReader hashedChunkReader = new HashedChunkReader(hashData, ffuFile, chunkSize, firstChunkOffset);
				byte[] nextChunk = hashedChunkReader.GetNextChunk();
				uint num = BitConverter.ToUInt32(nextChunk, 0);
				if (num != 24)
				{
					Console.WriteLine("Unable to read manifest length from image.");
					return -1;
				}
				string text2 = "ImageFlash  ";
				if (!Encoding.ASCII.GetString(nextChunk, 4, text2.Length).Equals(text2))
				{
					Console.WriteLine("Invalid manifest signature encountered.");
					return -1;
				}
				for (uint num2 = BitConverter.ToUInt32(nextChunk, text2.Length + 4) + num; num2 > chunkSize; num2 -= chunkSize)
				{
					hashedChunkReader.GetNextChunk();
				}
				byte[] nextChunk2 = hashedChunkReader.GetNextChunk();
				BitConverter.ToUInt32(nextChunk2, 0);
				ushort num3 = BitConverter.ToUInt16(nextChunk2, 4);
				BitConverter.ToUInt16(nextChunk2, 6);
				if (num3 < 1 || num3 > 2)
				{
					Console.WriteLine("Image appears to be corrupt, or newer tools are required.");
					return -1;
				}
				int count = 192;
				string[] array = Encoding.ASCII.GetString(nextChunk2, 12, count).TrimEnd(default(char)).Split(default(char));
				if (array.Length == 0)
				{
					Console.WriteLine("Unable to read platform IDs from image.");
					return -1;
				}
				string[] array2 = array;
				foreach (string arg in array2)
				{
					Console.WriteLine("Platform ID: {0}", arg);
				}
			}
			catch (HashedChunkReaderException ex)
			{
				Console.WriteLine(ex.Message);
				return -1;
			}
			return 0;
		}

		private static int TruncateImage(string ffuPath, string outputPath)
		{
			FileStream fileStream = File.OpenRead(ffuPath);
			byte[] catalogData;
			int num = ReadCatalogDataFromStream(fileStream, out catalogData);
			if (num != 0)
			{
				return num;
			}
			long position = fileStream.Position;
			fileStream.Seek(0L, SeekOrigin.Begin);
			byte[] array = new byte[position];
			fileStream.Read(array, 0, array.Length);
			File.WriteAllBytes(outputPath, array);
			return 0;
		}

		private static uint Align(uint value, uint boundary)
		{
			return (value + (boundary - 1)) / boundary * boundary;
		}

		private static void ReadHeaderFromStream(FileStream ffu, out uint headerSize, out uint chunkSize, out uint algorithmId, out uint paddingSize, out byte[] catalogData, out byte[] hashData)
		{
			byte[] array = new byte[4];
			ffu.Read(array, 0, 4);
			headerSize = BitConverter.ToUInt32(array, 0);
			if (headerSize > 65536)
			{
				throw new FormatException("bad header size.");
			}
			byte[] array2 = new byte[headerSize];
			ffu.Read(array2, 0, array2.Length - 4);
			BinaryReader binaryReader = new BinaryReader(new MemoryStream(array2));
			byte[] array3 = new byte[12]
			{
				83, 105, 103, 110, 101, 100, 73, 109, 97, 103,
				101, 32
			};
			for (int i = 0; i < array3.Length; i++)
			{
				if (binaryReader.ReadByte() != array3[i])
				{
					throw new FormatException("bad signature in header.");
				}
			}
			chunkSize = binaryReader.ReadUInt32() * 1024;
			algorithmId = binaryReader.ReadUInt32();
			uint num = binaryReader.ReadUInt32();
			uint num2 = binaryReader.ReadUInt32();
			catalogData = new byte[num];
			if (num != 0)
			{
				ffu.Read(catalogData, 0, catalogData.Length);
			}
			hashData = new byte[num2];
			ffu.Read(hashData, 0, hashData.Length);
			uint num3 = headerSize + num + num2;
			paddingSize = Align(num3, chunkSize) - num3;
			ffu.Seek(0L, SeekOrigin.Begin);
		}
	}
}
