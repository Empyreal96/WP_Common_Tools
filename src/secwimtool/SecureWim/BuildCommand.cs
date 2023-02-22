using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SecureWim.Properties;

namespace SecureWim
{
	internal class BuildCommand : IToolCommand
	{
		private string wimPath;

		private string outputPath;

		private uint wimSize;

		private string[] platformIds;

		private Guid[] serialNumbers;

		public BuildCommand(string[] args)
		{
			if (args.Length < 2)
			{
				throw new ArgParseException("Expected at least two arguments.", PrintUsage);
			}
			wimPath = args[1];
			try
			{
				if (!Helpers.HasWimHeader(wimPath))
				{
					throw new ArgParseException($"Specified file does not appear to be a valid WIM: {wimPath}", PrintUsage);
				}
			}
			catch (IOException ex)
			{
				throw new ArgParseException(ex.Message, PrintUsage);
			}
			if (args.Length < 3)
			{
				throw new ArgParseException("Expected an output file argument.", PrintUsage);
			}
			outputPath = null;
			Regex regex = new Regex("^[/-]platform$", RegexOptions.Compiled);
			Regex regex2 = new Regex("^[/-]serial$", RegexOptions.Compiled);
			for (int i = 2; i < args.Length; i++)
			{
				if (regex.IsMatch(args[i]))
				{
					i++;
					if (i >= args.Length)
					{
						throw new ArgParseException("No platform IDs specified.", PrintUsage);
					}
					CheckEmpty(platformIds);
					platformIds = args[i].Split(';');
				}
				else if (regex2.IsMatch(args[i]))
				{
					i++;
					if (i >= args.Length)
					{
						throw new ArgParseException("No device serial number specified.", PrintUsage);
					}
					try
					{
						CheckEmpty(serialNumbers);
						serialNumbers = Helpers.GetGuids(args[i].Split(';'));
					}
					catch (FormatException ex2)
					{
						throw new ArgParseException(ex2.Message, PrintUsage);
					}
				}
				else
				{
					CheckEmpty(outputPath);
					outputPath = args[i];
				}
			}
			if (outputPath == null)
			{
				throw new ArgParseException("Expected an output file argument but none was specified.", PrintUsage);
			}
		}

		public static void PrintUsage()
		{
			Console.WriteLine(Resources.BuildUsageString);
		}

		public int Run()
		{
			try
			{
				using (MemoryStream memStm = new MemoryStream())
				{
					WriteSdiAndWim(memStm);
					GenerateTargetingData(memStm);
					GenerateCatalog(memStm);
					WriteToOutputFile(memStm);
				}
			}
			catch (BuildCommandException ex)
			{
				Console.WriteLine("\"build\" command failed.");
				Console.WriteLine("Details: {0}", ex.Message);
				Console.WriteLine("Exit code: 0x{0:x}", ex.ErrorCode);
				return -1;
			}
			catch (IOException ex2)
			{
				Console.WriteLine("\"build\" command failed.  File IO operation encountered an error.");
				Console.WriteLine("Details: {0}", ex2.Message);
				return -1;
			}
			catch (UnauthorizedAccessException ex3)
			{
				Console.WriteLine("\"build\" command failed. Problem accessing file.");
				Console.WriteLine("Details: {0}", ex3.Message);
				return -1;
			}
			return 0;
		}

		private void CheckEmpty(object p)
		{
			if (p != null)
			{
				throw new ArgParseException("Invalid command line specified: extra arguments present.", PrintUsage);
			}
		}

		private void WriteSdiAndWim(MemoryStream memStm)
		{
			byte[] sdiData = Resources.sdiData;
			byte[] array = File.ReadAllBytes(wimPath);
			byte[][] array2 = new byte[2][] { sdiData, array };
			foreach (byte[] array3 in array2)
			{
				memStm.Write(array3, 0, array3.Length);
			}
			Helpers.AddPadding(memStm, 4u);
			wimSize = (uint)(memStm.Length - sdiData.Length);
		}

		private void GenerateTargetingData(MemoryStream memStm)
		{
			long length = memStm.Length;
			if (platformIds != null)
			{
				Helpers.WriteUintToStream(1952541808u, memStm);
				Helpers.WriteUintToStream(0u, memStm);
				string[] array = platformIds;
				foreach (string s in array)
				{
					byte[] bytes = Encoding.ASCII.GetBytes(s);
					memStm.Write(bytes, 0, bytes.Length);
					memStm.WriteByte(0);
				}
				memStm.WriteByte(0);
				Helpers.AddPadding(memStm, 4u);
			}
			long num = memStm.Length - length;
			length = memStm.Length;
			if (serialNumbers != null)
			{
				Helpers.WriteUintToStream(1769366884u, memStm);
				Helpers.WriteUintToStream((uint)num, memStm);
				Helpers.WriteUintToStream((uint)serialNumbers.Length, memStm);
				Guid[] array2 = serialNumbers;
				foreach (Guid guid in array2)
				{
					byte[] array3 = guid.ToByteArray();
					memStm.Write(array3, 0, array3.Length);
				}
				num = memStm.Length - length;
				length = memStm.Length;
				if (memStm.Length % 4 != 0L)
				{
					throw new BuildCommandException("Device IDs not DWORD aligned", -1);
				}
			}
			uint num2 = 1702521203u;
			uint num3 = (uint)num;
			uint num4 = (uint)Resources.sdiData.Length;
			uint num5 = wimSize;
			uint[] array4 = new uint[4] { num2, num3, num4, num5 };
			for (int i = 0; i < array4.Length; i++)
			{
				Helpers.WriteUintToStream(array4[i], memStm);
			}
			if (memStm.Length % 4 != 0L)
			{
				throw new BuildCommandException("Size structure not DWORD aligned", -1);
			}
		}

		private void GenerateCatalog(MemoryStream memStm)
		{
			string tempFileName = Path.GetTempFileName();
			string tempFileName2 = Path.GetTempFileName();
			string tempFileName3 = Path.GetTempFileName();
			string arg = $"<hash>{tempFileName3}";
			File.WriteAllBytes(tempFileName3, memStm.ToArray());
			using (StreamWriter streamWriter = new StreamWriter(tempFileName2))
			{
				streamWriter.WriteLine("[CatalogHeader]");
				streamWriter.WriteLine("CatalogVersion=2");
				streamWriter.WriteLine("HashAlgorithms=SHA256");
				streamWriter.WriteLine("Name={0}", tempFileName);
				streamWriter.WriteLine("[CatalogFiles]");
				streamWriter.WriteLine("{0}={1}", arg, tempFileName3);
			}
			using (Process process = new Process())
			{
				process.StartInfo.FileName = "makecat.exe";
				process.StartInfo.Arguments = $"\"{tempFileName2}\"";
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.CreateNoWindow = true;
				process.Start();
				process.WaitForExit();
				if (process.ExitCode != 0)
				{
					throw new BuildCommandException("Failed to run makecat.exe.", process.ExitCode);
				}
			}
			byte[] array = File.ReadAllBytes(tempFileName);
			memStm.Write(array, 0, array.Length);
			Helpers.WriteUintToStream((uint)array.Length, memStm);
			File.Delete(tempFileName);
			File.Delete(tempFileName3);
			File.Delete(tempFileName2);
		}

		private void WriteToOutputFile(MemoryStream memStm)
		{
			using (FileStream fileStream = File.OpenWrite(outputPath))
			{
				fileStream.SetLength(memStm.Length);
				memStm.WriteTo(fileStream);
			}
		}
	}
}
