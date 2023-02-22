using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.CsiToPkg
{
	[Export(typeof(IPkgPlugin))]
	internal class RegistryValue : PkgPlugin
	{
		public override void ConvertEntries(XElement ToPkg, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromCsi)
		{
			string attributeValue = PkgBldrHelpers.GetAttributeValue(FromCsi, "name");
			string attributeValue2 = PkgBldrHelpers.GetAttributeValue(FromCsi, "valueType");
			string attributeValue3 = PkgBldrHelpers.GetAttributeValue(FromCsi, "value");
			attributeValue3 = enviorn.Macros.Resolve(attributeValue3);
			if (attributeValue3.StartsWith("$(ERROR)", StringComparison.OrdinalIgnoreCase))
			{
				Console.WriteLine("warning: skipping key {0}", attributeValue3);
				return;
			}
			XElement xElement = RegHelpers.PkgRegValue(attributeValue, attributeValue2, attributeValue3);
			if (xElement != null)
			{
				ToPkg.Add(xElement);
			}
		}
	}
}
