using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon
{
	[XmlRoot(Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00", ElementName = "Macros")]
	public class MacroTable
	{
		[XmlElement(ElementName = "Macro")]
		public List<Macro> Macros { get; set; }

		[XmlIgnore]
		public IEnumerable<KeyValuePair<string, Macro>> Values => Macros.Select((Macro x) => new KeyValuePair<string, Macro>(x.Name, x));

		public MacroTable()
		{
			Macros = new List<Macro>();
		}
	}
}
