using System.Collections.Generic;
using System.Linq;
using Microsoft.CompPlat.PkgBldr.Base.Security.SecurityPolicy;

namespace Microsoft.CompPlat.PkgBldr.Base.Security
{
	public class GlobalSecurity
	{
		private Dictionary<string, string> m_FileSecurityDescriptorDefinitionList;

		private Dictionary<string, string> m_DirectorySecurityDescriptorDefinitionList;

		private Dictionary<string, string> m_RegKeySecurityDescriptorDefinitionList;

		private Dictionary<string, string> m_ServiceAccessSecurityDescriptorDefinitionList;

		private Dictionary<string, WnfValue> m_WnfSecurityDescriptorDefinitionList;

		private Dictionary<string, SdRegValue> m_SecurityDescriptorRegKeyList;

		public Dictionary<string, string> FileSddlList => m_FileSecurityDescriptorDefinitionList;

		public Dictionary<string, string> DirSddlList => m_DirectorySecurityDescriptorDefinitionList;

		public Dictionary<string, string> RegKeySddlList => m_RegKeySecurityDescriptorDefinitionList;

		public Dictionary<string, string> ServiceSddlList => m_ServiceAccessSecurityDescriptorDefinitionList;

		public Dictionary<string, WnfValue> WnfValueList => m_WnfSecurityDescriptorDefinitionList;

		public Dictionary<string, SdRegValue> SdRegValuelList => m_SecurityDescriptorRegKeyList;

		public GlobalSecurity()
		{
			m_FileSecurityDescriptorDefinitionList = new Dictionary<string, string>();
			m_DirectorySecurityDescriptorDefinitionList = new Dictionary<string, string>();
			m_RegKeySecurityDescriptorDefinitionList = new Dictionary<string, string>();
			m_ServiceAccessSecurityDescriptorDefinitionList = new Dictionary<string, string>();
			m_WnfSecurityDescriptorDefinitionList = new Dictionary<string, WnfValue>();
			m_SecurityDescriptorRegKeyList = new Dictionary<string, SdRegValue>();
		}

		public bool SddlListsAreEmpty()
		{
			if (m_DirectorySecurityDescriptorDefinitionList.Count() > 0)
			{
				return false;
			}
			if (m_FileSecurityDescriptorDefinitionList.Count() > 0)
			{
				return false;
			}
			if (m_RegKeySecurityDescriptorDefinitionList.Count() > 0)
			{
				return false;
			}
			if (m_SecurityDescriptorRegKeyList.Count() > 0)
			{
				return false;
			}
			if (m_ServiceAccessSecurityDescriptorDefinitionList.Count() > 0)
			{
				return false;
			}
			if (m_WnfSecurityDescriptorDefinitionList.Count() > 0)
			{
				return false;
			}
			return true;
		}

		public void AddCapability(string capId, WnfValue wnfValue, string rights, bool adminOnMultiSession)
		{
			if (wnfValue == null)
			{
				throw new PkgGenException("Invalid WnfValue parameter");
			}
			ResourceType type = ResourceType.Wnf;
			AccessControlPolicy accessControlPolicy = new Capability(capId, type, rights, adminOnMultiSession);
			string id = wnfValue.GetId();
			WnfValue value;
			if (m_WnfSecurityDescriptorDefinitionList.TryGetValue(id, out value))
			{
				m_WnfSecurityDescriptorDefinitionList[id].SecurityDescriptor = AccessControlPolicy.MergeUniqueAccessControlEntries(m_WnfSecurityDescriptorDefinitionList[id].SecurityDescriptor, accessControlPolicy.GetUniqueAccessControlEntries());
				return;
			}
			wnfValue.SecurityDescriptor = accessControlPolicy.GetSecurityDescriptor();
			m_WnfSecurityDescriptorDefinitionList.Add(id, wnfValue);
		}

		public void AddPrivateResource(WnfValue wnfValue, string resourceClaimer, PrivateResourceClaimerType resourceClaimerType, bool readOnly)
		{
			if (wnfValue == null)
			{
				throw new PkgGenException("Invalid WnfValue parameter");
			}
			AccessControlPolicy accessControlPolicy = new PrivateResource(ResourceType.Wnf, resourceClaimer, resourceClaimerType, readOnly);
			string id = wnfValue.GetId();
			WnfValue value;
			if (m_WnfSecurityDescriptorDefinitionList.TryGetValue(id, out value))
			{
				m_WnfSecurityDescriptorDefinitionList[id].SecurityDescriptor = AccessControlPolicy.MergeUniqueAccessControlEntries(m_WnfSecurityDescriptorDefinitionList[id].SecurityDescriptor, accessControlPolicy.GetUniqueAccessControlEntries());
				return;
			}
			wnfValue.SecurityDescriptor = accessControlPolicy.GetSecurityDescriptor();
			m_WnfSecurityDescriptorDefinitionList.Add(id, wnfValue);
		}

		public void AddCapability(string capId, SdRegValue sdRegValue, ResourceType resourceType, string rights, bool adminOnMultiSession)
		{
			AddCapability(capId, sdRegValue, resourceType, rights, adminOnMultiSession, false);
		}

		public void AddCapability(string capId, SdRegValue sdRegValue, ResourceType resourceType, string rights, bool adminOnMultiSession, bool protectToUser)
		{
			if (sdRegValue == null)
			{
				throw new PkgGenException("Invalid SdRegValue parameter");
			}
			AccessControlPolicy accessControlPolicy = new Capability(capId, resourceType, rights, adminOnMultiSession, protectToUser);
			bool flag = false;
			SdRegValue value = null;
			string uniqueIdentifier = sdRegValue.GetUniqueIdentifier();
			if (resourceType == ResourceType.ComLaunch)
			{
				flag = true;
			}
			if (m_SecurityDescriptorRegKeyList.TryGetValue(uniqueIdentifier, out value))
			{
				if (flag)
				{
					if (value.AdditionalValue == null)
					{
						value.AdditionalValue = accessControlPolicy.GetSecurityDescriptor();
					}
					else
					{
						value.AdditionalValue = AccessControlPolicy.MergeUniqueAccessControlEntries(value.AdditionalValue, accessControlPolicy.GetUniqueAccessControlEntries());
					}
				}
				else if (value.Value == null)
				{
					value.Value = accessControlPolicy.GetSecurityDescriptor();
				}
				else
				{
					value.Value = AccessControlPolicy.MergeUniqueAccessControlEntries(value.Value, accessControlPolicy.GetUniqueAccessControlEntries());
				}
			}
			else
			{
				if (flag)
				{
					sdRegValue.AdditionalValue = accessControlPolicy.GetSecurityDescriptor();
				}
				else
				{
					sdRegValue.Value = accessControlPolicy.GetSecurityDescriptor();
				}
				m_SecurityDescriptorRegKeyList.Add(uniqueIdentifier, sdRegValue);
			}
		}

		public void AddPrivateResource(SdRegValue sdRegValue, ResourceType resourceType, string resourceClaimer, PrivateResourceClaimerType resourceClaimerType, bool readOnly)
		{
			AddPrivateResource(sdRegValue, resourceType, resourceClaimer, resourceClaimerType, readOnly, false);
		}

		public void AddPrivateResource(SdRegValue sdRegValue, ResourceType resourceType, string resourceClaimer, PrivateResourceClaimerType resourceClaimerType, bool readOnly, bool protectToUser)
		{
			if (sdRegValue == null)
			{
				throw new PkgGenException("Invalid SdRegValue parameter");
			}
			AccessControlPolicy accessControlPolicy = new PrivateResource(resourceType, resourceClaimer, resourceClaimerType, readOnly, protectToUser);
			bool flag = false;
			SdRegValue value = null;
			string uniqueIdentifier = sdRegValue.GetUniqueIdentifier();
			if (resourceType == ResourceType.ComLaunch)
			{
				flag = true;
			}
			if (m_SecurityDescriptorRegKeyList.TryGetValue(uniqueIdentifier, out value))
			{
				if (flag)
				{
					if (value.AdditionalValue == null)
					{
						value.AdditionalValue = accessControlPolicy.GetSecurityDescriptor();
					}
					else
					{
						value.AdditionalValue = AccessControlPolicy.MergeUniqueAccessControlEntries(value.AdditionalValue, accessControlPolicy.GetUniqueAccessControlEntries());
					}
				}
				else if (value.Value == null)
				{
					value.Value = accessControlPolicy.GetSecurityDescriptor();
				}
				else
				{
					value.Value = AccessControlPolicy.MergeUniqueAccessControlEntries(value.Value, accessControlPolicy.GetUniqueAccessControlEntries());
				}
			}
			else
			{
				if (flag)
				{
					sdRegValue.AdditionalValue = accessControlPolicy.GetSecurityDescriptor();
				}
				else
				{
					sdRegValue.Value = accessControlPolicy.GetSecurityDescriptor();
				}
				m_SecurityDescriptorRegKeyList.Add(uniqueIdentifier, sdRegValue);
			}
		}

		public void AddCapability(string capId, string path, ResourceType resourceType, string rights, bool adminOnMultiSession)
		{
			AddCapability(capId, path, resourceType, rights, adminOnMultiSession, false, null);
		}

		public void AddCapability(string capId, string path, ResourceType resourceType, string rights, bool adminOnMultiSession, bool protectToUser, string phoneSddl)
		{
			if (path == null)
			{
				throw new PkgGenException("Invalid path parameter");
			}
			Dictionary<string, string> dictionary = null;
			switch (resourceType)
			{
			case ResourceType.Registry:
				dictionary = m_RegKeySecurityDescriptorDefinitionList;
				path = path.TrimEnd("(*)".ToCharArray());
				path = path.TrimEnd('\\');
				break;
			case ResourceType.File:
				dictionary = m_FileSecurityDescriptorDefinitionList;
				break;
			case ResourceType.Directory:
				dictionary = m_DirectorySecurityDescriptorDefinitionList;
				path = path.TrimEnd("(*)".ToCharArray());
				path = path.TrimEnd('\\');
				break;
			case ResourceType.ServiceAccess:
				dictionary = m_ServiceAccessSecurityDescriptorDefinitionList;
				break;
			default:
				throw new PkgGenException("Invalid resource type. Failed to add capability");
			}
			if (phoneSddl == null)
			{
				AccessControlPolicy accessControlPolicy = new Capability(capId, resourceType, rights, adminOnMultiSession, protectToUser);
				string value;
				if (dictionary.TryGetValue(path, out value))
				{
					dictionary[path] = AccessControlPolicy.MergeUniqueAccessControlEntries(dictionary[path], accessControlPolicy.GetUniqueAccessControlEntries());
				}
				else
				{
					dictionary.Add(path, accessControlPolicy.GetSecurityDescriptor());
				}
			}
			else
			{
				dictionary.Add(path, phoneSddl);
			}
		}

		public void AddPrivateResource(string path, ResourceType resourceType, string resourceClaimer, PrivateResourceClaimerType resourceClaimerType, bool readOnly)
		{
			AddPrivateResource(path, resourceType, resourceClaimer, resourceClaimerType, readOnly, false);
		}

		public void AddPrivateResource(string path, ResourceType resourceType, string resourceClaimer, PrivateResourceClaimerType resourceClaimerType, bool readOnly, bool protectToUser)
		{
			if (path == null)
			{
				throw new PkgGenException("Invalid path parameter");
			}
			Dictionary<string, string> dictionary = null;
			switch (resourceType)
			{
			case ResourceType.Registry:
				dictionary = m_RegKeySecurityDescriptorDefinitionList;
				path = path.TrimEnd("(*)".ToCharArray());
				path = path.TrimEnd('\\');
				break;
			case ResourceType.File:
				dictionary = m_FileSecurityDescriptorDefinitionList;
				break;
			case ResourceType.Directory:
				dictionary = m_DirectorySecurityDescriptorDefinitionList;
				path = path.TrimEnd("(*)".ToCharArray());
				path = path.TrimEnd('\\');
				break;
			case ResourceType.ServiceAccess:
				dictionary = m_ServiceAccessSecurityDescriptorDefinitionList;
				break;
			default:
				throw new PkgGenException("Invalid resource type. Failed to add private resource");
			}
			AccessControlPolicy accessControlPolicy = new PrivateResource(resourceType, resourceClaimer, resourceClaimerType, readOnly, protectToUser);
			string value;
			if (dictionary.TryGetValue(path, out value))
			{
				dictionary[path] = AccessControlPolicy.MergeUniqueAccessControlEntries(dictionary[path], accessControlPolicy.GetUniqueAccessControlEntries());
			}
			else
			{
				dictionary.Add(path, accessControlPolicy.GetSecurityDescriptor());
			}
		}
	}
}
