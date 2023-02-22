using System.ComponentModel;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class FeatureCondition
	{
		public enum Action
		{
			Install,
			Remove,
			NoUpdate
		}

		[XmlAttribute]
		[DefaultValue(Action.Install)]
		public Action UpdateAction;

		[XmlElement]
		public ConditionSet ConditionSet { get; set; }

		[XmlElement]
		public Condition Condition { get; set; }

		public FeatureCondition()
		{
		}

		public FeatureCondition(ConditionSet conditionSet)
		{
			ConditionSet = new ConditionSet(conditionSet);
			Condition = null;
		}

		public FeatureCondition(Condition condition)
		{
			Condition = new Condition(condition);
			ConditionSet = null;
		}

		public FeatureCondition(FeatureCondition srcFC)
		{
			if (srcFC.Condition != null)
			{
				Condition = new Condition(srcFC.Condition);
			}
			if (srcFC.ConditionSet != null)
			{
				ConditionSet = new ConditionSet(srcFC.ConditionSet);
			}
			UpdateAction = srcFC.UpdateAction;
		}

		public override string ToString()
		{
			if (UpdateAction.ToString() + ": " + ConditionSet != null)
			{
				return "ConditionSet=(" + ConditionSet.ToString() + ")";
			}
			return "Condition=" + Condition.ToString();
		}
	}
}
