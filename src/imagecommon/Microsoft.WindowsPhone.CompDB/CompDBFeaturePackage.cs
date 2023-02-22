using System.ComponentModel;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.CompDB
{
	public class CompDBFeaturePackage
	{
		public enum UpdateTypes
		{
			Removal,
			Diff,
			Canonical,
			NoUpdate
		}

		public enum PackageTypes
		{
			FeaturePackage,
			MediaFileList,
			MetadataESD
		}

		[XmlAttribute]
		public string ID;

		[XmlAttribute]
		[DefaultValue(false)]
		public bool FIP;

		[XmlAttribute]
		[DefaultValue(UpdateTypes.Canonical)]
		public UpdateTypes UpdateType = UpdateTypes.Canonical;

		[XmlAttribute]
		public PackageTypes PackageType;

		public CompDBFeaturePackage()
		{
		}

		public CompDBFeaturePackage(string id, bool featureIdentifierPackage)
		{
			ID = id;
			FIP = featureIdentifierPackage;
		}

		public CompDBFeaturePackage(CompDBFeaturePackage srcPkg)
		{
			ID = srcPkg.ID;
			FIP = srcPkg.FIP;
			UpdateType = srcPkg.UpdateType;
		}

		public CompDBFeaturePackage SetUpdateType(UpdateTypes type)
		{
			UpdateType = type;
			return this;
		}

		public override string ToString()
		{
			return ID + (FIP ? "(FIP)" : "");
		}
	}
}
