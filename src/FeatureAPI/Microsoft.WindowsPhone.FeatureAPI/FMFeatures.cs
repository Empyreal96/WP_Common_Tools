using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class FMFeatures
	{
		public const string MSFeaturePrefix = "MS_";

		public const string OEMFeaturePrefix = "OEM_";

		[XmlArrayItem(ElementName = "PackageFile", Type = typeof(MSOptionalPkgFile), IsNullable = false)]
		[XmlArray]
		public List<MSOptionalPkgFile> Microsoft;

		[XmlArrayItem(ElementName = "ConditionalFeature", Type = typeof(FMConditionalFeature), IsNullable = false)]
		[XmlArray]
		public List<FMConditionalFeature> MSConditionalFeatures;

		[XmlArrayItem(ElementName = "FeatureGroup", Type = typeof(FMFeatureGrouping), IsNullable = false)]
		[XmlArray]
		public List<FMFeatureGrouping> MSFeatureGroups;

		[XmlArrayItem(ElementName = "PackageFile", Type = typeof(OEMOptionalPkgFile), IsNullable = false)]
		[XmlArray]
		public List<OEMOptionalPkgFile> OEM;

		[XmlArrayItem(ElementName = "FeatureGroup", Type = typeof(FMFeatureGrouping), IsNullable = false)]
		[XmlArray]
		public List<FMFeatureGrouping> OEMFeatureGroups;

		public bool ShouldSerializeMicrosoft()
		{
			return Microsoft != null;
		}

		public bool ShouldSerializeMSConditionalFeatures()
		{
			if (MSConditionalFeatures != null)
			{
				return MSConditionalFeatures.Count() > 0;
			}
			return false;
		}

		public bool ShouldSerializeMSFeatureGroups()
		{
			if (MSFeatureGroups != null)
			{
				return MSFeatureGroups.Count() > 0;
			}
			return false;
		}

		public bool ShouldSerializeOEM()
		{
			return OEM != null;
		}

		public bool ShouldSerializeOEMFeatureGroups()
		{
			if (OEMFeatureGroups != null)
			{
				return OEMFeatureGroups.Count() > 0;
			}
			return false;
		}

		public void ValidateConstraints(List<string> MSFeatures, List<string> OEMFeatures)
		{
			bool flag = true;
			StringBuilder stringBuilder = new StringBuilder();
			if (MSFeatureGroups != null)
			{
				foreach (FMFeatureGrouping mSFeatureGroup in MSFeatureGroups)
				{
					string errorMessage;
					if (!mSFeatureGroup.ValidateConstraints(MSFeatures, out errorMessage))
					{
						flag = false;
						stringBuilder.AppendLine();
						stringBuilder.AppendLine("Errors in Microsoft Features:");
						stringBuilder.AppendLine(errorMessage);
					}
				}
			}
			if (OEMFeatureGroups != null)
			{
				foreach (FMFeatureGrouping oEMFeatureGroup in OEMFeatureGroups)
				{
					string errorMessage2;
					if (!oEMFeatureGroup.ValidateConstraints(OEMFeatures, out errorMessage2))
					{
						flag = false;
						stringBuilder.AppendLine();
						stringBuilder.AppendLine("Errors in OEM Features:");
						stringBuilder.AppendLine(errorMessage2);
					}
				}
			}
			if (!flag)
			{
				throw new FeatureAPIException("FeatureAPI!ValidateConstraints: OEMInput file contains invalid Feature combinations:" + stringBuilder.ToString());
			}
		}

		public void Merge(FMFeatures srcFeatures)
		{
			if (srcFeatures.MSConditionalFeatures != null)
			{
				if (MSConditionalFeatures == null)
				{
					MSConditionalFeatures = srcFeatures.MSConditionalFeatures;
				}
				else
				{
					MSConditionalFeatures.AddRange(srcFeatures.MSConditionalFeatures);
				}
			}
			if (srcFeatures.MSFeatureGroups != null)
			{
				if (MSFeatureGroups == null)
				{
					MSFeatureGroups = srcFeatures.MSFeatureGroups;
				}
				else
				{
					MSFeatureGroups.AddRange(srcFeatures.MSFeatureGroups);
				}
			}
			if (srcFeatures.Microsoft != null)
			{
				if (Microsoft == null)
				{
					Microsoft = srcFeatures.Microsoft;
				}
				else
				{
					Microsoft.AddRange(srcFeatures.Microsoft);
				}
			}
			if (srcFeatures.OEMFeatureGroups != null)
			{
				if (OEMFeatureGroups == null)
				{
					OEMFeatureGroups = srcFeatures.OEMFeatureGroups;
				}
				else
				{
					OEMFeatureGroups.AddRange(srcFeatures.OEMFeatureGroups);
				}
			}
			if (srcFeatures.OEM != null)
			{
				if (OEM == null)
				{
					OEM = srcFeatures.OEM;
				}
				else
				{
					OEM.AddRange(srcFeatures.OEM);
				}
			}
		}

		public static string GetFeatureIDWithoutPrefix(string featureID)
		{
			string text = featureID;
			if (text.StartsWith("MS_", StringComparison.OrdinalIgnoreCase))
			{
				text = text.Substring("MS_".Length);
			}
			if (text.StartsWith("OEM_", StringComparison.OrdinalIgnoreCase))
			{
				text = text.Substring("OEM_".Length);
			}
			return text;
		}
	}
}
