using System.IO;
using System.Reflection;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon
{
	public static class PkgGenResources
	{
		public static Stream GetProjSchemaStream()
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(PkgGenResources).Namespace + ".ProjSchema.xsd");
		}

		public static Stream GetGlobalMacroStream()
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(PkgGenResources).Namespace + ".PkgGen.cfg.xml");
		}
	}
}
