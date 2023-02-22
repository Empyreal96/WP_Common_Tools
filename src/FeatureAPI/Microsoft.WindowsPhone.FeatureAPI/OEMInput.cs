using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	[XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
	[XmlRoot(ElementName = "OEMInput", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class OEMInput
	{
		[Flags]
		public enum OEMFeatureTypes
		{
			NONE = 0,
			BASE = 1,
			BOOTUI = 2,
			BOOTLOCALE = 4,
			RELEASE = 8,
			SV = 0x20,
			SOC = 0x40,
			DEVICE = 0x80,
			KEYBOARD = 0x100,
			SPEECH = 0x200,
			MSFEATURES = 0x400,
			OEMFEATURES = 0x800,
			PRERELEASE = 0x1000,
			UILANGS = 0x2000,
			RESOULTIONS = 0x4000
		}

		private static readonly string DefaultProduct = "Windows Phone";

		public static readonly string BuildType_FRE = "fre";

		public static readonly string BuildType_CHK = "chk";

		private const string ExcludePrereleaseTrueValue = "REPLACEMENT";

		private const string ExcludePrereleaseFalseValue = "PROTECTED";

		private string _product = DefaultProduct;

		private static StringComparer IgnoreCase = StringComparer.OrdinalIgnoreCase;

		private Edition _edition;

		public string Description;

		public string SOC;

		public string SV;

		public string Device;

		public string ReleaseType;

		public string BuildType;

		public SupportedLangs SupportedLanguages;

		public string BootUILanguage;

		public string BootLocale;

		[XmlArrayItem(ElementName = "Resolution", Type = typeof(string), IsNullable = false)]
		[XmlArray]
		[DefaultValue(null)]
		public List<string> Resolutions;

		[XmlArrayItem(ElementName = "AdditionalFM", Type = typeof(string), IsNullable = false)]
		[XmlArray]
		[DefaultValue(null)]
		public List<string> AdditionalFMs;

		public OEMInputFeatures Features;

		[XmlArrayItem(ElementName = "AdditionalSKU", Type = typeof(string), IsNullable = false)]
		[XmlArray]
		[DefaultValue(null)]
		public List<string> AdditionalSKUs;

		[XmlArrayItem(ElementName = "OptionalFeature", Type = typeof(string), IsNullable = false)]
		[XmlArray]
		[DefaultValue(null)]
		public List<string> InternalOptionalFeatures;

		[XmlArrayItem(ElementName = "OptionalFeatures", Type = typeof(string), IsNullable = false)]
		[XmlArray]
		[DefaultValue(null)]
		public List<string> ProductionOptionalFeatures;

		[XmlArrayItem(ElementName = "OptionalFeature", Type = typeof(string), IsNullable = false)]
		[XmlArray]
		[DefaultValue(null)]
		public List<string> MSOptionalFeatures;

		public UserStoreMapData UserStoreMapData;

		public string FormatDPP;

		[DefaultValue(false)]
		public bool ExcludePrereleaseFeatures;

		[XmlArrayItem(ElementName = "PackageFile", Type = typeof(string), IsNullable = false)]
		[XmlArray]
		[DefaultValue(null)]
		public List<string> PackageFiles;

		private string _msPackageRoot;

		[XmlIgnore]
		public string CPUType;

		public const OEMFeatureTypes ALLFEATURES = (OEMFeatureTypes)268435455;

		public const OEMFeatureTypes MSONLYFEATURES = (OEMFeatureTypes)268433407;

		public const OEMFeatureTypes OEMONLYFEATURES = (OEMFeatureTypes)268434431;

		[XmlIgnore]
		public Edition Edition
		{
			get
			{
				if (_edition == null)
				{
					_edition = ImagingEditions.GetProductEdition(Product);
				}
				return _edition;
			}
		}

		public string Product
		{
			get
			{
				if (_product == null)
				{
					_product = DefaultProduct;
				}
				return _product;
			}
			set
			{
				_product = value;
				_edition = null;
			}
		}

		public bool IsMMOS => Edition.IsProduct("Phone Manufacturing OS");

		[XmlIgnore]
		public string MSPackageRoot
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_msPackageRoot))
				{
					_msPackageRoot = Edition.MSPackageRoot;
				}
				return _msPackageRoot;
			}
			set
			{
				if (value == null)
				{
					_msPackageRoot = null;
					return;
				}
				char[] trimChars = new char[1] { '\\' };
				_msPackageRoot = value.TrimEnd(trimChars);
			}
		}

		[XmlIgnore]
		public List<string> FeatureIDs => GetFeatureList();

		[XmlIgnore]
		public List<string> MSFeatureIDs => GetFeatureList((OEMFeatureTypes)268433407);

		[XmlIgnore]
		public List<string> OEMFeatureIDs => GetFeatureList((OEMFeatureTypes)268434431);

		public OEMInput()
		{
		}

		public OEMInput(OEMInput srcOEMInput)
		{
			if (srcOEMInput == null)
			{
				return;
			}
			_msPackageRoot = srcOEMInput._msPackageRoot;
			_product = srcOEMInput._product;
			if (srcOEMInput.AdditionalFMs != null)
			{
				AdditionalFMs = new List<string>();
				AdditionalFMs.AddRange(srcOEMInput.AdditionalFMs);
			}
			BootLocale = srcOEMInput.BootLocale;
			BootUILanguage = srcOEMInput.BootUILanguage;
			BuildType = srcOEMInput.BuildType;
			CPUType = srcOEMInput.CPUType;
			Description = srcOEMInput.Description;
			Device = srcOEMInput.Device;
			ExcludePrereleaseFeatures = srcOEMInput.ExcludePrereleaseFeatures;
			if (srcOEMInput.Features != null)
			{
				Features = new OEMInputFeatures();
				if (srcOEMInput.Features.Microsoft != null)
				{
					Features.Microsoft = new List<string>();
					Features.Microsoft.AddRange(srcOEMInput.Features.Microsoft);
				}
				if (srcOEMInput.Features.OEM != null)
				{
					Features.OEM = new List<string>();
					Features.OEM.AddRange(srcOEMInput.Features.OEM);
				}
			}
			FormatDPP = srcOEMInput.FormatDPP;
			if (srcOEMInput.PackageFiles != null)
			{
				PackageFiles = new List<string>();
				PackageFiles.AddRange(srcOEMInput.PackageFiles);
			}
			ReleaseType = srcOEMInput.ReleaseType;
			if (srcOEMInput.Resolutions != null)
			{
				Resolutions = new List<string>();
				Resolutions.AddRange(srcOEMInput.Resolutions);
			}
			SOC = srcOEMInput.SOC;
			if (srcOEMInput.SupportedLanguages != null)
			{
				SupportedLanguages = new SupportedLangs();
				if (srcOEMInput.SupportedLanguages.UserInterface != null)
				{
					SupportedLanguages.UserInterface = new List<string>();
					SupportedLanguages.UserInterface.AddRange(srcOEMInput.SupportedLanguages.UserInterface);
				}
				if (srcOEMInput.SupportedLanguages.Keyboard != null)
				{
					SupportedLanguages.Keyboard = new List<string>();
					SupportedLanguages.Keyboard.AddRange(srcOEMInput.SupportedLanguages.Keyboard);
				}
				if (srcOEMInput.SupportedLanguages.Speech != null)
				{
					SupportedLanguages.Speech = new List<string>();
					SupportedLanguages.Speech.AddRange(srcOEMInput.SupportedLanguages.Speech);
				}
			}
			SV = srcOEMInput.SV;
			UserStoreMapData = srcOEMInput.UserStoreMapData;
		}

		public string ProcessOEMInputVariables(string value)
		{
			return value.Replace("$(device)", Device, StringComparison.OrdinalIgnoreCase).Replace("$(releasetype)", ReleaseType, StringComparison.OrdinalIgnoreCase).Replace("$(buildtype)", BuildType, StringComparison.OrdinalIgnoreCase)
				.Replace("$(cputype)", CPUType, StringComparison.OrdinalIgnoreCase)
				.Replace("$(bootuilanguage)", BootUILanguage, StringComparison.OrdinalIgnoreCase)
				.Replace("$(bootlocale)", BootLocale, StringComparison.OrdinalIgnoreCase)
				.Replace("$(mspackageroot)", MSPackageRoot, StringComparison.OrdinalIgnoreCase);
		}

		public static void ValidateInput(ref OEMInput xmlInput, string xmlFile, IULogger logger, string msPackageDir, string cpuType)
		{
			string text = string.Empty;
			string oEMInputSchema = DevicePaths.OEMInputSchema;
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
			foreach (string text2 in manifestResourceNames)
			{
				if (text2.Contains(oEMInputSchema))
				{
					text = text2;
					break;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				throw new FeatureAPIException("FeatureAPI!OEMInput::ValidateInput: XSD resource was not found: " + oEMInputSchema);
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
					throw new FeatureAPIException($"FeatureAPI!ValidateInput: Unable to validate OEM Input XSD for file '{xmlFile}'", innerException);
				}
			}
			logger.LogInfo("FeatureAPI: Successfully validated the OEM Input XML: {0}", xmlFile);
			TextReader textReader = new StreamReader(LongPathFile.OpenRead(xmlFile));
			try
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(OEMInput));
				xmlInput = (OEMInput)xmlSerializer.Deserialize(textReader);
				if (!string.IsNullOrWhiteSpace(msPackageDir))
				{
					xmlInput.MSPackageRoot = msPackageDir;
				}
				xmlInput.CPUType = cpuType;
				xmlInput.BuildType = Environment.ExpandEnvironmentVariables(xmlInput.BuildType);
				OEMInput obj = xmlInput;
				obj.Description = obj.ProcessOEMInputVariables(obj.Description);
				xmlInput.Description = Environment.ExpandEnvironmentVariables(xmlInput.Description);
				if (xmlInput.PackageFiles != null)
				{
					for (int j = 0; j < xmlInput.PackageFiles.Count; j++)
					{
						List<string> packageFiles = xmlInput.PackageFiles;
						int index = j;
						OEMInput obj2 = xmlInput;
						packageFiles[index] = obj2.ProcessOEMInputVariables(obj2.PackageFiles[j]);
						xmlInput.PackageFiles[j] = Environment.ExpandEnvironmentVariables(xmlInput.PackageFiles[j]);
					}
				}
				if (xmlInput.Edition.RequiresKeyboard && (xmlInput.SupportedLanguages.Keyboard == null || xmlInput.SupportedLanguages.Keyboard.Count == 0))
				{
					throw new FeatureAPIException("FeatureAPI!ValidateInput: At least one Keyboard language must be specified.");
				}
				if (xmlInput.AdditionalFMs != null)
				{
					for (int k = 0; k < xmlInput.AdditionalFMs.Count; k++)
					{
						xmlInput.AdditionalFMs[k] = Environment.ExpandEnvironmentVariables(xmlInput.AdditionalFMs[k]);
					}
				}
				if (xmlInput.Features == null)
				{
					xmlInput.Features = new OEMInputFeatures();
				}
			}
			catch (Exception innerException2)
			{
				throw new FeatureAPIException("FeatureAPI!ValidateInput: Unable to parse OEM Input XML file.", innerException2);
			}
			finally
			{
				textReader.Close();
			}
		}

		public void WriteToFile(string fileName)
		{
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.Indent = true;
			using (XmlWriter xmlWriter = XmlWriter.Create(fileName, xmlWriterSettings))
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(OEMInput));
				try
				{
					xmlSerializer.Serialize(xmlWriter, this);
				}
				catch (Exception innerException)
				{
					throw new FeatureAPIException("FeatureAPI!WriteToFile: Unable to write OEM Input XML file '" + fileName + "'", innerException);
				}
			}
		}

		public static List<string> GetPackagesFromDSMs(List<string> dsmPaths)
		{
			string text = ".dsm.xml";
			List<string> list = new List<string>();
			foreach (string dsmPath in dsmPaths)
			{
				string[] files = LongPathDirectory.GetFiles(dsmPath, "*" + text);
				for (int i = 0; i < files.Length; i++)
				{
					string fileName = Path.GetFileName(files[i]);
					fileName = fileName.Substring(0, fileName.Length - text.Length);
					list.Add(fileName);
				}
			}
			return list.Distinct(IgnoreCase).ToList();
		}

		public void InferOEMInputFromPackageList(string msFMPattern, List<string> packages)
		{
			IULogger logger = new IULogger();
			string[] array = ((!LongPathDirectory.Exists(msFMPattern)) ? LongPathDirectory.GetFiles(LongPath.GetDirectoryName(msFMPattern), Path.GetFileName(msFMPattern)) : LongPathDirectory.GetFiles(msFMPattern));
			string[] array2 = array;
			foreach (string xmlFile in array2)
			{
				FeatureManifest fm = new FeatureManifest();
				try
				{
					FeatureManifest.ValidateAndLoad(ref fm, xmlFile, logger);
				}
				catch
				{
					continue;
				}
				InferOEMInput(fm, packages);
			}
		}

		public void InferOEMInput(FeatureManifest fm, List<string> packages)
		{
			if (SupportedLanguages == null)
			{
				SupportedLanguages = new SupportedLangs();
				SupportedLanguages.UserInterface = new List<string>();
				SupportedLanguages.Speech = new List<string>();
				SupportedLanguages.Keyboard = new List<string>();
			}
			if (SupportedLanguages.UserInterface.Count() == 0)
			{
				SupportedLanguages.UserInterface = fm.GetUILangFeatures(packages);
			}
			if (Resolutions == null)
			{
				Resolutions = new List<string>();
			}
			if (Resolutions.Count() == 0)
			{
				Resolutions = fm.GetResolutionFeatures(packages);
			}
			if (Features == null)
			{
				Features = new OEMInputFeatures();
				Features.Microsoft = new List<string>();
				Features.OEM = new List<string>();
			}
			foreach (FeatureManifest.FMPkgInfo featureIdentifierPackage in fm.GetFeatureIdentifierPackages())
			{
				string text = featureIdentifierPackage.ID;
				if (featureIdentifierPackage.FMGroup == FeatureManifest.PackageGroups.KEYBOARD || featureIdentifierPackage.FMGroup == FeatureManifest.PackageGroups.SPEECH)
				{
					text = text + PkgFile.DefaultLanguagePattern + featureIdentifierPackage.GroupValue;
				}
				if (packages.Contains(text, IgnoreCase))
				{
					SetOEMInputValue(featureIdentifierPackage);
				}
			}
			if (fm.BootLocalePackageFile != null)
			{
				string bootLocaleBaseName = fm.BootLocalePackageFile.ID.Replace("$(bootlocale)", "", StringComparison.OrdinalIgnoreCase);
				List<string> list = packages.Where((string pkg) => pkg.StartsWith(bootLocaleBaseName, StringComparison.OrdinalIgnoreCase)).ToList();
				if (list.Count > 0)
				{
					BootLocale = list[0].Substring(bootLocaleBaseName.Length);
				}
			}
			if (fm.BootUILanguagePackageFile != null)
			{
				string bootLangBaseName = fm.BootUILanguagePackageFile.ID.Replace("$(bootuilanguage)", "", StringComparison.OrdinalIgnoreCase);
				List<string> list2 = packages.Where((string pkg) => pkg.StartsWith(bootLangBaseName, StringComparison.OrdinalIgnoreCase)).ToList();
				if (list2.Count > 0)
				{
					BootUILanguage = list2[0].Substring(bootLangBaseName.Length);
				}
			}
		}

		private void SetOEMInputValue(FeatureManifest.FMPkgInfo FeatureIDPkg)
		{
			switch (FeatureIDPkg.FMGroup)
			{
			case FeatureManifest.PackageGroups.RELEASE:
				ReleaseType = FeatureIDPkg.GroupValue;
				break;
			case FeatureManifest.PackageGroups.DEVICELAYOUT:
				SOC = FeatureIDPkg.GroupValue;
				break;
			case FeatureManifest.PackageGroups.OEMDEVICEPLATFORM:
				Device = FeatureIDPkg.GroupValue;
				break;
			case FeatureManifest.PackageGroups.SV:
				SV = FeatureIDPkg.GroupValue;
				break;
			case FeatureManifest.PackageGroups.SOC:
				SOC = FeatureIDPkg.GroupValue;
				break;
			case FeatureManifest.PackageGroups.DEVICE:
				Device = FeatureIDPkg.GroupValue;
				break;
			case FeatureManifest.PackageGroups.MSFEATURE:
				Features.Microsoft.Add(FeatureIDPkg.GroupValue);
				break;
			case FeatureManifest.PackageGroups.OEMFEATURE:
				Features.OEM.Add(FeatureIDPkg.GroupValue);
				break;
			case FeatureManifest.PackageGroups.KEYBOARD:
				SupportedLanguages.Keyboard.Add(FeatureIDPkg.GroupValue);
				break;
			case FeatureManifest.PackageGroups.SPEECH:
				SupportedLanguages.Speech.Add(FeatureIDPkg.GroupValue);
				break;
			case FeatureManifest.PackageGroups.PRERELEASE:
				ExcludePrereleaseFeatures = FeatureIDPkg.GroupValue.Equals("replacement", StringComparison.OrdinalIgnoreCase);
				break;
			}
		}

		public List<string> GetFeatureList()
		{
			return GetFeatureList((OEMFeatureTypes)268435455);
		}

		public List<string> GetFeatureList(OEMFeatureTypes forFeatures)
		{
			List<string> list = new List<string>();
			if (forFeatures.HasFlag(OEMFeatureTypes.BASE))
			{
				list.Add(FeatureManifest.PackageGroups.BASE.ToString());
			}
			if (forFeatures.HasFlag(OEMFeatureTypes.SV))
			{
				list.Add(FMGroupAndValueToFeatureID(FeatureManifest.PackageGroups.SV, SV));
			}
			if (forFeatures.HasFlag(OEMFeatureTypes.SOC))
			{
				list.Add(FMGroupAndValueToFeatureID(FeatureManifest.PackageGroups.SOC, SOC));
			}
			if (forFeatures.HasFlag(OEMFeatureTypes.DEVICE))
			{
				list.Add(FMGroupAndValueToFeatureID(FeatureManifest.PackageGroups.DEVICE, Device));
			}
			if (forFeatures.HasFlag(OEMFeatureTypes.RELEASE))
			{
				list.Add(FMGroupAndValueToFeatureID(FeatureManifest.PackageGroups.RELEASE, ReleaseType));
			}
			if (forFeatures.HasFlag(OEMFeatureTypes.UILANGS) && SupportedLanguages != null && SupportedLanguages.UserInterface != null)
			{
				foreach (string item in SupportedLanguages.UserInterface)
				{
					list.Add(KeyAndValueToFeatureID(OEMFeatureTypes.UILANGS.ToString(), item));
				}
			}
			if (forFeatures.HasFlag(OEMFeatureTypes.KEYBOARD) && SupportedLanguages != null)
			{
				foreach (string item2 in SupportedLanguages.Keyboard)
				{
					list.Add(FMGroupAndValueToFeatureID(FeatureManifest.PackageGroups.KEYBOARD, item2));
				}
			}
			if (forFeatures.HasFlag(OEMFeatureTypes.SPEECH) && SupportedLanguages != null)
			{
				foreach (string item3 in SupportedLanguages.Speech)
				{
					list.Add(FMGroupAndValueToFeatureID(FeatureManifest.PackageGroups.SPEECH, item3));
				}
			}
			if (forFeatures.HasFlag(OEMFeatureTypes.BOOTUI))
			{
				list.Add(FMGroupAndValueToFeatureID(FeatureManifest.PackageGroups.BOOTUI, BootUILanguage));
			}
			if (forFeatures.HasFlag(OEMFeatureTypes.BOOTLOCALE))
			{
				list.Add(FMGroupAndValueToFeatureID(FeatureManifest.PackageGroups.BOOTLOCALE, BootLocale));
			}
			if (forFeatures.HasFlag(OEMFeatureTypes.RESOULTIONS) && Resolutions != null)
			{
				foreach (string resolution in Resolutions)
				{
					list.Add(KeyAndValueToFeatureID(OEMFeatureTypes.RESOULTIONS.ToString(), resolution));
				}
			}
			if (forFeatures.HasFlag(OEMFeatureTypes.PRERELEASE))
			{
				string value = (ExcludePrereleaseFeatures ? "REPLACEMENT" : "PROTECTED");
				list.Add(FMGroupAndValueToFeatureID(FeatureManifest.PackageGroups.PRERELEASE, value));
			}
			if (Features != null)
			{
				if (forFeatures.HasFlag(OEMFeatureTypes.MSFEATURES) && Features.Microsoft != null)
				{
					list.AddRange(Features.Microsoft.Select((string feature) => "MS_" + feature));
				}
				if (forFeatures.HasFlag(OEMFeatureTypes.OEMFEATURES) && Features.OEM != null)
				{
					list.AddRange(Features.OEM.Select((string feature) => "OEM_" + feature));
				}
			}
			return list;
		}

		private string FMGroupAndValueToFeatureID(FeatureManifest.PackageGroups group, string value)
		{
			return KeyAndValueToFeatureID(group.ToString(), value);
		}

		private string KeyAndValueToFeatureID(string key, string value)
		{
			string text = key.ToUpper(CultureInfo.InvariantCulture) + "_";
			if (string.IsNullOrEmpty(value))
			{
				return text + "INVALID";
			}
			return text + value.ToUpper(CultureInfo.InvariantCulture);
		}

		public List<string> GetFMs()
		{
			List<string> list = new List<string>();
			if (Edition != null)
			{
				list.AddRange(Edition.CoreFeatureManifestPackages.Select((EditionPackage pkg) => pkg.FMDeviceName));
			}
			foreach (string additionalFM in AdditionalFMs)
			{
				string item = Path.GetFileName(additionalFM).ToUpper(CultureInfo.InvariantCulture);
				list.Add(item);
			}
			return list;
		}
	}
}
