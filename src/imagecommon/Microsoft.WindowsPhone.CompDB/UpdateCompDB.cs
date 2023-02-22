using System;
using System.Collections.Generic;
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
	public class UpdateCompDB : BuildCompDB
	{
		public const string c_UpdateCompDBRevision = "1";

		public const string c_UpdateCompDBSchemaVersion = "1.2";

		[XmlAttribute]
		public Guid TargetBuildID;

		[XmlAttribute]
		public string TargetBuildInfo;

		[XmlAttribute]
		public string TargetOSVersion;

		public UpdateCompDB()
		{
		}

		public UpdateCompDB(UpdateCompDB srcDB)
			: base(srcDB)
		{
			TargetBuildID = srcDB.TargetBuildID;
			TargetBuildInfo = srcDB.TargetBuildInfo;
			TargetOSVersion = srcDB.TargetOSVersion;
		}

		public UpdateCompDB(BuildCompDB srcBuild, BuildCompDB tgtBuild, IULogger logger)
		{
			_iuLogger = logger;
			if (string.IsNullOrEmpty(srcBuild.Product) != string.IsNullOrEmpty(tgtBuild.Product))
			{
				throw new ImageCommonException("ImageCommon::UpdateCompDB!UpdateCompDB: The source and target DBs must be for the same Product: src=" + (string.IsNullOrEmpty(srcBuild.Product) ? "null" : srcBuild.Product) + " tgt=" + (string.IsNullOrEmpty(tgtBuild.Product) ? "null" : tgtBuild.Product));
			}
			if (!srcBuild.Product.Equals(tgtBuild.Product))
			{
				throw new ImageCommonException("ImageCommon::UpdateCompDB!UpdateCompDB: The source and target DBs must be for the same Product: src=" + srcBuild.Product + " tgt=" + tgtBuild.Product);
			}
			if (srcBuild.ReleaseType != tgtBuild.ReleaseType)
			{
				throw new ImageCommonException("ImageCommon::UpdateCompDB!UpdateCompDB: The source and target DBs must have the same Release Type: src=" + srcBuild.ReleaseType.ToString() + " tgt=" + tgtBuild.ReleaseType);
			}
			TargetBuildID = tgtBuild.BuildID;
			BuildID = srcBuild.BuildID;
			BuildArch = srcBuild.BuildArch;
			TargetBuildInfo = tgtBuild.BuildInfo;
			BuildInfo = srcBuild.BuildInfo;
			TargetOSVersion = tgtBuild.OSVersion;
			OSVersion = srcBuild.OSVersion;
			Revision = "1";
			SchemaVersion = "1.2";
			Packages.AddRange(srcBuild.Packages.Intersect(tgtBuild.Packages, CompDBPackageInfoComparer.IgnorePackageHash));
			List<CompDBPackageInfo> collection = (from pkg in srcBuild.Packages
				where pkg.Payload.Count() == 1
				select pkg into srcPkg
				join destPkg in from pkg in tgtBuild.Packages
					where pkg.Payload.Count() == 1
					select pkg on srcPkg.ID equals destPkg.ID
				where !destPkg.FirstPayloadItem.Path.Equals(srcPkg.FirstPayloadItem.Path, StringComparison.OrdinalIgnoreCase) && destPkg.Equals(srcPkg, CompDBPackageInfo.CompDBPackageInfoComparison.IgnorePayloadPaths)
				select destPkg.SetPreviousPath(srcPkg.FirstPayloadItem.Path)).ToList();
			Packages.AddRange(collection);
			Packages = Packages.Select((CompDBPackageInfo pkg) => new CompDBPackageInfo(pkg).ClearPackageHashes()).ToList();
			Packages = Packages.Select((CompDBPackageInfo pkg) => pkg.ClearSkipForPublishing()).ToList();
			Packages = Packages.Select((CompDBPackageInfo pkg) => pkg.ClearSkipForPRSSigning()).ToList();
			Packages = Packages.Select((CompDBPackageInfo pkg) => pkg.SetPayloadType(CompDBPayloadInfo.PayloadTypes.Diff)).ToList();
			List<FMConditionalFeature> source = tgtBuild.MSConditionalFeatures.Where((FMConditionalFeature feat) => feat.UpdateAction == FeatureCondition.Action.NoUpdate).ToList();
			List<string> noUpdateFeatureIDs = source.Select((FMConditionalFeature feat) => feat.FeatureIDWithFMID).ToList();
			List<CompDBFeature> list = tgtBuild.Features.Select((CompDBFeature feat) => new CompDBFeature(feat)).ToList();
			list.RemoveAll((CompDBFeature feat) => noUpdateFeatureIDs.Contains(feat.FeatureIDWithFMID, StringComparer.OrdinalIgnoreCase));
			foreach (CompDBFeature srcFeature in srcBuild.Features)
			{
				List<CompDBFeaturePackage> srcFeaturePackages = new List<CompDBFeaturePackage>(srcFeature.Packages);
				CompDBFeature newFeature = new CompDBFeature(srcFeature.FeatureID, srcFeature.FMID, srcFeature.Type, srcFeature.Group);
				if (source.FirstOrDefault((FMConditionalFeature noUpdate) => noUpdate.FeatureIDWithFMID.Equals(newFeature.FeatureIDWithFMID, StringComparison.OrdinalIgnoreCase)) != null)
				{
					newFeature.Packages = srcFeaturePackages.Select((CompDBFeaturePackage pkg) => pkg.SetUpdateType(CompDBFeaturePackage.UpdateTypes.NoUpdate)).ToList();
					foreach (CompDBFeaturePackage featurePkg in newFeature.Packages)
					{
						Packages.RemoveAll((CompDBPackageInfo pkg) => pkg.ID.Equals(featurePkg.ID, StringComparison.OrdinalIgnoreCase));
					}
					Features.Add(newFeature);
					continue;
				}
				CompDBFeature compDBFeature = list.FirstOrDefault((CompDBFeature ftr) => ftr.FeatureIDWithFMID.Equals(srcFeature.FeatureIDWithFMID, StringComparison.OrdinalIgnoreCase));
				if (compDBFeature == null)
				{
					newFeature.Packages = srcFeaturePackages.Select((CompDBFeaturePackage pkg) => pkg.SetUpdateType(CompDBFeaturePackage.UpdateTypes.Removal)).ToList();
				}
				else
				{
					newFeature.Packages = new List<CompDBFeaturePackage>();
					List<CompDBFeaturePackage> tgtFeaturePackages = new List<CompDBFeaturePackage>(compDBFeature.Packages);
					List<CompDBFeaturePackage> source2 = srcFeaturePackages.Where((CompDBFeaturePackage pkg) => tgtFeaturePackages.Select((CompDBFeaturePackage pkg2) => pkg2.ID).Contains(pkg.ID, IgnoreCase)).ToList();
					newFeature.Packages.AddRange(source2.Select((CompDBFeaturePackage pkg) => pkg.SetUpdateType(CompDBFeaturePackage.UpdateTypes.Diff)));
					List<CompDBFeaturePackage> source3 = srcFeaturePackages.Where((CompDBFeaturePackage pkg) => !tgtFeaturePackages.Select((CompDBFeaturePackage pkg2) => pkg2.ID).Contains(pkg.ID, IgnoreCase)).ToList();
					newFeature.Packages.AddRange(source3.Select((CompDBFeaturePackage pkg) => pkg.SetUpdateType(CompDBFeaturePackage.UpdateTypes.Removal)));
					List<CompDBFeaturePackage> collection2 = tgtFeaturePackages.Where((CompDBFeaturePackage pkg) => !srcFeaturePackages.Select((CompDBFeaturePackage pkg2) => pkg2.ID).Contains(pkg.ID, IgnoreCase)).ToList();
					newFeature.Packages.AddRange(collection2);
				}
				Features.Add(newFeature);
			}
		}

		public new static UpdateCompDB ValidateAndLoad(string xmlFile, IULogger logger)
		{
			UpdateCompDB updateCompDB = new UpdateCompDB();
			string text = string.Empty;
			string updateCompDBSchema = BuildPaths.UpdateCompDBSchema;
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
			foreach (string text2 in manifestResourceNames)
			{
				if (text2.Contains(updateCompDBSchema))
				{
					text = text2;
					break;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				throw new ImageCommonException("ImageCommon::UpdateCompDB!ValidateAndLoad: XSD resource was not found: " + updateCompDBSchema);
			}
			TextReader textReader = new StreamReader(LongPathFile.OpenRead(xmlFile));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(UpdateCompDB));
			try
			{
				updateCompDB = (UpdateCompDB)xmlSerializer.Deserialize(textReader);
			}
			catch (Exception innerException)
			{
				throw new ImageCommonException("ImageCommon::UpdateCompDB!ValidateAndLoad: Unable to parse Update CompDB XML file '" + xmlFile + "'", innerException);
			}
			finally
			{
				textReader.Close();
			}
			bool flag = "1.2".Equals(updateCompDB.SchemaVersion);
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
						throw new ImageCommonException("ImageCommon::UpdateCompDB!ValidateAndLoad: Unable to validate Update CompDB XSD for file '" + xmlFile + "'.", innerException2);
					}
					logger.LogWarning("Warning: ImageCommon::UpdateCompDB!ValidateAndLoad: Unable to validate Update CompDB XSD for file '" + xmlFile + "'.");
					if (string.IsNullOrEmpty(updateCompDB.SchemaVersion))
					{
						logger.LogWarning("Warning: ImageCommon::UpdateCompDB!ValidateAndLoad: Schema Version was not given in Update CompDB. Most up to date Schema Version is {1}.", "1.2");
					}
					else
					{
						logger.LogWarning("Warning: ImageCommon::UpdateCompDB!ValidateAndLoad: Schema Version given in Update CompDB ({0}) does not match most up to date Schema Version ({1}).", updateCompDB.SchemaVersion, "1.2");
					}
				}
			}
			logger.LogInfo("UpdateCompDB: Successfully validated the Update CompDB XML: {0}", xmlFile);
			BuildCompDB parentDB = updateCompDB;
			updateCompDB.Packages = updateCompDB.Packages.Select((CompDBPackageInfo pkg) => pkg.SetParentDB(parentDB)).ToList();
			if (updateCompDB.ReleaseType == ReleaseType.Invalid)
			{
				if (updateCompDB.Packages.Any((CompDBPackageInfo pkg) => pkg.ReleaseType == ReleaseType.Test))
				{
					updateCompDB.ReleaseType = ReleaseType.Test;
				}
				else
				{
					updateCompDB.ReleaseType = ReleaseType.Production;
				}
			}
			return updateCompDB;
		}

		public override void WriteToFile(string xmlFile)
		{
			string directoryName = Path.GetDirectoryName(xmlFile);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			SchemaVersion = "1.2";
			Revision = "1";
			TextWriter textWriter = new StreamWriter(LongPathFile.OpenWrite(xmlFile));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(UpdateCompDB));
			try
			{
				xmlSerializer.Serialize(textWriter, this);
			}
			catch (Exception innerException)
			{
				throw new ImageCommonException("ImageCommon::UpdateCompDB!WriteToFile: Unable to write Update CompDB XML file '" + xmlFile + "'", innerException);
			}
			finally
			{
				textWriter.Close();
			}
		}

		public override string ToString()
		{
			return "Update DB: " + BuildInfo + " to " + TargetBuildInfo;
		}
	}
}
