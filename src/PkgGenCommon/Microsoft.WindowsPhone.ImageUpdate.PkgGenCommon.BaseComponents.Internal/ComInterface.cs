using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot(ElementName = "Interface", Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00")]
	public sealed class ComInterface : ComBase
	{
		[XmlAttribute("Name")]
		public string Name { get; set; }

		[XmlAttribute("ProxyStubClsId")]
		public string ProxyStubClsId { get; set; }

		[XmlAttribute("ProxyStubClsId32")]
		public string ProxyStubClsId32 { get; set; }

		[XmlAttribute("NumMethods")]
		public string NumMethods { get; set; }

		public ComInterface()
		{
			_defaultKey = "$(hkcr.iid)";
		}

		protected override void DoBuild(IPackageGenerator pkgGen)
		{
			base.DoBuild(pkgGen);
			if (ProxyStubClsId != null)
			{
				pkgGen.AddRegValue(_defaultKey + "\\ProxyStubClsId", "@", RegValueType.String, ProxyStubClsId);
			}
			if (ProxyStubClsId32 != null)
			{
				pkgGen.AddRegValue(_defaultKey + "\\ProxyStubClsId32", "@", RegValueType.String, ProxyStubClsId32);
			}
			if (Name != null)
			{
				pkgGen.AddRegValue("$(hkcr.iid)", "@", RegValueType.String, Name);
			}
			if (base.Version != null)
			{
				pkgGen.AddRegValue(_defaultKey + "\\TypeLib", "Version", RegValueType.String, base.Version);
			}
			if (NumMethods != null)
			{
				pkgGen.AddRegValue(_defaultKey + "\\NumMethods", "@", RegValueType.String, NumMethods);
			}
		}
	}
}
