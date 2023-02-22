using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents
{
	public class SvcHostGroupBuilder : PkgObjectBuilder<SvcHostGroup, SvcHostGroupBuilder>
	{
		public SvcHostGroupBuilder(string name)
		{
			pkgObject = new SvcHostGroup();
			pkgObject.Name = name;
		}

		public SvcHostGroupBuilder(XElement svcHostElement)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(SvcHostGroup));
			using (XmlReader xmlReader = svcHostElement.CreateReader())
			{
				pkgObject = (SvcHostGroup)xmlSerializer.Deserialize(xmlReader);
			}
		}

		public SvcHostGroupBuilder SetCoInitializeSecurityParam(bool flag)
		{
			pkgObject.CoInitializeSecurityParam = flag;
			return this;
		}

		public SvcHostGroupBuilder SetCoInitializeSecurityAllowLowBox(bool flag)
		{
			pkgObject.CoInitializeSecurityAllowLowBox = flag;
			return this;
		}

		public SvcHostGroupBuilder SetCoInitializeSecurityAppId(string appId)
		{
			pkgObject.CoInitializeSecurityAppId = appId;
			return this;
		}

		public SvcHostGroupBuilder SetDefaultRpcStackSize(int size)
		{
			pkgObject.DefaultRpcStackSize = size;
			return this;
		}

		public SvcHostGroupBuilder SetSystemCritical(bool flag)
		{
			pkgObject.SystemCritical = flag;
			return this;
		}

		public SvcHostGroupBuilder SetAuthenticationLevel(AuthenticationLevel level)
		{
			pkgObject.AuthenticationLevel = level;
			return this;
		}

		public SvcHostGroupBuilder SetAuthenticationCapabilities(AuthenticationCapabitities capabilities)
		{
			pkgObject.AuthenticationCapabitities = capabilities;
			return this;
		}

		public SvcHostGroupBuilder SetImpersonationLevel(ImpersonationLevel level)
		{
			pkgObject.ImpersonationLevel = level;
			return this;
		}

		public override SvcHostGroup ToPkgObject()
		{
			RegisterMacro("hklm.svchostgroup", "$(hklm.svchost)\\" + pkgObject.Name);
			return base.ToPkgObject();
		}
	}
}
