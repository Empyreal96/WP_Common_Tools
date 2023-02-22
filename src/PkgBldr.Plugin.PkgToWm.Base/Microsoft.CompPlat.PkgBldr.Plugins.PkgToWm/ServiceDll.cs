using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class ServiceDll : PkgPlugin
	{
		public override void ConvertEntries(XElement ToWm, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromPkg)
		{
			XElement xElement = new XElement(FromPkg);
			XAttribute xAttribute = xElement.Attribute("ServiceMain");
			XAttribute xAttribute2 = xElement.Attribute("UnloadOnStop");
			XAttribute xAttribute3 = xElement.Attribute("BinaryInOneCorePkg");
			if (xAttribute3 != null)
			{
				if (xAttribute3.Value.Equals("1") || xAttribute3.Value.Equals("true"))
				{
					return;
				}
				xAttribute3.Remove();
			}
			XElement xElement2 = new XElement(ToWm.Name.Namespace + "files");
			xAttribute?.Remove();
			xAttribute2?.Remove();
			XElement content = new FileConverter(enviorn).WmFile(ToWm.Name.Namespace, xElement);
			xElement2.Add(content);
			ToWm.Add(xElement2);
		}
	}
}
