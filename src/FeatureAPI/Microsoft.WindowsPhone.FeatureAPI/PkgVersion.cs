using System;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	[CLSCompliant(false)]
	public class PkgVersion
	{
		[XmlAttribute]
		public ushort Major;

		[XmlAttribute]
		public ushort Minor;

		[XmlAttribute]
		public ushort QFE;

		[XmlAttribute]
		public ushort Build;
	}
}
