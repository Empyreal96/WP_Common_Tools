using System.Xml.Linq;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	public class ComData
	{
		public XElement RegKeys { get; set; }

		public XElement Files { get; set; }

		public XElement InProcServer { get; set; }

		public XElement InProcServerClasses { get; set; }

		public XElement Interfaces { get; set; }
	}
}
