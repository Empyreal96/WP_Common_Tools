using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization.XML
{
	[XmlRoot(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class StaticVariant : Variant
	{
		[XmlIgnore]
		public override string Name => "Static";

		[XmlIgnore]
		public override List<TargetRef> TargetRefs => new List<TargetRef>();

		[XmlElement(ElementName = "DataAssets")]
		public List<DataAssets> DataAssetGroups { get; set; }

		public StaticVariant()
		{
			DataAssetGroups = new List<DataAssets>();
		}

		public override bool ShouldSerializeName()
		{
			return false;
		}

		public override void LinkToFile(IDefinedIn file)
		{
			if (file == null)
			{
				throw new ArgumentNullException("file");
			}
			base.LinkToFile(file);
			foreach (DataAsset item in DataAssetGroups.SelectMany((DataAssets x) => x.Items))
			{
				((IDefinedIn)item).DefinedInFile = file.DefinedInFile;
			}
		}

		public IEnumerable<CustomizationError> Merge(StaticVariant otherVariant, bool allowOverride)
		{
			if (otherVariant == null)
			{
				throw new ArgumentNullException("otherVariant");
			}
			List<CustomizationError> list = new List<CustomizationError>();
			IEnumerable<CustomizationError> collection = Merge((Variant)otherVariant, allowOverride);
			list.AddRange(collection);
			IEnumerable<CustomizationError> collection2 = MergeDataAssets(otherVariant.DataAssetGroups, allowOverride);
			list.AddRange(collection2);
			return list;
		}

		private IEnumerable<CustomizationError> MergeDataAssets(IEnumerable<DataAssets> otherAssets, bool allowOverride)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			IEnumerable<IGrouping<CustomizationDataAssetType, DataAsset>> enumerable = from x in otherAssets.Concat(DataAssetGroups)
				from item in x.Items
				select new
				{
					Key = x.Type,
					Value = item
				} into x
				group x.Value by x.Key;
			List<DataAssets> list2 = new List<DataAssets>();
			foreach (IGrouping<CustomizationDataAssetType, DataAsset> item3 in enumerable)
			{
				foreach (IGrouping<string, DataAsset> item4 in from x in item3
					group x by x.ExpandedSourcePath into grp
					where grp.Count() > 1
					select grp)
				{
					CustomizationError item2 = new CustomizationError((!allowOverride) ? CustomizationErrorSeverity.Error : CustomizationErrorSeverity.Warning, item4, Strings.DuplicateApplications, item4.Key);
					list.Add(item2);
				}
				DataAssets dataAssets = new DataAssets(item3.Key);
				dataAssets.Items = item3.DistinctBy((DataAsset x) => x.ExpandedSourcePath).ToList();
				list2.Add(dataAssets);
			}
			DataAssetGroups = list2;
			return list;
		}
	}
}
