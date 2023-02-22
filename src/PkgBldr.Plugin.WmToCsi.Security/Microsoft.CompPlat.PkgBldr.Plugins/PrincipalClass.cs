using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins
{
	[Export(typeof(IPkgPlugin))]
	internal class PrincipalClass : Authorization
	{
		private static string principalClassRegKeyName = "PrincipalClasses\\";

		public override string XmlElementName => "principalClass";

		public override void ConvertEntries(XElement authorizationRegKeys, Dictionary<string, IPkgPlugin> plugins, Config environ, XElement component)
		{
			string value = component.Attribute("name").Value;
			string text = Authorization.authorizationRegKeyRoot + principalClassRegKeyName + value;
			uint num = 1u;
			XElement xElement = new XElement(authorizationRegKeys.Name.Namespace + "registryKey");
			xElement.Add(new XAttribute("keyName", text));
			foreach (XElement item in component.Descendants().Descendants())
			{
				XElement xElement2 = new XElement(authorizationRegKeys.Name.Namespace + "registryKey");
				xElement2.Add(new XAttribute("keyName", text + "\\Certificate" + num.ToString(CultureInfo.InvariantCulture)));
				foreach (XAttribute item2 in item.Attributes())
				{
					XElement xElement3 = new XElement(authorizationRegKeys.Name.Namespace + "registryValue");
					xElement3.Add(new XAttribute("name", item2.Name.ToString()));
					xElement3.Add(new XAttribute("value", item2.Value));
					xElement3.Add(new XAttribute("valueType", "REG_SZ"));
					xElement2.Add(xElement3);
				}
				authorizationRegKeys.Add(xElement2);
				num++;
			}
			authorizationRegKeys.Add(xElement);
		}
	}
}
