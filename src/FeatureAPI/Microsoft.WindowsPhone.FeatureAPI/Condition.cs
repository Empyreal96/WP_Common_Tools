using System.ComponentModel;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class Condition
	{
		public enum ConditionType
		{
			NameValuePair,
			Registry,
			Feature
		}

		public enum ConditionOperator
		{
			GT,
			GTE,
			LT,
			LTE,
			EQ,
			NEQ,
			SET,
			NOTSET
		}

		public enum FeatureStatuses
		{
			Installed,
			NotInstalled
		}

		[XmlAttribute]
		public ConditionType Type;

		[XmlAttribute]
		[DefaultValue(ConditionOperator.EQ)]
		public ConditionOperator Operator = ConditionOperator.EQ;

		[XmlAttribute]
		[DefaultValue(FeatureStatuses.Installed)]
		public FeatureStatuses FeatureStatus;

		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public string Value { get; set; }

		[XmlAttribute]
		public string RegistryKey { get; set; }

		[XmlAttribute]
		public RegistryValueKind RegistryKeyType { get; set; }

		[XmlAttribute]
		public string FMID { get; set; }

		public Condition()
		{
		}

		public Condition(Condition srcCond)
		{
			FeatureStatus = srcCond.FeatureStatus;
			FMID = srcCond.FMID;
			Name = srcCond.Name;
			Operator = srcCond.Operator;
			RegistryKey = srcCond.RegistryKey;
			RegistryKeyType = srcCond.RegistryKeyType;
			Type = srcCond.Type;
			Value = srcCond.Value;
		}

		public void SetRegistry(string key, string name, RegistryValueKind keyType, string value)
		{
			SetRegistry(key, name, keyType, value, ConditionOperator.EQ);
		}

		public void SetRegistry(string key, string name, RegistryValueKind keyType, string value, ConditionOperator conditionOperator)
		{
			Type = ConditionType.Registry;
			Operator = conditionOperator;
			RegistryKey = key;
			Name = name;
			RegistryKeyType = keyType;
			Value = value;
		}

		public void SetNameValue(string name, string value)
		{
			SetNameValue(name, value, ConditionOperator.EQ);
		}

		public void SetNameValue(string name, string value, ConditionOperator conditionOperator)
		{
			Type = ConditionType.NameValuePair;
			Operator = conditionOperator;
			Name = name;
			Value = value;
		}

		public void SetFeature(string featureID, string FMID)
		{
			SetFeature(featureID, FMID, FeatureStatuses.Installed);
		}

		public void SetFeature(string featureID, string FMID, FeatureStatuses featureStatus)
		{
			Type = ConditionType.Feature;
			FeatureStatus = featureStatus;
			Name = featureID;
			this.FMID = FMID;
		}

		public override string ToString()
		{
			string text = Type.ToString() + ": ";
			switch (Type)
			{
			case ConditionType.Feature:
				text += Value;
				break;
			case ConditionType.NameValuePair:
				text = text + "Name=" + Name + " Value=" + Value;
				break;
			case ConditionType.Registry:
				text = text + "Path=" + RegistryKey + "\\" + Name + (string.IsNullOrEmpty(Value) ? "" : (" Value=" + Value));
				break;
			}
			return text;
		}
	}
}
