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
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.WindowsPhone.CompDB
{
	[XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
	[XmlRoot(ElementName = "CompDB", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class BuildCompDB
	{
		public enum CompDBType
		{
			Invalid = -1,
			Build,
			Update,
			Device,
			BSP,
			Baseless
		}

		public const string c_BuildCompDBRevision = "1";

		public const string c_BuildCompDBSchemaVersion = "1.2";

		public const string c_BuildInfoEnvStr = "%_RELEASELABEL%.%_PARENTBRANCHBUILDNUMBER%.%_QFELEVEL%.%_BUILDTIME%";

		public const string c_PhoneProductNamePrefix = "Mobile.BSP.";

		public const string c_AnalogProductNamePrefix = "HoloLens.BSP.";

		public const string c_IotProductNamePrefix = "IOTCore.BSP.";

		public const string c_OnecoreProductNamePrefix = "Onecore.BSP.";

		public const string c_OtherProductNamePrefix = ".BSP.";

		internal IULogger _iuLogger;

		[XmlAttribute]
		public DateTime CreatedDate = DateTime.Now.ToUniversalTime();

		[XmlAttribute]
		public string Revision = "1";

		[XmlAttribute]
		public string SchemaVersion = "1.2";

		private CompDBType _type = CompDBType.Invalid;

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

		[XmlArrayItem(ElementName = "Feature", Type = typeof(CompDBFeature), IsNullable = false)]
		[XmlArray]
		public List<CompDBFeature> Features;

		[XmlArrayItem(ElementName = "ConditionalFeature", Type = typeof(FMConditionalFeature), IsNullable = false)]
		[XmlArray]
		public List<FMConditionalFeature> MSConditionalFeatures;

		[XmlArrayItem(ElementName = "Package", Type = typeof(CompDBPackageInfo), IsNullable = false)]
		[XmlArray]
		public List<CompDBPackageInfo> Packages;

		[XmlIgnore]
		internal StringComparer IgnoreCase = StringComparer.OrdinalIgnoreCase;

		public static CompDBChunkMapping ChunkMapping;

		[DefaultValue(CompDBType.Invalid)]
		[XmlAttribute]
		public CompDBType Type
		{
			get
			{
				if (_type == CompDBType.Invalid)
				{
					if (this is UpdateCompDB)
					{
						_type = CompDBType.Update;
					}
					else if (this is BSPCompDB)
					{
						_type = CompDBType.BSP;
					}
					else if (this is DeviceCompDB)
					{
						_type = CompDBType.Device;
					}
					else
					{
						_type = CompDBType.Build;
					}
				}
				return _type;
			}
			set
			{
				_type = value;
			}
		}

		public bool ShouldSerializeMSConditionalFeatures()
		{
			if (MSConditionalFeatures != null)
			{
				return MSConditionalFeatures.Count() > 0;
			}
			return false;
		}

		public BuildCompDB()
		{
			if (Features == null)
			{
				Features = new List<CompDBFeature>();
			}
			if (Packages == null)
			{
				Packages = new List<CompDBPackageInfo>();
			}
		}

		public BuildCompDB(IULogger logger)
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

		public BuildCompDB(BuildCompDB srcDB)
		{
			BuildArch = srcDB.BuildArch;
			ReleaseType = srcDB.ReleaseType;
			Product = srcDB.Product;
			BuildID = srcDB.BuildID;
			BuildInfo = srcDB.BuildInfo;
			OSVersion = srcDB.OSVersion;
			if (srcDB.Features != null)
			{
				Features = srcDB.Features.Select((CompDBFeature feat) => new CompDBFeature(feat)).ToList();
			}
			if (srcDB.MSConditionalFeatures != null)
			{
				MSConditionalFeatures = srcDB.MSConditionalFeatures.Select((FMConditionalFeature feat) => new FMConditionalFeature(feat)).ToList();
			}
			if (srcDB.Packages != null)
			{
				Packages = srcDB.Packages.Select((CompDBPackageInfo pkg) => new CompDBPackageInfo(pkg)).ToList();
			}
			Revision = srcDB.Revision;
			SchemaVersion = srcDB.SchemaVersion;
			Type = srcDB.Type;
		}

		public static void InitializeChunkMapping(string mappingFile, List<string> languages, IULogger logger)
		{
			ChunkMapping = CompDBChunkMapping.ValidateAndLoad(mappingFile, languages, logger);
		}

		public static string GetProductNamePrefix(string product)
		{
			Edition edition = ImagingEditions.GetProductEdition(product);
			if (edition == null)
			{
				edition = ImagingEditions.Editions.FirstOrDefault((Edition ed) => ed.InternalProductDir.Equals(product, StringComparison.OrdinalIgnoreCase));
				if (edition == null)
				{
					return null;
				}
			}
			string result = product + ".BSP.";
			if (edition.IsProduct("Windows Phone"))
			{
				result = "Mobile.BSP.";
			}
			else if (edition.IsProduct("Windows Holographic"))
			{
				result = "HoloLens.BSP.";
			}
			else if (edition.IsProduct("Windows 10 IoT Core"))
			{
				result = "IOTCore.BSP.";
			}
			else if (edition.IsProduct("OneCore OS"))
			{
				result = "Onecore.BSP.";
			}
			return result;
		}

		public void GenerateCompDB(FMCollection fmCollection, string fmDirectory, string msPackageRoot, string buildType, CpuId buildArch, string buildInfo)
		{
			GenerateCompDB(fmCollection, fmDirectory, msPackageRoot, buildType, buildArch, buildInfo, false, false, false, OwnerType.Invalid, ReleaseType.Invalid);
		}

		public void GenerateCompDB(FMCollection fmCollection, string fmDirectory, string msPackageRoot, string buildType, CpuId buildArch, string buildInfo, bool generateHashes, bool ignoreSkipForPublishing, bool ignoreSkipForPRSSigning, OwnerType filterOnOwnerType, ReleaseType filterOnReleaseType)
		{
			if (fmCollection.Manifest == null)
			{
				throw new ImageCommonException("ImageCommon::BuildCompDB!GenerateCompDB: Unable to generate Build CompDB without a FM Collection.");
			}
			if (filterOnReleaseType == ReleaseType.Production)
			{
				ReleaseType = ReleaseType.Production;
			}
			else
			{
				ReleaseType = ReleaseType.Test;
			}
			Product = fmCollection.Manifest.Product;
			BuildArch = buildArch.ToString();
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			foreach (FMCollectionItem fM in fmCollection.Manifest.FMs)
			{
				if ((buildArch != fM.CPUType && fM.CPUType != 0) || (ignoreSkipForPublishing && fM.SkipForPublishing) || (filterOnReleaseType != 0 && fM.releaseType != filterOnReleaseType) || (filterOnOwnerType != 0 && fM.ownerType != filterOnOwnerType))
				{
					continue;
				}
				FeatureManifest fm = new FeatureManifest();
				string text = Environment.ExpandEnvironmentVariables(fM.Path);
				text = fM.ResolveFMPath(fmDirectory);
				FeatureManifest.ValidateAndLoad(ref fm, text, _iuLogger);
				if (fm.OwnerType == OwnerType.Microsoft && BuildID == Guid.Empty && !string.IsNullOrEmpty(fm.BuildID))
				{
					BuildID = new Guid(fm.BuildID);
					BuildInfo = fm.BuildInfo;
					OSVersion = fm.OSVersion;
				}
				List<FeatureManifest.FMPkgInfo> allPackageByGroups = fm.GetAllPackageByGroups(fmCollection.Manifest.SupportedLanguages, fmCollection.Manifest.SupportedLocales, fmCollection.Manifest.SupportedResolutions, buildType, buildArch.ToString(), msPackageRoot);
				foreach (FeatureManifest.FMPkgInfo item2 in allPackageByGroups.Where((FeatureManifest.FMPkgInfo pkg) => pkg.FMGroup == FeatureManifest.PackageGroups.OEMDEVICEPLATFORM))
				{
					item2.FMGroup = FeatureManifest.PackageGroups.DEVICE;
				}
				foreach (FeatureManifest.FMPkgInfo item3 in allPackageByGroups.Where((FeatureManifest.FMPkgInfo pkg) => pkg.FMGroup == FeatureManifest.PackageGroups.DEVICELAYOUT))
				{
					item3.FMGroup = FeatureManifest.PackageGroups.SOC;
				}
				foreach (string feature in allPackageByGroups.Select((FeatureManifest.FMPkgInfo pkg) => pkg.FeatureID).Distinct(IgnoreCase).ToList())
				{
					List<FeatureManifest.FMPkgInfo> list = allPackageByGroups.Where((FeatureManifest.FMPkgInfo pkg) => pkg.FeatureID.Equals(feature, StringComparison.OrdinalIgnoreCase)).ToList();
					CompDBFeature compDBFeature = new CompDBFeature(feature, fM.ID, CompDBFeature.CompDBFeatureTypes.MobileFeature, (fM.ownerType == OwnerType.Microsoft) ? fM.ownerType.ToString() : OwnerType.OEM.ToString());
					foreach (FeatureManifest.FMPkgInfo item4 in list)
					{
						CompDBFeaturePackage item = new CompDBFeaturePackage(item4.ID, item4.FeatureIdentifierPackage);
						compDBFeature.Packages.Add(item);
					}
					if (list.Any((FeatureManifest.FMPkgInfo pkg) => pkg.FMGroup == FeatureManifest.PackageGroups.MSFEATURE || pkg.FMGroup == FeatureManifest.PackageGroups.OEMFEATURE))
					{
						compDBFeature.Type = CompDBFeature.CompDBFeatureTypes.OptionalFeature;
					}
					Features.Add(compDBFeature);
				}
				List<CompDBPackageInfo> list2 = new List<CompDBPackageInfo>();
				StringBuilder stringBuilder2 = new StringBuilder();
				bool flag2 = false;
				foreach (FeatureManifest.FMPkgInfo item5 in allPackageByGroups)
				{
					CompDBPackageInfo compDBPackageInfo;
					try
					{
						compDBPackageInfo = new CompDBPackageInfo(item5, fM, msPackageRoot, this, generateHashes, fM.UserInstallable);
						if (!item5.ID.Equals(compDBPackageInfo.ID, StringComparison.OrdinalIgnoreCase))
						{
							compDBPackageInfo.ID = item5.ID;
						}
					}
					catch (FileNotFoundException)
					{
						flag2 = true;
						stringBuilder2.AppendFormat("Error:\t{0}\n", item5.ID);
						continue;
					}
					list2.Add(compDBPackageInfo);
				}
				if (flag2)
				{
					flag = true;
					stringBuilder.AppendFormat("\nThe FM File '{0}' following package file(s) could not be found: \n {1}", text, stringBuilder2.ToString());
				}
				if (fM.SkipForPublishing)
				{
					list2 = list2.Select((CompDBPackageInfo pkg) => pkg.SetSkipForPublishing()).ToList();
				}
				if (!ignoreSkipForPRSSigning && fM.SkipForPRSSigning)
				{
					list2 = list2.Select((CompDBPackageInfo pkg) => pkg.SetSkipForPRSSigning()).ToList();
				}
				Packages.AddRange(list2);
				foreach (CompDBFeature feature2 in Features)
				{
					if (feature2.Packages.Select((CompDBFeaturePackage pkg) => FindPackage(pkg)).Any((CompDBPackageInfo pkg) => pkg.UserInstallable))
					{
						feature2.Type = CompDBFeature.CompDBFeatureTypes.OnDemandFeature;
					}
				}
				if (fm.Features != null && fm.Features.MSConditionalFeatures != null)
				{
					if (MSConditionalFeatures == null)
					{
						MSConditionalFeatures = new List<FMConditionalFeature>();
					}
					MSConditionalFeatures.AddRange(fm.Features.MSConditionalFeatures);
				}
			}
			if (flag)
			{
				throw new ImageCommonException("ImageCommon::BuildCompDB!GenerateCompDB: Errors processing FM File(s):\n" + stringBuilder.ToString());
			}
			Packages = Packages.Distinct(CompDBPackageInfoComparer.Standard).ToList();
			if (BuildID == Guid.Empty)
			{
				BuildID = Guid.NewGuid();
				BuildInfo = buildInfo;
			}
		}

		public void GenerateHashes(string packageRoot)
		{
			bool flag = false;
			StringBuilder stringBuilder = new StringBuilder();
			foreach (CompDBPackageInfo package in Packages)
			{
				foreach (CompDBPayloadInfo item in package.Payload)
				{
					string text = Path.Combine(packageRoot, item.Path);
					if (!LongPathFile.Exists(text))
					{
						flag = true;
						stringBuilder.AppendLine("Error:\t" + text);
					}
					else
					{
						package.SetPackageHash(text);
					}
				}
			}
			if (flag)
			{
				throw new ImageCommonException("ImageCommon::BuildCompDB!GenerateHashes: Missing file(s):\n" + stringBuilder.ToString());
			}
		}

		public CompDBPackageInfo FindPackage(CompDBFeaturePackage pkg)
		{
			return Packages.FirstOrDefault((CompDBPackageInfo searchPkg) => searchPkg.ID.Equals(pkg.ID, StringComparison.OrdinalIgnoreCase));
		}

		public static BuildCompDB ValidateAndLoad(string xmlFile, IULogger logger)
		{
			BuildCompDB buildCompDB = new BuildCompDB();
			string text = string.Empty;
			string buildCompDBSchema = BuildPaths.BuildCompDBSchema;
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
			foreach (string text2 in manifestResourceNames)
			{
				if (text2.Contains(buildCompDBSchema))
				{
					text = text2;
					break;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				throw new ImageCommonException("ImageCommon::BuildCompDB!ValidateAndLoad: XSD resource was not found: " + buildCompDBSchema);
			}
			TextReader textReader = new StreamReader(LongPathFile.OpenRead(xmlFile));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(BuildCompDB));
			try
			{
				buildCompDB = (BuildCompDB)xmlSerializer.Deserialize(textReader);
			}
			catch (Exception innerException)
			{
				throw new ImageCommonException("ImageCommon::BuildCompDB!ValidateAndLoad: Unable to parse Build CompDB XML file '" + xmlFile + "'", innerException);
			}
			finally
			{
				textReader.Close();
			}
			bool flag = "1.2".Equals(buildCompDB.SchemaVersion);
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
						throw new ImageCommonException("ImageCommon::BuildCompDB!ValidateAndLoad: Unable to validate Build XSD for file '" + xmlFile + "'.", innerException2);
					}
					logger.LogWarning("Warning: ImageCommon::BuildCompDB!ValidateAndLoad: Unable to validate Build CompDB XSD for file '" + xmlFile + "'.");
					if (string.IsNullOrEmpty(buildCompDB.SchemaVersion))
					{
						logger.LogWarning("Warning: ImageCommon::BuildCompDB!ValidateAndLoad: Schema Version was not given in Build CompDB. Most up to date Schema Version is {1}.", "1.2");
					}
					else
					{
						logger.LogWarning("Warning: ImageCommon::BuildCompDB!ValidateAndLoad: Schema Version given in Build CompDB ({0}) does not match most up to date Schema Version ({1}).", buildCompDB.SchemaVersion, "1.2");
					}
				}
			}
			logger.LogInfo("BuildCompDB: Successfully validated the Build CompDB XML: {0}", xmlFile);
			BuildCompDB parentDB = buildCompDB;
			buildCompDB.Packages = buildCompDB.Packages.Select((CompDBPackageInfo pkg) => pkg.SetParentDB(parentDB)).ToList();
			if (buildCompDB.ReleaseType == ReleaseType.Invalid)
			{
				if (buildCompDB.Packages.Any((CompDBPackageInfo pkg) => pkg.ReleaseType == ReleaseType.Test))
				{
					buildCompDB.ReleaseType = ReleaseType.Test;
				}
				else
				{
					buildCompDB.ReleaseType = ReleaseType.Production;
				}
			}
			return buildCompDB;
		}

		public static BuildCompDB LoadCompDB(string xmlFile, IULogger logger)
		{
			BuildCompDB buildCompDB = new BuildCompDB();
			TextReader textReader = new StreamReader(LongPathFile.OpenRead(xmlFile));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(BuildCompDB));
			try
			{
				buildCompDB = (BuildCompDB)xmlSerializer.Deserialize(textReader);
			}
			catch (Exception innerException)
			{
				throw new ImageCommonException("ImageCommon::BuildCompDB!LoadCompDB: Unable to parse CompDB XML file '" + xmlFile + "'", innerException);
			}
			finally
			{
				textReader.Close();
			}
			BuildCompDB result = null;
			switch (buildCompDB.Type)
			{
			case CompDBType.BSP:
				result = BSPCompDB.ValidateAndLoad(xmlFile, logger);
				break;
			case CompDBType.Build:
			case CompDBType.Baseless:
				result = ValidateAndLoad(xmlFile, logger);
				break;
			case CompDBType.Device:
				result = DeviceCompDB.ValidateAndLoad(xmlFile, logger);
				break;
			case CompDBType.Update:
				result = UpdateCompDB.ValidateAndLoad(xmlFile, logger);
				break;
			}
			return result;
		}

		public virtual void WriteToFile(string xmlFile)
		{
			string directoryName = Path.GetDirectoryName(xmlFile);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			SchemaVersion = "1.2";
			Revision = "1";
			TextWriter textWriter = new StreamWriter(LongPathFile.OpenWrite(xmlFile));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(BuildCompDB));
			try
			{
				xmlSerializer.Serialize(textWriter, this);
			}
			catch (Exception innerException)
			{
				throw new ImageCommonException("ImageCommon::BuildCompDB!WriteToFile: Unable to write Build CompDB XML file '" + xmlFile + "'", innerException);
			}
			finally
			{
				textWriter.Close();
			}
		}

		public virtual void WriteToFile(string xmlFile, bool writePublishingFile)
		{
			WriteToFile(xmlFile);
			if (writePublishingFile)
			{
				CompDBPublishingInfo compDBPublishingInfo = new CompDBPublishingInfo(this, _iuLogger);
				string xmlFile2 = Path.Combine(path2: Path.GetFileNameWithoutExtension(xmlFile) + CompDBPublishingInfo.c_CompDBPublishingInfoFileIdentifier + ".xml", path1: Path.GetDirectoryName(xmlFile));
				compDBPublishingInfo.WriteToFile(xmlFile2);
			}
		}

		public static string GetBuildInfo()
		{
			return Environment.ExpandEnvironmentVariables("%_RELEASELABEL%.%_PARENTBRANCHBUILDNUMBER%.%_QFELEVEL%.%_BUILDTIME%");
		}

		public void CopyHashes(BuildCompDB srcCompDB)
		{
			foreach (CompDBPackageInfo pkg in Packages)
			{
				CompDBPackageInfo compDBPackageInfo = srcCompDB.Packages.FirstOrDefault((CompDBPackageInfo src) => src.Equals(pkg, CompDBPackageInfo.CompDBPackageInfoComparison.IgnorePayloadHashes));
				if (compDBPackageInfo == null)
				{
					continue;
				}
				foreach (CompDBPayloadInfo item in pkg.Payload)
				{
					CompDBPayloadInfo compDBPayloadInfo = compDBPackageInfo.FindPayload(item.Path);
					if (compDBPayloadInfo != null && string.IsNullOrEmpty(compDBPayloadInfo.PayloadHash))
					{
						item.PayloadHash = compDBPayloadInfo.PayloadHash;
					}
				}
			}
		}

		public void GenerateMissingHashes(string packageRoot)
		{
			foreach (CompDBPackageInfo package in Packages)
			{
				foreach (CompDBPayloadInfo item in package.Payload)
				{
					if (string.IsNullOrEmpty(item.PayloadHash))
					{
						string payloadHash = Path.Combine(packageRoot, item.Path);
						item.SetPayloadHash(payloadHash);
					}
				}
			}
		}

		public void FilterDB(OwnerType filterOnOwnerType, ReleaseType filterOnReleaseType)
		{
			FilterDB(filterOnOwnerType, filterOnReleaseType, false);
		}

		public void FilterDB(OwnerType filterOnOwnerType, ReleaseType filterOnReleaseType, bool filterOutSkipForPublishing)
		{
			if (filterOutSkipForPublishing)
			{
				Packages = Packages.Where((CompDBPackageInfo pkg) => !pkg.SkipForPublishing).ToList();
			}
			if (filterOnOwnerType != 0)
			{
				Packages = Packages.Where((CompDBPackageInfo pkg) => pkg.OwnerType == filterOnOwnerType).ToList();
				if (Features != null)
				{
					Features = Features.Where((CompDBFeature feat) => feat.Group.Equals(filterOnOwnerType.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
				}
			}
			if (filterOnReleaseType != 0)
			{
				Packages = Packages.Where((CompDBPackageInfo pkg) => pkg.ReleaseType == filterOnReleaseType).ToList();
				if (filterOnReleaseType == ReleaseType.Production)
				{
					ReleaseType = ReleaseType.Production;
				}
				else
				{
					ReleaseType = ReleaseType.Test;
				}
			}
			if (Features == null)
			{
				return;
			}
			Features = Features.Where((CompDBFeature feat) => feat.Packages.All((CompDBFeaturePackage pkg) => Packages.Any((CompDBPackageInfo pkg2) => pkg.ID.Equals(pkg2.ID, StringComparison.OrdinalIgnoreCase)))).ToList();
			List<string> featurePkgIDs = (from pkg in Features.SelectMany((CompDBFeature feat) => feat.Packages)
				select pkg.ID).ToList();
			Packages = Packages.Where((CompDBPackageInfo pkg) => featurePkgIDs.Contains(pkg.ID, StringComparer.OrdinalIgnoreCase)).ToList();
		}

		public void Merge(BuildCompDB srcDB)
		{
			if (Type != srcDB.Type)
			{
				throw new ImageCommonException("ImageCommon::BuildCompDB!Merge: Unable to generate the CompDBs because they are different Types " + Type.ToString() + "\\" + srcDB.Type);
			}
			foreach (CompDBFeature srcFeature in srcDB.Features)
			{
				CompDBFeature compDBFeature = Features.FirstOrDefault((CompDBFeature feat) => feat.FeatureIDWithFMID.Equals(srcFeature.FeatureIDWithFMID, StringComparison.OrdinalIgnoreCase));
				if (compDBFeature == null)
				{
					Features.Add(srcFeature);
					continue;
				}
				foreach (CompDBFeaturePackage srcFeaturePkg in srcFeature.Packages)
				{
					if (compDBFeature.Packages.Any((CompDBFeaturePackage pkg) => !pkg.ID.Equals(srcFeaturePkg.ID, StringComparison.OrdinalIgnoreCase)))
					{
						compDBFeature.Packages.Add(srcFeaturePkg);
					}
				}
			}
			List<string> pkgIDs = Packages.Select((CompDBPackageInfo pkg) => pkg.ID).ToList();
			List<CompDBPackageInfo> collection = srcDB.Packages.Where((CompDBPackageInfo pkg) => !pkgIDs.Contains(pkg.ID, StringComparer.OrdinalIgnoreCase)).ToList();
			Packages.AddRange(collection);
		}

		public override string ToString()
		{
			return "Build DB: " + BuildInfo;
		}
	}
}
