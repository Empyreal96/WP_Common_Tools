using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents
{
	internal class PackageWrapper : IPkgProject
	{
		private PackageProject package;

		private IPackageGenerator packageGenerator;

		private PackageLogger packageLogger;

		public string TempDirectory => packageGenerator.TempDirectory;

		public IPkgLogger Log => packageLogger;

		public IMacroResolver MacroResolver => packageGenerator.MacroResolver;

		public IDictionary<string, string> Attributes => new Dictionary<string, string>
		{
			{ "Name", package.Name },
			{ "Owner", package.Owner },
			{ "Partition", package.Partition },
			{ "Platform", package.Platform }
		};

		internal PackageWrapper(IPackageGenerator packageGenerator, PackageProject package)
		{
			packageLogger = new PackageLogger();
			this.packageGenerator = packageGenerator;
			this.package = package;
		}

		public IEnumerable<SatelliteId> GetSatelliteValues(SatelliteType type)
		{
			return packageGenerator.GetSatelliteValues(type);
		}

		public void AddToCapabilities(XElement element)
		{
			package.AddToCapabilities(element);
		}

		public void AddToAuthorization(XElement element)
		{
			package.AddToAuthorization(element);
		}
	}
}
