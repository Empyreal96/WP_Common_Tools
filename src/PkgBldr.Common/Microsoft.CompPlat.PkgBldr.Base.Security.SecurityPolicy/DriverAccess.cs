using System.Globalization;

namespace Microsoft.CompPlat.PkgBldr.Base.Security.SecurityPolicy
{
	public class DriverAccess : AccessControlPolicy
	{
		private DriverAccessType m_AccessType;

		private string m_Name;

		public DriverAccess(DriverAccessType AccessType, string Name, string Access)
			: base(ResourceType.Driver)
		{
			m_AccessType = AccessType;
			m_Name = Name;
			m_Access = Access;
		}

		public override string GetUniqueAccessControlEntries()
		{
			switch (m_AccessType)
			{
			case DriverAccessType.Capability:
				return new Capability(m_Name, ResourceType.Driver, m_Access, false).GetUniqueAccessControlEntries();
			case DriverAccessType.Application:
			{
				string arg = SidBuilder.BuildApplicationSidString(m_Name);
				string text = "IU";
				return string.Format("(A;;{0};;;{1})(A;;{0};;;{2})", m_Access, arg, text, CultureInfo.InvariantCulture);
			}
			case DriverAccessType.LegacyApplication:
			{
				string arg = SidBuilder.BuildLegacyApplicationSidString(m_Name);
				string text = "IU";
				return string.Format("(A;;{0};;;{1})(A;;{0};;;{2})", m_Access, arg, text, CultureInfo.InvariantCulture);
			}
			case DriverAccessType.Service:
			{
				string arg = SidBuilder.BuildServiceSidString(m_Name);
				return string.Format("(A;;{0};;;{1})", m_Access, arg, CultureInfo.InvariantCulture);
			}
			default:
				throw new PkgGenException("Invalid driver access type");
			}
		}
	}
}
