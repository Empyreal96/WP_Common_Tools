using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class FMConditionalFeature : FeatureCondition
	{
		[XmlAttribute]
		[DefaultValue(null)]
		public string FeatureID;

		[XmlAttribute]
		[DefaultValue(null)]
		public string FMID;

		[XmlIgnore]
		public string FeatureIDWithFMID
		{
			get
			{
				string text = FeatureID;
				if (!string.IsNullOrEmpty(FMID))
				{
					text = text + "." + FMID;
				}
				return text;
			}
		}

		public FMConditionalFeature()
		{
		}

		public FMConditionalFeature(FMConditionalFeature srcFeature)
		{
			if (srcFeature.Condition != null)
			{
				base.Condition = new Condition(srcFeature.Condition);
			}
			if (srcFeature.ConditionSet != null)
			{
				base.ConditionSet = new ConditionSet(srcFeature.ConditionSet);
			}
			FeatureID = srcFeature.FeatureID;
			FMID = srcFeature.FMID;
			UpdateAction = srcFeature.UpdateAction;
		}

		public List<Condition> GetAllConditions()
		{
			List<Condition> list = new List<Condition>();
			if (base.ConditionSet != null && base.ConditionSet.ConditionSets != null)
			{
				foreach (ConditionSet conditionSet in base.ConditionSet.ConditionSets)
				{
					list.AddRange(conditionSet.GetAllConditions());
				}
			}
			if (base.ConditionSet != null && base.ConditionSet.Conditions != null)
			{
				list.AddRange(base.ConditionSet.Conditions);
			}
			return list;
		}

		public override string ToString()
		{
			return FeatureIDWithFMID;
		}
	}
}
