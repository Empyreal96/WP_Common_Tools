using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.CsiToPkg
{
	[Export(typeof(IPkgPlugin))]
	internal class TrustInfo : PkgPlugin
	{
		public override bool Pass(BuildPass pass)
		{
			return pass == BuildPass.MACRO_PASS;
		}

		public override void ConvertEntries(XElement ToPkg, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromCsi)
		{
			MyContainter myContainter = (MyContainter)enviorn.arg;
			foreach (XElement item in FromCsi.Descendants(FromCsi.Name.Namespace + "securityDescriptorDefinition"))
			{
				string attributeValue = PkgBldrHelpers.GetAttributeValue(item, "name");
				string attributeValue2 = PkgBldrHelpers.GetAttributeValue(item, "sddl");
				attributeValue2 = enviorn.Macros.Resolve(attributeValue2);
				if (enviorn.Macros.PassThrough(attributeValue2))
				{
					Console.WriteLine("error: can't pass through SDDL macro {0}", attributeValue2);
					continue;
				}
				SDDL sDDL = new SDDL();
				sDDL.Owner = SddlHelpers.GetSddlOwner(attributeValue2);
				sDDL.Group = SddlHelpers.GetSddlGroup(attributeValue2);
				sDDL.Dacl = SddlHelpers.GetSddlDacl(attributeValue2);
				sDDL.Sacl = SddlHelpers.GetSddlSacl(attributeValue2);
				myContainter.Security.AddToLookupTable(attributeValue.ToUpperInvariant(), sDDL);
			}
		}
	}
}
