using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BasePlugins
{
	[Export(typeof(IPkgPlugin))]
	public class BCDStore : PkgPlugin
	{
		public override bool UseSecurityCompilerPassthrough => true;

		public override string XmlSchemaPath => PkgPlugin.BaseComponentSchemaPath;

		public override string XmlElementUniqueXPath => "@Source";

		public override void ValidateEntries(IPkgProject packageGenerator, IEnumerable<XElement> componentEntries)
		{
		}

		public override IEnumerable<PkgObject> ProcessEntries(IPkgProject packageGenerator, IEnumerable<XElement> componentEntries)
		{
			List<PkgObject> list = new List<PkgObject>();
			foreach (XElement componentEntry in componentEntries)
			{
				list.Add(new BCDStoreBuilder(componentEntry.LocalAttribute("Source").Value).ToPkgObject());
			}
			return list;
		}
	}
}
