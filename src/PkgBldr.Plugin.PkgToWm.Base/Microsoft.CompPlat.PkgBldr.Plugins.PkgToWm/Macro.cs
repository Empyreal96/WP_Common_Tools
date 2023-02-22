using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class Macro : Macros
	{
		public override void ConvertEntries(XElement ToWm, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromPkg)
		{
			string attributeValue = PkgBldrHelpers.GetAttributeValue(FromPkg, "Id");
			string attributeValue2 = PkgBldrHelpers.GetAttributeValue(FromPkg, "Value");
			enviorn.Macros.Register(attributeValue, attributeValue2);
		}
	}
}
