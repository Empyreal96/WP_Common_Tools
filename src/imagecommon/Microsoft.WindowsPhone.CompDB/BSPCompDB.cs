using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.WindowsPhone.CompDB
{
	[XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
	[XmlRoot(ElementName = "CompDB", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class BSPCompDB : BuildCompDB
	{
		public const string c_BSPCompDBRevision = "1";

		public const string c_BSPCompDBSchemaVersion = "1.3";

		[XmlAttribute]
		public string BSPVersion;

		[XmlAttribute]
		public string BSPProductName;

		public BSPCompDB()
		{
		}

		public BSPCompDB(IULogger logger)
		{
			_iuLogger = logger;
			if (Features == null)
			{
				Features = new List<CompDBFeature>();
			}
			if (Packages == null)
			{
				Packages = new List<CompDBPackageInfo>();
			}
		}

		public void GenerateBSPCompDB(string oemInputXMLFile, string fmDirectory, string msPackageRoot, string buildType, CpuId buildArch, string buildInfo)
		{
			Revision = "1";
			SchemaVersion = "1.3";
			BuildInfo = buildInfo;
			BuildArch = buildArch.ToString();
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			OEMInput xmlInput = new OEMInput();
			OEMInput.ValidateInput(ref xmlInput, oemInputXMLFile, _iuLogger, msPackageRoot, buildArch.ToString());
			CompDBFeature compDBFeature = new CompDBFeature(FeatureManifest.PackageGroups.BASE.ToString(), "", CompDBFeature.CompDBFeatureTypes.MobileFeature, OwnerType.OEM.ToString());
			compDBFeature.Packages = new List<CompDBFeaturePackage>();
			foreach (string additionalFM in xmlInput.AdditionalFMs)
			{
				string text = ((!additionalFM.ToUpper(CultureInfo.InvariantCulture).Contains("$(FMDIRECTORY)")) ? Path.Combine(fmDirectory, Path.GetFileName(additionalFM)) : Environment.ExpandEnvironmentVariables(additionalFM).ToUpper(CultureInfo.InvariantCulture).Replace("$(FMDIRECTORY)", fmDirectory, StringComparison.OrdinalIgnoreCase));
				FeatureManifest fm = new FeatureManifest();
				FeatureManifest.ValidateAndLoad(ref fm, text, _iuLogger);
				fm.OemInput = xmlInput;
				List<string> packageFileList = fm.GetPackageFileList();
				StringBuilder stringBuilder2 = new StringBuilder();
				bool flag2 = false;
				foreach (string item3 in packageFileList)
				{
					IPkgInfo pkgInfo = null;
					try
					{
						pkgInfo = Package.LoadFromCab(item3);
						CompDBFeaturePackage item = new CompDBFeaturePackage(pkgInfo.Name, false);
						compDBFeature.Packages.Add(item);
						CompDBPackageInfo item2 = new CompDBPackageInfo(pkgInfo, item3, msPackageRoot, text, this, true, false);
						Packages.Add(item2);
					}
					catch (FileNotFoundException)
					{
						flag2 = true;
						stringBuilder2.AppendFormat("\t{0}\n", (pkgInfo == null) ? item3 : pkgInfo.Name);
					}
				}
				if (flag2)
				{
					flag = true;
					stringBuilder.AppendFormat("\nThe FM File '{0}' following package file(s) could not be found: \n {1}", text, stringBuilder2.ToString());
				}
			}
			if (flag)
			{
				throw new ImageCommonException("ImageCommon::BSPCompDB!GetBSPCompDB: Errors processing FM File(s):\n" + stringBuilder.ToString());
			}
			Features.Add(compDBFeature);
			Packages = Packages.Distinct().ToList();
		}

		public new static BSPCompDB ValidateAndLoad(string xmlFile, IULogger logger)
		{
			BSPCompDB bSPCompDB = new BSPCompDB();
			string text = string.Empty;
			string bSPCompDBSchema = BuildPaths.BSPCompDBSchema;
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
			foreach (string text2 in manifestResourceNames)
			{
				if (text2.Contains(bSPCompDBSchema))
				{
					text = text2;
					break;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				throw new ImageCommonException("ImageCommon::BSPCompDB!ValidateAndLoad: XSD resource was not found: " + bSPCompDBSchema);
			}
			TextReader textReader = new StreamReader(LongPathFile.OpenRead(xmlFile));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(BSPCompDB));
			try
			{
				bSPCompDB = (BSPCompDB)xmlSerializer.Deserialize(textReader);
			}
			catch (Exception innerException)
			{
				throw new ImageCommonException("ImageCommon::BSPCompDB!ValidateAndLoad: Unable to parse BSP CompDB XML file '" + xmlFile + "'", innerException);
			}
			finally
			{
				textReader.Close();
			}
			bool flag = "1.2".Equals(bSPCompDB.SchemaVersion);
			using (Stream xsdStream = executingAssembly.GetManifestResourceStream(text))
			{
				XsdValidator xsdValidator = new XsdValidator();
				try
				{
					xsdValidator.ValidateXsd(xsdStream, xmlFile, logger);
				}
				catch (XsdValidatorException innerException2)
				{
					if (flag)
					{
						throw new ImageCommonException("ImageCommon::BSPCompDB!ValidateAndLoad: Unable to validate BSP CompDB XSD for file '" + xmlFile + "'.", innerException2);
					}
					logger.LogWarning("Warning: ImageCommon::BSPCompDB!ValidateAndLoad: Unable to validate BSP CompDB XSD for file '" + xmlFile + "'.");
					if (string.IsNullOrEmpty(bSPCompDB.SchemaVersion))
					{
						logger.LogWarning("Warning: ImageCommon::BSPCompDB!ValidateAndLoad: Schema Version was not given in BSP CompDB. Most up to date Schema Version is {1}.", "1.3");
					}
					else
					{
						logger.LogWarning("Warning: ImageCommon::BSPCompDB!ValidateAndLoad: Schema Version given in BSP CompDB ({0}) does not match most up to date Schema Version ({1}).", bSPCompDB.SchemaVersion, "1.3");
					}
				}
			}
			logger.LogInfo("BSPCompDB: Successfully validated the BSP CompDB XML: {0}", xmlFile);
			BuildCompDB parentDB = bSPCompDB;
			bSPCompDB.Packages = bSPCompDB.Packages.Select((CompDBPackageInfo pkg) => pkg.SetParentDB(parentDB)).ToList();
			if (bSPCompDB.ReleaseType == ReleaseType.Invalid)
			{
				if (bSPCompDB.Packages.Any((CompDBPackageInfo pkg) => pkg.ReleaseType == ReleaseType.Test))
				{
					bSPCompDB.ReleaseType = ReleaseType.Test;
				}
				else
				{
					bSPCompDB.ReleaseType = ReleaseType.Production;
				}
			}
			return bSPCompDB;
		}

		public override void WriteToFile(string xmlFile)
		{
			string directoryName = Path.GetDirectoryName(xmlFile);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			SchemaVersion = "1.3";
			Revision = "1";
			TextWriter textWriter = new StreamWriter(LongPathFile.OpenWrite(xmlFile));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(BSPCompDB));
			try
			{
				xmlSerializer.Serialize(textWriter, this);
			}
			catch (Exception innerException)
			{
				throw new ImageCommonException("ImageCommon::BSPCompDB!WriteToFile: Unable to write BSP CompDB XML file '" + xmlFile + "'", innerException);
			}
			finally
			{
				textWriter.Close();
			}
		}

		public override string ToString()
		{
			return "BSP DB: " + BSPProductName + " " + BSPVersion + " (" + BuildInfo + ")";
		}
	}
}
