using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	public class FilterGroup : PkgElement
	{
		private SatelliteType _satteliteType;

		private RestrictionType _restrictionType;

		private HashSet<SatelliteId> _expansionValues;

		[XmlAttribute("CpuFilter")]
		public CpuId CpuFilter;

		[XmlAttribute("Language")]
		public string Language
		{
			get
			{
				return GetExpansionString(SatelliteType.Language);
			}
			set
			{
				SetExpansionString(SatelliteType.Language, value);
			}
		}

		[XmlAttribute("Resolution")]
		public string Resolution
		{
			get
			{
				return GetExpansionString(SatelliteType.Resolution);
			}
			set
			{
				SetExpansionString(SatelliteType.Resolution, value);
			}
		}

		private string GetExpansionString(SatelliteType type)
		{
			if (type == SatelliteType.Neutral)
			{
				throw new ArgumentException("SatelliteType.Neutral is not valid for parameter 'type' of FilterGroup.GetExpansionString");
			}
			if (_satteliteType != type)
			{
				return null;
			}
			if (_expansionValues.Count == 0)
			{
				return "*";
			}
			return string.Format("{0}({1})", (_restrictionType == RestrictionType.Exclude) ? "!" : string.Empty, string.Join(";", _expansionValues.Select((SatelliteId x) => x.Id)));
		}

		private void SetExpansionString(SatelliteType type, string value)
		{
			if (type == SatelliteType.Neutral)
			{
				throw new ArgumentException("SatelliteType.Neutral is not a valid value for parameter 'type' of FilterGroup.SetExpansionString");
			}
			if (_satteliteType != 0)
			{
				throw new PkgGenException("Expansion list can't be set twice");
			}
			if (value == null)
			{
				throw new PkgGenException("Empty expansion string");
			}
			_satteliteType = type;
			_expansionValues = new HashSet<SatelliteId>();
			if (value == "*")
			{
				_restrictionType = RestrictionType.Exclude;
				return;
			}
			Match match = Regex.Match(value, "^(?<name>!?)\\((?<values>.*)\\)$");
			if (!match.Success)
			{
				throw new PkgGenException("Invalid expansion string");
			}
			_satteliteType = type;
			if (match.Groups["name"].Value.Length != 0)
			{
				_restrictionType = RestrictionType.Exclude;
			}
			string[] array = match.Groups["values"].Value.Split(';');
			foreach (string text in array)
			{
				SatelliteId satelliteId = SatelliteId.Create(_satteliteType, text.Trim());
				if (_expansionValues.Contains(satelliteId))
				{
					throw new PkgGenException("Duplicate langauge/resolution identifier in expansion list: {0}", satelliteId.Id);
				}
				_expansionValues.Add(satelliteId);
			}
		}

		public bool ShouldSerializeCpuFilter()
		{
			return CpuFilter != CpuId.Invalid;
		}

		public override void Build(IPackageGenerator pkgGen)
		{
			if (CpuFilter != 0 && CpuFilter != pkgGen.CPU)
			{
				return;
			}
			if (_satteliteType != 0)
			{
				IEnumerable<SatelliteId> satelliteValues = pkgGen.GetSatelliteValues(_satteliteType);
				satelliteValues = ((_restrictionType != RestrictionType.Exclude) ? satelliteValues.Intersect(_expansionValues) : satelliteValues.Except(_expansionValues));
				{
					foreach (SatelliteId item in satelliteValues)
					{
						pkgGen.MacroResolver.BeginLocal();
						pkgGen.MacroResolver.Register(item.MacroName, item.MacroValue);
						Build(pkgGen, item);
						pkgGen.MacroResolver.EndLocal();
					}
					return;
				}
			}
			Build(pkgGen, SatelliteId.Neutral);
		}
	}
}
