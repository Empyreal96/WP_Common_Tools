using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces
{
	public interface IPkgProject
	{
		string TempDirectory { get; }

		IPkgLogger Log { get; }

		IMacroResolver MacroResolver { get; }

		IDictionary<string, string> Attributes { get; }

		IEnumerable<SatelliteId> GetSatelliteValues(SatelliteType type);

		void AddToCapabilities(XElement element);

		void AddToAuthorization(XElement element);
	}
}
