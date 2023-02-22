using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class SupportedCPUType
	{
		[XmlAttribute("HostCpuType")]
		public string CpuType;

		[XmlAttribute("WowGuestCpuTypes")]
		public string WowGuestCpuTypesStr;

		private List<CpuId> _wowGuestCpuIds;

		[XmlIgnore]
		public List<CpuId> WowGuestCpuIds
		{
			get
			{
				if (_wowGuestCpuIds == null)
				{
					_wowGuestCpuIds = new List<CpuId>();
					if (!string.IsNullOrEmpty(WowGuestCpuTypesStr))
					{
						string[] array = WowGuestCpuTypesStr.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
						foreach (string value in array)
						{
							_wowGuestCpuIds.Add(CpuIdParser.Parse(value));
						}
					}
				}
				return _wowGuestCpuIds;
			}
		}
	}
}
