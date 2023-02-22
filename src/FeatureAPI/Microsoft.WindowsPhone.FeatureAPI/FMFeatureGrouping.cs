using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class FMFeatureGrouping
	{
		public enum FeatureConstraints
		{
			None,
			OneOrMore,
			ZeroOrOne,
			OneAndOnlyOne
		}

		private StringComparer IgnoreCase = StringComparer.OrdinalIgnoreCase;

		[XmlAttribute("Name")]
		public string Name;

		[XmlAttribute("PublishingFeatureGroup")]
		[DefaultValue(false)]
		public bool PublishingFeatureGroup;

		private string _fmID;

		[XmlAttribute("Constraint")]
		[DefaultValue(FeatureConstraints.None)]
		public FeatureConstraints Constraint;

		[XmlAttribute("GroupingType")]
		public string GroupingType;

		[XmlArrayItem(ElementName = "FeatureID", Type = typeof(string), IsNullable = false)]
		[XmlArray]
		public List<string> FeatureIDs;

		[XmlArrayItem(ElementName = "FeatureGroup", Type = typeof(FMFeatureGrouping), IsNullable = false)]
		[XmlArray]
		public List<FMFeatureGrouping> SubGroups;

		[XmlAttribute]
		[DefaultValue(null)]
		public string FMID
		{
			get
			{
				return _fmID;
			}
			set
			{
				_fmID = value;
				if (SubGroups == null)
				{
					return;
				}
				foreach (FMFeatureGrouping subGroup in SubGroups)
				{
					subGroup.FMID = value;
				}
			}
		}

		[XmlIgnore]
		public List<string> FeatureIDWithFMIDs
		{
			get
			{
				List<string> list = new List<string>();
				foreach (string featureID in FeatureIDs)
				{
					list.Add(FeatureManifest.GetFeatureIDWithFMID(featureID, FMID));
				}
				return list;
			}
		}

		[XmlIgnore]
		public List<string> AllFeatureIDWithFMIDs
		{
			get
			{
				List<string> list = new List<string>();
				if (FeatureIDs != null)
				{
					list.AddRange(FeatureIDWithFMIDs);
				}
				if (SubGroups != null)
				{
					foreach (FMFeatureGrouping subGroup in SubGroups)
					{
						list.AddRange(subGroup.AllFeatureIDWithFMIDs);
					}
					return list;
				}
				return list;
			}
		}

		[XmlIgnore]
		public List<string> AllFeatureIDs
		{
			get
			{
				List<string> list = new List<string>();
				if (FeatureIDs != null)
				{
					list.AddRange(FeatureIDs);
				}
				if (SubGroups != null)
				{
					foreach (FMFeatureGrouping subGroup in SubGroups)
					{
						list.AddRange(subGroup.AllFeatureIDs);
					}
					return list;
				}
				return list;
			}
		}

		public bool ValidateConstraints(IEnumerable<string> FeatureIDs, out string errorMessage)
		{
			bool result = true;
			StringBuilder stringBuilder = new StringBuilder();
			List<string> list = FeatureIDs.Intersect(AllFeatureIDs, IgnoreCase).ToList();
			int num = list.Count();
			switch (Constraint)
			{
			case FeatureConstraints.OneAndOnlyOne:
				if (num == 1)
				{
					break;
				}
				result = false;
				if (num == 0)
				{
					stringBuilder.AppendLine("One of the following features must be selected:");
					foreach (string allFeatureID in AllFeatureIDs)
					{
						stringBuilder.AppendFormat("\t{0}\n", GetFixedFeatureConstraintErrorStr(allFeatureID));
					}
					break;
				}
				stringBuilder.AppendLine("Only one of the following features may be selected:");
				foreach (string item in list)
				{
					stringBuilder.AppendFormat("\t{0}\n", GetFixedFeatureConstraintErrorStr(item));
				}
				break;
			case FeatureConstraints.OneOrMore:
				if (num != 0)
				{
					break;
				}
				result = false;
				stringBuilder.AppendLine("One or more of the following features must be selected:");
				foreach (string allFeatureID2 in AllFeatureIDs)
				{
					stringBuilder.AppendFormat("\t{0}\n", GetFixedFeatureConstraintErrorStr(allFeatureID2));
				}
				break;
			case FeatureConstraints.ZeroOrOne:
				if (num <= 1)
				{
					break;
				}
				result = false;
				stringBuilder.AppendLine("Only one (or none) of the following features may be selected:");
				foreach (string item2 in list)
				{
					stringBuilder.AppendFormat("\t{0}\n", GetFixedFeatureConstraintErrorStr(item2));
				}
				break;
			}
			errorMessage = stringBuilder.ToString();
			return result;
		}

		private string GetFixedFeatureConstraintErrorStr(string feature)
		{
			string text = null;
			string text2 = " (under Features\\Microsoft)";
			string text3 = " (under Features\\OEM)";
			if (feature.StartsWith("MS_", StringComparison.OrdinalIgnoreCase))
			{
				return feature.Replace("MS_", "", StringComparison.OrdinalIgnoreCase) + text2;
			}
			if (feature.StartsWith("OEM_", StringComparison.OrdinalIgnoreCase))
			{
				return feature.Replace("OEM_", "", StringComparison.OrdinalIgnoreCase) + text3;
			}
			return feature;
		}

		public override string ToString()
		{
			string text = "";
			if (!string.IsNullOrEmpty(Name))
			{
				text = Name + " ";
			}
			return string.Concat(text, "(Constraint= ", Constraint, ")");
		}
	}
}
