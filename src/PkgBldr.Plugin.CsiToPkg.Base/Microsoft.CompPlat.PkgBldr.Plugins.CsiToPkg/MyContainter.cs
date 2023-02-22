using System.Xml.Linq;

namespace Microsoft.CompPlat.PkgBldr.Plugins.CsiToPkg
{
	internal class MyContainter
	{
		public Security Security { get; set; }

		public XElement RegKeys { get; set; }

		public XElement Files { get; set; }
	}
}
