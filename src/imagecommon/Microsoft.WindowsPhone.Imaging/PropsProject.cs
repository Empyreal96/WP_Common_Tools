using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	[XmlType(Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
	[XmlRoot(ElementName = "Project", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003", IsNullable = false)]
	public class PropsProject
	{
		public enum FeatureTypes
		{
			Generated_Product_Packages
		}

		private List<string> _supportedUILangs;

		private List<string> _supportedLocales;

		private List<string> _supportedResolutions;

		private List<CpuId> _supportedWowCpuTypes;

		private string _buildType;

		private string _cpuType;

		private string _MSPackageRoot;

		[XmlArrayItem(ElementName = "File", Type = typeof(PropsFile), IsNullable = false)]
		[XmlArray]
		public List<PropsFile> ItemGroup;

		[XmlIgnore]
		public List<PropsFile> Files
		{
			get
			{
				return ItemGroup;
			}
			set
			{
				ItemGroup = value;
			}
		}

		public PropsProject()
		{
		}

		public PropsProject(List<string> supportedUILanguages, List<string> supportedLocales, List<string> supportedResolutions, List<CpuId> supportedWowGuestCpuTypes, string buildType, string cpuType, string MSPackageRoot)
		{
			_supportedUILangs = supportedUILanguages;
			_supportedLocales = supportedLocales;
			_supportedResolutions = supportedResolutions;
			_supportedWowCpuTypes = supportedWowGuestCpuTypes;
			_buildType = buildType;
			_cpuType = cpuType;
			_MSPackageRoot = MSPackageRoot;
		}

		public void AddPackages(FeatureManifest fm)
		{
			List<FeatureManifest.FMPkgInfo> allPackagesByGroups = fm.GetAllPackagesByGroups(_supportedUILangs, _supportedLocales, _supportedResolutions, _supportedWowCpuTypes, _buildType, _cpuType, _MSPackageRoot);
			if (Files == null)
			{
				Files = new List<PropsFile>();
			}
			foreach (FeatureManifest.FMPkgInfo item in allPackagesByGroups)
			{
				PropsFile propFile = new PropsFile();
				string rawBasePath = item.RawBasePath;
				string fileName = Path.GetFileName(item.PackagePath);
				propFile.Include = ConvertToInclude(rawBasePath, fileName);
				if (Files.Where((PropsFile prop) => prop.Include.Equals(propFile.Include, StringComparison.OrdinalIgnoreCase)).Count() == 0)
				{
					propFile.Feature = FeatureTypes.Generated_Product_Packages.ToString();
					propFile.InstallPath = ConvertToInstallPath(rawBasePath);
					SetGUID(ref propFile);
					propFile.Owner = "FeatureManifest";
					propFile.BusinessReason = "Device Imaging";
					Files.Add(propFile);
				}
			}
		}

		private string ConvertToInstallPath(string installPath)
		{
			string text = installPath;
			text = text.Replace(Path.GetFileName(text), "").TrimEnd('\\');
			text = text.Replace("$(cputype)", "$(_BuildArch)", StringComparison.OrdinalIgnoreCase);
			text = text.Replace("$(buildtype)", "$(_BuildType)", StringComparison.OrdinalIgnoreCase);
			return text.Replace("$(mspackageroot)", "$(WP_PACKAGES_INSTALL_PATH)", StringComparison.OrdinalIgnoreCase);
		}

		private string ConvertToInclude(string include, string fileName)
		{
			string text = include;
			text = text.Replace(Path.GetFileName(text), fileName, StringComparison.OrdinalIgnoreCase);
			text = text.Replace("$(cputype)", "$(_BuildArch)", StringComparison.OrdinalIgnoreCase);
			text = text.Replace("$(buildtype)", "$(_BuildType)", StringComparison.OrdinalIgnoreCase);
			return text.Replace("$(mspackageroot)", "$(BINARY_ROOT)\\prebuilt", StringComparison.OrdinalIgnoreCase);
		}

		private void SetGUID(ref PropsFile propsFile)
		{
			string text = Guid.NewGuid().ToString("B");
			if (string.Equals(_cpuType, FeatureManifest.CPUType_ARM, StringComparison.OrdinalIgnoreCase))
			{
				if (string.Equals(_buildType, "fre", StringComparison.OrdinalIgnoreCase))
				{
					propsFile.MC_ARM_FRE = text;
				}
				else
				{
					propsFile.MC_ARM_CHK = text;
				}
			}
			else if (string.Equals(_cpuType, FeatureManifest.CPUType_X86, StringComparison.OrdinalIgnoreCase))
			{
				if (string.Equals(_buildType, "fre", StringComparison.OrdinalIgnoreCase))
				{
					propsFile.MC_X86_FRE = text;
				}
				else
				{
					propsFile.MC_X86_CHK = text;
				}
			}
			else if (string.Equals(_cpuType, FeatureManifest.CPUType_ARM64, StringComparison.OrdinalIgnoreCase))
			{
				if (string.Equals(_buildType, "fre", StringComparison.OrdinalIgnoreCase))
				{
					propsFile.MC_ARM64_FRE = text;
				}
				else
				{
					propsFile.MC_ARM64_CHK = text;
				}
			}
			else if (string.Equals(_cpuType, FeatureManifest.CPUType_AMD64, StringComparison.OrdinalIgnoreCase))
			{
				if (string.Equals(_buildType, "fre", StringComparison.OrdinalIgnoreCase))
				{
					propsFile.MC_AMD64_FRE = text;
				}
				else
				{
					propsFile.MC_AMD64_CHK = text;
				}
			}
		}

		public static PropsProject ValidateAndLoad(string xmlFile, IULogger logger)
		{
			PropsProject propsProject = new PropsProject();
			string text = string.Empty;
			string propsProjectSchema = BuildPaths.PropsProjectSchema;
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
			foreach (string text2 in manifestResourceNames)
			{
				if (text2.Contains(propsProjectSchema))
				{
					text = text2;
					break;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				throw new ImageCommonException("ImageCommon!ValidateInput: XSD resource was not found: " + propsProjectSchema);
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
					throw new ImageCommonException("ImageCommon!ValidateInput: Unable to validate Props Project XSD.", innerException);
				}
			}
			logger.LogInfo("ImageCommon: Successfully validated the Props Project XML: {0}", xmlFile);
			TextReader textReader = new StreamReader(xmlFile);
			try
			{
				return (PropsProject)new XmlSerializer(typeof(PropsProject)).Deserialize(textReader);
			}
			catch (Exception innerException2)
			{
				throw new ImageCommonException("ImageCommon!ValidateInput: Unable to parse Props Project XML file.", innerException2);
			}
			finally
			{
				textReader.Close();
			}
		}

		public void Merge(PropsProject sourceProps)
		{
			if (Files == null)
			{
				if (sourceProps != null && sourceProps.Files != null)
				{
					Files = sourceProps.Files;
				}
			}
			else if (sourceProps != null && sourceProps.Files != null)
			{
				Files.AddRange(sourceProps.Files);
			}
		}

		public void WriteToFile(string fileName)
		{
			TextWriter textWriter = new StreamWriter(fileName);
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(PropsProject));
			try
			{
				xmlSerializer.Serialize(textWriter, this);
			}
			catch (Exception innerException)
			{
				throw new ImageCommonException("ImageCommon!WriteToFile: Unable to write Props Project XML file '" + fileName + "'", innerException);
			}
			finally
			{
				textWriter.Close();
			}
		}
	}
}
