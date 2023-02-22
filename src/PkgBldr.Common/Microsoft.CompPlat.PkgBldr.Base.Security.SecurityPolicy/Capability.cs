using System.Globalization;

namespace Microsoft.CompPlat.PkgBldr.Base.Security.SecurityPolicy
{
	public class Capability : AccessControlPolicy
	{
		private string m_CapabilityId;

		private string m_ApplicationCapabilityGroupTrustee;

		private bool m_AdminOnMultiSession;

		public Capability(string CapabilityId, ResourceType Type, string Access, bool AdminOnMultiSession)
			: base(Type)
		{
			Init(CapabilityId, Type, Access, AdminOnMultiSession);
		}

		public Capability(string CapabilityId, ResourceType Type, string Access, bool AdminOnMultiSession, bool ProtectToUser)
			: base(Type)
		{
			Init(CapabilityId, Type, Access, AdminOnMultiSession);
			if (ProtectToUser)
			{
				switch (Type)
				{
				case ResourceType.File:
				case ResourceType.Directory:
				case ResourceType.Registry:
					m_ApplicationCapabilityGroupTrustee = null;
					break;
				case ResourceType.TransientObject:
					m_ApplicationCapabilityGroupTrustee = "%s";
					break;
				case ResourceType.ComLaunch:
				case ResourceType.ComAccess:
				case ResourceType.WinRt:
					m_ApplicationCapabilityGroupTrustee = "PS";
					break;
				default:
					throw new PkgGenException("Invalid resource type for user protection");
				}
			}
			else
			{
				m_ApplicationCapabilityGroupTrustee = "IU";
			}
		}

		private void Init(string CapabilityId, ResourceType Type, string Access, bool AdminOnMultiSession)
		{
			m_CapabilityId = CapabilityId;
			m_AdminOnMultiSession = AdminOnMultiSession;
			m_Access = Access;
		}

		public override string GetUniqueAccessControlEntries()
		{
			string text;
			string text2;
			if (m_CapabilityId == "everyone")
			{
				text = "S-1-15-2-1";
				text2 = "AU";
			}
			else
			{
				text = SidBuilder.BuildApplicationCapabilitySidString(m_CapabilityId);
				text2 = SidBuilder.BuildServiceCapabilitySidString(m_CapabilityId);
			}
			if (m_AdminOnMultiSession)
			{
				if (m_ApplicationCapabilityGroupTrustee != null)
				{
					return string.Format("(XA;{0};{1};;;{2};(!(WIN://ISMULTISESSIONSKU)))(XA;{0};{1};;;{3};(!(WIN://ISMULTISESSIONSKU)))(XA;{0};{1};;;{4};(!(WIN://ISMULTISESSIONSKU)))", m_InheritanceFlags, m_Access, text, m_ApplicationCapabilityGroupTrustee, text2, CultureInfo.InvariantCulture);
				}
				return string.Format("(XA;{0};{1};;;{2};(!(WIN://ISMULTISESSIONSKU)))(XA;{0};{1};;;{3};(!(WIN://ISMULTISESSIONSKU)))", m_InheritanceFlags, m_Access, text, text2, CultureInfo.InvariantCulture);
			}
			if (m_ApplicationCapabilityGroupTrustee != null)
			{
				return string.Format("(A;{0};{1};;;{2})(A;{0};{1};;;{3})(A;{0};{1};;;{4})", m_InheritanceFlags, m_Access, text, m_ApplicationCapabilityGroupTrustee, text2, CultureInfo.InvariantCulture);
			}
			return string.Format("(A;{0};{1};;;{2})(A;{0};{1};;;{3})", m_InheritanceFlags, m_Access, text, text2, CultureInfo.InvariantCulture);
		}
	}
}
