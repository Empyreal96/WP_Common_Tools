using System.Xml.Linq;

namespace Microsoft.WindowsPhone.MCSF.Offline
{
	public class PolicyEnum
	{
		public string Value { get; set; }

		public string FriendlyName { get; set; }

		public PolicyEnum(XElement option, bool parseInteger)
		{
			if (parseInteger)
			{
				Value = Extensions.ParseInt((string)option.LocalAttribute("Value")).ToString();
			}
			else
			{
				Value = (string)option.LocalAttribute("Value");
			}
			FriendlyName = ((string)option.LocalAttribute("FriendlyName")) ?? Value;
		}

		public PolicyEnum(string friendlyName, string value)
		{
			FriendlyName = friendlyName;
			Value = value;
		}
	}
}
