using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins
{
	[Export(typeof(IPkgPlugin))]
	internal class AccountPrivileges : Account
	{
		public override string XmlElementName => "accountPrivileges";

		public override void ConvertEntries(XElement groupTrustee, Dictionary<string, IPkgPlugin> plugins, Config environ, XElement component)
		{
			string text = null;
			bool flag = true;
			foreach (XElement item in component.Descendants())
			{
				text += (flag ? null : " ");
				text += item.Attribute("name").Value;
				flag = false;
			}
			groupTrustee.Add(new XElement("privileges", text));
		}
	}
}
