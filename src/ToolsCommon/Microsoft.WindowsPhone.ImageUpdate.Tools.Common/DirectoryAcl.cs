using System;
using System.Globalization;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public class DirectoryAcl : ResourceAcl
	{
		private bool m_isRoot;

		private DirectoryInfo m_di;

		private NativeObjectSecurity m_objectSecurity;

		[XmlAttribute("SACL")]
		public override string MandatoryIntegrityLabel
		{
			get
			{
				if (!m_macLablelProcessed)
				{
					m_macLablelProcessed = true;
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
				if (m_objectSecurity == null)
				{
					DirectorySecurity directorySecurity = null;
					if (m_nos != null)
					{
						directorySecurity = new DirectorySecurity();
						directorySecurity.SetSecurityDescriptorBinaryForm(m_nos.GetSecurityDescriptorBinaryForm());
					}
					m_objectSecurity = directorySecurity;
				}
				return m_objectSecurity;
			}
		}

		protected override string TypeString => "Directory";

		public DirectoryAcl()
		{
		}

		public DirectoryAcl(string directory, string rootPath)
		{
			if (!LongPathDirectory.Exists(directory))
			{
				throw new DirectoryNotFoundException($"Folder {directory} cannot be found");
			}
			DirectoryInfo di = new DirectoryInfo(directory);
			Initialize(di, rootPath);
		}

		public DirectoryAcl(DirectoryInfo di, string rootPath)
		{
			if (di == null)
			{
				throw new ArgumentNullException("di");
			}
			Initialize(di, rootPath);
		}

		protected override string ComputeExplicitDACL()
		{
			string text = null;
			if (m_isRoot)
			{
				text = m_nos.GetSecurityDescriptorSddlForm(AccessControlSections.Access);
			}
			else
			{
				DirectorySecurity accessControl = m_di.GetAccessControl(AccessControlSections.All);
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
				if (base.DACLProtected || num > 0)
				{
					text = accessControl.GetSecurityDescriptorSddlForm(AccessControlSections.Access);
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				text = ResourceAcl.regexStripDacl.Replace(text, string.Empty);
			}
			return SddlNormalizer.FixAceSddl(text);
		}

		private void Initialize(DirectoryInfo di, string rootPath)
		{
			if (di == null)
			{
				throw new ArgumentNullException("di");
			}
			m_di = di;
			m_nos = di.GetAccessControl(AccessControlSections.All);
			m_fullPath = di.FullName;
			m_isRoot = string.Equals(di.FullName, rootPath, StringComparison.OrdinalIgnoreCase);
			m_path = System.IO.Path.Combine("\\", di.FullName.Remove(0, rootPath.Length)).ToUpper(CultureInfo.InvariantCulture);
		}
	}
}
