using System.ComponentModel;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class UpdateOSOutputPackage
	{
		public string Description;

		public string UpdateState;

		public string PackageFile;

		public string PackageIdentity;

		public UpdateOSOutputIdentity Identity;

		public ReleaseType ReleaseType;

		public OwnerType OwnerType;

		public BuildType BuildType;

		[XmlIgnore]
		public CpuId CpuType;

		[XmlElement("CpuType")]
		public string CpuTypeStr;

		public string Culture;

		public string Resolution;

		public string Partition;

		[DefaultValue(false)]
		public bool IsRemoval;

		[DefaultValue(false)]
		public bool IsBinaryPartition;

		public int Result;

		[XmlIgnore]
		public string Name => PackageTools.BuildPackageName(Identity.Owner, Identity.Component, Identity.SubComponent, Culture, Resolution);

		public override string ToString()
		{
			return Name + ":" + Partition;
		}
	}
}
