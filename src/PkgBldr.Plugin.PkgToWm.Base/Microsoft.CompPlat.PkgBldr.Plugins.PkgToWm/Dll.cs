using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;
using Microsoft.CompPlat.PkgBldr.Tools;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class Dll : PkgPlugin
	{
		public override void ConvertEntries(XElement toWm, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement fromPkg)
		{
			ComData obj = (ComData)enviorn.arg;
			XElement xElement = new FileConverter(enviorn).WmFile(toWm.Name.Namespace, fromPkg);
			obj.Files.Add(xElement);
			string attributeValue = PkgBldrHelpers.GetAttributeValue(xElement, "destinationDir");
			string text = PkgBldrHelpers.GetAttributeValue(xElement, "name");
			string attributeValue2 = PkgBldrHelpers.GetAttributeValue(xElement, "source");
			if (attributeValue == null)
			{
				throw new PkgGenException("a destination dir is required for a COM inProcServer DLL");
			}
			if (text == null)
			{
				text = LongPath.GetFileName(attributeValue2);
			}
			obj.InProcServer.Add(new XAttribute("path", attributeValue + "\\" + text));
		}
	}
}
