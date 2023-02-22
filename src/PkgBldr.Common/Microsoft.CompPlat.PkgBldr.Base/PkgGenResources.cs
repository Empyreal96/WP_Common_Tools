using System.IO;
using System.Reflection;

namespace Microsoft.CompPlat.PkgBldr.Base
{
	public static class PkgGenResources
	{
		public static Stream GetResourceStream(string embeddedFileName)
		{
			string text = typeof(PkgGenResources).Namespace + ".Resources." + embeddedFileName;
			Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(text);
			Assembly.GetCallingAssembly().GetManifestResourceNames();
			if (manifestResourceStream == null)
			{
				throw new PkgGenException("Failed to load resource stream {0}", text);
			}
			return manifestResourceStream;
		}
	}
}
