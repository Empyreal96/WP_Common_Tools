using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	public class PackageGenerator : IPackageGenerator
	{
		private BuildPass _buildPass;

		private List<SatelliteId> _allLangauges = new List<SatelliteId>();

		private List<SatelliteId> _allResolutions = new List<SatelliteId>();

		private MacroResolver _macroResolver;

		private IPackageSetBuilder _pkgSetBuilder;

		private XmlValidator _schemaValidator;

		private List<string> _certFiles = new List<string>();

		private List<string> _bcdStores = new List<string>();

		private string _tempDir;

		private string _toolPaths;

		private Dictionary<string, IPkgPlugin> _plugins;

		public ISecurityPolicyCompiler PolicyCompiler { get; private set; }

		public XmlValidator XmlValidator => _schemaValidator;

		public BuildPass BuildPass => _buildPass;

		public CpuId CPU => _pkgSetBuilder.CpuType;

		public IMacroResolver MacroResolver => _macroResolver;

		public string TempDirectory => _tempDir;

		public string ToolPaths => _toolPaths;

		private IPackageSetBuilder CreatePackageSetBuilder(CpuId cpuId, BuildType bldType, VersionInfo version, bool doCompression, bool isRazzleEnv, string spkgOutputFile = null)
		{
			if (_buildPass == BuildPass.BuildTOC)
			{
				return new PackageTocBuilder(cpuId, bldType, version);
			}
			return new PackageSetBuilder(cpuId, bldType, version, doCompression, isRazzleEnv, spkgOutputFile);
		}

		private void BuildSpecialFiles(PackageProject proj, string projPath, string tmpDir)
		{
			if (!proj.IsBinaryPartition)
			{
				string text = null;
				if (projPath.ToLower(CultureInfo.InvariantCulture).EndsWith(".pkg.xml", true, CultureInfo.InvariantCulture))
				{
					text = projPath.Remove(projPath.Length - 8);
					text += ".policy.xml";
				}
				if (text != null && LongPathFile.Exists(text))
				{
					_pkgSetBuilder.AddFile(SatelliteId.Neutral, new FileInfo
					{
						Type = FileType.SecurityPolicy,
						SourcePath = text,
						Attributes = (PkgConstants.c_defaultAttributes & ~FileAttributes.Compressed)
					});
				}
				else
				{
					_macroResolver.BeginLocal();
					_macroResolver.Register("runtime.root", string.Empty, true);
					string text2 = Path.Combine(tmpDir, "policy.xml");
					XmlDocument projectXml = proj.ToXmlDocument();
					if (PolicyCompiler.Compile(_pkgSetBuilder.Name, proj.ProjectFilePath, projectXml, _macroResolver, text2))
					{
						_pkgSetBuilder.AddFile(SatelliteId.Neutral, new FileInfo
						{
							Type = FileType.SecurityPolicy,
							SourcePath = text2,
							Attributes = (PkgConstants.c_defaultAttributes & ~FileAttributes.Compressed)
						});
					}
					_macroResolver.EndLocal();
				}
			}
			if (_certFiles.Count > 0)
			{
				string text3 = Path.Combine(tmpDir, "certs.dat");
				if (CertStoreBuilder.Build(_certFiles, text3))
				{
					_pkgSetBuilder.AddFile(SatelliteId.Neutral, new FileInfo
					{
						Type = FileType.Certificates,
						SourcePath = text3,
						Attributes = PkgConstants.c_defaultAttributes
					});
				}
			}
			if (_bcdStores.Count > 0)
			{
				int num = 0;
				foreach (string bcdStore in _bcdStores)
				{
					string path = $"{PackageTools.BuildPackageName(proj.Owner, proj.Component, proj.SubComponent)}_BCDStore_{num}{PkgConstants.c_strRguExtension}";
					string text4 = Path.Combine(tmpDir, path);
					string devicePath = Path.Combine(PkgConstants.c_strRguDeviceFolder, path);
					BcdConverter.ConvertBCD(bcdStore, text4);
					_pkgSetBuilder.AddFile(SatelliteId.Neutral, new FileInfo
					{
						Type = FileType.Registry,
						SourcePath = text4,
						DevicePath = devicePath,
						Attributes = PkgConstants.c_defaultAttributes
					});
					num++;
				}
			}
			_pkgSetBuilder.AddFile(SatelliteId.Neutral, new FileInfo
			{
				Type = FileType.Invalid,
				SourcePath = null,
				DevicePath = null
			});
		}

		public IEnumerable<SatelliteId> GetSatelliteValues(SatelliteType type)
		{
			switch (type)
			{
			case SatelliteType.Language:
				return _allLangauges;
			case SatelliteType.Resolution:
				return _allResolutions;
			default:
				throw new PkgGenException("Unsupported ExpansionKey '{0}'", type);
			}
		}

		public void AddRegMultiSzSegment(string keyName, string valueName, params string[] valueSegments)
		{
			_pkgSetBuilder.AddMultiSzSegment(_macroResolver.Resolve(keyName), _macroResolver.Resolve(valueName), valueSegments.Select((string x) => _macroResolver.Resolve(x)).ToArray());
		}

		public void AddRegValue(string keyName, string valueName, RegValueType valueType, string value, SatelliteId satelliteId)
		{
			_pkgSetBuilder.AddRegValue(satelliteId, new RegValueInfo
			{
				KeyName = _macroResolver.Resolve(keyName),
				ValueName = _macroResolver.Resolve(valueName),
				Type = valueType,
				Value = _macroResolver.Resolve(value)
			});
		}

		public void AddRegValue(string keyName, string valueName, RegValueType valueType, string value)
		{
			AddRegValue(keyName, valueName, valueType, value, SatelliteId.Neutral);
		}

		public void AddRegExpandValue(string keyName, string valueName, string value)
		{
			_macroResolver.BeginLocal();
			_macroResolver.Register("runtime.root", "%SystemDrive%", true);
			_macroResolver.Register("runtime.windows", "%SystemRoot%", true);
			AddRegValue(keyName, valueName, RegValueType.ExpandString, value, SatelliteId.Neutral);
			_macroResolver.EndLocal();
		}

		public void AddRegKey(string keyName, SatelliteId satelliteId)
		{
			_pkgSetBuilder.AddRegValue(satelliteId, new RegValueInfo
			{
				KeyName = _macroResolver.Resolve(keyName),
				ValueName = null
			});
		}

		public void AddRegKey(string keyName)
		{
			AddRegKey(keyName, SatelliteId.Neutral);
		}

		public void AddFile(string sourcePath, string devicePath, FileAttributes attributes, SatelliteId satelliteId, string embedSignCategory)
		{
			_pkgSetBuilder.AddFile(satelliteId, new FileInfo
			{
				Type = FileType.Regular,
				SourcePath = _macroResolver.Resolve(sourcePath),
				DevicePath = _macroResolver.Resolve(devicePath),
				Attributes = attributes,
				EmbeddedSigningCategory = embedSignCategory
			});
		}

		public void AddFile(string sourcePath, string devicePath, FileAttributes attributes, SatelliteId satelliteId)
		{
			AddFile(sourcePath, devicePath, attributes, satelliteId, "None");
		}

		public void AddFile(string sourcePath, string devicePath, FileAttributes attributes)
		{
			AddFile(sourcePath, devicePath, attributes, SatelliteId.Neutral);
		}

		public void AddCertificate(string sourcePath)
		{
			string item = _macroResolver.Resolve(sourcePath);
			_certFiles.Add(item);
		}

		public void AddBinaryPartition(string sourcePath)
		{
			_pkgSetBuilder.AddFile(SatelliteId.Neutral, new FileInfo
			{
				Type = FileType.BinaryPartition,
				SourcePath = _macroResolver.Resolve(sourcePath),
				DevicePath = null,
				Attributes = PkgConstants.c_defaultAttributes
			});
		}

		public void AddBCDStore(string sourcePath)
		{
			string item = _macroResolver.Resolve(sourcePath);
			_bcdStores.Add(item);
		}

		public RegGroup ImportRegistry(string sourcePath)
		{
			try
			{
				_schemaValidator.Validate(sourcePath);
			}
			catch (XmlSchemaValidationException ex)
			{
				throw new PkgGenProjectException(ex, sourcePath, ex.LineNumber, ex.LinePosition, ex.Message);
			}
			RegGroup regGroup = null;
			try
			{
				using (XmlReader regFileReader = XmlReader.Create(sourcePath))
				{
					return RegGroup.Load(regFileReader);
				}
			}
			catch (XmlException innerException)
			{
				throw new PkgGenProjectException(innerException, sourcePath, "Failed to import registry settings from file.");
			}
		}

		public void Build(string projPath, string outputDir, bool compress)
		{
			try
			{
				_macroResolver.BeginLocal();
				PackageProject packageProject = PackageProject.Load(projPath, _plugins, this);
				_pkgSetBuilder.OwnerType = packageProject.OwnerType;
				_pkgSetBuilder.ReleaseType = packageProject.ReleaseType;
				_pkgSetBuilder.Owner = _macroResolver.Resolve(packageProject.Owner);
				_pkgSetBuilder.Component = _macroResolver.Resolve(packageProject.Component);
				_pkgSetBuilder.SubComponent = _macroResolver.Resolve(packageProject.SubComponent);
				_pkgSetBuilder.Platform = _macroResolver.Resolve(packageProject.Platform);
				_pkgSetBuilder.Partition = _macroResolver.Resolve(packageProject.Partition);
				_pkgSetBuilder.GroupingKey = _macroResolver.Resolve(packageProject.GroupingKey);
				_pkgSetBuilder.Description = _macroResolver.Resolve(packageProject.Description);
				_pkgSetBuilder.Resolutions.Clear();
				_pkgSetBuilder.Resolutions.AddRange(_allResolutions);
				_macroResolver.Register("runtime.root", PackageTools.GetDefaultDriveLetter(packageProject.Partition), true);
				packageProject.Build(this);
				_macroResolver.EndLocal();
				if (_buildPass != 0)
				{
					BuildSpecialFiles(packageProject, projPath, _tempDir);
				}
				if (!LongPathDirectory.Exists(outputDir))
				{
					LongPathDirectory.CreateDirectory(outputDir);
				}
				_pkgSetBuilder.Save(outputDir);
			}
			catch (PkgGenProjectException)
			{
				throw;
			}
			catch (PackageException ex2)
			{
				throw new PkgGenProjectException(ex2, projPath, ex2.Message);
			}
			catch (PkgGenException ex3)
			{
				throw new PkgGenProjectException(ex3, projPath, ex3.Message);
			}
			catch (XmlSchemaValidationException ex4)
			{
				throw new PkgGenProjectException(ex4, projPath, ex4.LineNumber, ex4.LinePosition, ex4.Message);
			}
			finally
			{
				FileUtils.DeleteTree(_tempDir);
			}
		}

		public PackageGenerator(Dictionary<string, IPkgPlugin> plugins, BuildPass buildPass, CpuId cpuId, BuildType bldType, VersionInfo version, ISecurityPolicyCompiler policyCompiler, MacroResolver macroResolver, XmlValidator schemaValidator, string languages, string resolutions, string toolPaths, bool isRazzleEnv)
			: this(plugins, buildPass, cpuId, bldType, version, policyCompiler, macroResolver, schemaValidator, languages, resolutions, toolPaths, isRazzleEnv, false)
		{
		}

		public PackageGenerator(Dictionary<string, IPkgPlugin> plugins, BuildPass buildPass, CpuId cpuId, BuildType bldType, VersionInfo version, ISecurityPolicyCompiler policyCompiler, MacroResolver macroResolver, XmlValidator schemaValidator, string languages, string resolutions, string toolPaths, bool isRazzleEnv, bool doCompression)
			: this(plugins, buildPass, cpuId, bldType, version, policyCompiler, macroResolver, schemaValidator, languages, resolutions, toolPaths, isRazzleEnv, doCompression, null)
		{
		}

		public PackageGenerator(Dictionary<string, IPkgPlugin> plugins, BuildPass buildPass, CpuId cpuId, BuildType bldType, VersionInfo version, ISecurityPolicyCompiler policyCompiler, MacroResolver macroResolver, XmlValidator schemaValidator, string languages, string resolutions, string toolPaths, bool isRazzleEnv, bool doCompression, string spkgOutputFile)
		{
			_tempDir = FileUtils.GetTempDirectory();
			_plugins = plugins ?? new Dictionary<string, IPkgPlugin>();
			_buildPass = buildPass;
			_macroResolver = macroResolver;
			_schemaValidator = schemaValidator;
			_pkgSetBuilder = CreatePackageSetBuilder(cpuId, bldType, version, doCompression, isRazzleEnv, spkgOutputFile);
			_toolPaths = toolPaths;
			PolicyCompiler = policyCompiler;
			if (string.IsNullOrEmpty(languages))
			{
				_allLangauges = new List<SatelliteId>();
			}
			else
			{
				_allLangauges = (from x in languages.Split(';')
					select SatelliteId.Create(SatelliteType.Language, x)).ToList();
			}
			if (string.IsNullOrEmpty(resolutions))
			{
				_allResolutions = new List<SatelliteId>();
				return;
			}
			_allResolutions = (from x in resolutions.Split(';')
				select SatelliteId.Create(SatelliteType.Resolution, x)).ToList();
		}
	}
}
