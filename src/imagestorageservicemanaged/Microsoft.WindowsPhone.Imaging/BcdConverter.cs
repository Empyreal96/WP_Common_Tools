using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdConverter
	{
		public static readonly string HiveBase = "HKEY_LOCAL_MACHINE\\BCD";

		public static readonly string Header = "Windows Registry Editor Version 5.00";

		private IULogger _logger;

		private BcdInput _bcdInput;

		[CLSCompliant(false)]
		public BcdConverter(IULogger logger)
		{
			_logger = logger;
		}

		public void ProcessInputXml(string bcdLayoutFile, string bcdLayoutSchema)
		{
			if (!File.Exists(bcdLayoutSchema))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: BCD layout schema file is not found: {bcdLayoutSchema}.");
			}
			using (FileStream bcdLayoutSchema2 = new FileStream(bcdLayoutSchema, FileMode.Open, FileAccess.Read))
			{
				ProcessInputXml(bcdLayoutFile, bcdLayoutSchema2);
			}
		}

		public void ProcessInputXml(string bcdLayoutFile, Stream bcdLayoutSchema)
		{
			if (!File.Exists(bcdLayoutFile))
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: BCD layout file is not found: {bcdLayoutFile}.");
			}
			BCDXsdValidator bCDXsdValidator = new BCDXsdValidator();
			try
			{
				bCDXsdValidator.ValidateXsd(bcdLayoutSchema, bcdLayoutFile, _logger);
			}
			catch (BCDXsdValidatorException innerException)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Failed to validate the BCD layout file: {bcdLayoutFile}.", innerException);
			}
			FileStream fileStream = null;
			XmlReader xmlReader = null;
			XmlSerializer xmlSerializer = null;
			try
			{
				fileStream = new FileStream(bcdLayoutFile, FileMode.Open, FileAccess.Read, FileShare.Read);
				xmlReader = XmlReader.Create(fileStream);
				xmlSerializer = new XmlSerializer(typeof(BcdInput));
				_bcdInput = (BcdInput)xmlSerializer.Deserialize(xmlReader);
			}
			catch (SecurityException innerException2)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Unable to access the BCD layout file.", innerException2);
			}
			catch (UnauthorizedAccessException innerException3)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Unable to open the BCD layout file for reading.", innerException3);
			}
			catch (InvalidOperationException innerException4)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: Unable to deserialize the BCD layout file.", innerException4);
			}
			finally
			{
				fileStream?.Close();
				xmlSerializer = null;
				xmlReader = null;
				fileStream = null;
			}
		}

		public void SaveToRegFile(Stream stream)
		{
			StreamWriter streamWriter = new StreamWriter(stream, Encoding.Unicode);
			try
			{
				if (_bcdInput.IncludeRegistryHeader)
				{
					streamWriter.WriteLine("{0}", Header);
					streamWriter.WriteLine();
				}
				if (_bcdInput.SaveKeyToRegistry)
				{
					streamWriter.WriteLine("[{0}]", HiveBase);
					streamWriter.WriteLine();
				}
				if (_bcdInput.IncludeDescriptions)
				{
					streamWriter.WriteLine("[{0}\\Description]", HiveBase);
					streamWriter.WriteLine("\"KeyName\"=\"BCD00000000\"");
					streamWriter.WriteLine("\"System\"=dword:{0:x8}", 1);
					streamWriter.WriteLine("\"TreatAsSystem\"=dword:{0:x8}", 1);
					streamWriter.WriteLine();
				}
				_bcdInput.SaveAsRegFile(streamWriter, HiveBase);
			}
			finally
			{
				streamWriter.Flush();
				streamWriter = null;
			}
		}

		public void SaveToRegData(BcdRegData bcdRegData)
		{
			if (_bcdInput.SaveKeyToRegistry)
			{
				bcdRegData.AddRegKey(HiveBase);
			}
			if (_bcdInput.IncludeDescriptions)
			{
				string regKey = $"{HiveBase}\\Description";
				bcdRegData.AddRegKey(regKey);
				bcdRegData.AddRegValue(regKey, "KeyName", "BCD00000000", "REG_SZ");
				bcdRegData.AddRegValue(regKey, "System", $"{1:x8}", "REG_DWORD");
				bcdRegData.AddRegValue(regKey, "TreatAsSystem", $"{1:x8}", "REG_DWORD");
			}
			_bcdInput.SaveAsRegData(bcdRegData, HiveBase);
		}

		public static void ConvertBCD(string inputFile, string outputFile)
		{
			BcdConverter bcdConverter = new BcdConverter(new IULogger());
			using (Stream bcdLayoutSchema = Assembly.GetExecutingAssembly().GetManifestResourceStream("BcdLayout.xsd"))
			{
				bcdConverter.ProcessInputXml(inputFile, bcdLayoutSchema);
			}
			using (FileStream stream = new FileStream(outputFile, FileMode.Create, FileAccess.ReadWrite))
			{
				bcdConverter.SaveToRegFile(stream);
			}
		}
	}
}
