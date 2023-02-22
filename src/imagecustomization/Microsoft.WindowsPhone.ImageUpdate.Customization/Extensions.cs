using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.WindowsPhone.ImageUpdate.Customization.XML;
using Microsoft.WindowsPhone.MCSF.Offline;
using Microsoft.WindowsPhone.Multivariant.Offline;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization
{
	internal static class Extensions
	{
		public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			HashSet<TKey> knownKeys = new HashSet<TKey>();
			foreach (TSource item in source)
			{
				if (knownKeys.Add(keySelector(item)))
				{
					yield return item;
				}
			}
		}

		public static TSource FirstOrNull<TSource>(this IEnumerable<TSource> source) where TSource : class
		{
			if (source.Count() <= 0)
			{
				return null;
			}
			return source.First();
		}

		public static IEnumerable<TSource> ToEnumerable<TSource>(this TSource source)
		{
			yield return source;
		}

		public static string DestinationPath(this CustomizationDataAssetType source)
		{
			switch (source)
			{
			case CustomizationDataAssetType.MapData:
				return "SharedData\\MapData";
			case CustomizationDataAssetType.RetailDemo_Microsoft:
				return "SharedData\\RetailDemo\\OfflineContent\\Microsoft";
			case CustomizationDataAssetType.RetailDemo_OEM:
				return "SharedData\\RetailDemo\\OfflineContent\\OEM";
			case CustomizationDataAssetType.RetailDemo_MO:
				return "SharedData\\RetailDemo\\OfflineContent\\MO";
			case CustomizationDataAssetType.RetailDemo_Apps:
				return "SharedData\\RetailDemo\\OfflineContent\\Apps";
			default:
				return "";
			}
		}

		public static MVSetting ToVariantSetting(this Asset asset, PolicyAssetInfo assetInfo, IEnumerable<string> provisionPath, MVSettingProvisioning provisionTime, PolicyMacroTable settingMacro)
		{
			string text = asset.GetDevicePath(assetInfo.TargetDir);
			string text2 = settingMacro.ReplaceMacros((asset.Type == CustomizationAssetOwner.OEM) ? assetInfo.OemRegKey : assetInfo.MORegKey);
			if (assetInfo.HasOEMMacros)
			{
				PolicyMacroTable policyMacroTable = new PolicyMacroTable(assetInfo.Name, asset.Name);
				text = policyMacroTable.ReplaceMacros(assetInfo.TargetDir);
				text2 = policyMacroTable.ReplaceMacros(text2);
			}
			string text3 = (assetInfo.FileNameOnly ? Path.GetFileName(text) : text);
			return new MVSetting(provisionPath.Concat(text3.ToEnumerable()), text2, text3, "REG_SZ")
			{
				Value = (asset.DisplayName ?? string.Empty),
				ProvisioningTime = provisionTime
			};
		}

		public static MVSetting ToVariantSetting(this PolicyAssetInfo assetInfo, IEnumerable<Asset> assets, IEnumerable<string> provisionPath, MVSettingProvisioning provisionTime, PolicyMacroTable settingMacro)
		{
			List<string> source;
			if (assetInfo.HasOEMMacros)
			{
				source = assets.Select((Asset x) => x.GetDevicePathWithMacros(assetInfo)).ToList();
				source = source.Select((string x) => settingMacro.ReplaceMacros(x)).ToList();
			}
			else
			{
				source = assets.Select((Asset x) => settingMacro.ReplaceMacros(x.GetDevicePath(assetInfo.TargetDir))).ToList();
			}
			if (assetInfo.HasOEMMacros)
			{
				source = source.Select((string x) => new PolicyMacroTable(assetInfo.Name, x).ReplaceMacros(x)).ToList();
			}
			return new MVSetting(provisionPath, settingMacro.ReplaceMacros(assetInfo.OemRegKey), settingMacro.ReplaceMacros(assetInfo.OemRegValue), "REG_MULTI_SZ")
			{
				Value = string.Join(new string(';', 1), source),
				ProvisioningTime = provisionTime
			};
		}
	}
}
