using System;

namespace Microsoft.CompPlat.PkgBldr.Base.Security.SecurityPolicy
{
	public abstract class AccessControlPolicy
	{
		protected string m_InheritanceFlags;

		protected string m_Owner;

		protected string m_Group;

		protected string m_DefaultDacl;

		protected string m_DefaultSacl;

		protected string m_Access;

		public AccessControlPolicy(ResourceType Type)
		{
			switch (Type)
			{
			case ResourceType.File:
				m_Owner = "O:S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464";
				m_Group = "G:S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464";
				m_DefaultDacl = "(A;;0x111FFFFF;;;SY)(A;;0x111FFFFF;;;BA)";
				break;
			case ResourceType.Directory:
				m_InheritanceFlags = "CIOI";
				m_Owner = "O:S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464";
				m_Group = "G:S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464";
				m_DefaultDacl = "(A;CIOI;0x111FFFFF;;;CO)(A;CIOI;0x111FFFFF;;;SY)(A;CIOI;0x111FFFFF;;;BA)";
				break;
			case ResourceType.Registry:
				m_InheritanceFlags = "CI";
				m_Owner = "O:S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464";
				m_Group = "G:S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464";
				m_DefaultDacl = "(A;CI;0x111FFFFF;;;CO)(A;CI;0x111FFFFF;;;SY)(A;CI;0x111FFFFF;;;BA)";
				break;
			case ResourceType.TransientObject:
				m_DefaultDacl = "(A;;0x111FFFFF;;;CO)(A;;0x111FFFFF;;;SY)(A;;0x111FFFFF;;;BA)";
				break;
			case ResourceType.ServiceAccess:
				m_Owner = "O:SY";
				m_Group = "G:SY";
				m_DefaultDacl = "(A;;GRCR;;;IU)(A;;GRCR;;;SU)(A;;0x111FFFFF;;;SY)(A;;0x111FFFFF;;;BA)";
				break;
			case ResourceType.ComLaunch:
				m_Owner = "O:SY";
				m_Group = "G:SY";
				m_DefaultDacl = "(A;;0x111FFFFF;;;SY)(A;;0x111FFFFF;;;BA)";
				m_DefaultSacl = "(ML;;NX;;;LW)";
				break;
			case ResourceType.ComAccess:
				m_Owner = "O:SY";
				m_Group = "G:SY";
				m_DefaultDacl = "(A;;0x111FFFFF;;;SY)(A;;0x111FFFFF;;;BA)";
				m_DefaultSacl = "(ML;;NX;;;LW)";
				break;
			case ResourceType.WinRt:
				m_Owner = "O:SY";
				m_Group = "G:SY";
				m_DefaultDacl = "(A;;0x111FFFFF;;;SY)(A;;0x111FFFFF;;;BA)";
				m_DefaultSacl = "(ML;;NX;;;LW)";
				break;
			case ResourceType.EtwProvider:
				m_Owner = "O:SY";
				m_Group = "G:SY";
				m_DefaultDacl = "(A;;0x111FFFFF;;;SY)(A;;0x111FFFFF;;;BA)";
				break;
			case ResourceType.Wnf:
				m_DefaultDacl = "(A;;0x111FFFFF;;;SY)(A;;0x111FFFFF;;;BA)";
				break;
			case ResourceType.SdReg:
				m_Owner = "O:SY";
				m_Group = "G:SY";
				m_DefaultDacl = "(A;;0x111FFFFF;;;SY)(A;;0x111FFFFF;;;BA)";
				break;
			case ResourceType.Driver:
				m_DefaultDacl = "P(A;;GA;;;SY)";
				break;
			default:
				throw new PkgGenException("Invalid resource type. Failed to intialize policy object");
			}
		}

		public abstract string GetUniqueAccessControlEntries();

		public virtual string GetDefaultDacl()
		{
			return "D:" + m_DefaultDacl;
		}

		public virtual string GetDefaultSacl()
		{
			if (m_DefaultSacl != null)
			{
				return "S:" + m_DefaultSacl;
			}
			return null;
		}

		public string GetSecurityDescriptor()
		{
			return m_Owner + m_Group + GetDefaultDacl() + GetUniqueAccessControlEntries() + GetDefaultSacl();
		}

		public static string MergeUniqueAccessControlEntries(string SecurityDescriptor, string UniqueAccessControlEntries)
		{
			string text = null;
			string[] separator = new string[1] { "S:" };
			string[] array = SecurityDescriptor.Split(separator, StringSplitOptions.RemoveEmptyEntries);
			string[] array2 = UniqueAccessControlEntries.Split(separator, StringSplitOptions.RemoveEmptyEntries);
			text = array[0] + array2[0];
			if (array.Length > 1)
			{
				text = text + "S:" + array[1];
			}
			return text;
		}
	}
}
