using System;
using System.Globalization;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public abstract class ResourceAcl
	{
		protected string m_explicitDacl;

		protected string m_macLabel;

		protected string m_owner;

		protected string m_elementId;

		protected string m_attributeHash;

		protected string m_path;

		protected bool m_isProtected;

		private static readonly ResourceAclComparer ResourceAclComparer = new ResourceAclComparer();

		protected NativeObjectSecurity m_nos;

		protected AuthorizationRuleCollection m_accessRules;

		protected string m_fullPath = string.Empty;

		protected static readonly Regex regexExtractMIL = new Regex("(?<MIL>\\(ML[^\\)]*\\))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		protected static readonly Regex regexStripDacl = new Regex("^D:", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		protected static readonly Regex regexStripDriveLetter = new Regex("^[A-Z]:", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		[XmlAttribute("DACL")]
		public string ExplicitDACL
		{
			get
			{
				if (m_nos != null)
				{
					m_explicitDacl = ComputeExplicitDACL();
				}
				return m_explicitDacl;
			}
			set
			{
				m_explicitDacl = value;
			}
		}

		[XmlAttribute("SACL")]
		public abstract string MandatoryIntegrityLabel { get; set; }

		[XmlAttribute("Owner")]
		public string Owner
		{
			get
			{
				if (m_nos != null)
				{
					m_owner = m_nos.GetSecurityDescriptorSddlForm(AccessControlSections.Owner | AccessControlSections.Group);
					m_owner = SddlNormalizer.FixOwnerSddl(m_owner);
				}
				return m_owner;
			}
			set
			{
				m_owner = value;
			}
		}

		[XmlAttribute]
		public string ElementID
		{
			get
			{
				if (!string.IsNullOrEmpty(m_path))
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append(TypeString);
					stringBuilder.Append(m_path.ToUpper(new CultureInfo("en-US", false)));
					m_elementId = CommonUtils.GetSha256Hash(Encoding.Unicode.GetBytes(stringBuilder.ToString()));
				}
				return m_elementId;
			}
			set
			{
				m_elementId = value;
			}
		}

		[XmlAttribute]
		public virtual string AttributeHash
		{
			get
			{
				if (string.IsNullOrEmpty(m_attributeHash))
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append(TypeString);
					stringBuilder.Append(m_path.ToUpper(new CultureInfo("en-US", false)));
					stringBuilder.Append(Protected);
					string owner = Owner;
					if (!string.IsNullOrEmpty(owner))
					{
						stringBuilder.Append(owner);
					}
					string explicitDACL = ExplicitDACL;
					if (!string.IsNullOrEmpty(explicitDACL))
					{
						stringBuilder.Append(explicitDACL);
					}
					string mandatoryIntegrityLabel = MandatoryIntegrityLabel;
					if (!string.IsNullOrEmpty(mandatoryIntegrityLabel))
					{
						stringBuilder.Append(mandatoryIntegrityLabel);
					}
					m_attributeHash = CommonUtils.GetSha256Hash(Encoding.Unicode.GetBytes(stringBuilder.ToString()));
				}
				return m_attributeHash;
			}
			set
			{
				m_attributeHash = value;
			}
		}

		[XmlAttribute]
		public string Path
		{
			get
			{
				return m_path;
			}
			set
			{
				m_path = value;
			}
		}

		[XmlIgnore]
		public string Protected
		{
			get
			{
				if (m_nos != null)
				{
					m_isProtected = m_nos.AreAccessRulesProtected;
				}
				if (!m_isProtected)
				{
					return "No";
				}
				return "Yes";
			}
			set
			{
				m_isProtected = ((value != null && value.Equals("Yes", StringComparison.OrdinalIgnoreCase)) ? true : false);
			}
		}

		public bool IsEmpty
		{
			get
			{
				if (string.IsNullOrEmpty(ExplicitDACL) && string.IsNullOrEmpty(MandatoryIntegrityLabel))
				{
					return !DACLProtected;
				}
				return false;
			}
		}

		public string DACL
		{
			get
			{
				string text = string.Empty;
				if (m_nos != null)
				{
					text = m_nos.GetSecurityDescriptorSddlForm(AccessControlSections.Access);
					if (!string.IsNullOrEmpty(text))
					{
						text = regexStripDacl.Replace(text, string.Empty);
					}
				}
				return SddlNormalizer.FixAceSddl(text);
			}
		}

		public string FullACL
		{
			get
			{
				string result = string.Empty;
				if (m_nos != null)
				{
					result = m_nos.GetSecurityDescriptorSddlForm(AccessControlSections.All);
				}
				return result;
			}
		}

		public static ResourceAclComparer Comparer => ResourceAclComparer;

		public abstract NativeObjectSecurity ObjectSecurity { get; }

		protected abstract string TypeString { get; }

		protected AuthorizationRuleCollection AccessRules
		{
			get
			{
				if (m_accessRules == null && m_nos != null)
				{
					m_accessRules = m_nos.GetAccessRules(true, false, typeof(NTAccount));
				}
				return m_accessRules;
			}
		}

		public bool PreserveInheritance
		{
			get
			{
				if (m_nos != null)
				{
					return m_nos.GetAccessRules(false, true, typeof(NTAccount)).Count > 0;
				}
				return false;
			}
		}

		public bool DACLProtected
		{
			get
			{
				if (m_nos != null)
				{
					return m_nos.AreAccessRulesProtected;
				}
				return false;
			}
		}

		protected abstract string ComputeExplicitDACL();
	}
}
