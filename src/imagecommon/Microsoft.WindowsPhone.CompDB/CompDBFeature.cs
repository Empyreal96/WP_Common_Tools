using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.CompDB
{
	public class CompDBFeature
	{
		public enum CompDBFeatureTypes
		{
			None,
			MobileFeature,
			DesktopMedia,
			OptionalFeature,
			OnDemandFeature,
			LanguagePack,
			GDR,
			CritGDR,
			Tool
		}

		[XmlAttribute]
		[DefaultValue(CompDBFeatureTypes.None)]
		public CompDBFeatureTypes Type;

		[XmlAttribute]
		public string FeatureID;

		[XmlAttribute]
		public string FMID;

		[XmlAttribute]
		public string Group;

		[XmlArrayItem(ElementName = "Package", Type = typeof(CompDBFeaturePackage), IsNullable = false)]
		[XmlArray]
		public List<CompDBFeaturePackage> Packages = new List<CompDBFeaturePackage>();

		[XmlIgnore]
		public string FeatureIDWithFMID
		{
			get
			{
				string text = FeatureID;
				if (!string.IsNullOrEmpty(FMID))
				{
					text = text + "." + FMID;
				}
				return text;
			}
		}

		public bool ShouldSerializePackages()
		{
			if (Packages != null)
			{
				return Packages.Count() > 0;
			}
			return false;
		}

		public CompDBFeature()
		{
		}

		public CompDBFeature(string featureID, string fmID, CompDBFeatureTypes type, string group)
		{
			FeatureID = featureID;
			FMID = fmID;
			Group = group;
			Type = type;
		}

		public CompDBFeature(CompDBFeature srcFeature)
		{
			FeatureID = srcFeature.FeatureID;
			FMID = srcFeature.FMID;
			Group = srcFeature.Group;
			Type = srcFeature.Type;
			Packages = new List<CompDBFeaturePackage>();
			foreach (CompDBFeaturePackage package in srcFeature.Packages)
			{
				Packages.Add(new CompDBFeaturePackage(package));
			}
		}

		public CompDBFeaturePackage FindPackage(string packageID)
		{
			return Packages.FirstOrDefault((CompDBFeaturePackage pkg) => pkg.ID.Equals(packageID, StringComparison.OrdinalIgnoreCase));
		}

		public override string ToString()
		{
			return FeatureIDWithFMID + " : " + Group + " : Count=" + Packages.Count();
		}
	}
}
