using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents
{
	public abstract class PkgObject
	{
		internal MacroTable LocalMacros { get; set; }

		internal PkgObject()
		{
		}

		protected virtual void DoPreprocess(PackageProject proj, IMacroResolver macroResolver)
		{
		}

		protected virtual void DoBuild(IPackageGenerator pkgGen)
		{
		}

		public void Preprocess(PackageProject proj, IMacroResolver macroResolver)
		{
			DoPreprocess(proj, macroResolver);
		}

		public void Build(IPackageGenerator pkgGen)
		{
			pkgGen.MacroResolver.BeginLocal();
			if (LocalMacros != null)
			{
				pkgGen.MacroResolver.Register(LocalMacros.Values);
			}
			DoBuild(pkgGen);
			pkgGen.MacroResolver.EndLocal();
		}
	}
}
