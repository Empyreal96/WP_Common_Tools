using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.WmToCsi
{
	[Export(typeof(IPkgPlugin))]
	internal class Directories : PkgPlugin
	{
		public override void ConvertEntries(XElement ToCsi, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromWm)
		{
			ToCsi = PkgBldrHelpers.AddIfNotFound(ToCsi, "directories");
			string attributeValue = PkgBldrHelpers.GetAttributeValue(FromWm, "buildFilter");
			if (attributeValue != null)
			{
				ToCsi.Add(new XAttribute("buildFilter", attributeValue));
			}
			base.ConvertEntries(ToCsi, plugins, enviorn, FromWm);
		}
	}
}
