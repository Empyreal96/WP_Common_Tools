using System;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public sealed class RegValueInfo
	{
		public RegValueType Type;

		public string KeyName;

		public string ValueName;

		public string Value;

		public string Partition;

		public RegValueInfo()
		{
		}

		public RegValueInfo(RegValueInfo regValueInfo)
		{
			if (regValueInfo == null)
			{
				throw new ArgumentNullException("regValueInfo");
			}
			Type = regValueInfo.Type;
			KeyName = regValueInfo.KeyName;
			ValueName = regValueInfo.ValueName;
			Value = regValueInfo.Value;
			Partition = regValueInfo.Partition;
		}

		public void SetRegValueType(string strType)
		{
			Type = RegUtil.RegValueTypeForString(strType);
		}
	}
}
