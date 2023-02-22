using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.MCSF.Offline;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization.XML
{
	[XmlRoot(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class Settings : IDefinedIn
	{
		private string _definedInFile;

		private string _path;

		[XmlIgnore]
		public string DefinedInFile
		{
			get
			{
				return _definedInFile;
			}
			set
			{
				foreach (Setting item in Items)
				{
					item.DefinedInFile = value;
				}
				foreach (Asset asset in Assets)
				{
					asset.DefinedInFile = value;
				}
				_definedInFile = value;
			}
		}

		[XmlAttribute]
		public string Path
		{
			get
			{
				return _path;
			}
			set
			{
				_path = PolicyMacroTable.MacroDollarToTilde(value);
			}
		}

		[XmlElement(ElementName = "Setting")]
		public List<Setting> Items { get; set; }

		[XmlElement(ElementName = "Asset")]
		public List<Asset> Assets { get; set; }

		public Settings()
		{
			Items = new List<Setting>();
			Assets = new List<Asset>();
		}

		public IEnumerable<CustomizationError> MergeSettingGroup(Settings otherSettingGroup, bool allowOverride)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (!Path.Equals(otherSettingGroup.Path, StringComparison.OrdinalIgnoreCase))
			{
				throw new Exception("Cannot merge two setting groups with different paths!");
			}
			IEnumerable<IGrouping<string, Setting>> source = otherSettingGroup.Items.Concat(Items).GroupBy((Setting x) => x.Name, StringComparer.OrdinalIgnoreCase);
			Func<Setting, bool> func = default(Func<Setting, bool>);
			foreach (IGrouping<string, Setting> dupe2 in source.Where((IGrouping<string, Setting> grp) => grp.Count() > 1))
			{
				CustomizationErrorSeverity severity = ((!allowOverride) ? CustomizationErrorSeverity.Error : CustomizationErrorSeverity.Warning);
				if (dupe2.All((Setting dupeSetting) => dupeSetting.Value.Equals(dupe2.First().Value)))
				{
					severity = CustomizationErrorSeverity.Warning;
				}
				CustomizationError item = new CustomizationError(severity, dupe2, Strings.DuplicateSettings, dupe2.Key, Path);
				list.Add(item);
				Items.RemoveAll((Setting x) => dupe2.Contains(x));
				List<Setting> items = Items;
				IGrouping<string, Setting> source2 = dupe2;
				Func<Setting, bool> func2 = func;
				if (func2 == null)
				{
					func2 = (func = (Setting x) => otherSettingGroup.Items.Contains(x));
				}
				items.AddRange(source2.Where(func2));
			}
			Func<Setting, bool> func3 = default(Func<Setting, bool>);
			foreach (IGrouping<string, Setting> item3 in source.Where((IGrouping<string, Setting> grp) => grp.Count() == 1))
			{
				List<Setting> items2 = Items;
				Func<Setting, bool> func4 = func3;
				if (func4 == null)
				{
					func4 = (func3 = (Setting x) => otherSettingGroup.Items.Contains(x));
				}
				items2.AddRange(item3.Where(func4));
			}
			IEnumerable<IGrouping<string, Asset>> source3 = otherSettingGroup.Assets.Concat(Assets).GroupBy((Asset x) => x.Name, StringComparer.OrdinalIgnoreCase);
			Func<Asset, bool> func5 = default(Func<Asset, bool>);
			foreach (IGrouping<string, Asset> item4 in source3.Where((IGrouping<string, Asset> grp) => grp.Count() > 1))
			{
				IEnumerable<IGrouping<string, Asset>> source4 = item4.GroupBy((Asset x) => x.Id, StringComparer.OrdinalIgnoreCase);
				foreach (IGrouping<string, Asset> dupe in source4.Where((IGrouping<string, Asset> grp) => grp.Count() > 1))
				{
					CustomizationError item2 = new CustomizationError((!allowOverride) ? CustomizationErrorSeverity.Error : CustomizationErrorSeverity.Warning, dupe, Strings.DuplicateAssets, item4.Key, Path);
					list.Add(item2);
					Assets.RemoveAll((Asset x) => dupe.Contains(x));
					Assets.AddRange(dupe.Where((Asset x) => x.DefinedInFile.Equals(dupe.First().DefinedInFile)));
				}
				foreach (IGrouping<string, Asset> item5 in source4.Where((IGrouping<string, Asset> grp) => grp.Count() == 1))
				{
					List<Asset> assets = Assets;
					Func<Asset, bool> func6 = func5;
					if (func6 == null)
					{
						func6 = (func5 = (Asset x) => otherSettingGroup.Assets.Contains(x));
					}
					assets.AddRange(item5.Where(func6));
				}
			}
			Func<Asset, bool> func7 = default(Func<Asset, bool>);
			foreach (IGrouping<string, Asset> item6 in source3.Where((IGrouping<string, Asset> grp) => grp.Count() == 1))
			{
				List<Asset> assets2 = Assets;
				Func<Asset, bool> func8 = func7;
				if (func8 == null)
				{
					func8 = (func7 = (Asset x) => otherSettingGroup.Assets.Contains(x));
				}
				assets2.AddRange(item6.Where(func8));
			}
			return list;
		}
	}
}
