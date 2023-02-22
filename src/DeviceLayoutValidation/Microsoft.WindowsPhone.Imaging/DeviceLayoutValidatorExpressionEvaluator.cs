using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public static class DeviceLayoutValidatorExpressionEvaluator
	{
		private struct ExpressionSyntax
		{
			public Regex RegularExpression;

			public Func<string, string[], bool> Evaluator;
		}

		private static OEMInput _oemInput;

		private static IULogger _logger;

		private static Dictionary<string, string> _oemInputFields = new Dictionary<string, string>();

		private static Func<string, string[], bool, Func<uint, uint, uint, bool>, bool> UintValueEvaluator = delegate(string compareValue, string[] parameters, bool compareAll, Func<uint, uint, uint, bool> comparator)
		{
			uint Value;
			if (DeviceLayoutValidator.StringToUint(compareValue, out Value))
			{
				uint num2 = 0u;
				for (int j = 0; j < parameters.Length; j++)
				{
					string text2 = parameters[j].Trim();
					string fieldValue2;
					if (!GetOemInputField(text2, out fieldValue2))
					{
						fieldValue2 = text2;
					}
					uint Value2;
					if (!DeviceLayoutValidator.StringToUint(fieldValue2, out Value2))
					{
						return false;
					}
					if (comparator(Value, Value2, num2))
					{
						if (!compareAll)
						{
							return true;
						}
					}
					else if (compareAll)
					{
						return false;
					}
					num2++;
				}
				return compareAll;
			}
			return false;
		};

		private static Func<string, string[], bool, Func<string, string, uint, bool>, bool> StringValueEvaluator = delegate(string compareValue, string[] parameters, bool compareAll, Func<string, string, uint, bool> comparator)
		{
			uint num = 0u;
			for (int i = 0; i < parameters.Length; i++)
			{
				string text = parameters[i].Trim();
				string fieldValue;
				if (!GetOemInputField(text, out fieldValue))
				{
					fieldValue = text;
				}
				if (comparator(compareValue, fieldValue, num))
				{
					if (!compareAll)
					{
						return true;
					}
				}
				else if (compareAll)
				{
					return false;
				}
				num++;
			}
			return compareAll;
		};

		private static ExpressionSyntax[] _implementedSyntax = new ExpressionSyntax[7]
		{
			new ExpressionSyntax
			{
				RegularExpression = new Regex("^equal\\((.+)\\)$", RegexOptions.IgnoreCase),
				Evaluator = delegate(string stringValue, string[] parameters)
				{
					if (UintValueEvaluator(stringValue, parameters, true, (uint uintValue, uint uintParam, uint index) => uintValue == uintParam))
					{
						return true;
					}
					return StringValueEvaluator(stringValue, parameters, true, (string strValue, string strParam, uint index) => strValue.Equals(strParam, StringComparison.OrdinalIgnoreCase)) ? true : false;
				}
			},
			new ExpressionSyntax
			{
				RegularExpression = new Regex("^greater\\((.+)\\)$", RegexOptions.IgnoreCase),
				Evaluator = (string stringValue, string[] parameters) => UintValueEvaluator(stringValue, parameters, true, (uint uintValue, uint uintParam, uint index) => uintValue > uintParam)
			},
			new ExpressionSyntax
			{
				RegularExpression = new Regex("^less\\((.+)\\)$", RegexOptions.IgnoreCase),
				Evaluator = (string stringValue, string[] parameters) => UintValueEvaluator(stringValue, parameters, true, (uint uintValue, uint uintParam, uint index) => uintValue < uintParam)
			},
			new ExpressionSyntax
			{
				RegularExpression = new Regex("^greater_or_equal\\((.+)\\)$", RegexOptions.IgnoreCase),
				Evaluator = (string stringValue, string[] parameters) => UintValueEvaluator(stringValue, parameters, true, (uint uintValue, uint uintParam, uint index) => uintValue >= uintParam)
			},
			new ExpressionSyntax
			{
				RegularExpression = new Regex("^less_or_equal\\((.+)\\)$", RegexOptions.IgnoreCase),
				Evaluator = (string stringValue, string[] parameters) => UintValueEvaluator(stringValue, parameters, true, (uint uintValue, uint uintParam, uint index) => uintValue <= uintParam)
			},
			new ExpressionSyntax
			{
				RegularExpression = new Regex("^between\\((.+)\\)$", RegexOptions.IgnoreCase),
				Evaluator = (string stringValue, string[] parameters) => UintValueEvaluator(stringValue, parameters, true, delegate(uint uintValue, uint uintParam, uint index)
				{
					switch (index)
					{
					case 0u:
						return uintValue >= uintParam;
					case 1u:
						return uintValue <= uintParam;
					default:
						return false;
					}
				})
			},
			new ExpressionSyntax
			{
				RegularExpression = new Regex("^one_of\\((.+)\\)$", RegexOptions.IgnoreCase),
				Evaluator = delegate(string stringValue, string[] parameters)
				{
					if (UintValueEvaluator(stringValue, parameters, false, (uint uintValue, uint uintParam, uint index) => uintValue == uintParam))
					{
						return true;
					}
					return StringValueEvaluator(stringValue, parameters, false, (string strValue, string strParam, uint index) => strValue.Equals(strParam, StringComparison.OrdinalIgnoreCase)) ? true : false;
				}
			}
		};

		private static bool GetOemInputField(string fieldName, out string fieldValue)
		{
			return _oemInputFields.TryGetValue(fieldName.ToLower(), out fieldValue);
		}

		public static void Initialize(OEMInput OemInput, IULogger Logger)
		{
			_oemInput = OemInput;
			_logger = Logger;
			_oemInputFields["oeminput.edition"] = _oemInput.Edition.ToString();
			_oemInputFields["oeminput.product"] = _oemInput.Product;
			_oemInputFields["oeminput.ismmos"] = _oemInput.IsMMOS.ToString();
			_oemInputFields["oeminput.description"] = _oemInput.Description;
			_oemInputFields["oeminput.soc"] = _oemInput.SOC;
			_oemInputFields["oeminput.sv"] = _oemInput.SV;
			_oemInputFields["oeminput.device"] = _oemInput.Device;
			_oemInputFields["oeminput.releasetype"] = _oemInput.ReleaseType;
			_oemInputFields["oeminput.buildtype"] = _oemInput.BuildType;
			_oemInputFields["oeminput.formatdpp"] = _oemInput.FormatDPP;
			_oemInputFields["oeminput.cputype"] = _oemInput.CPUType;
		}

		public static bool EvaluateBooleanExpression(string StringValue, string Expression)
		{
			bool flag = false;
			ExpressionSyntax[] implementedSyntax = _implementedSyntax;
			for (int i = 0; i < implementedSyntax.Length; i++)
			{
				ExpressionSyntax expressionSyntax = implementedSyntax[i];
				Match match = expressionSyntax.RegularExpression.Match(Expression);
				if (match.Success)
				{
					flag = expressionSyntax.Evaluator(StringValue, match.Groups[1].Value.Split(',', ':'));
					if (flag)
					{
						_logger.LogInfo("DeviceLayoutValidation: EvaluateBooleanExpression SUCCEEDED '" + StringValue + "->" + Expression);
					}
					else
					{
						_logger.LogError("DeviceLayoutValidation: EvaluateBooleanExpression FAILED '" + StringValue + "->" + Expression);
					}
				}
			}
			return flag;
		}
	}
}
