using System.ComponentModel;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;

namespace Microsoft.WindowsPhone.Imaging
{
	public class FeatureIdentifierPackage
	{
		public enum FixUpActions
		{
			None,
			Ignore,
			MoveToAnotherFeature,
			AndFeature
		}

		[XmlAttribute]
		public string FeatureID;

		[XmlAttribute]
		public string FMID;

		[XmlAttribute]
		public string ID;

		[XmlAttribute]
		[DefaultValue("MainOS")]
		public string Partition = "MainOS";

		[XmlAttribute("OwnerType")]
		[DefaultValue(OwnerType.Microsoft)]
		public OwnerType ownerType = OwnerType.Microsoft;

		[XmlAttribute]
		[DefaultValue(FixUpActions.None)]
		public FixUpActions FixUpAction;

		[XmlAttribute]
		public string FixUpActionValue;

		[XmlIgnore]
		public string FeatureIDWithFMID => FeatureManifest.GetFeatureIDWithFMID(FeatureID, FMID);

		public FeatureIdentifierPackage()
		{
		}

		public FeatureIdentifierPackage(PublishingPackageInfo pkg)
		{
			ID = pkg.ID;
			Partition = pkg.Partition;
			ownerType = pkg.OwnerType;
			FeatureID = pkg.FeatureID;
			FMID = pkg.FMID;
		}

		public override string ToString()
		{
			string text = FeatureIDWithFMID + " : " + ID + ":" + Partition;
			switch (FixUpAction)
			{
			case FixUpActions.MoveToAnotherFeature:
			case FixUpActions.AndFeature:
				text = text + " (" + FixUpAction.ToString() + " = " + FixUpActionValue + ")";
				break;
			case FixUpActions.Ignore:
				text = text + " (" + FixUpAction.ToString() + ")";
				break;
			}
			return text;
		}
	}
}
