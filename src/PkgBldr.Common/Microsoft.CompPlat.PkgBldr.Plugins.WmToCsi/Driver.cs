using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Base.Security.SecurityPolicy;
using Microsoft.CompPlat.PkgBldr.Interfaces;
using Microsoft.CompPlat.PkgBldr.Tools;
using Microsoft.WindowsPhone.Security.SecurityPolicyCompiler;

namespace Microsoft.CompPlat.PkgBldr.Plugins.WmToCsi
{
	[Export(typeof(IPkgPlugin))]
	internal class Driver : PkgPlugin
	{
		private class WindowsManifestIdentity
		{
			public string Name { get; set; }

			public string NameSpace { get; set; }

			public string Owner { get; set; }

			public string OwnerType { get; set; }

			public string ReleaseType { get; set; }
		}

		private static readonly string MobileCoreProductFilter = "mobilecore";

		private static string CapabilityListFile;

		private const string DeconstructedDriverPrefix = "dual_";

		private const string LegacyDriverPrefix = "legacy_";

		private static readonly XNamespace PkgXml = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00";

		private static readonly Regex regexInfSddl = new Regex("^HKR,,Security,,\"(?<sddl>.*)\"", RegexOptions.Compiled);

		private static readonly Regex regexDefaultDacl = new Regex("(\\(.*\\))*\\(A;;GA;;;SY\\)", RegexOptions.Compiled);

		private Config _environ;

		private string _targetProduct;

		private string _workingFolder;

		private XElement _driverWmElement;

		private bool _legacyDriver;

		private Dictionary<string, string> _fileNameSubDirMap;

		private Dictionary<string, string> _fileNameDestinationMap;

		private IDeploymentLogger _logger;

		private static Dictionary<string, string> _regSddlMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{ "hkey_local_machine\\software\\microsoft\\windows nt\\currentversion\\wudf", "D:(A;CIOI;KA;;;SY)(A;CIOI;KR;;;LS)(A;CIOI;KR;;;NS)(A;CIOI;KA;;;BA)(A;CIOI;KR;;;BU)" },
			{ "hkey_local_machine\\software\\microsoft\\windows nt\\currentversion\\perflib", "D:P(A;CI;GR;;;IU)(A;CI;GA;;;BA)(A;CI;GA;;;SY)(A;CI;GA;;;CO)(A;CI;GR;;;LS)(A;CI;GR;;;NS)(A;CI;GR;;;LU)(A;CI;GR;;;MU)" },
			{ "hkey_local_machine\\software\\microsoft\\windows nt\\currentversion", "O:S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464G:S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464D:P(A;CI;GA;;;S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464)(A;CI;GA;;;SY)(A;CI;GA;;;BA)(A;CI;GR;;;BU)(A;;DC;;;S-1-5-80-123231216-2592883651-3715271367-3753151631-4175906628)(A;CI;GR;;;S-1-15-2-1)" },
			{ "hkey_local_machine\\SYSTEM\\CurrentControlSet\\Services\\BTHPORT\\Parameters", "D:AR(A;CI;KA;;;LS)(A;CI;KA;;;SY)(A;CI;KA;;;BA)(A;CIIO;KA;;;CO)(A;CI;KR;;;AC)(A;CI;KR;;;BU)" },
			{ "hkey_local_machine\\SYSTEM\\CurrentControlSet\\Services\\BTHPORT\\Parameters\\Devices", "D:AR(A;CI;KA;;;LS)(A;CI;KA;;;SY)(A;CI;KA;;;BA)(A;CIIO;KA;;;CO)(A;CI;KR;;;AC)(A;CI;KR;;;BU)" },
			{ "hkey_local_machine\\SYSTEM\\CurrentControlSet\\Services\\BTHPORT\\Parameters\\Services", "D:AR(A;CI;KA;;;LS)(A;CI;KA;;;SY)(A;CI;KA;;;BA)(A;CIIO;KA;;;CO)(A;CI;KR;;;AC)(A;CI;KR;;;BU)" },
			{ "hkey_local_machine\\SYSTEM\\CurrentControlSet\\Services\\BTHENUM\\Parameters", "D:AR(A;CI;KA;;;S-1-5-80-2586557155-168560303-1373426920-983201488-1499765686)(A;CI;KA;;;SY)(A;CI;KA;;;BA)(A;CIIO;KA;;;CO)(A;CI;KR;;;AC)(A;CI;KR;;;BU)" },
			{ "hkey_local_machine\\SYSTEM\\CurrentControlSet\\Services\\BTHENUM\\Parameters\\Policy", "D:AR(A;CI;KA;;;S-1-5-80-2586557155-168560303-1373426920-983201488-1499765686)(A;CI;KA;;;SY)(A;CI;KA;;;BA)(A;CIIO;KA;;;CO)(A;CI;KR;;;AC)(A;CI;KR;;;BU)" },
			{ "hkey_local_machine\\SYSTEM\\CurrentControlSet\\Services\\bthl2cap\\Parameters", "D:AR(A;CI;KA;;;S-1-5-80-2586557155-168560303-1373426920-983201488-1499765686)(A;CI;KA;;;SY)(A;CI;KA;;;BA)(A;CIIO;KA;;;CO)(A;CI;KR;;;AC)(A;CI;KR;;;BU)" },
			{ "hkey_local_machine\\SYSTEM\\CurrentControlSet\\Services\\bthl2cap\\Parameters\\Policy", "D:AR(A;CI;KA;;;S-1-5-80-2586557155-168560303-1373426920-983201488-1499765686)(A;CI;KA;;;SY)(A;CI;KA;;;BA)(A;CIIO;KA;;;CO)(A;CI;KR;;;AC)(A;CI;KR;;;BU)" },
			{ "hkey_local_machine\\SYSTEM\\CurrentControlSet\\Services\\BthLEEnum\\Parameters", "D:AR(A;CI;KA;;;S-1-5-80-2586557155-168560303-1373426920-983201488-1499765686)(A;CI;KA;;;SY)(A;CI;KA;;;BA)(A;CIIO;KA;;;CO)(A;CI;KR;;;AC)(A;CI;KR;;;BU)" },
			{ "hkey_local_machine\\SYSTEM\\CurrentControlSet\\Services\\BthLEEnum\\Parameters\\Policy", "D:AR(A;CI;KA;;;S-1-5-80-2586557155-168560303-1373426920-983201488-1499765686)(A;CI;KA;;;SY)(A;CI;KA;;;BA)(A;CIIO;KA;;;CO)(A;CI;KR;;;AC)(A;CI;KR;;;BU)" },
			{ "hkey_local_machine\\SYSTEM\\CurrentControlSet\\Services\\RFCOMM\\Parameters", "D:AR(A;CI;KA;;;S-1-5-80-2586557155-168560303-1373426920-983201488-1499765686)(A;CI;KA;;;SY)(A;CI;KA;;;BA)(A;CIIO;KA;;;CO)(A;CI;KR;;;BU)" },
			{ "hkey_local_machine\\SYSTEM\\CurrentControlSet\\Services\\RFCOMM\\Parameters\\Policy", "D:AR(A;CI;KA;;;S-1-5-80-2586557155-168560303-1373426920-983201488-1499765686)(A;CI;KA;;;SY)(A;CI;KA;;;BA)(A;CIIO;KA;;;CO)(A;CI;KR;;;BU)" }
		};

		private static void ApplySddlExceptions(XDocument deconstructedDriver)
		{
			foreach (XElement item in deconstructedDriver.Root.Descendants(deconstructedDriver.Root.Name.Namespace + "registryKey"))
			{
				string attributeValue = PkgBldrHelpers.GetAttributeValue(item, "keyName");
				if (!_regSddlMap.ContainsKey(attributeValue))
				{
					continue;
				}
				string value = Microsoft.CompPlat.PkgBldr.Base.Security.SecurityPolicy.HashCalculator.CalculateSha1Hash(attributeValue);
				foreach (XElement item2 in item.Elements(item.Name.Namespace + "securityDescriptor"))
				{
					item2.Remove();
				}
				XElement xElement = new XElement(item.Name.Namespace + "securityDescriptor");
				xElement.Add(new XAttribute("name", value));
				item.Add(xElement);
				XElement xElement2 = PkgBldrHelpers.AddIfNotFound(PkgBldrHelpers.AddIfNotFound(PkgBldrHelpers.AddIfNotFound(PkgBldrHelpers.AddIfNotFound(deconstructedDriver.Root, "trustInfo"), "security"), "accessControl"), "securityDescriptorDefinitions");
				XElement xElement3 = new XElement(xElement2.Name.Namespace + "securityDescriptorDefinition");
				xElement3.Add(new XAttribute("name", value));
				xElement3.Add(new XAttribute("sddl", _regSddlMap[attributeValue]));
				xElement2.Add(xElement3);
			}
		}

		public override void ConvertEntries(XElement toCsi, Dictionary<string, IPkgPlugin> plugins, Config environ, XElement driverWmElement)
		{
			CheckIfRunningElevated();
			_environ = environ;
			_logger = environ.Logger;
			_targetProduct = environ.Bld.Product;
			if (string.IsNullOrEmpty(_targetProduct))
			{
				_targetProduct = "windows";
			}
			_targetProduct = _targetProduct.ToLowerInvariant();
			_driverWmElement = driverWmElement;
			_workingFolder = LongPath.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			LongPathDirectory.CreateDirectory(_workingFolder);
			CapabilityListFile = Environment.ExpandEnvironmentVariables(environ.pkgBldrArgs.capabilityListCfg);
			string infPath = GetInfPath();
			string fileName = LongPath.GetFileName(infPath);
			string deconstructedInfName = GetDeconstructedInfName(fileName);
			XAttribute xAttribute = _driverWmElement.Attribute("legacy");
			if (xAttribute != null)
			{
				_legacyDriver = xAttribute.Value.Equals("true", StringComparison.OrdinalIgnoreCase);
			}
			try
			{
				if (!CommonSettings.Default.ErrorOnDeconstructionFailure)
				{
					_logger.LogWarning("Error detection is disabled");
				}
				BuildDriverManifests(infPath, fileName, deconstructedInfName);
				FileUtils.DeleteTree(_workingFolder);
			}
			catch (Exception ex)
			{
				string format = string.Format(CultureInfo.InvariantCulture, "Driver deconstruction failed for {0}", new object[1] { fileName });
				if (!CommonSettings.Default.ErrorOnDeconstructionFailure)
				{
					_logger.LogInfo(format);
					_logger.LogInfo(ex.ToString());
					return;
				}
				_logger.LogError(format);
				_logger.LogError(ex.ToString());
				throw;
			}
		}

		private static void CheckIfRunningElevated()
		{
			if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
			{
				throw new PkgGenException("!!!Drvstore.dll requires admin privileges. PkgGen.exe needs this dll when deconstructing drivers!!!");
			}
		}

		private void BuildBinaryFileMaps(string infPath)
		{
			string processOutput;
			Run.RunProcess(_workingFolder, "infutil.exe", string.Format(CultureInfo.InvariantCulture, "/noincludes /files /arch {0} {1}", new object[2]
			{
				_environ.Bld.ArchAsString,
				LongPath.GetFileName(infPath)
			}), true, true, out processOutput);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			string[] array = Regex.Split(processOutput, "\r\n\r\n");
			foreach (string text in array)
			{
				if (string.IsNullOrWhiteSpace(text))
				{
					continue;
				}
				string[] array2 = Regex.Split(text, "\r\n");
				string path = Regex.Split(array2[0], "          ")[1];
				if (!dictionary.ContainsKey(LongPath.GetFileName(path)))
				{
					dictionary.Add(LongPath.GetFileName(path), LongPath.GetDirectoryName(path));
				}
				string text2 = Regex.Split(array2[1], "     ")[1];
				string text3;
				if (text2.StartsWith(".\\", StringComparison.OrdinalIgnoreCase))
				{
					text3 = LongPath.Combine("$(runtime.drivers)", LongPath.GetFileName(text2));
				}
				else
				{
					text3 = Regex.Replace(text2, "%systemroot%\\\\system32\\\\drivers\\\\", "$(runtime.drivers)\\", RegexOptions.IgnoreCase);
					if (text3.Equals(text2, StringComparison.OrdinalIgnoreCase))
					{
						text3 = Regex.Replace(text2, "%systemroot%\\\\system32\\\\", "$(runtime.system32)\\", RegexOptions.IgnoreCase);
						if (text3.Equals(text2, StringComparison.OrdinalIgnoreCase))
						{
							text3 = Regex.Replace(text2, "%systemroot%\\\\", "$(runtime.windows)\\", RegexOptions.IgnoreCase);
							if (text3.Equals(text2, StringComparison.OrdinalIgnoreCase))
							{
								text3 = Regex.Replace(text2, "%systemdrive%\\\\", "$(runtime.bootDrive)\\", RegexOptions.IgnoreCase);
							}
						}
					}
				}
				string key = LongPath.GetFileName(text3).ToLowerInvariant();
				if (!dictionary2.ContainsKey(key))
				{
					dictionary2.Add(key, LongPath.GetDirectoryName(text3) + "\\");
				}
			}
			_fileNameSubDirMap = dictionary;
			_fileNameDestinationMap = dictionary2;
		}

		private string GetInfPath()
		{
			string text = "";
			XElement xElement = _driverWmElement.Element(_driverWmElement.GetDefaultNamespace() + "inf");
			if (xElement != null)
			{
				XAttribute xAttribute = xElement.Attribute("source");
				if (xAttribute != null)
				{
					text = ResolveNtTree(ResolvePath(xAttribute.Value));
					if (!LongPathFile.Exists(text))
					{
						throw new PkgGenException("Can't find source INF {0}", text);
					}
				}
			}
			if (string.IsNullOrWhiteSpace(text))
			{
				throw new PkgGenException("Missing driver inf source attribute");
			}
			return LongPath.GetFullPath(text);
		}

		private string GetDeconstructedInfName(string infName)
		{
			return "dual_" + infName;
		}

		private void BuildDriverManifests(string infPath, string infName, string deconstructedInfName)
		{
			string text = ProductVariantProcessing(infPath, infName);
			UpdateInfSecurity(text);
			BuildBinaryFileMaps(text);
			string spkgPath = RunSpkgGen(text);
			XDocument xDocument = RunConvertDSM(_workingFolder, spkgPath);
			XElement resourceManifestDependencyNode = ResourceManifestDependencyNode(infName);
			GeneratePigeonManifest(xDocument, infName, resourceManifestDependencyNode);
			GenerateDeconstructedDriver(xDocument, deconstructedInfName, resourceManifestDependencyNode);
		}

		private void GenerateDeconstructedDriver(XDocument dsmConvertedCsiManifest, string deconstructedInfName, XElement resourceManifestDependencyNode)
		{
			XDocument xDocument = new XDocument(dsmConvertedCsiManifest);
			_logger.LogInfo(string.Format(CultureInfo.InvariantCulture, "Deconstructing driver {0}", new object[1] { deconstructedInfName }));
			PostprocessManifest(xDocument, deconstructedInfName, resourceManifestDependencyNode);
			_logger.LogInfo(string.Format(CultureInfo.InvariantCulture, "Driver deconstruction complete {0}", new object[1] { deconstructedInfName }));
			ApplySddlExceptions(xDocument);
			SaveManifest(xDocument, deconstructedInfName);
		}

		private void SaveManifest(XDocument manifest, string deconstructedInfName)
		{
			string directoryName = LongPath.GetDirectoryName(_environ.Output);
			string text = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}.man", new object[2] { directoryName, deconstructedInfName });
			_logger.LogInfo(string.Format(CultureInfo.InvariantCulture, "Writing: {0}", new object[1] { text }));
			PkgBldrHelpers.XDocumentSaveToLongPath(manifest, text);
			if (_legacyDriver)
			{
				XElement root = manifest.Root;
				XElement xElement = root.Element(root.Name.Namespace + "assemblyIdentity");
				xElement.Attribute("type").Remove();
				xElement.SetAttributeValue(value: xElement.Attribute("name").Value.Replace("dual_", "legacy_"), name: "name");
				root.Element(root.Name.Namespace + "deployment")?.Remove();
				PkgBldrHelpers.XDocumentSaveToLongPath(new XDocument(root), text.Replace("dual_", "legacy_"));
			}
		}

		private string ProductVariantProcessing(string infPath, string deconstructedInfName)
		{
			_logger.LogInfo(string.Format(CultureInfo.InvariantCulture, "Process inf for target product [{0}]", new object[1] { _targetProduct }));
			string text = LongPath.Combine(_workingFolder, deconstructedInfName);
			string text2 = text + ".tmp1";
			string text3 = text + ".tmp2";
			try
			{
				if (_environ.ProcessInf)
				{
					string arguments = string.Format(CultureInfo.InvariantCulture, "/k \"{0} /DLANGUAGE_ID=0x0409 /DTARGET_PRODUCT_{1} -nologo /EP /C {2}\" > {3} & exit %ERRORLEVEL%", _environ.pkgBldrArgs.toolPaths["cl"], _targetProduct.ToUpperInvariant(), infPath, text2);
					Run.RunProcess(_workingFolder, "cmd.exe", arguments, _logger);
					arguments = string.Format(CultureInfo.InvariantCulture, "-m -1252 {0} {1}", new object[2] { text2, text3 });
					Run.RunProcess(_workingFolder, _environ.pkgBldrArgs.toolPaths["unitext"], arguments, _logger);
					arguments = string.Format(CultureInfo.InvariantCulture, "prodfilt -u {0} {1} +i", new object[2] { text3, text });
					Run.RunProcess(_workingFolder, _environ.pkgBldrArgs.toolPaths["prodfilt"], arguments, _logger);
					arguments = string.Format(CultureInfo.InvariantCulture, "-f {0}", new object[1] { text });
					Run.RunProcess(_workingFolder, _environ.pkgBldrArgs.toolPaths["stampinf"], arguments, _logger);
					return text;
				}
				LongPathFile.WriteAllLines(text, LongPathFile.ReadAllLines(infPath));
				return text;
			}
			finally
			{
				DeleteFile(text2);
				DeleteFile(text3);
			}
		}

		private void UpdateInfSecurity(string tempInfPath)
		{
			XElement xElement = _driverWmElement.Element(_driverWmElement.Name.Namespace + "security");
			if (xElement == null)
			{
				return;
			}
			string value = xElement.Attribute("infSectionName").Value;
			List<string> list = LongPathFile.ReadAllLines(tempInfPath).ToList();
			StringBuilder stringBuilder = new StringBuilder("HKR,,Security,,\"");
			stringBuilder.Append("D:P");
			bool flag = false;
			string sectionSddl = GetSectionSddl(list, value);
			if (sectionSddl != null)
			{
				string text = sectionSddl.Remove(3);
				sectionSddl = sectionSddl.Remove(0, 3);
				if (text != "D:P")
				{
					throw new PkgGenException("Invalid INF security header : {0}", text);
				}
				stringBuilder.Append(sectionSddl);
				if (regexDefaultDacl.Match(sectionSddl).Success)
				{
					flag = true;
				}
			}
			foreach (XElement item in xElement.Elements())
			{
				if (item.Name.LocalName.Equals("accessedByCapability", StringComparison.OrdinalIgnoreCase))
				{
					string value2 = item.Attribute("id").Value;
					string text2 = _environ.Macros.Resolve(item.Attribute("rights").Value);
					DriverAccess driverAccess = new DriverAccess(DriverAccessType.Capability, value2, text2);
					string text3 = null;
					text3 = ((!ShouldUsePhoneSecurity()) ? driverAccess.GetSecurityDescriptor() : Microsoft.WindowsPhone.Security.SecurityPolicyCompiler.GlobalVariables.GetPhoneSDDL(CapabilityListFile, value2, text2));
					string text4 = text3.Remove(3);
					text3 = text3.Remove(0, 3);
					if (text4 != "D:P")
					{
						throw new PkgGenException("Invalid INF security header : {0}", text4);
					}
					if (regexDefaultDacl.Match(text3).Success)
					{
						if (flag)
						{
							text3 = text3.Remove(0, 12);
						}
						else
						{
							flag = true;
						}
						stringBuilder.Append(text3);
					}
					continue;
				}
				throw new PkgGenException("Unsupported <accessedBy*> element {0}", item.Name.LocalName);
			}
			stringBuilder.Append("\"");
			SetSectionSddl(stringBuilder.ToString(), list, value);
			LongPathFile.WriteAllLines(tempInfPath, list);
		}

		private bool ShouldUsePhoneSecurity()
		{
			return _targetProduct.Equals(MobileCoreProductFilter, StringComparison.OrdinalIgnoreCase);
		}

		private string GetSectionSddl(List<string> infLines, string infSectionName)
		{
			int sectionStart = GetSectionStart(infLines, infSectionName);
			if (sectionStart == -1)
			{
				throw new PkgGenException("Section name {0} missing from inf", infSectionName);
			}
			int sectionEnd = GetSectionEnd(infLines, sectionStart);
			string text = null;
			for (int i = sectionStart; i < sectionEnd; i++)
			{
				string input = infLines[i];
				Match match = regexInfSddl.Match(input);
				if (match.Success)
				{
					if (text != null)
					{
						throw new PkgGenException("More than one security SDDL string specified near line {0}", i);
					}
					text = match.Groups["sddl"].Value;
				}
			}
			return text;
		}

		private int GetSectionStart(List<string> infLines, string infSectionName)
		{
			int result = -1;
			string pattern = string.Format(CultureInfo.InvariantCulture, "^\\[[ \\t]*{0}[ \\t]*\\](.*)$", new object[1] { infSectionName });
			for (int i = 0; i < infLines.Count; i++)
			{
				string text = infLines[i].Trim();
				if (!text.StartsWith(";", StringComparison.OrdinalIgnoreCase) && Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
				{
					result = i;
					break;
				}
			}
			return result;
		}

		private int GetSectionEnd(List<string> infLines, int sectionStart)
		{
			int num = -1;
			for (int i = sectionStart + 1; i < infLines.Count; i++)
			{
				string text = infLines[i].Trim();
				if (!text.StartsWith(";", StringComparison.OrdinalIgnoreCase) && new Regex("^\\[.*\\](.*)$").IsMatch(text))
				{
					num = i;
					break;
				}
			}
			if (-1 == num)
			{
				num = infLines.Count;
			}
			return num;
		}

		private void SetSectionSddl(string newSddl, List<string> infLines, string infSectionName)
		{
			int sectionStart = GetSectionStart(infLines, infSectionName);
			if (sectionStart == -1)
			{
				throw new PkgGenException("Section name {0} missing from inf", infSectionName);
			}
			int sectionEnd = GetSectionEnd(infLines, sectionStart);
			List<string> list = new List<string>(from x in infLines.Skip(sectionStart).Take(sectionEnd - sectionStart)
				where !regexInfSddl.Match(x).Success
				select x);
			if (!string.IsNullOrEmpty(newSddl))
			{
				if (list[list.Count - 1].Equals(string.Empty))
				{
					list.RemoveAt(list.Count - 1);
				}
				list.Add(newSddl);
				list.Add(string.Empty);
			}
			infLines.RemoveRange(sectionStart, sectionEnd - sectionStart);
			infLines.InsertRange(sectionStart, list);
		}

		private string RunSpkgGen(string infPath)
		{
			WindowsManifestIdentity windowsManifestIdentity = GetWindowsManifestIdentity();
			string text = LongPath.Combine(_workingFolder, string.Format(CultureInfo.InvariantCulture, "{0}.pkg.xml", new object[1] { windowsManifestIdentity.Name }));
			CreatePkgXml(infPath, text, "s", "w", windowsManifestIdentity.OwnerType, windowsManifestIdentity.ReleaseType);
			List<string> list = ConstructSPkgGenArguments();
			list.Add("/output:" + _workingFolder);
			list.Add("/nohives");
			list.Add(text);
			list.Add(string.Format(CultureInfo.InvariantCulture, "/toolPaths:{0}", new object[1] { _environ.pkgBldrArgs.spkgGenToolDirs }));
			if (_environ.pkgBldrArgs.isRazzleEnv)
			{
				list.Add("/isRazzleEnv");
			}
			if (_environ.pkgBldrArgs.diagnostic)
			{
				list.Add("/diagnostic");
			}
			bool inWindows = true;
			string result = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}.{2}.spkg", new object[3] { _workingFolder, "w", "s" });
			Run.RunSPkgGen(list, inWindows, _logger, _environ.pkgBldrArgs);
			return result;
		}

		private XDocument RunConvertDSM(string workingDirectory, string spkgPath)
		{
			try
			{
				ConvertDSM.RunDsmConverter(spkgPath, workingDirectory, false, false, 32u);
				return PkgBldrHelpers.XDocumentLoadFromLongPath(LongPathDirectory.GetFiles(workingDirectory, "*.manifest", SearchOption.AllDirectories)[0]);
			}
			catch (PkgGenException)
			{
				throw;
			}
			catch (Exception ex2)
			{
				throw new PkgGenException("Failed to run ConvertDSM", ex2);
			}
		}

		private void PostprocessManifest(XDocument csiManifest, string assemblyIdentity, XElement resourceManifestDependencyNode)
		{
			_logger.LogInfo("Postprocessing manifest");
			XElement root = csiManifest.Root;
			XElement xElement = root.Element(root.Name.Namespace + "assemblyIdentity");
			xElement.SetAttributeValue("name", assemblyIdentity);
			xElement.SetAttributeValue("buildType", "$(build.buildType)");
			xElement.SetAttributeValue("processorArchitecture", "$(build.arch)");
			xElement.SetAttributeValue("publicKeyToken", "$(build.WindowsPublicKeyToken)");
			xElement.SetAttributeValue("version", "$(build.version)");
			xElement.Add(new XAttribute("type", "dualModeDriver"));
			if (!_targetProduct.Equals("windows"))
			{
				xElement.Add(new XAttribute("product", "$(build.product)"));
			}
			if (resourceManifestDependencyNode != null)
			{
				xElement.AddAfterSelf(resourceManifestDependencyNode);
			}
			IEnumerable<XElement> enumerable = _driverWmElement.Descendants(_driverWmElement.Name.Namespace + "file");
			foreach (XElement item in root.Descendants(root.Name.Namespace + "file"))
			{
				string fileName = LongPath.GetFileName(item.Attribute("name").Value);
				item.SetAttributeValue("name", fileName);
				item.Elements().Remove();
				foreach (XElement item2 in enumerable)
				{
					string text = LongPath.GetFileName(item2.Attribute("source").Value);
					XAttribute xAttribute = item2.Attribute("name");
					if (xAttribute != null)
					{
						text = xAttribute.Value;
					}
					if (!fileName.Equals(text))
					{
						continue;
					}
					item.Attribute("attributes")?.Remove();
					XElement xElement2 = item2.Element(item2.Name.Namespace + "signatureInfo");
					if (xElement2 != null)
					{
						XElement xElement3 = new XElement(item.Name.Namespace + "signatureInfo");
						foreach (XElement item3 in xElement2.Elements(xElement2.Name.Namespace + "signatureDescriptor"))
						{
							xElement3.Add(new XElement(xElement3.Name.Namespace + "signatureDescriptor", item3.Attributes()));
						}
						item.Add(xElement3);
					}
					XAttribute xAttribute2 = item2.Attribute("buildFilter");
					if (xAttribute2 != null)
					{
						item.Add(xAttribute2);
					}
					string value = item2.Attribute("source").Value;
					string path = CleanNtTreePath(ResolvePath(value));
					item.Add(new XAttribute("importPath", LongPath.GetDirectoryName(path) + "\\"));
					_logger.LogInfo("Processing: [{0}]", text);
					string extendedFileName = GetExtendedFileName(text);
					if (!extendedFileName.Equals(text, StringComparison.OrdinalIgnoreCase))
					{
						item.SetAttributeValue("name", extendedFileName);
						item.Add(new XAttribute("sourceName", text));
					}
					if (_fileNameDestinationMap.ContainsKey(text.ToLowerInvariant()))
					{
						item.SetAttributeValue("destinationPath", _fileNameDestinationMap[text.ToLowerInvariant()]);
					}
				}
			}
			IEnumerable<XElement> enumerable2 = root.Descendants(root.GetDefaultNamespace() + "registryKey");
			List<XElement> list = new List<XElement>();
			foreach (XElement item4 in enumerable2)
			{
				XAttribute xAttribute3 = item4.Attribute("keyName");
				if (xAttribute3 == null)
				{
					throw new PkgGenException("Invalid <registryKey> element, missing required keyName attribute");
				}
				switch (xAttribute3.Value.ToLowerInvariant())
				{
				case "hkey_local_machine\\system\\currentcontrolset":
				case "hkey_local_machine\\system\\currentcontrolset\\control":
				case "hkey_local_machine\\system\\currentcontrolset\\enum":
				case "hkey_local_machine\\system\\currentcontrolset\\services":
					if (item4.Descendants().All((XElement x) => x.Name.LocalName.Equals("securityDescriptor", StringComparison.OrdinalIgnoreCase)))
					{
						list.Add(item4);
					}
					break;
				case "hkey_local_machine\\software\\microsoft\\windows\\currentversion\\setup":
				case "hkey_local_machine\\software\\microsoft\\windows\\currentversion\\setup\\pnplockdownfiles":
				case "hkey_local_machine\\software\\microsoft\\windows\\currentversion\\setup\\pnpresources":
				case "hkey_local_machine\\system\\currentcontrolset\\control\\class":
				case "hkey_local_machine\\system\\driverdatabase":
				case "hkey_local_machine\\system\\driverdatabase\\deviceids":
				case "hkey_local_machine\\system\\driverdatabase\\driverinffiles":
				case "hkey_local_machine\\system\\driverdatabase\\driverpackages":
					list.Add(item4);
					break;
				}
			}
			list.Remove();
			foreach (XElement item5 in root.Descendants(root.Name.Namespace + "registryValue"))
			{
				item5.Attribute("mutable")?.Remove();
				XAttribute xAttribute4 = item5.Attribute("name");
				if (xAttribute4 != null && string.IsNullOrEmpty(xAttribute4.Value))
				{
					xAttribute4.Remove();
				}
			}
			string value2 = _driverWmElement.Element(_driverWmElement.Name.Namespace + "inf").Attribute("source").Value;
			string path2 = CleanNtTreePath(value2);
			XElement xElement4 = new XElement(root.Name.Namespace + "file", new XAttribute("importPath", LongPath.GetDirectoryName(path2) + "\\"), new XAttribute("name", LongPath.GetFileName(value2)), new XAttribute("sourceName", LongPath.GetFileName(value2)), new XElement(root.Name.Namespace + "infFile"));
			XElement xElement5 = new XElement(root.Name.Namespace + "deconstructionTool", new XAttribute("version", "10.0.0.0"));
			root.Add(xElement4, xElement5);
			root.Add(new XElement(root.Name.Namespace + "deployment"));
		}

		private XElement ResourceManifestDependencyNode(string infName)
		{
			XAttribute xAttribute = _driverWmElement.Attribute("hasResources");
			if (xAttribute != null && xAttribute.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				return XElement.Parse(string.Format(CultureInfo.InvariantCulture, "\r\n                    <dependency\r\n                       discoverable=\"false\"\r\n                       optional=\"false\"\r\n                       resourceType=\"Resources\"\r\n                       >\r\n                       <dependentAssembly dependencyType=\"prerequisite\">\r\n                          <assemblyIdentity\r\n                              name=\"{0}.Resources\"\r\n                              language=\"*\"\r\n                              processorArchitecture=\"{1}\"\r\n                              publicKeyToken=\"$(Build.WindowsPublicKeyToken)\"\r\n                              version=\"$(build.version)\"\r\n                              versionScope=\"nonSxS\"\r\n                           />\r\n                       </dependentAssembly>\r\n                   </dependency>", new object[2]
				{
					infName,
					_environ.Bld.ArchAsString
				}));
			}
			return null;
		}

		private void GeneratePigeonManifest(XDocument dsmConvertedManifest, string infName, XElement resourceManifestDependencyNode)
		{
			XElement root = dsmConvertedManifest.Root;
			XElement xElement = new XElement(root.Name.Namespace + "assembly", new XAttribute("manifestVersion", root.Attribute("manifestVersion").Value));
			XElement content = XElement.Parse(string.Format(CultureInfo.InvariantCulture, "\r\n               <assemblyIdentity\r\n                   buildType=\"$(build.buildType)\"\r\n                   language=\"neutral\"\r\n                   name=\"{0}\"\r\n                   processorArchitecture=\"{1}\"\r\n                   publicKeyToken=\"$(Build.WindowsPublicKeyToken)\"\r\n                   type=\"driverUpdate\"\r\n                   version=\"$(build.version)\"\r\n                   versionScope=\"nonSxS\"/>", new object[2]
			{
				infName,
				_environ.Bld.ArchAsString
			}));
			xElement.Add(content);
			if (resourceManifestDependencyNode != null)
			{
				xElement.Add(resourceManifestDependencyNode);
			}
			XNamespace xNamespace = "urn:schemas-microsoft-com:asm.v3";
			foreach (XElement item in xElement.DescendantsAndSelf())
			{
				item.Name = xNamespace + item.Name.LocalName;
			}
			if (!_targetProduct.Equals("windows"))
			{
				foreach (XElement item2 in xElement.Descendants(xElement.Name.Namespace + "assemblyIdentity"))
				{
					item2.Add(new XAttribute("product", "$(build.product)"));
				}
			}
			string value = _driverWmElement.Element(_driverWmElement.Name.Namespace + "inf").Attribute("source").Value;
			string rawPath = ResolvePath(value);
			rawPath = CleanNtTreePath(rawPath);
			XElement content2 = new XElement(xElement.Name.Namespace + "file", new XAttribute("name", infName), new XAttribute("sourceName", LongPath.GetFileName(rawPath)), new XAttribute("importPath", LongPath.GetDirectoryName(rawPath) + "\\"), new XElement(xElement.Name.Namespace + "infFile"));
			xElement.Add(content2);
			XElement xElement2 = _driverWmElement.Element(_driverWmElement.Name.Namespace + "files");
			if (xElement2 != null)
			{
				foreach (XElement item3 in xElement2.Elements())
				{
					string value2 = item3.Attribute("source").Value;
					string directoryName = LongPath.GetDirectoryName(value2);
					string text = CleanNtTreePath(_environ.Macros.Resolve(directoryName));
					string fileName = LongPath.GetFileName(value2);
					_logger.LogInfo("Processing: [{0}]", fileName);
					string extendedFileName = GetExtendedFileName(fileName);
					XElement xElement3 = new XElement(xElement.Name.Namespace + "file", new XAttribute("name", extendedFileName), new XAttribute("sourceName", fileName), new XAttribute("importPath", text.TrimEnd('\\') + "\\"));
					IEnumerable<XElement> source = item3.Descendants(item3.Name.Namespace + "signatureDescriptor");
					if (source.Any())
					{
						XElement pigeonSigInfo = new XElement(xElement3.Name.Namespace + "signatureInfo");
						source.ToList().ForEach(delegate(XElement desc)
						{
							pigeonSigInfo.Add(new XElement(pigeonSigInfo.Name.Namespace + "signatureDescriptor", desc.Attributes()));
						});
						xElement3.Add(pigeonSigInfo);
					}
					xElement.Add(xElement3);
				}
			}
			else
			{
				_logger.LogInfo("Manifest for INF [{0}] does not carry any files.", infName);
			}
			xElement.Add(new XElement(xElement.Name.Namespace + "deployment"));
			PkgBldrHelpers.XDocumentSaveToLongPath(new XDocument(xElement), string.Format(CultureInfo.InvariantCulture, "{0}\\{1}.pigeon.man", new object[2]
			{
				LongPath.GetDirectoryName(_environ.Output),
				infName
			}));
		}

		private string GetExtendedFileName(string fileName)
		{
			string result = fileName;
			if (_fileNameSubDirMap.ContainsKey(fileName) && !string.IsNullOrWhiteSpace(_fileNameSubDirMap[fileName]))
			{
				_logger.LogInfo("Key found!");
				result = _fileNameSubDirMap[fileName] + "\\" + fileName;
			}
			return result;
		}

		private string CleanNtTreePath(string rawPath)
		{
			return rawPath.ToLowerInvariant().Replace(Environment.ExpandEnvironmentVariables(_environ.pkgBldrArgs.nttree).ToLowerInvariant(), "$(build.nttree)");
		}

		private string ResolvePath(string rawPath)
		{
			string text = _environ.Macros.Resolve(rawPath);
			if (!text.Contains('$'))
			{
				text = ((!Path.IsPathRooted(text)) ? LongPath.GetFullPath(LongPath.Combine(LongPath.GetDirectoryName(_environ.Input), text)) : LongPath.GetFullPath(text));
			}
			return text;
		}

		private string ResolveNtTree(string rawPath)
		{
			return rawPath.ToLowerInvariant().Replace("$(build.nttree)", Environment.ExpandEnvironmentVariables(_environ.pkgBldrArgs.nttree).ToLowerInvariant());
		}

		private WindowsManifestIdentity GetWindowsManifestIdentity()
		{
			try
			{
				return new WindowsManifestIdentity
				{
					Name = _environ.Bld.WM.Root.Attribute("name").Value,
					NameSpace = ((_environ.Bld.WM.Root.Attribute("namespace") != null) ? _environ.Bld.WM.Root.Attribute("namespace").Value : string.Empty),
					Owner = ((_environ.Bld.WM.Root.Attribute("namespace") != null) ? _environ.Bld.WM.Root.Attribute("owner").Value : "Microsoft"),
					OwnerType = "Microsoft",
					ReleaseType = "Production"
				};
			}
			catch (Exception)
			{
				throw new PkgGenException("Unable to parse the identity element from the Windows Manifest");
			}
		}

		private List<string> ConstructSPkgGenArguments()
		{
			return new List<string>
			{
				"/version:0.0.0.0",
				"/cpu:" + _environ.Bld.ArchAsString
			};
		}

		private void CreatePkgXml(string infPath, string pkgXmlPath, string component, string owner, string ownerType, string releaseType)
		{
			XElement xElement = new XElement(PkgXml + "Package");
			xElement.Add(new XAttribute("Owner", owner));
			xElement.Add(new XAttribute("Component", component));
			xElement.Add(new XAttribute("OwnerType", ownerType));
			xElement.Add(new XAttribute("ReleaseType", releaseType));
			XElement xElement2 = new XElement(PkgXml + "Components");
			XElement xElement3 = new XElement(PkgXml + "Driver");
			xElement3.Add(new XAttribute("InfSource", infPath));
			IEnumerable<XElement> enumerable = _driverWmElement.Descendants(_driverWmElement.Name.Namespace + "file");
			if (enumerable.Count() != 0)
			{
				XElement xElement4 = new XElement(PkgXml + "Files");
				foreach (XElement item in enumerable)
				{
					string text = LongPath.GetFileName(item.Attribute("source").Value);
					XAttribute xAttribute = item.Attribute("name");
					if (xAttribute != null)
					{
						text = xAttribute.Value;
					}
					string text2 = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", new object[2] { _workingFolder, text });
					using (LongPathFile.Open(text2, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
					{
					}
					XElement xElement5 = new XElement(PkgXml + "Reference", new XAttribute("Source", text2));
					if (_fileNameSubDirMap.ContainsKey(text) && !string.IsNullOrWhiteSpace(_fileNameSubDirMap[text]))
					{
						xElement5.Add(new XAttribute("StagingSubDir", _fileNameSubDirMap[text]));
					}
					xElement3.Add(xElement5);
					xElement4.Add(new XElement(PkgXml + "File", new XAttribute("Source", text2)));
				}
				xElement3.Add(xElement4);
			}
			xElement2.Add(xElement3);
			xElement.Add(xElement2);
			PkgBldrHelpers.XDocumentSaveToLongPath(new XDocument(xElement), pkgXmlPath);
		}

		private static void DeleteFile(string path)
		{
			if (LongPathFile.Exists(path))
			{
				LongPathFile.Delete(path);
			}
		}
	}
}
