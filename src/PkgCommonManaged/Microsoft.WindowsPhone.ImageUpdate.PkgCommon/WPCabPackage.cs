using System.Linq;
using Microsoft.WindowsPhone.ImageUpdate.Tools;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	internal class WPCabPackage : WPCanonicalPackage
	{
		private string m_strCabPath;

		protected WPCabPackage(string cabPath, PkgManifest pkgManifest)
			: base(pkgManifest)
		{
			m_strCabPath = cabPath;
		}

		protected override void ExtractFiles(FileEntryBase[] files, string[] targetPaths)
		{
			CabApiWrapper.ExtractSelected(m_strCabPath, files.Select((FileEntryBase x) => x.CabPath).ToArray(), targetPaths);
		}

		public static WPCabPackage Load(string cabPath)
		{
			return new WPCabPackage(cabPath, PkgManifest.Load(cabPath, PkgConstants.c_strDsmFile));
		}
	}
}
