using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Xml.Serialization;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public class FileAcl : ResourceAcl
	{
		private FileInfo m_fi;

		[XmlAttribute("SACL")]
		public override string MandatoryIntegrityLabel
		{
			get
			{
				if (m_nos != null)
				{
					m_macLabel = SecurityUtils.GetFileSystemMandatoryLevel(m_fullPath);
					if (string.IsNullOrEmpty(m_macLabel))
					{
						m_macLabel = null;
					}
					else
					{
						m_macLabel = SddlNormalizer.FixAceSddl(m_macLabel);
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
				FileSecurity fileSecurity = null;
				if (m_nos != null)
				{
					fileSecurity = new FileSecurity();
					fileSecurity.SetSecurityDescriptorBinaryForm(m_nos.GetSecurityDescriptorBinaryForm());
				}
				return fileSecurity;
			}
		}

		protected override string TypeString => "File";

		public FileAcl()
		{
		}

		public FileAcl(string file, string rootPath)
		{
			if (!LongPathFile.Exists(file))
			{
				throw new FileNotFoundException("Specified file cannot be found", file);
			}
			FileInfo fi = new FileInfo(file);
			Initialize(fi, rootPath);
		}

		public FileAcl(FileInfo fi, string rootPath)
		{
			if (fi == null)
			{
				throw new ArgumentNullException("fi");
			}
			Initialize(fi, rootPath);
		}

		protected override string ComputeExplicitDACL()
		{
			FileSecurity accessControl = m_fi.GetAccessControl(AccessControlSections.All);
			AuthorizationRuleCollection accessRules = accessControl.GetAccessRules(true, false, typeof(NTAccount));
			int num = accessRules.Count;
			foreach (FileSystemAccessRule item in accessRules)
			{
				if (item.IsInherited)
				{
					accessControl.RemoveAccessRule(item);
					num--;
				}
			}
			if (base.DACLProtected && accessControl.AreAccessRulesCanonical)
			{
				accessControl.SetAccessRuleProtection(true, base.PreserveInheritance);
			}
			string text = null;
			if (base.DACLProtected || num > 0)
			{
				text = accessControl.GetSecurityDescriptorSddlForm(AccessControlSections.Access);
				if (!string.IsNullOrEmpty(text))
				{
					text = ResourceAcl.regexStripDacl.Replace(text, string.Empty);
				}
			}
			return SddlNormalizer.FixAceSddl(text);
		}

		private void Initialize(FileInfo fi, string rootPath)
		{
			if (fi == null)
			{
				throw new ArgumentNullException("fi");
			}
			m_fi = fi;
			m_nos = fi.GetAccessControl(AccessControlSections.All);
			m_fullPath = fi.FullName;
			m_path = LongPath.Combine("\\", m_fullPath.Remove(0, rootPath.Length)).ToUpperInvariant();
		}
	}
}
