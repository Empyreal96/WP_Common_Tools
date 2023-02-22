using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Base.Security.SecurityPolicy;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins
{
	[Export(typeof(IPkgPlugin))]
	internal class AccountCapabilities : Account
	{
		public override string XmlElementName => "accountCapabilities";

		public override void ConvertEntries(XElement groupTrustee, Dictionary<string, IPkgPlugin> plugins, Config environ, XElement component)
		{
			string text = null;
			bool flag = true;
			foreach (XElement item in component.Descendants())
			{
				text += (flag ? null : " ");
				text += SidBuilder.BuildServiceCapabilitySidString(item.Attribute("id").Value);
				flag = false;
			}
			groupTrustee.Add(new XElement("capabilities", text));
		}
	}
}
