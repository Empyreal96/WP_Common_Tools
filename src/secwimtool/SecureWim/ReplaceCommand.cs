using System;
using System.IO;
using SecureWim.Properties;

namespace SecureWim
{
	internal class ReplaceCommand : IToolCommand
	{
		private string catalogFile;

		private string wimFile;

		public ReplaceCommand(string[] args)
		{
			if (args.Length < 3)
			{
				throw new ArgParseException("Expected both a catalog and a WIM to be specified.", PrintUsage);
			}
			catalogFile = args[1];
			wimFile = args[2];
			try
			{
				if (!Helpers.HasSdiHeader(wimFile))
				{
					throw new ArgParseException($"The specified .secwim does not appear to be valid: {wimFile}.", PrintUsage);
				}
			}
			catch (IOException ex)
			{
				throw new ArgParseException(ex.Message, PrintUsage);
			}
		}

		public static void PrintUsage()
		{
			Console.WriteLine(Resources.ReplaceUsageString);
		}

		public int Run()
		{
			byte[] array = File.ReadAllBytes(catalogFile);
			try
			{
				using (FileStream fileStream = File.Open(wimFile, FileMode.Open, FileAccess.ReadWrite))
				{
					Helpers.SeekStreamToCatalogStart(fileStream);
					fileStream.SetLength(fileStream.Position + array.Length + 4);
					fileStream.Write(array, 0, array.Length);
					Helpers.WriteUintToStream((uint)array.Length, fileStream);
				}
			}
			catch (IOException ex)
			{
				Console.WriteLine("\"replacecat\" command failed.  File IO operation encountered an error.");
				Console.WriteLine("Details: {0}", ex.Message);
				return -1;
			}
			return 0;
		}
	}
}
