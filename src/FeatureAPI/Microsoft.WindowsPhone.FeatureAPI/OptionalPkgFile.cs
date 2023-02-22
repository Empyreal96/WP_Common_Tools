using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class OptionalPkgFile : PkgFile
	{
		[XmlArrayItem(ElementName = "FeatureID", Type = typeof(string), IsNullable = false)]
		[XmlArray]
		public List<string> FeatureIDs;

		[XmlIgnore]
		public override string GroupValue
		{
			get
			{
				return string.Join(";", FeatureIDs.ToArray());
			}
			set
			{
				FeatureIDs = value.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
			}
		}

		public OptionalPkgFile(FeatureManifest.PackageGroups fmGroup)
			: base(fmGroup)
		{
		}

		public OptionalPkgFile(OptionalPkgFile srcPkg)
			: base(srcPkg)
		{
			FMGroup = srcPkg.FMGroup;
			FeatureIDs = srcPkg.FeatureIDs;
		}

		public new void CopyPkgFile(PkgFile srcPkgFile)
		{
			base.CopyPkgFile(srcPkgFile);
			OptionalPkgFile optionalPkgFile = srcPkgFile as OptionalPkgFile;
			FeatureIDs = new List<string>(optionalPkgFile.FeatureIDs);
		}
	}
}
