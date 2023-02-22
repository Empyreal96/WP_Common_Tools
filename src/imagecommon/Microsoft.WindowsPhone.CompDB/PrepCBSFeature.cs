using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.WindowsPhone.CompDB
{
	public class PrepCBSFeature
	{
		private static string c_DeclareCapabilityElement = "declareCapability";

		private static string c_CapabilityIdentityElement = "capabilityIdentity";

		private static string c_CapabilityElement = "capability";

		private static string c_AssemblyIdentityElement = "assemblyIdentity";

		private static string c_PackageElement = "package";

		private static string c_CustomInformationElement = "customInformation";

		private static string c_FeaturePackageElement = "featurePackage";

		private static string c_TargetPartitionElement = "targetPartition";

		private static string c_BinaryPartitionAttribute = "binaryPartition";

		private static string c_NameAttribute = "name";

		private static string c_ProcessorArchitectureAttribute = "processorArchitecture";

		private static string c_PublicKeyTokenAttribute = "publicKeyToken";

		private static string c_VersionAttribute = "version";

		private static string c_LanguageAttribute = "language";

		private static string c_GroupAttribute = "group";

		private static string c_FMIDAttribute = "FMID";

		private static string c_SatelliteTypeAttribute = "satelliteType";

		public static void Prep(string sourcePackage, string FMID, string groupName, string groupType, string buildArch, List<FeatureManifest.FMPkgInfo> packages, bool usePhoneSigning)
		{
			string text = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(text);
			string text2 = Path.ChangeExtension(sourcePackage, PkgConstants.c_strPackageExtension);
			string text3 = Path.ChangeExtension(sourcePackage, PkgConstants.c_strCBSPackageExtension);
			bool flag = FileUtils.IsTargetUpToDate(text2, text3);
			if (flag)
			{
				text = Path.Combine(text, Path.GetFileNameWithoutExtension(text3));
				if (IsFeatureInfoAlreadyAdded(text3, FMID, groupName, groupType, buildArch, packages, text))
				{
					return;
				}
				CabApiWrapper.Extract(text3, text);
			}
			else
			{
				List<string> list = new List<string>();
				list.Add(text2);
				PkgConvertDSM.ConvertPackagesToCBS(PkgConvertDSM.CONVERTDSM_PARAMETERS_FLAGS.CONVERTDSM_PARAMETERS_FLAGS_NONE, list, text);
				text = Path.Combine(text, Path.GetFileNameWithoutExtension(text2));
			}
			if (packages != null && packages.Count > 0)
			{
				string text4 = Path.Combine(text, PkgConstants.c_strMumFile);
				XDocument xDocument = XDocument.Load(text4);
				XNamespace @namespace = xDocument.Root.Name.Namespace;
				IEnumerable<XElement> source = xDocument.Descendants(@namespace + c_DeclareCapabilityElement);
				while (source.Any())
				{
					source.First().Remove();
					source = xDocument.Descendants(@namespace + c_DeclareCapabilityElement);
				}
				XElement content = GenFeatureElement(@namespace, FMID, groupName, groupType, buildArch, packages);
				XElement xElement = xDocument.Root.Elements(@namespace + c_PackageElement).First();
				if (xElement.Element(@namespace + c_CustomInformationElement) != null)
				{
					xElement.Element(@namespace + c_CustomInformationElement).AddFirst(content);
				}
				else
				{
					xElement.AddFirst(content);
				}
				xDocument.Save(text4);
			}
			string name = Package.LoadFromCab(flag ? text3 : text2).Name;
			if (File.Exists(text3))
			{
				File.Delete(text3);
			}
			string text5 = Path.Combine(text, PkgConstants.c_strCBSCatalogFile);
			if (File.Exists(text5))
			{
				File.Delete(text5);
			}
			string[] files = Directory.GetFiles(text, "*.*", SearchOption.AllDirectories);
			PackageTools.CreateCatalog(files, files, name, text5);
			Process process = new Process();
			process.StartInfo.FileName = "cmd.exe";
			if (usePhoneSigning)
			{
				process.StartInfo.Arguments = "/c sign.cmd \"" + text5 + "\"";
			}
			else
			{
				process.StartInfo.Arguments = "/c ntsign.cmd \"" + text5 + "\"";
			}
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.Start();
			string text6 = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			if (process.ExitCode != 0)
			{
				throw new ImageCommonException(string.Format("Error: ImageCommon!PrepCBSFeature: {3} failed to resign {0}.\nErr: {1}\nOutput: {2}", text5, process.ExitCode, text6, usePhoneSigning ? "sign.cmd" : "ntsign.cmd"));
			}
			Console.WriteLine(text6);
			string text7 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(text7);
			CabApiWrapper.CreateCab(text3, text, text7, "*.*", CompressionType.FastLZX);
			if (usePhoneSigning)
			{
				process.StartInfo.Arguments = "/c sign.cmd \"" + sourcePackage + "\"";
			}
			else
			{
				process.StartInfo.Arguments = "/c ntsign.cmd \"" + sourcePackage + "\"";
			}
			process.Start();
			text6 = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			if (process.ExitCode != 0)
			{
				throw new ImageCommonException($"Error: ImageCommon!PrepCBSFeature: sign.cmd failed to resign {sourcePackage}.\nErr: {process.ExitCode}\nOutput: {text6}");
			}
			Console.WriteLine(text6);
		}

		public static string GetFeatureInfoXML(string updateMumFile)
		{
			string result = "";
			XDocument xDocument = XDocument.Load(updateMumFile);
			XNamespace @namespace = xDocument.Root.Name.Namespace;
			IEnumerable<XElement> source = xDocument.Descendants(@namespace + c_DeclareCapabilityElement);
			if (source.Count() == 1)
			{
				result = source.First().ToString();
			}
			return result;
		}

		public static void ParseFeatureInfoXML(string dcXML, out string fmID, out string groupName, out string groupType, out string buildArch, out List<FeatureManifest.FMPkgInfo> packages)
		{
			XDocument xDocument = XDocument.Parse(dcXML);
			packages = new List<FeatureManifest.FMPkgInfo>();
			List<string> list = new List<string>();
			fmID = "";
			groupName = "";
			groupType = "";
			buildArch = "";
			foreach (XElement item in xDocument.DescendantNodes())
			{
				if (item.Name.LocalName.Equals(c_CapabilityElement))
				{
					foreach (XAttribute item2 in item.Attributes())
					{
						if (item2.Name.LocalName.Equals(c_FMIDAttribute))
						{
							fmID = item2.Value;
						}
						else if (item2.Name.LocalName.Equals(c_GroupAttribute))
						{
							groupType = item2.Value;
						}
					}
				}
				else if (item.Name.LocalName.Equals(c_CapabilityIdentityElement))
				{
					foreach (XAttribute item3 in item.Attributes())
					{
						if (item3.Name.LocalName.Equals(c_NameAttribute))
						{
							groupName = item3.Value;
						}
					}
				}
				else
				{
					if (!item.Name.LocalName.Equals(c_PackageElement))
					{
						continue;
					}
					FeatureManifest.FMPkgInfo fMPkgInfo = new FeatureManifest.FMPkgInfo();
					CompDBPackageInfo.SatelliteTypes result = CompDBPackageInfo.SatelliteTypes.Base;
					foreach (XAttribute item4 in item.Attributes())
					{
						if (item4.Name.LocalName.Equals(c_BinaryPartitionAttribute))
						{
							fMPkgInfo.BinaryPartition = true;
						}
						else if (item4.Name.LocalName.Equals(c_TargetPartitionElement))
						{
							fMPkgInfo.Partition = item4.Value;
						}
						else if (item4.Name.LocalName.Equals(c_SatelliteTypeAttribute) && !Enum.TryParse<CompDBPackageInfo.SatelliteTypes>(item4.Value, out result))
						{
							result = CompDBPackageInfo.SatelliteTypes.Base;
						}
					}
					foreach (XElement item5 in item.DescendantNodes())
					{
						if (!item5.Name.LocalName.Equals(c_AssemblyIdentityElement))
						{
							continue;
						}
						foreach (XAttribute item6 in item5.Attributes())
						{
							if (item6.Name.LocalName.Equals(c_NameAttribute))
							{
								fMPkgInfo.ID = item6.Value;
							}
							else if (item6.Name.LocalName.Equals(c_VersionAttribute))
							{
								fMPkgInfo.Version = VersionInfo.Parse(item6.Value);
							}
							else if (item6.Name.LocalName.Equals(c_PublicKeyTokenAttribute))
							{
								fMPkgInfo.PublicKey = item6.Value;
							}
							else if (item6.Name.LocalName.Equals(c_ProcessorArchitectureAttribute))
							{
								list.Add(item6.Value);
							}
						}
					}
					switch (result)
					{
					case CompDBPackageInfo.SatelliteTypes.Language:
					{
						int startIndex2 = fMPkgInfo.ID.IndexOf(PkgFile.DefaultLanguagePattern, StringComparison.OrdinalIgnoreCase) + PkgFile.DefaultLanguagePattern.Length;
						fMPkgInfo.Language = fMPkgInfo.ID.Substring(startIndex2);
						break;
					}
					case CompDBPackageInfo.SatelliteTypes.Resolution:
					{
						int startIndex = fMPkgInfo.ID.IndexOf(PkgFile.DefaultResolutionPattern, StringComparison.OrdinalIgnoreCase) + PkgFile.DefaultResolutionPattern.Length;
						fMPkgInfo.Resolution = fMPkgInfo.ID.Substring(startIndex);
						break;
					}
					}
					packages.Add(fMPkgInfo);
				}
			}
			buildArch = (from pkg in list
				group pkg by pkg into g
				orderby g.Count() descending
				select g.Key).First();
		}

		private static bool IsFeatureInfoAlreadyAdded(string sourcePackage, string fmID, string groupName, string groupType, string buildArch, List<FeatureManifest.FMPkgInfo> packages, string extractDir)
		{
			CabApiWrapper.ExtractOne(sourcePackage, extractDir, PkgConstants.c_strMumFile);
			XDocument xDocument = XDocument.Load(Path.Combine(extractDir, PkgConstants.c_strMumFile));
			XNamespace @namespace = xDocument.Root.Name.Namespace;
			IEnumerable<XElement> source = xDocument.Descendants(@namespace + c_DeclareCapabilityElement);
			if (source.Count() != 1)
			{
				return false;
			}
			string value = source.First().ToString();
			source.First().Remove();
			return GenFeatureElement(@namespace, fmID, groupName, groupType, buildArch, packages).ToString().Equals(value);
		}

		private static XElement GenFeatureElement(XNamespace rootNS, string fmID, string groupName, string groupType, string buildArch, List<FeatureManifest.FMPkgInfo> packages)
		{
			XElement xElement = new XElement(rootNS + c_DeclareCapabilityElement);
			XElement xElement2 = ((!string.IsNullOrEmpty(fmID)) ? new XElement(rootNS + c_CapabilityElement, new XAttribute(c_GroupAttribute, groupType), new XAttribute(c_FMIDAttribute, fmID), new XElement(rootNS + c_CapabilityIdentityElement, new XAttribute(c_NameAttribute, groupName))) : new XElement(rootNS + c_CapabilityElement, new XAttribute(c_GroupAttribute, groupType), new XElement(rootNS + c_CapabilityIdentityElement, new XAttribute(c_NameAttribute, groupName))));
			XElement xElement3 = new XElement(rootNS + c_FeaturePackageElement);
			foreach (FeatureManifest.FMPkgInfo package in packages)
			{
				string text = Path.ChangeExtension(package.PackagePath, PkgConstants.c_strPackageExtension);
				string text2 = Path.ChangeExtension(package.PackagePath, PkgConstants.c_strCBSPackageExtension);
				IPkgInfo pkgInfo = Package.LoadFromCab(FileUtils.IsTargetUpToDate(text, text2) ? text2 : text);
				CompDBPackageInfo.SatelliteTypes satelliteTypeFromFMPkgInfo = CompDBPackageInfo.GetSatelliteTypeFromFMPkgInfo(package, pkgInfo);
				XElement xElement4 = new XElement(rootNS + c_PackageElement, new XAttribute(c_SatelliteTypeAttribute, satelliteTypeFromFMPkgInfo));
				if (!string.IsNullOrEmpty(package.Partition))
				{
					xElement4.Add(new XAttribute(c_TargetPartitionElement, package.Partition));
				}
				if (pkgInfo.IsBinaryPartition)
				{
					xElement4.Add(new XAttribute(c_BinaryPartitionAttribute, "true"));
				}
				string value = CompDBPackageInfo.CpuString(pkgInfo.ComplexCpuType);
				if (string.IsNullOrEmpty(pkgInfo.Culture))
				{
					xElement4.Add(new XElement(rootNS + c_AssemblyIdentityElement, new XAttribute(c_NameAttribute, pkgInfo.Name), new XAttribute(c_ProcessorArchitectureAttribute, value), new XAttribute(c_PublicKeyTokenAttribute, pkgInfo.PublicKey), new XAttribute(c_VersionAttribute, pkgInfo.Version.ToString())));
				}
				else
				{
					xElement4.Add(new XElement(rootNS + c_AssemblyIdentityElement, new XAttribute(c_LanguageAttribute, pkgInfo.Culture), new XAttribute(c_NameAttribute, pkgInfo.Name), new XAttribute(c_ProcessorArchitectureAttribute, value), new XAttribute(c_PublicKeyTokenAttribute, pkgInfo.PublicKey), new XAttribute(c_VersionAttribute, pkgInfo.Version.ToString())));
				}
				xElement3.Add(xElement4);
			}
			xElement2.Add(xElement3);
			xElement.Add(xElement2);
			return xElement;
		}
	}
}
