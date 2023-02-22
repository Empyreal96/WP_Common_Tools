using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.CsiToPkg
{
	[Export(typeof(IPkgPlugin))]
	internal class AssemblyIdentity : PkgPlugin
	{
		public override bool Pass(BuildPass pass)
		{
			return pass == BuildPass.MACRO_PASS;
		}

		public override void ConvertEntries(XElement ToPkg, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromCsi)
		{
			XNamespace @namespace = ToPkg.Name.Namespace;
			Share.PhoneIdentity phoneIdentity = Share.CsiNameToPhoneIdentity(PkgBldrHelpers.GetAttributeValue(FromCsi, "name"));
			string owner = phoneIdentity.Owner;
			string ownerType = phoneIdentity.OwnerType;
			string component = phoneIdentity.Component;
			string subComponent = phoneIdentity.SubComponent;
			string value = "Production";
			string value2 = "MainOS";
			ToPkg.Add(new XAttribute("Owner", owner));
			ToPkg.Add(new XAttribute("OwnerType", ownerType));
			ToPkg.Add(new XAttribute("Component", component));
			ToPkg.Add(new XAttribute("SubComponent", subComponent));
			ToPkg.Add(new XAttribute("ReleaseType", value));
			ToPkg.Add(new XAttribute("Partition", value2));
			((MyContainter)enviorn.arg).Security.PolicyID = component + "." + subComponent;
			base.ConvertEntries(ToPkg, plugins, enviorn, FromCsi);
		}
	}
}
