using System;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	public class PkgElement
	{
		public virtual void Build(IPackageGenerator pkgGen, SatelliteId satelliteId)
		{
			throw new NotImplementedException();
		}

		public virtual void Build(IPackageGenerator pkgGen)
		{
			Build(pkgGen, SatelliteId.Neutral);
		}
	}
}
