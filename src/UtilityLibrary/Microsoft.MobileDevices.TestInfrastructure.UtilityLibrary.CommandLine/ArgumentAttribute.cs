using System;

namespace Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary.CommandLine
{
	[AttributeUsage(AttributeTargets.Field)]
	public class ArgumentAttribute : Attribute
	{
		private string shortName;

		private string longName;

		private string helpText;

		private object defaultValue;

		private ArgumentType type;

		public ArgumentType Type => type;

		public bool DefaultShortName => null == shortName;

		public string ShortName
		{
			get
			{
				return shortName;
			}
			set
			{
				shortName = value;
			}
		}

		public bool DefaultLongName => null == longName;

		public string LongName
		{
			get
			{
				return longName;
			}
			set
			{
				longName = value;
			}
		}

		public object DefaultValue
		{
			get
			{
				return defaultValue;
			}
			set
			{
				defaultValue = value;
			}
		}

		public bool HasDefaultValue => null != defaultValue;

		public bool HasHelpText => null != helpText;

		public string HelpText
		{
			get
			{
				return helpText;
			}
			set
			{
				helpText = value;
			}
		}

		public ArgumentAttribute(ArgumentType type)
		{
			this.type = type;
		}
	}
}
