using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;
using Microsoft.CompPlat.PkgBldr.Plugins.WmToCsi;
using Microsoft.CompPlat.PkgBldr.Tools;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class Package : PkgPlugin
	{
		private void FullyExpandPolicyMacros(XElement customizationPolicy, Config environ)
		{
			MacroResolver macroResolver = new MacroResolver();
			macroResolver.Load(XmlReader.Create(PkgGenResources.GetResourceStream("Macros_Policy.xml")));
			foreach (XElement item in customizationPolicy.Descendants())
			{
				string attributeValue = PkgBldrHelpers.GetAttributeValue(item, "Path");
				if (attributeValue != null)
				{
					string text = null;
					try
					{
						text = macroResolver.Resolve(attributeValue);
					}
					catch
					{
						environ.Logger.LogWarning("Could not fully expand policy Macro in path {0}, please add this macro to Macros_Policy.xml", attributeValue);
					}
					if (text != null)
					{
						item.Attribute("Path").Value = text;
					}
				}
			}
		}

		public override void ConvertEntries(XElement toWm, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement fromPkg)
		{
			enviorn.ExitStatus = ExitStatus.SUCCESS;
			enviorn.Macros = new MacroResolver();
			enviorn.Macros.Load(XmlReader.Create(PkgGenResources.GetResourceStream("Macros_PkgToWm.xml")));
			if (enviorn.Bld.BuildMacros != null)
			{
				Dictionary<string, Microsoft.CompPlat.PkgBldr.Base.Macro> macroTable = enviorn.Bld.BuildMacros.GetMacroTable();
				enviorn.Macros.Register(macroTable, true);
			}
			enviorn.Pass = BuildPass.PLUGIN_PASS;
			enviorn.Bld.WM.Root = toWm;
			enviorn.Bld.PKG.Root = fromPkg;
			FileConverter arg = new FileConverter(enviorn);
			enviorn.arg = arg;
			XNamespace xNamespace = "urn:Microsoft.CompPlat/ManifestSchema.v1.00";
			toWm.Name = xNamespace + "identity";
			toWm.Add(new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"));
			toWm.Add(new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"));
			string attributeValue = PkgBldrHelpers.GetAttributeValue(fromPkg, "Owner");
			string attributeValue2 = PkgBldrHelpers.GetAttributeValue(fromPkg, "Component");
			string attributeValue3 = PkgBldrHelpers.GetAttributeValue(fromPkg, "SubComponent");
			enviorn.Bld.PKG.Component = attributeValue2;
			enviorn.Bld.PKG.SubComponent = attributeValue3;
			string input = attributeValue;
			string text = null;
			string text2 = null;
			if (string.IsNullOrEmpty(attributeValue3))
			{
				text2 = attributeValue2;
			}
			else
			{
				text = attributeValue2;
				text2 = attributeValue3;
			}
			string pattern = "[^A-Za-z0-9\\-]";
			input = Regex.Replace(input, pattern, "-");
			text2 = Regex.Replace(text2, pattern, "-");
			toWm.Add(new XAttribute("owner", input));
			toWm.Add(new XAttribute("name", text2));
			if (!string.IsNullOrEmpty(text))
			{
				text = Regex.Replace(text, pattern, "-");
				toWm.Add(new XAttribute("namespace", text));
			}
			string text3 = input + "-";
			if (text != null)
			{
				text3 = text3 + text + "-";
			}
			text3 += text2;
			if (enviorn.AutoGenerateOutput)
			{
				enviorn.Output = enviorn.Output.TrimEnd('\\') + "\\" + text3;
				enviorn.Output += ".wm.xml";
			}
			enviorn.Bld.PKG.Partition = "MainOS";
			foreach (XAttribute item in fromPkg.Attributes())
			{
				switch (item.Name.LocalName)
				{
				case "BinaryPartition":
					enviorn.Logger.LogWarning("<Package> BinaryPartition not converted");
					break;
				case "OwnerType":
					enviorn.Bld.PKG.OwnerType = item.Value;
					break;
				case "ReleaseType":
					enviorn.Bld.PKG.ReleaseType = item.Value;
					break;
				case "Partition":
					enviorn.Bld.PKG.Partition = item.Value;
					break;
				case "Platform":
					enviorn.Logger.LogWarning("<Package> Platform not converted");
					break;
				case "GroupingKey":
					enviorn.Logger.LogWarning("<Package> GroupingKey not converted");
					break;
				case "BuildWow":
					toWm.Add(new XAttribute("buildWow", "true"));
					break;
				}
			}
			BuildPass[] array = (BuildPass[])Enum.GetValues(typeof(BuildPass));
			foreach (BuildPass pass in array)
			{
				enviorn.Pass = pass;
				base.ConvertEntries(toWm, plugins, enviorn, fromPkg);
			}
			List<XElement> list = toWm.Descendants(fromPkg.Name.Namespace + "SettingsGroup").ToList();
			if (list.Count != 0)
			{
				string text4 = enviorn.Output.Replace(".wm.xml", ".policy.xml");
				XElement xElement = new XElement(fromPkg.Name.Namespace + "CustomizationPolicy");
				foreach (XElement item2 in list)
				{
					xElement.Add(item2);
					item2.Remove();
				}
				FullyExpandPolicyMacros(xElement, enviorn);
				XDocument xDocument = new XDocument(xElement);
				enviorn.Logger.LogInfo("Saving: {0}", text4);
				xDocument.Save(text4);
				XElement xElement2 = new XElement(toWm.Name.Namespace + "files");
				XElement xElement3 = new XElement(toWm.Name.Namespace + "file");
				xElement3.Add(new XAttribute("source", $"$(build.nttree)\\bin\\retail\\{Path.GetFileName(text4)}"));
				xElement3.Add(new XAttribute("destinationDir", "$(runtime.windows)\\CustomizationPolicy"));
				xElement2.Add(xElement3);
				toWm.Add(xElement2);
			}
			if (enviorn.Bld.JsonDepot != null)
			{
				string text5 = Environment.ExpandEnvironmentVariables("%sdxroot%\\" + enviorn.Bld.JsonDepot);
				if (!Directory.Exists(text5))
				{
					throw new PkgGenException($"Can't find directory {text5}");
				}
				string attributeValue4 = PkgBldrHelpers.GetAttributeValue(toWm, "owner");
				string attributeValue5 = PkgBldrHelpers.GetAttributeValue(toWm, "namespace");
				string attributeValue6 = PkgBldrHelpers.GetAttributeValue(toWm, "name");
				string attributeValue7 = PkgBldrHelpers.GetAttributeValue(toWm, "buildWow");
				string text6 = null;
				string text7 = null;
				switch (enviorn.Bld.PKG.Partition.ToLowerInvariant())
				{
				case "efiesp":
					text6 = "efiesp";
					break;
				case "updateos":
					text6 = "updateos";
					break;
				case "mainos":
					text6 = "mainos";
					break;
				case "data":
					text6 = "data";
					break;
				case "plat":
					text6 = "plat";
					break;
				default:
					enviorn.Logger.LogWarning("Partition {0} not supported, setting it to mainos");
					text6 = "mainos";
					break;
				}
				string text8 = enviorn.Bld.PKG.ReleaseType.ToLowerInvariant();
				if (!(text8 == "production"))
				{
					if (text8 == "test")
					{
						text7 = "test";
					}
					else
					{
						enviorn.Logger.LogWarning("ReleaseType {0} not supported, setting it to production");
						text7 = "production";
					}
				}
				else
				{
					text7 = "production";
				}
				string text9 = Identity.WmIdentityNameToCsiAssemblyName(attributeValue4, attributeValue5, attributeValue6, null);
				string arg2 = text9;
				string jsonDepot = enviorn.Bld.JsonDepot;
				string newLine = Environment.NewLine;
				StringBuilder stringBuilder = new StringBuilder("{" + newLine);
				stringBuilder.Append(string.Format("\"package\": \"{0}\"," + newLine, arg2));
				stringBuilder.Append("\"onecoreCapable\": \"true\"," + newLine);
				stringBuilder.Append("\"codesets\": \"cs_phone\"," + newLine);
				stringBuilder.Append("\"onecorePackageInfo\": {" + newLine);
				stringBuilder.Append(string.Format("\"targetPartition\": \"{0}\"," + newLine, text6));
				stringBuilder.Append(string.Format("\"releaseType\": \"{0}\"" + newLine, text7));
				stringBuilder.Append("}," + newLine);
				stringBuilder.Append("\"components\": [" + newLine);
				stringBuilder.Append("{" + newLine);
				stringBuilder.Append(string.Format("\"component\": \"{0}\"," + newLine, text9));
				stringBuilder.Append(string.Format("\"depot\": \"{0}\"," + newLine, jsonDepot));
				if (!string.IsNullOrEmpty(attributeValue7))
				{
					stringBuilder.Append("\"wow\": \"wow64\"," + newLine);
				}
				stringBuilder.Append("\"codesets\": \"cs_phone\"" + newLine);
				stringBuilder.Append("}" + newLine);
				stringBuilder.Append("]" + newLine);
				stringBuilder.Append("}" + newLine);
				string projFileDir = CreatePbxProjFileForComponent(enviorn.Output, text9, jsonDepot, enviorn, true);
				CreateJsonInDepotPkggenDirectory(stringBuilder.ToString(), text9, jsonDepot, projFileDir, enviorn, true);
			}
			else
			{
				enviorn.Logger.LogWarning("Partition and other onecoreCapable attributes are lost unless /json:depotName is specifed on the command line.");
			}
		}

		private void CreateJsonInDepotPkggenDirectory(string jsonStr, string csiComponentName, string depot, string projFileDir, Config environ, bool mirrorDepot)
		{
			string text = Environment.ExpandEnvironmentVariables($"%sdxroot%\\{depot}\\PkgGen");
			string path = csiComponentName + ".json";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			text = Path.Combine(text, path);
			environ.Logger.LogInfo("Saving: {0}", text);
			if (System.IO.File.Exists(text))
			{
				SdCommand.Run("edit", text);
			}
			System.IO.File.WriteAllText(text, jsonStr);
			SdCommand.Run("add", text);
			ReformatJsonMan(text, environ);
			string directoryName = Path.GetDirectoryName(text);
			directoryName = directoryName + "\\build\\mbs\\" + csiComponentName;
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			string text2 = Path.GetDirectoryName(directoryName) + "\\dirs";
			if (System.IO.File.Exists(text2))
			{
				SdCommand.Run("edit", text2);
			}
			UpdateDirsWithNewIdentity(text2, Path.GetFileName(directoryName));
			SdCommand.Run("add", text2);
			string arg = ReplaceFirstOccurrence(text, Environment.ExpandEnvironmentVariables("%sdxroot%"), "$(Sdxroot)");
			XElement xElement = XElement.Parse($"\r\n            <Project\r\n                DefaultTargets=\"ProductBuild\"\r\n                ToolsVersion=\"4.0\"\r\n                >\r\n              <Import Project=\"$(CustomizationsRoot)\\Mbs\\common\\Microsoft.Build.ModularBuild.ManifestProjectConfiguration.props\" />\r\n              <ItemGroup>\r\n                 <ManifestDefinitionFiles Include=\"{arg}\" />\r\n              </ItemGroup>\r\n              <Import Project=\"$(CustomizationsRoot)\\Mbs\\common\\Microsoft.Build.ModularBuild.ManifestProjectConfiguration.targets\"/>\r\n            </Project>");
			SetProjNameSpace(xElement);
			XDocument xDocument = new XDocument(xElement);
			string text3 = Path.Combine(directoryName, "product.pbxproj");
			environ.Logger.LogInfo($"Saving: {text3}");
			if (System.IO.File.Exists(text3))
			{
				SdCommand.Run("edit", text3);
			}
			xDocument.Save(text3);
			SdCommand.Run("add", text3);
			string text4 = Path.Combine(Path.GetDirectoryName(text3), "sources.dep");
			if (System.IO.File.Exists(text4))
			{
				SdCommand.Run("edit", text4);
				System.IO.File.Delete(text4);
			}
			projFileDir = Path.GetFullPath(projFileDir);
			projFileDir = ReplaceFirstOccurrence(projFileDir, Environment.ExpandEnvironmentVariables("%sdxroot%\\"), "");
			if (mirrorDepot)
			{
				projFileDir = PathReplace(projFileDir, 0, depot);
			}
			using (StreamWriter streamWriter = new StreamWriter(text4))
			{
				streamWriter.WriteLine("PUBLIC_PASS0_CONSUMES= \\");
				streamWriter.WriteLine("    redist\\mspartners\\netfx45\\core\\binary_release|PASS0");
				streamWriter.WriteLine();
				streamWriter.WriteLine("BUILD_PASS3_CONSUMES= \\");
				streamWriter.WriteLine($"    {projFileDir.ToLowerInvariant()}|PASS3 \\");
			}
			SdCommand.Run("add", text4);
		}

		private void UpdateDirsWithNewIdentity(string dirsPath, string identity)
		{
			HashSet<string> hashSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
			if (System.IO.File.Exists(dirsPath))
			{
				foreach (string item in System.IO.File.ReadLines(dirsPath).ToList())
				{
					string text = item.Trim(" \t \\".ToCharArray());
					if (!string.IsNullOrEmpty(text) && !text.Equals("DIRS=", StringComparison.InvariantCultureIgnoreCase) && !hashSet.Contains(text))
					{
						hashSet.Add(text);
					}
				}
				System.IO.File.Delete(dirsPath);
			}
			if (!hashSet.Contains(identity))
			{
				hashSet.Add(identity);
			}
			using (StreamWriter streamWriter = new StreamWriter(dirsPath))
			{
				streamWriter.WriteLine("DIRS= \\");
				foreach (string item2 in hashSet)
				{
					streamWriter.WriteLine($"   {item2} \\");
				}
			}
		}

		private string CreatePbxProjFileForComponent(string wmXmlPath, string componentIdentityName, string depot, Config environ, bool mirrorDepot)
		{
			string directoryName = Path.GetDirectoryName(wmXmlPath);
			bool flag = false;
			if (Directory.GetFiles(directoryName, "*.pkg.xml").Count() != 1)
			{
				flag = true;
			}
			string text = null;
			if (flag)
			{
				string text2 = Path.GetFullPath(Path.Combine(directoryName, "..\\components\\")).TrimEnd('\\');
				string text3 = Path.Combine(Path.GetDirectoryName(text2), "dirs");
				if (System.IO.File.Exists(text3))
				{
					SdCommand.Run("edit", text3);
				}
				UpdateDirsWithNewIdentity(text3, "components");
				SdCommand.Run("add", text3);
				text = Path.Combine(text2, componentIdentityName);
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
				text3 = Path.Combine(Path.GetDirectoryName(text), "dirs");
				if (System.IO.File.Exists(text3))
				{
					SdCommand.Run("edit", text3);
				}
				UpdateDirsWithNewIdentity(text3, componentIdentityName);
				SdCommand.Run("add", text3);
				text = Path.Combine(text, "product.pbxproj");
			}
			else
			{
				text = Path.Combine(directoryName, "product.pbxproj");
			}
			if (System.IO.File.Exists(text))
			{
				SdCommand.Run("edit", text);
				System.IO.File.Delete(text);
			}
			XElement xElement = XElement.Parse($"\r\n            <Project\r\n                DefaultTargets=\"ProductBuild\"\r\n                ToolsVersion=\"4.0\"\r\n                >\r\n                <Import Project=\"$(CustomizationsRoot)\\Mbs\\common\\Microsoft.Build.ModularBuild.ManifestProjectConfiguration.props\" />\r\n                <ItemGroup />\r\n                <Import Project=\"$(CustomizationsRoot)\\Mbs\\common\\Microsoft.Build.ModularBuild.ManifestProjectConfiguration.targets\"/>\r\n            </Project>");
			SetProjNameSpace(xElement);
			string text4 = ReplaceFirstOccurrence(wmXmlPath, Environment.ExpandEnvironmentVariables("%sdxroot%"), "$(Sdxroot)");
			if (mirrorDepot)
			{
				text4 = PathReplace(text4, 1, depot);
			}
			string text5 = $"\r\n            <WindowsManifest Include=\"{text4}\">\r\n                <Type>Windows</Type>\r\n            </WindowsManifest>\r\n            ";
			XElement xElement2 = xElement.Element(xElement.Name.Namespace + "ItemGroup");
			XElement xElement3 = XElement.Parse(text5);
			SetProjNameSpace(xElement3);
			xElement2.Add(xElement3);
			XDocument xDocument = new XDocument(xElement);
			environ.Logger.LogInfo($"Saving: {text}");
			xDocument.Save(text);
			SdCommand.Run("add", text);
			return Path.GetDirectoryName(text);
		}

		private static string PathReplace(string path, int position, string depot)
		{
			string[] array = path.Split('\\');
			array[position] = depot;
			string text = null;
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				text = text + text2 + "\\";
			}
			return text.TrimEnd('\\');
		}

		private static void SetProjNameSpace(XElement xProj)
		{
			XNamespace xNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
			foreach (XElement item in xProj.DescendantsAndSelf())
			{
				item.Name = xNamespace + item.Name.LocalName;
			}
		}

		public static string ReplaceFirstOccurrence(string Source, string Find, string Replace)
		{
			int startIndex = Source.IndexOf(Find, StringComparison.InvariantCultureIgnoreCase);
			return Source.Remove(startIndex, Find.Length).Insert(startIndex, Replace);
		}

		public static void ReformatJsonMan(string fileName, Config environ)
		{
			Process process = new Process();
			process.StartInfo.FileName = Environment.ExpandEnvironmentVariables("%sdxroot%\\tools\\reformatjsonman.cmd");
			process.StartInfo.Arguments = $"-inplace {fileName} ";
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.Start();
			process.StandardOutput.ReadToEnd();
			process.WaitForExit();
		}
	}
}
