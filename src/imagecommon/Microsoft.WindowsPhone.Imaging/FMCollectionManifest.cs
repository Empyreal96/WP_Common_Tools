using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	[XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
	[XmlRoot(ElementName = "FMCollectionManifest", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class FMCollectionManifest
	{
		[XmlAttribute]
		public string Product;

		[DefaultValue(false)]
		public bool IsBuildFeatureEnabled;

		[XmlArrayItem(ElementName = "Language", Type = typeof(string), IsNullable = false)]
		[XmlArray]
		public List<string> SupportedLanguages = new List<string>();

		[XmlArrayItem(ElementName = "Locale", Type = typeof(string), IsNullable = false)]
		[XmlArray]
		public List<string> SupportedLocales = new List<string>();

		[XmlArrayItem(ElementName = "Resolution", Type = typeof(string), IsNullable = false)]
		[XmlArray]
		public List<string> SupportedResolutions = new List<string>();

		[XmlArrayItem(ElementName = "FM", Type = typeof(FMCollectionItem), IsNullable = false)]
		[XmlArray]
		public List<FMCollectionItem> FMs = new List<FMCollectionItem>();

		[XmlArrayItem(ElementName = "FeatureIdentifierPackage", Type = typeof(FeatureIdentifierPackage), IsNullable = false)]
		[XmlArray]
		public List<FeatureIdentifierPackage> FeatureIdentifierPackages = new List<FeatureIdentifierPackage>();

		public string ChunkMappingsFile;

		public bool ShouldSerializeSupportedLocales()
		{
			if (SupportedLocales != null)
			{
				return SupportedLocales.Count() > 0;
			}
			return false;
		}

		public bool ShouldSerializeSupportedResolutions()
		{
			if (SupportedResolutions != null)
			{
				return SupportedResolutions.Count() > 0;
			}
			return false;
		}

		public bool ShouldSerializeFeatureIdentifierPackages()
		{
			if (FeatureIdentifierPackages != null)
			{
				return FeatureIdentifierPackages.Count() > 0;
			}
			return false;
		}

		public string GetChunkMappingFile(string fmDirectory)
		{
			if (string.IsNullOrEmpty(ChunkMappingsFile))
			{
				return ChunkMappingsFile;
			}
			return ChunkMappingsFile.Replace(FMCollection.c_FMDirectoryVariable, fmDirectory, StringComparison.OrdinalIgnoreCase);
		}

		public List<CpuId> GetWowGuestCpuTypes(CpuId cpuType)
		{
			return ImagingEditions.GetWowGuestCpuTypes((from fm in FMs
				where fm.ownerType == OwnerType.Microsoft
				select Path.GetFileName(fm.Path)).ToList(), cpuType);
		}

		public static FMCollectionManifest ValidateAndLoad(string xmlFile, IULogger logger)
		{
			FMCollectionManifest fMCollectionManifest = new FMCollectionManifest();
			string text = string.Empty;
			string fMCollectionSchema = BuildPaths.FMCollectionSchema;
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
			foreach (string text2 in manifestResourceNames)
			{
				if (text2.Contains(fMCollectionSchema))
				{
					text = text2;
					break;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				throw new ImageCommonException("ImageCommon!ValidateAndLoad: XSD resource was not found: " + fMCollectionSchema);
			}
			using (Stream xsdStream = executingAssembly.GetManifestResourceStream(text))
			{
				XsdValidator xsdValidator = new XsdValidator();
				try
				{
					xsdValidator.ValidateXsd(xsdStream, xmlFile, logger);
				}
				catch (XsdValidatorException innerException)
				{
					throw new ImageCommonException("ImageCommon!ValidateAndLoad: Unable to validate FM Collection Manifest XSD for file '" + xmlFile + "'.", innerException);
				}
			}
			logger.LogInfo("ImageCommon: Successfully validated the Feature Manifest XML: {0}", xmlFile);
			TextReader textReader = new StreamReader(LongPathFile.OpenRead(xmlFile));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(FMCollectionManifest));
			try
			{
				fMCollectionManifest = (FMCollectionManifest)xmlSerializer.Deserialize(textReader);
			}
			catch (Exception innerException2)
			{
				throw new ImageCommonException("ImageCommon!ValidateAndLoad: Unable to parse FM Collection XML file '" + xmlFile + "'.", innerException2);
			}
			finally
			{
				textReader.Close();
			}
			List<IGrouping<string, FMCollectionItem>> list = (from g in fMCollectionManifest.FMs.GroupBy((FMCollectionItem fm) => fm.ID, StringComparer.OrdinalIgnoreCase)
				where g.Count() > 1
				select g).ToList();
			if (list.Count() > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (IGrouping<string, FMCollectionItem> item in list)
				{
					stringBuilder.AppendLine(Environment.NewLine + "\t" + item.Key + ": ");
					foreach (FMCollectionItem item2 in item)
					{
						stringBuilder.AppendLine("\t\t" + item2.Path);
					}
				}
				throw new ImageCommonException("ImageCommon!ValidateAndLoad: Duplicate FMIDs found in FM Collection XML file '" + xmlFile + "': " + stringBuilder.ToString());
			}
			return fMCollectionManifest;
		}

		public void WriteToFile(string xmlFile)
		{
			TextWriter textWriter = new StreamWriter(LongPathFile.OpenWrite(xmlFile));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(FMCollectionManifest));
			try
			{
				xmlSerializer.Serialize(textWriter, this);
			}
			catch (Exception innerException)
			{
				throw new FeatureAPIException("FMCollection!WriteToFile: Unable to write FM Collection XML file '" + xmlFile + "'", innerException);
			}
			finally
			{
				textWriter.Close();
			}
		}

		public void ValidateFeatureIdentiferPackages(List<PublishingPackageInfo> packages)
		{
			if (FeatureIdentifierPackages == null || !FeatureIdentifierPackages.Any())
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			foreach (FeatureIdentifierPackage fip in FeatureIdentifierPackages)
			{
				if (fip.FixUpAction != 0 && (fip.FixUpAction != FeatureIdentifierPackage.FixUpActions.AndFeature || string.IsNullOrEmpty(fip.ID)))
				{
					continue;
				}
				List<PublishingPackageInfo> list = null;
				if (packages.Any())
				{
					list = packages.Where((PublishingPackageInfo pkg) => string.Equals(pkg.FeatureID, fip.FeatureID, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(fip.ID) && string.Equals(pkg.ID, fip.ID, StringComparison.OrdinalIgnoreCase) && string.Equals(pkg.Partition, fip.Partition, StringComparison.OrdinalIgnoreCase)).ToList();
				}
				if (list == null || !list.Any())
				{
					flag = true;
					stringBuilder.AppendFormat("\t{0} (FeatureID={1})\n", fip.ID, fip.FeatureID);
				}
			}
			if (!flag)
			{
				return;
			}
			throw new ImageCommonException("ImageCommon!ValidateAndLoad: The following Feature Identifier Packages specified in the FMCollectionManifest could not be found:" + stringBuilder);
		}
	}
}
