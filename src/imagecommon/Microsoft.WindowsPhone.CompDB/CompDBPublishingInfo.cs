using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.WindowsPhone.CompDB
{
	[XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
	[XmlRoot(ElementName = "CompDBPublishingInfo", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class CompDBPublishingInfo
	{
		private IULogger _iuLogger;

		public static string c_CompDBPublishingInfoFileIdentifier = "_publish";

		public const string c_CompDBPublishingInfoVersion = "1.2";

		[XmlAttribute]
		public string Version = "1.2";

		[XmlAttribute]
		public string Product;

		[XmlAttribute]
		public Guid BuildID;

		[XmlAttribute]
		public string BuildInfo;

		[XmlAttribute]
		public string OSVersion;

		[XmlAttribute]
		public string BuildArch;

		[XmlAttribute]
		[DefaultValue(ReleaseType.Invalid)]
		public ReleaseType ReleaseType;

		[XmlAttribute]
		public string BSPVersion;

		[XmlAttribute]
		public string BSPProductName;

		[XmlArrayItem(ElementName = "Package", Type = typeof(CompDBPublishingPackageInfo), IsNullable = false)]
		[XmlArray]
		public List<CompDBPublishingPackageInfo> Packages;

		public CompDBPublishingInfo()
		{
		}

		public CompDBPublishingInfo(CompDBPublishingInfo srcInfo)
		{
			Version = srcInfo.Version;
			Packages = srcInfo.Packages.Select((CompDBPublishingPackageInfo pkg) => new CompDBPublishingPackageInfo(pkg)).ToList();
		}

		public CompDBPublishingInfo(BuildCompDB srcDB, IULogger logger)
		{
			_iuLogger = logger;
			Product = srcDB.Product;
			BuildID = srcDB.BuildID;
			BuildInfo = srcDB.BuildInfo;
			OSVersion = srcDB.OSVersion;
			BuildArch = srcDB.BuildArch;
			ReleaseType = srcDB.ReleaseType;
			if (srcDB is BSPCompDB)
			{
				BSPCompDB bSPCompDB = srcDB as BSPCompDB;
				BSPProductName = bSPCompDB.BSPProductName;
				BSPVersion = bSPCompDB.BSPVersion;
			}
			Packages = new List<CompDBPublishingPackageInfo>();
			foreach (CompDBPackageInfo package in srcDB.Packages)
			{
				Packages.AddRange(package.GetPublishingPackages());
			}
		}

		public static CompDBPublishingInfo ValidateAndLoad(string xmlFile, IULogger logger)
		{
			CompDBPublishingInfo result = new CompDBPublishingInfo();
			string text = string.Empty;
			string compDBPublishingInfoSchema = BuildPaths.CompDBPublishingInfoSchema;
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
			foreach (string text2 in manifestResourceNames)
			{
				if (text2.Contains(compDBPublishingInfoSchema))
				{
					text = text2;
					break;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				throw new ImageCommonException("ImageCommon::CompDBPublishingInfo!ValidateAndLoad: XSD resource was not found: " + compDBPublishingInfoSchema);
			}
			TextReader textReader = new StreamReader(LongPathFile.OpenRead(xmlFile));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(CompDBPublishingInfo));
			try
			{
				result = (CompDBPublishingInfo)xmlSerializer.Deserialize(textReader);
			}
			catch (Exception innerException)
			{
				throw new ImageCommonException("ImageCommon::CompDBPublishingInfo!ValidateAndLoad: Unable to parse CompDB Publishing Info XML file '" + xmlFile + "'", innerException);
			}
			finally
			{
				textReader.Close();
			}
			using (Stream xsdStream = executingAssembly.GetManifestResourceStream(text))
			{
				XsdValidator xsdValidator = new XsdValidator();
				try
				{
					xsdValidator.ValidateXsd(xsdStream, xmlFile, logger);
				}
				catch (XsdValidatorException innerException2)
				{
					throw new ImageCommonException("ImageCommon::CompDBPublishingInfo!ValidateAndLoad: Unable to validate CompDB Publishing Info XSD for file '" + xmlFile + "'.", innerException2);
				}
			}
			logger.LogInfo("CompDBPublishingInfo: Successfully validated the CompDB Publishing Info XML: {0}", xmlFile);
			return result;
		}

		public void WriteToFile(string xmlFile)
		{
			string directoryName = Path.GetDirectoryName(xmlFile);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			TextWriter textWriter = new StreamWriter(LongPathFile.OpenWrite(xmlFile));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(CompDBPublishingInfo));
			try
			{
				xmlSerializer.Serialize(textWriter, this);
			}
			catch (Exception innerException)
			{
				throw new ImageCommonException("ImageCommon::CompDBPublishingInfo!WriteToFile: Unable to write Publishing Info CompDB XML file '" + xmlFile + "'", innerException);
			}
			finally
			{
				textWriter.Close();
			}
		}

		public override string ToString()
		{
			return "Publishing Info: Count=" + ((Packages == null) ? "0" : (Packages.Count() + " (Version: " + Version + ")"));
		}
	}
}
