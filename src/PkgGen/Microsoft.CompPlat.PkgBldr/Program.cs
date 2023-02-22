using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Base.Security;
using Microsoft.CompPlat.PkgBldr.Base.Tools;
using Microsoft.CompPlat.PkgBldr.Tools;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.CompPlat.PkgBldr
{
	public class Program
	{
		public enum ErrorLevel
		{
			error,
			warn,
			silent
		}

		private const int ERROR_STATUS_SUCCESS = 0;

		private const int ERROR_STATUS_FAILED = -1;

		private const int ERROR_STATUS_SKIPPED = 1;

		private static ErrorLevel m_errorLevel;

		private static PkgBldrLoader m_pkgFilterLoader;

		private static PkgBldrLoader m_pkgToWmLoader;

		private static PkgBldrLoader m_wmToCsiLoader;

		private static PkgBldrLoader m_csiToCsiLoader;

		private static PkgBldrCmd m_cmdArgs;

		private static Logger logger;

		public static void BuildPackage(Config config)
		{
			if (config.Convert == ConversionType.csi2pkg)
			{
				PkgBldrLoader pkgBldrLoader = new PkgBldrLoader(PluginType.Csi2Pkg, m_cmdArgs);
				XDocument xDocument = PkgBldrHelpers.XDocumentLoadFromLongPath(config.Input);
				XElement xElement = new XElement((XNamespace)"urn:Microsoft.WindowsPhone/PackageSchema.v8.00" + "Package");
				pkgBldrLoader.Plugins[xDocument.Root.Name.LocalName].ConvertEntries(xElement, pkgBldrLoader.Plugins, config, xDocument.Root);
				if (config.ExitStatus == ExitStatus.SKIPPED)
				{
					logger.LogInfo("Skipping {0}", config.Input);
					return;
				}
				XDocument xDocument2 = new XDocument(xElement);
				pkgBldrLoader.ValidateOutput(xDocument2);
				PkgBldrHelpers.XDocumentSaveToLongPath(xDocument2, config.Output);
			}
			else if (config.Convert == ConversionType.csi2wm)
			{
				PkgBldrLoader pkgBldrLoader2 = new PkgBldrLoader(PluginType.CsiToWm, m_cmdArgs);
				XDocument xDocument3 = PkgBldrHelpers.XDocumentLoadFromLongPath(config.Input);
				if (!IsThisACsiManifest(xDocument3.Root))
				{
					logger.LogWarning("Can't convert, assemblyIdentity not found ");
					return;
				}
				XElement xElement2 = new XElement((XNamespace)"urn:Microsoft.CompPlat/ManifestSchema.v1.00" + "identity");
				pkgBldrLoader2.Plugins[xDocument3.Root.Name.LocalName].ConvertEntries(xElement2, pkgBldrLoader2.Plugins, config, xDocument3.Root);
				if (config.ExitStatus == ExitStatus.SKIPPED)
				{
					logger.LogInfo("Skipping {0}", config.Input);
					return;
				}
				XDocument xDocument4 = new XDocument(xElement2);
				pkgBldrLoader2.ValidateOutput(xDocument4);
				PkgBldrHelpers.XDocumentSaveToLongPath(xDocument4, config.Output);
			}
			else if (config.Convert == ConversionType.pkg2wm)
			{
				if (m_pkgToWmLoader == null)
				{
					m_pkgToWmLoader = new PkgBldrLoader(PluginType.PkgToWm, m_cmdArgs);
				}
				if (config.Bld.PKG.Root == null)
				{
					XDocument xDocument5 = PkgBldrHelpers.XDocumentLoadFromLongPath(config.Input);
					m_pkgToWmLoader.ValidateInput(xDocument5);
					config.Bld.PKG.Root = xDocument5.Root;
				}
				XElement xElement3 = new XElement("identity");
				config.ExitStatus = ExitStatus.SUCCESS;
				m_pkgToWmLoader.Plugins[config.Bld.PKG.Root.Name.LocalName].ConvertEntries(xElement3, m_pkgToWmLoader.Plugins, config, config.Bld.PKG.Root);
				if (config.ExitStatus == ExitStatus.SKIPPED)
				{
					return;
				}
				XDocument xDocument6 = new XDocument(xElement3);
				m_pkgToWmLoader.ValidateOutput(xDocument6);
				if (config.Output != null)
				{
					logger.LogInfo("Writing {0}", config.Output);
					if (File.Exists(config.Output))
					{
						SdCommand.Run("edit", config.Output);
					}
					PkgBldrHelpers.XDocumentSaveToLongPath(xDocument6, config.Output);
					SdCommand.Run("add", config.Output);
				}
			}
			else
			{
				if (config.Convert != 0)
				{
					return;
				}
				if (m_wmToCsiLoader == null)
				{
					m_wmToCsiLoader = new PkgBldrLoader(PluginType.WmToCsi, m_cmdArgs);
				}
				if (config.Bld.WM.Root == null)
				{
					XDocument xDocument7 = PkgBldrHelpers.XDocumentLoadFromLongPath(config.Input);
					m_wmToCsiLoader.ValidateInput(xDocument7);
					config.Bld.WM.Root = xDocument7.Root;
				}
				XElement xElement4 = new XElement("assembly");
				config.GlobalSecurity = new GlobalSecurity();
				config.ExitStatus = ExitStatus.SUCCESS;
				m_wmToCsiLoader.Plugins[config.Bld.WM.Root.Name.LocalName].ConvertEntries(xElement4, m_wmToCsiLoader.Plugins, config, config.Bld.WM.Root);
				if (config.ExitStatus != ExitStatus.SKIPPED)
				{
					m_wmToCsiLoader.ValidateOutput(new XDocument(xElement4));
					if (m_csiToCsiLoader == null)
					{
						m_csiToCsiLoader = new PkgBldrLoader(PluginType.CsiToCsi, m_cmdArgs);
					}
					m_csiToCsiLoader.Plugins[xElement4.Name.LocalName].ConvertEntries(xElement4, m_csiToCsiLoader.Plugins, config, xElement4);
					XDocument xDocument8 = new XDocument(xElement4);
					m_csiToCsiLoader.ValidateOutput(xDocument8);
					if (config.GenerateCab)
					{
						config.Macros = new MacroResolver();
						PkgBldrLoader pkgBldrLoader3 = new PkgBldrLoader(PluginType.CsiToCab, m_cmdArgs);
						pkgBldrLoader3.Plugins[xElement4.Name.LocalName].ConvertEntries(xElement4, pkgBldrLoader3.Plugins, config, xElement4);
					}
					else
					{
						PkgBldrHelpers.XDocumentSaveToLongPath(xDocument8, config.Output);
					}
				}
			}
		}

		private static void WriteSchemas(string outputDir)
		{
			PkgBldrLoader pkgBldrLoader = new PkgBldrLoader(PluginType.WmToCsi, m_cmdArgs);
			List<XmlSchema> list = pkgBldrLoader.WmSchemaSet();
			int num = 0;
			outputDir = outputDir.TrimEnd('\\');
			foreach (XmlSchema item in list)
			{
				string text = outputDir + "\\WM" + num.ToString(CultureInfo.InvariantCulture) + ".xsd";
				num++;
				logger.LogInfo("Writing {0}", text);
				WriteXmlSchema(item, text);
			}
			List<XmlSchema> list2 = pkgBldrLoader.CsiSchemaSet();
			num = 0;
			foreach (XmlSchema item2 in list2)
			{
				string text2 = outputDir + "\\CSI" + num.ToString(CultureInfo.InvariantCulture) + ".xsd";
				num++;
				logger.LogInfo("Writing {0}", text2);
				WriteXmlSchema(item2, text2);
			}
		}

		private static void PkgToCab(Config config, List<string> spkgGenArgs)
		{
			Build build = config.build;
			bool flag = false;
			if (BuildWow(config.Input))
			{
				flag = true;
				build.AddGuest();
			}
			string text = build.WowDir ?? GetWowDirFromOutput(spkgGenArgs);
			if (flag)
			{
				XElement filtered = null;
				{
					foreach (Build.WowType wowType in build.GetWowTypes())
					{
						if ((config.Bld.Arch == CpuType.amd64 || config.Bld.Arch == CpuType.arm64) && wowType == Build.WowType.guest)
						{
							continue;
						}
						WowBuildType? wowBuilds = config.build.WowBuilds;
						WowBuildType wowBuildType = WowBuildType.HostOnly;
						if (wowBuilds.GetValueOrDefault() == wowBuildType && wowBuilds.HasValue && wowType == Build.WowType.guest)
						{
							continue;
						}
						wowBuilds = config.build.WowBuilds;
						wowBuildType = WowBuildType.GuestOnly;
						if (wowBuilds.GetValueOrDefault() == wowBuildType && wowBuilds.HasValue && wowType != Build.WowType.guest)
						{
							continue;
						}
						XDocument unfiltered = PkgBldrHelpers.XDocumentLoadFromLongPath(config.Input);
						config.build.wow = wowType;
						FilterPkgXml(unfiltered, out filtered, config);
						string text2 = Path.GetTempFileName();
						bool wow = false;
						switch (wowType)
						{
						case Build.WowType.host:
							text2 += ".host.pkg.xml";
							break;
						case Build.WowType.guest:
							text2 += ".guest.pkg.xml";
							wow = true;
							break;
						}
						XDocument document = new XDocument(filtered);
						logger.LogInfo("PkgFilter: {0}", text2);
						PkgBldrHelpers.XDocumentSaveToLongPath(document, text2);
						ChangeSpkgGenInput(spkgGenArgs, text2);
						string tempDirectory = Microsoft.CompPlat.PkgBldr.Tools.FileUtils.GetTempDirectory();
						if (wowType == Build.WowType.guest)
						{
							RedirectOutput(spkgGenArgs, tempDirectory);
						}
						bool inWindows = false;
						List<string> list = new List<string>();
						Run.RunSPkgGen(spkgGenArgs, inWindows, logger, m_cmdArgs, list);
						foreach (string item in list)
						{
							if (wowType == Build.WowType.guest)
							{
								Run.RunDsmConverter(item, text, wow, false);
								CopyDsmXmlToWowDir(item, tempDirectory, text);
							}
							else
							{
								Run.RunDsmConverter(item, wow, false);
							}
							logger.LogInfo("PkgFilter: {0}", item.Replace(".spkg", ".cab"));
							if (wowType == Build.WowType.guest)
							{
								Microsoft.CompPlat.PkgBldr.Tools.LongPathFile.Delete(item);
							}
						}
					}
					return;
				}
			}
			bool inWindows2 = false;
			List<string> list2 = new List<string>();
			Run.RunSPkgGen(spkgGenArgs, inWindows2, logger, m_cmdArgs, list2);
			foreach (string item2 in list2)
			{
				Run.RunDsmConverter(item2, false, false);
				logger.LogInfo("PkgFilter: {0}", item2.Replace(".spkg", ".cab"));
			}
		}

		private static void CopyDsmXmlToWowDir(string spkg, string tempDir, string wowDir)
		{
			string path = Path.GetFileNameWithoutExtension(spkg) + ".man.dsm.xml";
			string text = Path.Combine(tempDir, path);
			string destinationPath = Path.Combine(wowDir, path);
			if (Microsoft.CompPlat.PkgBldr.Tools.LongPathFile.Exists(text))
			{
				Microsoft.CompPlat.PkgBldr.Tools.LongPathFile.Copy(text, destinationPath, true);
			}
		}

		private static string GetWowDirFromOutput(List<string> spkgGenArgs)
		{
			string result = null;
			string outputOption = GetOutputOption(spkgGenArgs);
			if (outputOption != null)
			{
				int num = outputOption.IndexOf(':');
				result = Regex.Replace(outputOption.Substring(num + 1), "\\\\prebuilt\\\\", "\\prebuilt\\wow\\", RegexOptions.IgnoreCase);
			}
			return result;
		}

		private static int Main(string[] args)
		{
			logger = new Logger();
			try
			{
				logger.LogInfo(Microsoft.CompPlat.PkgBldr.Tools.CommonUtils.GetCopyrightString());
				m_cmdArgs = Microsoft.WindowsPhone.ImageUpdate.Tools.Common.CmdArgsParser.ParseArgs<PkgBldrCmd>(FixArgs(args), new object[1] { CmdModes.LegacySwitchFormat });
				if (args.Length == 0)
				{
					Microsoft.WindowsPhone.ImageUpdate.Tools.Common.CmdArgsParser.ParseUsage<PkgBldrCmd>(new List<CmdModes> { CmdModes.LegacySwitchFormat });
					return -1;
				}
				if (m_cmdArgs == null)
				{
					return -1;
				}
				if (m_cmdArgs.quiet)
				{
					logger.SetLoggingLevel(LoggingLevel.Warning);
				}
				else if (m_cmdArgs.diagnostic)
				{
					logger.SetLoggingLevel(LoggingLevel.Debug);
				}
				string version = m_cmdArgs.version;
				CheckVersion(ref version, m_cmdArgs.usentverp);
				if (!string.IsNullOrEmpty(m_cmdArgs.wmxsd))
				{
					m_cmdArgs.wmxsd = Microsoft.CompPlat.PkgBldr.Tools.LongPath.GetFullPath(m_cmdArgs.wmxsd.TrimEnd('\\'));
					if (!Microsoft.CompPlat.PkgBldr.Tools.LongPathDirectory.Exists(m_cmdArgs.wmxsd))
					{
						throw new PkgGenException("wmxsd directory {0} does not exist", m_cmdArgs.wmxsd);
					}
					WriteSchemas(m_cmdArgs.wmxsd);
					return 0;
				}
				if (string.IsNullOrEmpty(m_cmdArgs.variables))
				{
					m_cmdArgs.variables = $"BUILD_OS_VERSION={version}";
				}
				else
				{
					m_cmdArgs.variables = m_cmdArgs.variables.TrimEnd(';');
					m_cmdArgs.variables += $";BUILD_OS_VERSION={version}";
				}
				List<string> spkgGenArguments = GetSpkgGenArguments(m_cmdArgs, version);
				if (string.IsNullOrEmpty(m_cmdArgs.project) && m_cmdArgs.convert == ConversionType.pkg2cab)
				{
					bool inWindows = false;
					Run.RunSPkgGen(spkgGenArguments, inWindows, logger, m_cmdArgs);
					return 0;
				}
				List<SatelliteId> list = new List<SatelliteId>();
				SatelliteId satelliteId = SatelliteId.Create(SatelliteType.Neutral, null);
				list.Add(satelliteId);
				if (!string.IsNullOrEmpty(m_cmdArgs.languages))
				{
					List<SatelliteId> collection = (from x in m_cmdArgs.languages.Split(';')
						select SatelliteId.Create(SatelliteType.Language, x)).ToList();
					list.AddRange(collection);
				}
				if (!string.IsNullOrEmpty(m_cmdArgs.resolutions))
				{
					List<SatelliteId> collection2 = (from x in m_cmdArgs.resolutions.Split(';')
						select SatelliteId.Create(SatelliteType.Resolution, x)).ToList();
					list.AddRange(collection2);
				}
				Config config = new Config();
				Build build = new Build();
				config.Input = m_cmdArgs.project;
				config.Output = m_cmdArgs.output;
				build.WowDir = m_cmdArgs.wowdir;
				config.Convert = m_cmdArgs.convert;
				build.WowBuilds = m_cmdArgs.wowbuild;
				config.ProcessInf = m_cmdArgs.processInf;
				config.GenerateCab = m_cmdArgs.makecab;
				config.build = build;
				config.Bld = new Bld();
				config.Bld.BuildMacros = new MacroResolver();
				config.Logger = logger;
				config.pkgBldrArgs = m_cmdArgs;
				if (!string.IsNullOrEmpty(m_cmdArgs.variables))
				{
					config.Bld.BuildMacros.Register(ImportCommandLineMacros(m_cmdArgs.variables));
				}
				config.Bld.Version = version;
				config.Bld.Arch = m_cmdArgs.cpu;
				config.Bld.Product = m_cmdArgs.product;
				if (config.Convert == ConversionType.pkg2cab)
				{
					PkgToCab(config, spkgGenArguments);
					return 0;
				}
				VerifyInputExtension(config.Convert, config.Input);
				if (config.Convert == ConversionType.pkg2csi || config.Convert == ConversionType.pkg2wm)
				{
					config.build.satellite = satelliteId;
					config.Bld.JsonDepot = m_cmdArgs.json;
					if (config.Convert != ConversionType.pkg2csi)
					{
						BuildPackage(config);
						return 0;
					}
					config.Convert = ConversionType.pkg2wm;
					config.Output = null;
					BuildPackage(config);
					config.Convert = ConversionType.wm2csi;
					PkgBldrHelpers.XDocumentSaveToLongPath(new XDocument(config.Bld.WM.Root), Microsoft.CompPlat.PkgBldr.Tools.LongPath.Combine(m_cmdArgs.output, Path.GetRandomFileName() + ".wm.xml"));
				}
				build.AddGuest();
				foreach (SatelliteId item in list)
				{
					foreach (Build.WowType wowType in build.GetWowTypes())
					{
						if ((config.Bld.Arch != CpuType.amd64 && config.Bld.Arch != CpuType.arm64) || wowType != Build.WowType.guest)
						{
							config.Output = m_cmdArgs.output;
							build.satellite = item;
							build.wow = wowType;
							config.Bld.Lang = null;
							config.Bld.Resolution = null;
							if (item.Culture != null)
							{
								config.Bld.Lang = item.Culture.ToLowerInvariant();
								config.Bld.BuildMacros.Register("langid", config.Bld.Lang, true);
							}
							else
							{
								config.Bld.Lang = "neutral";
							}
							if (item.Resolution != null)
							{
								config.Bld.Resolution = item.Resolution.ToLowerInvariant();
							}
							if (wowType == Build.WowType.guest)
							{
								config.Bld.IsGuest = true;
							}
							else
							{
								config.Bld.IsGuest = false;
							}
							BuildPackage(config);
						}
					}
				}
				return 0;
			}
			catch (Exception ex)
			{
				logger.LogInfo("--Stack Trace--");
				logger.LogInfo(ex.StackTrace);
				logger.LogInfo("--End Stack Trace--");
				string format = ex.Message.Replace(',', ' ');
				switch (m_errorLevel)
				{
				case ErrorLevel.error:
					logger.LogError(format);
					return -1;
				case ErrorLevel.silent:
					logger.LogInfo(format);
					return 0;
				case ErrorLevel.warn:
					logger.LogWarning(format);
					return 0;
				default:
					return 0;
				}
			}
		}

		private static string[] FixArgs(string[] args)
		{
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			foreach (string text in args)
			{
				if (IsNamedArg(text))
				{
					list2.Add(text);
				}
				else
				{
					list.Add(text);
				}
			}
			list.AddRange(list2);
			return list.ToArray();
		}

		private static bool IsNamedArg(string arg)
		{
			if (!arg.StartsWith("/", StringComparison.OrdinalIgnoreCase) && !arg.StartsWith("+", StringComparison.OrdinalIgnoreCase))
			{
				return arg.StartsWith("-", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}

		private static void CheckVersion(ref string version, bool usentverp)
		{
			if (!Regex.Match(version, "^[0-9]+\\.[0-9]+\\.[0-9]+\\.[0-9]+$", RegexOptions.CultureInvariant).Success)
			{
				throw new PkgGenException("Input version '{0}' is not correctly formatted.", version);
			}
			if (usentverp)
			{
				version = GetNtVerpVersion();
			}
			if (version.Split('.').Sum((string x) => int.Parse(x, CultureInfo.InvariantCulture)) == 0)
			{
				throw new PkgGenException("Version '{0}' can't be zero or package will fail to install", version);
			}
		}

		private static string GetNtVerpVersion()
		{
			string workingDirectory = Environment.ExpandEnvironmentVariables(m_cmdArgs.razzleToolPath);
			string processName = Environment.ExpandEnvironmentVariables(m_cmdArgs.toolPaths["perl"]);
			return Regex.Match(Run.RunProcess(workingDirectory, processName, "version.pl", logger), "[0-9]+\\.[0-9]+\\.[0-9]+\\.[0-9]+", RegexOptions.CultureInvariant).Value;
		}

		private static void ChangeSpkgGenInput(List<string> sPkgGenArgs, string NewInput)
		{
			string item = null;
			foreach (string sPkgGenArg in sPkgGenArgs)
			{
				if (sPkgGenArg.TrimEnd('"').EndsWith(".pkg.xml", StringComparison.OrdinalIgnoreCase))
				{
					item = sPkgGenArg;
					break;
				}
			}
			sPkgGenArgs.Remove(item);
			sPkgGenArgs.Add(NewInput);
		}

		private static void RedirectOutput(List<string> spkgGenArgs, string newOutputDir)
		{
			string outputOption = GetOutputOption(spkgGenArgs);
			if (!string.IsNullOrEmpty(outputOption))
			{
				ReplaceOutputDirectory(spkgGenArgs, outputOption, newOutputDir);
			}
		}

		private static void ReplaceOutputDirectory(List<string> spkgGenArgs, string oldOutputOption, string newOutputDirectory)
		{
			string text = AddQuotes("/output:" + newOutputDirectory);
			spkgGenArgs.Remove(oldOutputOption);
			spkgGenArgs.Add(text);
			logger.LogInfo("PkgFilter: SPkgGen output redirected to {0}", text);
		}

		private static string GetOutputOption(List<string> spkgGenArgs)
		{
			string result = null;
			foreach (string spkgGenArg in spkgGenArgs)
			{
				if (spkgGenArg.StartsWith("/output:", StringComparison.OrdinalIgnoreCase))
				{
					return spkgGenArg;
				}
			}
			return result;
		}

		private static bool BuildWow(string input)
		{
			string attributeValue = PkgBldrHelpers.GetAttributeValue(PkgBldrHelpers.XDocumentLoadFromLongPath(input).Root, "BuildWow");
			if (attributeValue == null)
			{
				return false;
			}
			if (attributeValue.Equals("yes", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			throw new PkgGenException("BuildWow!=Yes");
		}

		private static void FilterPkgXml(XDocument unfiltered, out XElement filtered, Config config)
		{
			if (m_pkgFilterLoader == null)
			{
				m_pkgFilterLoader = new PkgBldrLoader(PluginType.PkgFilter, m_cmdArgs);
			}
			filtered = new XElement("Package");
			m_pkgFilterLoader.Plugins["PkgFilter"].ConvertEntries(filtered, m_pkgFilterLoader.Plugins, config, unfiltered.Root);
		}

		private static bool IsThisACsiManifest(XElement csiRoot)
		{
			if (csiRoot.Element(csiRoot.Name.Namespace + "assemblyIdentity") == null)
			{
				return false;
			}
			return true;
		}

		private static List<string> GetSpkgGenArguments(PkgBldrCmd pkgBldrArgs, string versionOverride)
		{
			List<string> list = new List<string>();
			if (!string.IsNullOrEmpty(pkgBldrArgs.project))
			{
				list.Add(AddQuotes(pkgBldrArgs.project));
			}
			if (!string.IsNullOrEmpty(pkgBldrArgs.config))
			{
				list.Add(string.Format(CultureInfo.InvariantCulture, "/config:{0}", new object[1] { AddQuotes(pkgBldrArgs.config) }));
			}
			if (!string.IsNullOrEmpty(pkgBldrArgs.xsd))
			{
				list.Add(string.Format(CultureInfo.InvariantCulture, "/xsd:{0}", new object[1] { AddQuotes(pkgBldrArgs.xsd) }));
			}
			if (!string.IsNullOrEmpty(pkgBldrArgs.output))
			{
				list.Add(string.Format(CultureInfo.InvariantCulture, "/output:{0}", new object[1] { AddQuotes(pkgBldrArgs.output) }));
			}
			BuildType build = pkgBldrArgs.build;
			string text = ((build == BuildType.fre || build != BuildType.chk) ? "fre" : "chk");
			list.Add(string.Format(CultureInfo.InvariantCulture, "/build:{0}", new object[1] { text }));
			string text2;
			switch (pkgBldrArgs.cpu)
			{
			case CpuType.x86:
				text2 = "X86";
				break;
			case CpuType.amd64:
				text2 = "AMD64";
				break;
			case CpuType.arm64:
				text2 = "ARM64";
				break;
			default:
				text2 = "ARM";
				break;
			}
			list.Add(string.Format(CultureInfo.InvariantCulture, "/cpu:{0}", new object[1] { text2 }));
			if (!string.IsNullOrEmpty(pkgBldrArgs.languages))
			{
				list.Add(string.Format(CultureInfo.InvariantCulture, "/languages:{0}", new object[1] { AddQuotes(pkgBldrArgs.languages) }));
			}
			if (!string.IsNullOrEmpty(pkgBldrArgs.resolutions))
			{
				list.Add(string.Format(CultureInfo.InvariantCulture, "/resolutions:{0}", new object[1] { AddQuotes(pkgBldrArgs.resolutions) }));
			}
			if (!string.IsNullOrEmpty(pkgBldrArgs.variables))
			{
				list.Add(string.Format(CultureInfo.InvariantCulture, "/variables:{0}", new object[1] { AddQuotes(pkgBldrArgs.variables) }));
			}
			if (!string.IsNullOrEmpty(pkgBldrArgs.spkgGenToolDirs))
			{
				list.Add(string.Format(CultureInfo.InvariantCulture, "/toolPaths:{0}", new object[1] { AddQuotes(pkgBldrArgs.spkgGenToolDirs) }));
			}
			if (pkgBldrArgs.toc)
			{
				list.Add("/toc");
			}
			if (pkgBldrArgs.compress)
			{
				list.Add("/compress");
			}
			if (pkgBldrArgs.diagnostic)
			{
				list.Add("/diagnostic");
			}
			if (pkgBldrArgs.nohives || pkgBldrArgs.onecore)
			{
				list.Add("/nohives");
			}
			if (pkgBldrArgs.isRazzleEnv)
			{
				list.Add("/isRazzleEnv");
			}
			list.Add(string.Format(CultureInfo.InvariantCulture, "/version:{0}", new object[1] { versionOverride }));
			return list;
		}

		private static string AddQuotes(string arg)
		{
			string result = arg;
			if (arg.Contains(' '))
			{
				string value = Regex.Match(arg, "^/[^:]+:").Value;
				result = ((!string.IsNullOrEmpty(value)) ? Regex.Replace(arg, value, value + "\"") : ("\"" + arg));
				result += "\"";
			}
			return result;
		}

		private static void WriteXmlSchema(XmlSchema schema, string filePath)
		{
			filePath = Microsoft.CompPlat.PkgBldr.Tools.LongPath.GetFullPath(filePath);
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				NewLineHandling = NewLineHandling.Replace
			};
			using (XmlWriter writer = XmlWriter.Create(filePath, settings))
			{
				schema.Write(writer);
			}
		}

		private static Dictionary<string, Macro> ImportCommandLineMacros(string variables)
		{
			Regex regex = new Regex("^(?<name>[[A-Za-z.0-9_{-][A-Za-z.0-9_+{}-]*)=\\s*(?<value>.*?)\\s*$");
			Dictionary<string, Macro> dictionary = new Dictionary<string, Macro>(StringComparer.OrdinalIgnoreCase);
			string[] array = variables.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string text in array)
			{
				Match match = regex.Match(text);
				if (match == null || !match.Success)
				{
					throw new PkgGenException("Incorrect syntax in variable definition '{0}'", text);
				}
				string value = match.Groups["name"].Value;
				string text2 = match.Groups["value"].Value;
				if (text2.StartsWith("\"", StringComparison.OrdinalIgnoreCase))
				{
					if (!text2.EndsWith("\"", StringComparison.OrdinalIgnoreCase))
					{
						throw new PkgGenException("Incorrect syntax in variable definition '{0}'", text);
					}
					text2 = text2.Substring(1, text2.Length - 2);
				}
				Macro value2 = null;
				if (dictionary.TryGetValue(value, out value2))
				{
					logger.LogWarning("Command line macro value overwriting, macro name:'{0}', old value:'{1}', new value:'{2}'", value, value2.Value, text2);
				}
				dictionary[value] = new Macro(value, text2);
			}
			return dictionary;
		}

		private static void VerifyInputExtension(ConversionType usage, string inputXml)
		{
			if (inputXml == null)
			{
				throw new PkgGenException("PkgGen project not set");
			}
			string text = null;
			switch (usage)
			{
			case ConversionType.pkg2wm:
			case ConversionType.pkg2csi:
			case ConversionType.pkg2cab:
				text = ".pkg.xml";
				break;
			case ConversionType.csi2wm:
			case ConversionType.csi2pkg:
				text = ".man";
				break;
			case ConversionType.wm2csi:
				text = ".wm.xml";
				break;
			}
			if (text != null && !inputXml.EndsWith(text, StringComparison.OrdinalIgnoreCase))
			{
				throw new PkgGenException("Input file {0} does not end with {1}", inputXml, text);
			}
		}
	}
}
