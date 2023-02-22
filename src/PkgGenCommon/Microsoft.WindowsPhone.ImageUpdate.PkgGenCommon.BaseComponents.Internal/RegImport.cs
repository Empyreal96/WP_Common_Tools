using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	public class RegImport
	{
		[XmlAttribute("Source")]
		public string Source;

		public RegImport()
		{
		}

		public RegImport(string source)
		{
			Source = source;
		}

		public void Preprocess(IMacroResolver macroResolver)
		{
			Source = macroResolver.Resolve(Source);
		}

		public void Build(IPackageGenerator pkgGen)
		{
			pkgGen.ImportRegistry(Source).Build(pkgGen);
		}
	}
}
