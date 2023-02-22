using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public class RegistryAcl : ResourceAcl
	{
		private ORRegistryKey m_key;

		[XmlAttribute("SACL")]
		public override string MandatoryIntegrityLabel
		{
			get
			{
				if (m_nos != null)
				{
					m_macLabel = null;
					string text = SecurityUtils.ConvertSDToStringSD(m_nos.GetSecurityDescriptorBinaryForm(), (SecurityInformationFlags)24u);
					if (!string.IsNullOrEmpty(text))
					{
						Match match = ResourceAcl.regexExtractMIL.Match(text);
						if (match.Success)
						{
							Group group = match.Groups["MIL"];
							if (group != null)
							{
								m_macLabel = SddlNormalizer.FixAceSddl(group.Value);
							}
						}
					}
				}
				return m_macLabel;
			}
			set
			{
				m_macLabel = value;
			}
		}

		public override NativeObjectSecurity ObjectSecurity
		{
			get
			{
				RegistrySecurity registrySecurity = null;
				if (m_nos != null)
				{
					registrySecurity = new RegistrySecurity();
					registrySecurity.SetSecurityDescriptorBinaryForm(m_nos.GetSecurityDescriptorBinaryForm());
				}
				return registrySecurity;
			}
		}

		protected override string TypeString => "RegKey";

		public RegistryAcl()
		{
		}

		public RegistryAcl(ORRegistryKey key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			m_key = key;
			m_nos = key.RegistrySecurity;
			m_path = key.FullName;
			m_fullPath = key.FullName;
		}

		protected override string ComputeExplicitDACL()
		{
			RegistrySecurity registrySecurity = m_key.RegistrySecurity;
			AuthorizationRuleCollection accessRules = registrySecurity.GetAccessRules(true, false, typeof(NTAccount));
			int num = accessRules.Count;
			foreach (RegistryAccessRule item in accessRules)
			{
				if (item.IsInherited)
				{
					registrySecurity.RemoveAccessRule(item);
					num--;
				}
			}
			if (base.DACLProtected && registrySecurity.AreAccessRulesCanonical)
			{
				registrySecurity.SetAccessRuleProtection(true, base.PreserveInheritance);
			}
			string text = null;
			if (base.DACLProtected || num > 0)
			{
				text = registrySecurity.GetSecurityDescriptorSddlForm(AccessControlSections.Access);
				if (!string.IsNullOrEmpty(text))
				{
					text = ResourceAcl.regexStripDacl.Replace(text, string.Empty);
				}
			}
			return SddlNormalizer.FixAceSddl(text);
		}
	}
}
