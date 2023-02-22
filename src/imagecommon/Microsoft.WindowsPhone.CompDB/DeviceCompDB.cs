using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.WindowsPhone.CompDB
{
	[XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
	[XmlRoot(ElementName = "CompDB", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class DeviceCompDB : BuildCompDB
	{
		public const string c_DeviceCompDBRevision = "1";

		public const string c_DeviceCompDBSchemaVersion = "1.2";

		[XmlAttribute]
		public Guid BSPBuildID;

		[XmlAttribute]
		public string BSPBuildInfo;

		[XmlArrayItem(ElementName = "Language", Type = typeof(CompDBLanguage), IsNullable = false)]
		[XmlArray]
		[DefaultValue(null)]
		public List<CompDBLanguage> Languages;

		[XmlArrayItem(ElementName = "Resolution", Type = typeof(CompDBResolution), IsNullable = false)]
		[XmlArray]
		[DefaultValue(null)]
		public List<CompDBResolution> Resolutions;

		[XmlElement("ConditionAnswers")]
		public DeviceConditionAnswers ConditionAnswers = new DeviceConditionAnswers();

		public bool ShouldSerializeConditionAnswers()
		{
			if (ConditionAnswers != null)
			{
				return ConditionAnswers.GetAllConditions().Count() > 0;
			}
			return false;
		}

		public DeviceCompDB()
		{
		}

		public DeviceCompDB(DeviceCompDB srcDB)
			: base(srcDB)
		{
			BSPBuildID = srcDB.BSPBuildID;
			BSPBuildInfo = srcDB.BSPBuildInfo;
			if (srcDB.ConditionAnswers != null)
			{
				ConditionAnswers = new DeviceConditionAnswers(srcDB.ConditionAnswers);
			}
			if (srcDB.Languages != null)
			{
				Languages = srcDB.Languages.Select((CompDBLanguage lang) => new CompDBLanguage(lang)).ToList();
			}
			if (srcDB.MSConditionalFeatures != null)
			{
				MSConditionalFeatures = srcDB.MSConditionalFeatures.Select((FMConditionalFeature cf) => new FMConditionalFeature(cf)).ToList();
			}
			if (srcDB.Resolutions != null)
			{
				Resolutions = srcDB.Resolutions.Select((CompDBResolution res) => new CompDBResolution(res)).ToList();
			}
		}

		public new static DeviceCompDB ValidateAndLoad(string xmlFile, IULogger logger)
		{
			DeviceCompDB deviceCompDB = new DeviceCompDB();
			string text = string.Empty;
			string deviceCompDBSchema = BuildPaths.DeviceCompDBSchema;
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
			foreach (string text2 in manifestResourceNames)
			{
				if (text2.Contains(deviceCompDBSchema))
				{
					text = text2;
					break;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				throw new ImageCommonException("ImageCommon::DeviceCompDB!ValidateAndLoad: XSD resource was not found: " + deviceCompDBSchema);
			}
			TextReader textReader = new StreamReader(LongPathFile.OpenRead(xmlFile));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(DeviceCompDB));
			try
			{
				deviceCompDB = (DeviceCompDB)xmlSerializer.Deserialize(textReader);
			}
			catch (Exception innerException)
			{
				throw new ImageCommonException("ImageCommon::DeviceCompDB!ValidateAndLoad: Unable to parse Device CompDB XML file '" + xmlFile + "'", innerException);
			}
			finally
			{
				textReader.Close();
			}
			bool flag = "1.2".Equals(deviceCompDB.SchemaVersion);
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
						throw new ImageCommonException("ImageCommon::DeviceCompDB!ValidateAndLoad: Unable to validate Device CompDB XSD for file '" + xmlFile + "'.", innerException2);
					}
					logger.LogWarning("ImageCommon::DeviceCompDB!ValidateAndLoad: Unable to validate Device CompDB XSD for file '" + xmlFile + "'.");
					if (string.IsNullOrEmpty(deviceCompDB.SchemaVersion))
					{
						logger.LogWarning("Warning: ImageCommon::DeviceCompDB!ValidateAndLoad: Schema Version was not given in Device CompDB. Most up to date Schema Version is {1}.", "1.2");
					}
					else
					{
						logger.LogWarning("Warning: ImageCommon::DeviceCompDB!ValidateAndLoad: Schema Version given in Device CompDB ({0}) does not match most up to date Schema Version ({1}).", deviceCompDB.SchemaVersion, "1.2");
					}
				}
			}
			logger.LogInfo("DeviceCompDB: Successfully validated the Device CompDB XML: {0}", xmlFile);
			BuildCompDB parentDB = deviceCompDB;
			deviceCompDB.Packages = deviceCompDB.Packages.Select((CompDBPackageInfo pkg) => pkg.SetParentDB(parentDB)).ToList();
			if (deviceCompDB.ReleaseType == ReleaseType.Invalid)
			{
				if (deviceCompDB.Packages.Any((CompDBPackageInfo pkg) => pkg.ReleaseType == ReleaseType.Test))
				{
					deviceCompDB.ReleaseType = ReleaseType.Test;
				}
				else
				{
					deviceCompDB.ReleaseType = ReleaseType.Production;
				}
			}
			return deviceCompDB;
		}

		public override void WriteToFile(string xmlFile)
		{
			string directoryName = Path.GetDirectoryName(xmlFile);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			foreach (CompDBPackageInfo package in Packages)
			{
				package.Payload.RemoveAll((CompDBPayloadInfo pay) => true);
			}
			SchemaVersion = "1.2";
			Revision = "1";
			TextWriter textWriter = new StreamWriter(LongPathFile.OpenWrite(xmlFile));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(DeviceCompDB));
			try
			{
				xmlSerializer.Serialize(textWriter, this);
			}
			catch (Exception innerException)
			{
				throw new ImageCommonException("ImageCommon::DeviceCompDB!WriteToFile: Unable to write Device CompDB XML file '" + xmlFile + "'", innerException);
			}
			finally
			{
				textWriter.Close();
			}
		}

		public static DeviceCompDB CreateDBFromPackages(List<IPkgInfo> packages, List<Hashtable> registryTable, string targetDBFile, IULogger logger)
		{
			Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
			DeviceCompDB deviceCompDB = new DeviceCompDB();
			deviceCompDB.BuildArch = (from pkg in packages
				group pkg by pkg.CpuType into g
				orderby g.Count() descending
				select g.Key).First().ToString();
			deviceCompDB.Features = new List<CompDBFeature>();
			List<CompDBPackageInfo> list = new List<CompDBPackageInfo>();
			deviceCompDB.Resolutions = (from res in (from pkg in packages
					where !string.IsNullOrEmpty(pkg.Resolution)
					select pkg.Resolution).Distinct()
				select new CompDBResolution(res)).ToList();
			List<string> source = deviceCompDB.Resolutions.Select((CompDBResolution res) => res.Id).ToList();
			foreach (IPkgInfo package in packages)
			{
				string text = FileUtils.GetTempFile() + "." + package.Name + "." + PkgConstants.c_strMumFile;
				string mumDevicePath = "\\" + PkgConstants.c_strMumDeviceFolder + "\\" + PkgConstants.c_strMumFile;
				bool flag = false;
				try
				{
					package.ExtractFile(mumDevicePath, text, true);
					flag = true;
				}
				catch
				{
				}
				if (!flag)
				{
					IFileEntry fileEntry = package.Files.FirstOrDefault((IFileEntry file) => file.DevicePath.Equals(mumDevicePath, StringComparison.OrdinalIgnoreCase));
					if (fileEntry != null)
					{
						string sourcePath = (fileEntry as FileEntry).SourcePath;
						sourcePath = Path.GetDirectoryName(sourcePath);
						string text2 = (string.IsNullOrEmpty(package.Culture) ? "" : package.Culture);
						string text3 = $"{package.Name}~{package.PublicKey}~{package.CpuType}~{text2}~{package.Version.ToString()}";
						sourcePath = Path.Combine(sourcePath, text3 + ".mum");
						LongPathFile.Copy(sourcePath, text, true);
					}
				}
				string featureInfoXML = PrepCBSFeature.GetFeatureInfoXML(text);
				File.Delete(text);
				if (!string.IsNullOrEmpty(featureInfoXML))
				{
					string fmID;
					string groupName;
					string groupType;
					string buildArch;
					List<FeatureManifest.FMPkgInfo> packages2;
					PrepCBSFeature.ParseFeatureInfoXML(featureInfoXML, out fmID, out groupName, out groupType, out buildArch, out packages2);
					CompDBFeature compDBFeature = new CompDBFeature(groupName, fmID, CompDBFeature.CompDBFeatureTypes.MobileFeature, groupType);
					compDBFeature.Packages = new List<CompDBFeaturePackage>();
					foreach (FeatureManifest.FMPkgInfo item3 in packages2)
					{
						CompDBFeaturePackage item = new CompDBFeaturePackage(item3.ID, package.Name.Equals(item3.ID, StringComparison.OrdinalIgnoreCase));
						if (!string.IsNullOrEmpty(item3.Language))
						{
							if (!dictionary.ContainsKey(item3.Language))
							{
								dictionary[item3.Language] = new List<string>();
							}
							dictionary[item3.Language].Add(item3.ID);
						}
						else if (!string.IsNullOrEmpty(item3.Resolution) && !source.Contains(item3.Resolution, StringComparer.OrdinalIgnoreCase))
						{
							continue;
						}
						compDBFeature.Packages.Add(item);
					}
					deviceCompDB.Features.Add(compDBFeature);
				}
				CompDBPackageInfo item2 = new CompDBPackageInfo(package, null, null, null, deviceCompDB, false, false);
				list.Add(item2);
			}
			List<string> langModelPackageIDs = (from pkg in deviceCompDB.Features.Where((CompDBFeature feat) => feat.FeatureID.StartsWith(FeatureManifest.PackageGroups.KEYBOARD.ToString() + "_", StringComparison.OrdinalIgnoreCase) || feat.FeatureID.StartsWith(FeatureManifest.PackageGroups.SPEECH.ToString() + "_", StringComparison.OrdinalIgnoreCase)).SelectMany((CompDBFeature feat) => feat.Packages)
				select pkg.ID).ToList();
			Func<CompDBPackageInfo, bool> func = default(Func<CompDBPackageInfo, bool>);
			Func<CompDBPackageInfo, bool> func2 = func;
			if (func2 == null)
			{
				func2 = (func = (CompDBPackageInfo pkg) => langModelPackageIDs.Contains(pkg.ID, StringComparer.OrdinalIgnoreCase));
			}
			foreach (CompDBPackageInfo item4 in list.Where(func2))
			{
				item4.SatelliteType = CompDBPackageInfo.SatelliteTypes.LangModel;
			}
			List<string> featurePackageIDs = (from pkg in deviceCompDB.Features.SelectMany((CompDBFeature feat) => feat.Packages)
				select pkg.ID).ToList();
			deviceCompDB.Packages = list.Where((CompDBPackageInfo pkg) => featurePackageIDs.Contains(pkg.ID, StringComparer.OrdinalIgnoreCase)).ToList();
			deviceCompDB.Languages = (from lang in (from pkg in deviceCompDB.Packages
					where pkg.SatelliteType == CompDBPackageInfo.SatelliteTypes.Language
					select pkg.SatelliteValue).Distinct()
				select new CompDBLanguage(lang)).ToList();
			List<string> languages = deviceCompDB.Languages.Select((CompDBLanguage lang) => lang.Id).ToList();
			List<string> removeLanguagePackageIDs = dictionary.Where((KeyValuePair<string, List<string>> pair) => !languages.Contains(pair.Key, StringComparer.OrdinalIgnoreCase)).SelectMany((KeyValuePair<string, List<string>> pair) => pair.Value.Select((string id) => id)).ToList();
			Predicate<CompDBFeaturePackage> predicate = default(Predicate<CompDBFeaturePackage>);
			foreach (CompDBFeature feature in deviceCompDB.Features)
			{
				List<CompDBFeaturePackage> packages3 = feature.Packages;
				Predicate<CompDBFeaturePackage> predicate2 = predicate;
				if (predicate2 == null)
				{
					predicate2 = (predicate = (CompDBFeaturePackage pkg) => removeLanguagePackageIDs.Contains(pkg.ID, StringComparer.OrdinalIgnoreCase));
				}
				packages3.RemoveAll(predicate2);
			}
			if (!string.IsNullOrEmpty(targetDBFile) && LongPathFile.Exists(targetDBFile))
			{
				BuildCompDB buildCompDB = BuildCompDB.LoadCompDB(targetDBFile, logger);
				if (buildCompDB.MSConditionalFeatures.SelectMany((FMConditionalFeature feat) => feat.GetAllConditions()).ToList().Any((Condition cond) => cond.Type != Condition.ConditionType.Feature))
				{
					deviceCompDB.ConditionAnswers = new DeviceConditionAnswers(logger);
					deviceCompDB.ConditionAnswers.PopulateConditionAnswers(buildCompDB.MSConditionalFeatures, registryTable);
				}
			}
			string fmDevicePath = "\\" + DevicePaths.MSFMPath;
			IPkgInfo pkgInfo = packages.Where((IPkgInfo pkg) => pkg.Files.Any((IFileEntry fi) => fi.DevicePath.StartsWith(fmDevicePath, StringComparison.OrdinalIgnoreCase))).FirstOrDefault();
			if (pkgInfo != null)
			{
				IFileEntry fileEntry2 = pkgInfo.Files.FirstOrDefault((IFileEntry fi) => fi.DevicePath.StartsWith(fmDevicePath, StringComparison.OrdinalIgnoreCase));
				if (fileEntry2 != null)
				{
					string text4 = FileUtils.GetTempFile() + "." + Path.GetFileName(fileEntry2.DevicePath);
					pkgInfo.ExtractFile(fileEntry2.DevicePath, text4, true);
					FeatureManifest fm = new FeatureManifest();
					FeatureManifest.ValidateAndLoad(ref fm, text4, logger);
					File.Delete(text4);
					if (!string.IsNullOrEmpty(fm.BuildID))
					{
						deviceCompDB.BuildID = (deviceCompDB.BSPBuildID = new Guid(fm.BuildID));
					}
					if (!string.IsNullOrEmpty(fm.BuildInfo))
					{
						deviceCompDB.BuildInfo = (deviceCompDB.BSPBuildInfo = fm.BuildInfo);
					}
				}
			}
			return deviceCompDB;
		}

		public override string ToString()
		{
			return "Device DB: " + BSPBuildInfo;
		}
	}
}
