using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization.XML
{
	[XmlRoot(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class Variant : IDefinedIn
	{
		[XmlArray(ElementName = "TargetRefs")]
		[XmlArrayItem(ElementName = "TargetRef", Type = typeof(TargetRef), IsNullable = false)]
		private List<TargetRef> _targetRefs = new List<TargetRef>();

		[XmlIgnore]
		public string DefinedInFile { get; set; }

		[XmlAttribute]
		public virtual string Name { get; set; }

		public virtual List<TargetRef> TargetRefs
		{
			get
			{
				return _targetRefs;
			}
			set
			{
				_targetRefs = value;
			}
		}

		[XmlElement(ElementName = "Settings")]
		public List<Settings> SettingGroups { get; set; }

		[XmlElement(ElementName = "Applications")]
		public List<Applications> ApplicationGroups { get; set; }

		public Variant()
		{
			ApplicationGroups = new List<Applications>();
			SettingGroups = new List<Settings>();
		}

		public bool ShouldSerializeTargetRefs()
		{
			return TargetRefs.Count > 0;
		}

		public virtual bool ShouldSerializeName()
		{
			return true;
		}

		public virtual void LinkToFile(IDefinedIn file)
		{
			if (file == null)
			{
				throw new ArgumentNullException("file");
			}
			DefinedInFile = file.DefinedInFile;
			foreach (TargetRef targetRef in TargetRefs)
			{
				targetRef.DefinedInFile = file.DefinedInFile;
			}
			foreach (Application item in ApplicationGroups.SelectMany((Applications x) => x.Items))
			{
				((IDefinedIn)item).DefinedInFile = file.DefinedInFile;
			}
			foreach (Settings settingGroup in SettingGroups)
			{
				settingGroup.DefinedInFile = file.DefinedInFile;
			}
		}

		public IEnumerable<CustomizationError> Merge(Variant otherVariant, bool allowOverride)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if (otherVariant == null)
			{
				throw new ArgumentNullException("otherVariant");
			}
			IEnumerable<CustomizationError> collection = MergeApplicationGroups(otherVariant.ApplicationGroups, allowOverride);
			list.AddRange(collection);
			collection = MergeSettingGroups(otherVariant.SettingGroups, allowOverride);
			list.AddRange(collection);
			if (otherVariant.TargetRefs.Count != TargetRefs.Count)
			{
				List<IDefinedIn> filesInvolved = new List<IDefinedIn> { this, otherVariant };
				CustomizationError item = new CustomizationError(CustomizationErrorSeverity.Error, filesInvolved, Strings.MismatchedTargets, Name, otherVariant.Name);
				list.Add(item);
			}
			IEnumerable<TargetRef> enumerable = from x in TargetRefs.Concat(otherVariant.TargetRefs)
				group x by x.Id into x
				where x.Count() == 1
				select x.First();
			if (enumerable.Count() > 0)
			{
				CustomizationError item2 = new CustomizationError(CustomizationErrorSeverity.Error, enumerable, Strings.MismatchedTargets, Name, otherVariant.Name);
				list.Add(item2);
			}
			if (allowOverride)
			{
				Name = otherVariant.Name;
			}
			return list;
		}

		private IEnumerable<CustomizationError> MergeApplicationGroups(IEnumerable<Applications> otherApps, bool allowOverride)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			IEnumerable<Application> source = otherApps.Concat(ApplicationGroups).SelectMany((Applications x) => x.Items);
			foreach (IGrouping<string, Application> item2 in from x in source
				group x by x.ExpandedSourcePath into grp
				where grp.Count() > 1
				select grp)
			{
				CustomizationError item = new CustomizationError((!allowOverride) ? CustomizationErrorSeverity.Error : CustomizationErrorSeverity.Warning, item2, Strings.DuplicateApplications, item2.Key);
				list.Add(item);
			}
			Applications applications = new Applications();
			applications.Items = source.DistinctBy((Application x) => x.Source).ToList();
			ApplicationGroups = new List<Applications> { applications };
			return list;
		}

		private IEnumerable<CustomizationError> MergeSettingGroups(List<Settings> otherSettings, bool allowOverride)
		{
			List<CustomizationError> list = new List<CustomizationError>();
			if ((from x in otherSettings
				group x by x.Path).Any((IGrouping<string, Settings> grp) => grp.Count() > 1))
			{
				otherSettings = CollapseSettingGroup(otherSettings);
			}
			if ((from x in SettingGroups
				group x by x.Path).Any((IGrouping<string, Settings> grp) => grp.Count() > 1))
			{
				SettingGroups = CollapseSettingGroup(SettingGroups);
			}
			foreach (Settings otherSetting in otherSettings)
			{
				string settingPath = otherSetting.Path;
				Settings settings = SettingGroups.SingleOrDefault((Settings x) => x.Path.Equals(settingPath, StringComparison.OrdinalIgnoreCase));
				if (settings == null)
				{
					SettingGroups.Add(otherSetting);
					continue;
				}
				IEnumerable<CustomizationError> collection = settings.MergeSettingGroup(otherSetting, allowOverride);
				list.AddRange(collection);
			}
			return list;
		}

		private List<Settings> CollapseSettingGroup(List<Settings> settingGroup)
		{
			List<Settings> list = new List<Settings>();
			foreach (IGrouping<string, Settings> item in from x in settingGroup
				group x by x.Path into grp
				where grp.Count() > 1
				select grp)
			{
				IEnumerable<Setting> collection = item.SelectMany((Settings x) => x.Items);
				IEnumerable<Asset> collection2 = item.SelectMany((Settings x) => x.Assets);
				Settings settings = new Settings();
				settings.DefinedInFile = item.First().DefinedInFile;
				settings.Path = item.First().Path;
				settings.Items.AddRange(collection);
				settings.Assets.AddRange(collection2);
				list.Add(settings);
			}
			list.AddRange(from x in settingGroup
				group x by x.Path into grp
				where grp.Count() == 1
				select grp into x
				select x.First());
			return list;
		}
	}
}
