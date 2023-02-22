using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public class RegistryStoredAcl : ResourceAcl
	{
		protected string m_typeName = "Unknown";

		protected byte[] m_rawsd;

		[XmlAttribute("Type")]
		public string SDRegValueTypeName
		{
			get
			{
				return m_typeName;
			}
			set
			{
				m_typeName = value;
			}
		}

		[XmlAttribute("SACL")]
		public override string MandatoryIntegrityLabel
		{
			get
			{
				m_macLabel = null;
				string text = SecurityUtils.ConvertSDToStringSD(m_rawsd, SecurityInformationFlags.MANDATORY_ACCESS_LABEL);
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
				return m_macLabel;
			}
			set
			{
				m_macLabel = value;
			}
		}

		[XmlAttribute]
		public override string AttributeHash
		{
			get
			{
				if (string.IsNullOrEmpty(m_attributeHash))
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append(TypeString);
					stringBuilder.Append(m_path);
					stringBuilder.Append(base.Protected);
					string owner = base.Owner;
					if (!string.IsNullOrEmpty(owner))
					{
						stringBuilder.Append(owner);
					}
					string explicitDACL = base.ExplicitDACL;
					if (!string.IsNullOrEmpty(explicitDACL))
					{
						stringBuilder.Append(explicitDACL);
					}
					string mandatoryIntegrityLabel = MandatoryIntegrityLabel;
					if (!string.IsNullOrEmpty(mandatoryIntegrityLabel))
					{
						stringBuilder.Append(mandatoryIntegrityLabel);
					}
					stringBuilder.Append(SDRegValueTypeName);
					m_attributeHash = CommonUtils.GetSha256Hash(Encoding.Unicode.GetBytes(stringBuilder.ToString()));
				}
				return m_attributeHash;
			}
			set
			{
				m_attributeHash = value;
			}
		}

		public override NativeObjectSecurity ObjectSecurity
		{
			get
			{
				RegistrySecurity registrySecurity = new RegistrySecurity();
				registrySecurity.SetSecurityDescriptorBinaryForm(m_rawsd);
				return registrySecurity;
			}
		}

		protected override string TypeString => "SDRegValue";

		public RegistryStoredAcl()
		{
		}

		public RegistryStoredAcl(string typeName, string path, byte[] rawSecurityDescriptor)
		{
			if (rawSecurityDescriptor == null || string.IsNullOrEmpty(path) || string.IsNullOrEmpty(typeName))
			{
				throw new ArgumentException("SDRegValue is null");
			}
			RegistrySecurity registrySecurity = new RegistrySecurity();
			registrySecurity.SetSecurityDescriptorBinaryForm(rawSecurityDescriptor);
			m_rawsd = rawSecurityDescriptor;
			m_nos = registrySecurity;
			m_path = path;
			m_fullPath = path;
			m_typeName = typeName;
		}

		protected override string ComputeExplicitDACL()
		{
			RegistrySecurity registrySecurity = new RegistrySecurity();
			registrySecurity.SetSecurityDescriptorBinaryForm(m_rawsd);
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
					text = SddlNormalizer.FixAceSddl(text);
				}
			}
			return text;
		}
	}
}
