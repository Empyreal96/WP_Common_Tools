using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.CompDB
{
	public class DeviceConditionAnswers : ConditionSet
	{
		private IULogger _iuLogger;

		public DeviceConditionAnswers()
		{
		}

		public DeviceConditionAnswers(IULogger logger)
		{
			_iuLogger = logger;
		}

		public DeviceConditionAnswers(DeviceConditionAnswers srcCA)
			: base(srcCA)
		{
		}

		public void PopulateConditionAnswers(List<FMConditionalFeature> condFeatures, List<Hashtable> registryTable)
		{
			List<Condition> list = new List<Condition>();
			foreach (Condition item in condFeatures.SelectMany((FMConditionalFeature cf) => cf.GetAllConditions()))
			{
				switch (item.Type)
				{
				case Condition.ConditionType.Registry:
				{
					Condition nameValuePairAnswer = GetRegistryAnswer(item, registryTable);
					if (nameValuePairAnswer != null)
					{
						list.Add(nameValuePairAnswer);
					}
					break;
				}
				case Condition.ConditionType.NameValuePair:
				{
					Condition nameValuePairAnswer = GetNameValuePairAnswer(item);
					if (nameValuePairAnswer != null)
					{
						list.Add(nameValuePairAnswer);
					}
					break;
				}
				}
			}
			if (list.Any())
			{
				if (Conditions == null)
				{
					Conditions = new List<Condition>(list);
				}
				else
				{
					Conditions.AddRange(list);
				}
			}
		}

		private Condition GetRegistryAnswer(Condition cond, List<Hashtable> registryTable)
		{
			string registryAnswer = GetRegistryAnswer(cond.RegistryKey, cond.Name, registryTable);
			if (!string.IsNullOrEmpty(registryAnswer))
			{
				Condition condition = new Condition();
				condition.SetRegistry(cond.RegistryKey, cond.Name, cond.RegistryKeyType, registryAnswer);
				return condition;
			}
			return null;
		}

		public static string GetRegistryAnswer(string registryKey, string name, List<Hashtable> registryTables)
		{
			string result = null;
			foreach (Hashtable registryTable in registryTables)
			{
				Hashtable hashtable = registryTable[registryKey] as Hashtable;
				if (hashtable != null)
				{
					object obj = hashtable[name];
					if (obj != null)
					{
						return obj.ToString();
					}
				}
			}
			return result;
		}

		private Condition GetNameValuePairAnswer(Condition cond)
		{
			throw new NotImplementedException("ImageCommon::DeviceCompDB!GetNameValuePairAnswer:  Not yet implemented.");
		}

		public override string ToString()
		{
			return "Device DB Condition Answers: Count=" + GetAllConditions().Count();
		}
	}
}
