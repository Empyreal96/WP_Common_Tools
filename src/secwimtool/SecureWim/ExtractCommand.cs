using System;
using System.IO;
using SecureWim.Properties;

namespace SecureWim
{
	internal class ExtractCommand : IToolCommand
	{
		private string file;

		private string outputfile;

		public ExtractCommand(string[] args)
		{
			if (args.Length < 2)
			{
				throw new ArgParseException("Please specify a .secwim to extract a catalog from.", PrintUsage);
			}
			file = args[1];
			try
			{
				if (!Helpers.HasSdiHeader(file))
				{
					throw new ArgParseException($"The specified file does not appear to be a valid .secwim: {file}", PrintUsage);
				}
			}
			catch (IOException ex)
			{
				throw new ArgParseException(ex.Message, PrintUsage);
			}
			if (args.Length > 2)
			{
				outputfile = args[2];
			}
		}

		public static void PrintUsage()
		{
			Console.WriteLine(Resources.ExtractUsageString);
		}

		public int Run()
		{
			try
			{
				byte[] array;
				using (FileStream fileStream = File.OpenRead(file))
				{
					uint catalogSize = Helpers.GetCatalogSize(fileStream);
					Helpers.SeekStreamToCatalogStart(fileStream);
					array = new byte[catalogSize];
					fileStream.Read(array, 0, array.Length);
				}
				Stream stream = ((outputfile == null) ? Console.OpenStandardOutput() : File.OpenWrite(outputfile));
				stream.Write(array, 0, array.Length);
			}
			catch (IOException ex)
			{
				Console.WriteLine("\"extractcat\" command failed.  File IO operation encountered an error.");
				Console.WriteLine("Details: {0}", ex.Message);
				return -1;
			}
			return 0;
		}
	}
}
