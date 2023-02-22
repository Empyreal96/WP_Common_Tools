using System.Collections.Generic;
using Microsoft.CompPlat.PkgBldr.Base.Tools;
using Microsoft.CompPlat.PkgBldr.Tools;

namespace Microsoft.CompPlat.PkgBldr.Base
{
	public class Build
	{
		public enum WowType
		{
			host,
			guest
		}

		private List<WowType> m_wowTypeList = new List<WowType>();

		private string m_wowdir;

		private WowType m_wow;

		private SatelliteId m_satellite;

		public string WowDir
		{
			get
			{
				return m_wowdir;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					m_wowdir = null;
					return;
				}
				m_wowdir = LongPath.GetFullPath(value.TrimEnd('\\'));
				if (!LongPathDirectory.Exists(m_wowdir))
				{
					LongPathDirectory.CreateDirectory(m_wowdir);
				}
			}
		}

		public WowType wow
		{
			get
			{
				return m_wow;
			}
			set
			{
				m_wow = value;
			}
		}

		public WowBuildType? WowBuilds { get; set; }

		public SatelliteId satellite
		{
			get
			{
				return m_satellite;
			}
			set
			{
				m_satellite = value;
			}
		}

		public Build()
		{
			m_wowTypeList.Add(WowType.host);
		}

		public List<WowType> GetWowTypes()
		{
			return m_wowTypeList;
		}

		public void AddGuest()
		{
			if (!m_wowTypeList.Contains(WowType.guest))
			{
				m_wowTypeList.Add(WowType.guest);
			}
		}

		public bool BuildGuest()
		{
			if (m_wowTypeList.Contains(WowType.guest))
			{
				return true;
			}
			return false;
		}
	}
}
