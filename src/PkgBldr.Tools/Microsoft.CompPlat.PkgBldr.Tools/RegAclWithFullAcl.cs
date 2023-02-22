using System.Security.AccessControl;
using System.Xml.Serialization;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public class RegAclWithFullAcl : RegistryAcl
	{
		[XmlAttribute("FullACL")]
		public string FullRegACL
		{
			get
			{
				return base.FullACL;
			}
			set
			{
			}
		}

		public RegAclWithFullAcl()
		{
		}

		public RegAclWithFullAcl(NativeObjectSecurity nos)
		{
			m_nos = nos;
		}
	}
}
