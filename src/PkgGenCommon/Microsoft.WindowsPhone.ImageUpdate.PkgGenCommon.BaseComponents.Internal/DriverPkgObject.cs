using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot(ElementName = "Driver", Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00")]
	public sealed class DriverPkgObject : OSComponentPkgObject
	{
		private static readonly string STR_SYSTEM_HIVE = Enum.GetName(typeof(SystemRegistryHiveFiles), SystemRegistryHiveFiles.SYSTEM);

		private static readonly string STR_DRIVERS_HIVE = Enum.GetName(typeof(SystemRegistryHiveFiles), SystemRegistryHiveFiles.DRIVERS);

		private static readonly string STR_SOFTWARE_HIVE = Enum.GetName(typeof(SystemRegistryHiveFiles), SystemRegistryHiveFiles.SOFTWARE);

		[XmlAttribute("InfSource")]
		public string InfSource { get; set; }

		[XmlElement("Reference")]
		public List<Reference> References { get; }

		[XmlElement("Security")]
		public List<Security> InfSecurity { get; }

		[XmlIgnore]
		internal string Partition { get; set; }

		[XmlIgnore]
		private string ProjectFilePath { get; set; }

		public DriverPkgObject()
		{
			References = new List<Reference>();
			InfSecurity = new List<Security>();
		}

		protected override void DoPreprocess(PackageProject proj, IMacroResolver macroResolver)
		{
			Partition = proj.Partition;
			ProjectFilePath = proj.ProjectFilePath;
			base.DoPreprocess(proj, macroResolver);
		}

		protected override void DoBuild(IPackageGenerator pkgGen)
		{
			base.DoBuild(pkgGen);
			if (pkgGen.BuildPass == BuildPass.BuildTOC)
			{
				return;
			}
			string[] references = null;
			string[] stagingSubdirs = null;
			if (References != null)
			{
				references = References.Select((Reference x) => pkgGen.MacroResolver.Resolve(x.Source)).ToArray();
				stagingSubdirs = References.Select((Reference x) => x.StagingSubDir).ToArray();
			}
			string text = pkgGen.MacroResolver.Resolve(InfSource);
			string hiveRoot = ((!pkgGen.MacroResolver.Resolve("$(__nohives)", MacroResolveOptions.SkipOnUnknownMacro).Equals("true")) ? pkgGen.MacroResolver.Resolve("$(HIVE_ROOT)") : "no:hives");
			string wimRoot = pkgGen.MacroResolver.Resolve("$(WIM_ROOT)", MacroResolveOptions.SkipOnUnknownMacro);
			string productName = pkgGen.MacroResolver.Resolve("$(PRODUCT_NAME)", MacroResolveOptions.SkipOnUnknownMacro);
			string tempDirectory = FileUtils.GetTempDirectory();
			string tempDirectory2 = FileUtils.GetTempDirectory();
			try
			{
				string text2 = text;
				ISecurityPolicyCompiler policyCompiler = pkgGen.PolicyCompiler;
				policyCompiler.DriverSecurityInitialize(ProjectFilePath, pkgGen.MacroResolver);
				if (InfSecurity != null && InfSecurity.Count > 0)
				{
					InfFile infFile = new InfFile(text2);
					infFile.SecurityCompiler = policyCompiler;
					foreach (Security item in InfSecurity)
					{
						infFile.UpdateSecurityPolicy(item.InfSectionName);
					}
					text2 = Path.Combine(tempDirectory2, Path.GetFileName(text2));
					infFile.SaveInf(text2);
				}
				InfFileConverter.DoConvert(text2, references, stagingSubdirs, hiveRoot, wimRoot, Partition, pkgGen.CPU, tempDirectory, productName, pkgGen.ToolPaths);
				SystemRegistryHiveFiles[] array = new SystemRegistryHiveFiles[2]
				{
					SystemRegistryHiveFiles.SYSTEM,
					SystemRegistryHiveFiles.SOFTWARE
				};
				foreach (SystemRegistryHiveFiles hive in array)
				{
					foreach (KeyValuePair<string, InfFileConverter.RegKeyData> item2 in InfFileConverter.ExtractKeys(tempDirectory, hive))
					{
						string key = item2.Key;
						List<InfFileConverter.RegKeyValue> valueList = item2.Value.ValueList;
						pkgGen.AddRegKey(key);
						foreach (InfFileConverter.RegKeyValue item3 in valueList)
						{
							if (item3.IsMultiSz)
							{
								pkgGen.AddRegMultiSzSegment(key, item3.MultiSzName, item3.MultiSzValue);
							}
							else
							{
								pkgGen.AddRegValue(key, item3.Value.Name, item3.Value.RegValType, item3.Value.Value);
							}
						}
					}
				}
			}
			catch (IUException ex)
			{
				throw new PkgGenException(ex, ex.Message);
			}
			finally
			{
				FileUtils.DeleteTree(tempDirectory);
				FileUtils.DeleteTree(tempDirectory2);
			}
		}
	}
}
