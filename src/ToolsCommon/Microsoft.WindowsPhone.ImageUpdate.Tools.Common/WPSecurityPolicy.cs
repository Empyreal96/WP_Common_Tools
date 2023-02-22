using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	[XmlRoot("PhoneSecurityPolicy")]
	public class WPSecurityPolicy
	{
		private string m_descr = "Mobile Core Policy";

		private string m_vendor = "Microsoft";

		private string m_OSVersion = "8.00";

		private string m_fileVersion = "8.00";

		private string m_hashType = "Sha2";

		private string m_packageId = "";

		private AclCollection m_aclCollection = new AclCollection();

		private const string WP8PolicyNamespace = "urn:Microsoft.WindowsPhone/PhoneSecurityPolicyInternal.v8.00";

		[XmlAttribute]
		public string Description
		{
			get
			{
				return m_descr;
			}
			set
			{
				m_descr = value;
			}
		}

		[XmlAttribute]
		public string Vendor
		{
			get
			{
				return m_vendor;
			}
			set
			{
				m_vendor = value;
			}
		}

		[XmlAttribute]
		public string RequiredOSVersion
		{
			get
			{
				return m_OSVersion;
			}
			set
			{
				m_OSVersion = value;
			}
		}

		[XmlAttribute]
		public string FileVersion
		{
			get
			{
				return m_fileVersion;
			}
			set
			{
				m_fileVersion = value;
			}
		}

		[XmlAttribute]
		public string HashType
		{
			get
			{
				return m_hashType;
			}
			set
			{
				m_hashType = value;
			}
		}

		[XmlAttribute]
		public string PackageID
		{
			get
			{
				return m_packageId;
			}
			set
			{
				m_packageId = value;
			}
		}

		[XmlArrayItem(typeof(DirectoryAcl), ElementName = "Directory")]
		[XmlArrayItem(typeof(FileAcl), ElementName = "File")]
		[XmlArrayItem(typeof(RegistryAcl), ElementName = "RegKey")]
		[XmlArrayItem(typeof(RegAclWithFullAcl), ElementName = "RegKeyFullACL")]
		[XmlArrayItem(typeof(RegistryStoredAcl), ElementName = "SDRegValue")]
		public ResourceAcl[] Rules
		{
			get
			{
				return m_aclCollection.ToArray();
			}
			set
			{
				m_aclCollection.Clear();
				m_aclCollection.UnionWith(value);
			}
		}

		public WPSecurityPolicy()
		{
		}

		public WPSecurityPolicy(string packageName)
		{
			m_packageId = packageName;
		}

		public void Add(IEnumerable<ResourceAcl> acls)
		{
			m_aclCollection.UnionWith(acls);
		}

		public void SaveToXml(string policyFile)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(WPSecurityPolicy), "urn:Microsoft.WindowsPhone/PhoneSecurityPolicyInternal.v8.00");
			using (TextWriter textWriter = new StreamWriter(LongPathFile.OpenWrite(policyFile)))
			{
				xmlSerializer.Serialize(textWriter, this);
			}
		}

		public static WPSecurityPolicy LoadFromXml(string policyFile)
		{
			if (!LongPathFile.Exists(policyFile))
			{
				throw new FileNotFoundException($"Policy file {policyFile} does not exist, or cannot be read", policyFile);
			}
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(WPSecurityPolicy), "urn:Microsoft.WindowsPhone/PhoneSecurityPolicyInternal.v8.00");
			WPSecurityPolicy wPSecurityPolicy = null;
			using (TextReader textReader = new StreamReader(LongPathFile.OpenRead(policyFile)))
			{
				return (WPSecurityPolicy)xmlSerializer.Deserialize(textReader);
			}
		}
	}
}
