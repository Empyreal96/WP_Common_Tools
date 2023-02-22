using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class ConditionSet
	{
		public enum ConditionSetOperator
		{
			AND,
			OR
		}

		[XmlAttribute]
		[DefaultValue(ConditionSetOperator.AND)]
		public ConditionSetOperator Operator;

		[XmlArrayItem(ElementName = "Condition", Type = typeof(Condition), IsNullable = false)]
		public List<Condition> Conditions;

		[XmlArrayItem(ElementName = "ConditionSet", Type = typeof(ConditionSet), IsNullable = false)]
		public List<ConditionSet> ConditionSets;

		public bool ShouldSerializeConditions()
		{
			if (Conditions != null)
			{
				return Conditions.Count() > 0;
			}
			return false;
		}

		public bool ShouldSerializeConditionSets()
		{
			if (ConditionSets != null)
			{
				return ConditionSets.Count() > 0;
			}
			return false;
		}

		public ConditionSet()
		{
		}

		public ConditionSet(ConditionSet srcCS)
		{
			if (srcCS.Conditions != null)
			{
				Conditions = srcCS.Conditions.Select((Condition cs) => new Condition(cs)).ToList();
			}
			if (srcCS.ConditionSets != null)
			{
				ConditionSets = srcCS.ConditionSets.Select((ConditionSet cs) => new ConditionSet(cs)).ToList();
			}
			Operator = srcCS.Operator;
		}

		public ConditionSet(ConditionSetOperator conditionSetOperator)
		{
			Operator = conditionSetOperator;
			Conditions = new List<Condition>();
			ConditionSets = new List<ConditionSet>();
		}

		public ConditionSet(ConditionSetOperator conditionSetOperator, List<Condition> conditionList, List<ConditionSet> conditionSetList)
		{
			Operator = conditionSetOperator;
			Conditions = conditionList;
			ConditionSets = conditionSetList;
		}

		public List<Condition> GetAllConditions()
		{
			List<Condition> list = new List<Condition>();
			if (ConditionSets != null)
			{
				foreach (ConditionSet conditionSet in ConditionSets)
				{
					list.AddRange(conditionSet.GetAllConditions());
				}
			}
			if (Conditions != null)
			{
				list.AddRange(Conditions);
			}
			return list;
		}

		public override string ToString()
		{
			return Operator.ToString() + ": Conditions=Count (" + GetAllConditions().Count() + ")";
		}
	}
}
