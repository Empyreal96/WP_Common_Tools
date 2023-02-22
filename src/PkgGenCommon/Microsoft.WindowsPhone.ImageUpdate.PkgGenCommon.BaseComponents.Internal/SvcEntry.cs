using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	public class SvcEntry : PkgFile
	{
		public override void Build(IPackageGenerator pkgGen, SatelliteId satelliteId)
		{
			SvcDll svcDll = this as SvcDll;
			bool flag = false;
			if (satelliteId != SatelliteId.Neutral)
			{
				throw new PkgGenException("SvcDll object should not be langauge/resolution specific");
			}
			if (svcDll != null)
			{
				flag = svcDll.BinaryInOneCorePkg;
			}
			if (!flag)
			{
				base.Build(pkgGen, SatelliteId.Neutral);
			}
		}
	}
}
