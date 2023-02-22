using System.Globalization;

namespace Microsoft.CompPlat.PkgBldr.Base.Security.SecurityPolicy
{
	public class PrivateResource : AccessControlPolicy
	{
		private string m_ResourceClaimerTrustee;

		private string m_ResourceClaimerAdditionalTrustee;

		public PrivateResource(ResourceType Type, string ResourceClaimer, PrivateResourceClaimerType ResourceClaimerType, bool ReadOnly)
			: base(Type)
		{
			Init(Type, ResourceClaimer, ResourceClaimerType, ReadOnly);
		}

		public PrivateResource(ResourceType Type, string ResourceClaimer, PrivateResourceClaimerType ResourceClaimerType, bool ReadOnly, bool ProtectToUser)
			: base(Type)
		{
			Init(Type, ResourceClaimer, ResourceClaimerType, ReadOnly);
			if (ProtectToUser)
			{
				switch (Type)
				{
				case ResourceType.File:
				case ResourceType.Directory:
				case ResourceType.Registry:
					m_ResourceClaimerTrustee = null;
					break;
				case ResourceType.TransientObject:
					m_ResourceClaimerTrustee = "%s";
					break;
				case ResourceType.ComLaunch:
				case ResourceType.ComAccess:
				case ResourceType.WinRt:
					m_ResourceClaimerTrustee = "PS";
					break;
				default:
					throw new PkgGenException("Invalid resource type for user protection");
				}
			}
			else if (ResourceClaimerType == PrivateResourceClaimerType.Task)
			{
				m_ResourceClaimerTrustee = "IU";
			}
			else
			{
				m_ResourceClaimerTrustee = SidBuilder.BuildServiceSidString(ResourceClaimer);
			}
		}

		private void Init(ResourceType Type, string ResourceClaimer, PrivateResourceClaimerType ResourceClaimerType, bool ReadOnly)
		{
			switch (Type)
			{
			case ResourceType.ServiceAccess:
				m_Access = (ReadOnly ? "GR" : "CCLCSWRPLO");
				break;
			case ResourceType.ComAccess:
				m_Access = "CCDC";
				break;
			case ResourceType.ComLaunch:
				m_Access = "CCDCSW";
				break;
			default:
				m_Access = (ReadOnly ? "GR" : "0x111FFFFF");
				break;
			}
			if (ResourceClaimerType == PrivateResourceClaimerType.Task)
			{
				m_ResourceClaimerAdditionalTrustee = SidBuilder.BuildTaskSidString(ResourceClaimer);
			}
			else
			{
				m_ResourceClaimerAdditionalTrustee = null;
			}
		}

		public override string GetUniqueAccessControlEntries()
		{
			string result = null;
			if (m_ResourceClaimerTrustee != null)
			{
				result = ((m_ResourceClaimerAdditionalTrustee == null) ? string.Format("(A;{0};{1};;;{2})", m_InheritanceFlags, m_Access, m_ResourceClaimerTrustee, CultureInfo.InvariantCulture) : string.Format("(A;{0};{1};;;{2})(A;{0};{1};;;{3})", m_InheritanceFlags, m_Access, m_ResourceClaimerTrustee, m_ResourceClaimerAdditionalTrustee, CultureInfo.InvariantCulture));
			}
			else if (m_ResourceClaimerAdditionalTrustee != null)
			{
				result = string.Format("(A;{0};{1};;;{2})", m_InheritanceFlags, m_Access, m_ResourceClaimerAdditionalTrustee, CultureInfo.InvariantCulture);
			}
			return result;
		}

		public override string GetDefaultDacl()
		{
			return "D:P" + m_DefaultDacl;
		}
	}
}
