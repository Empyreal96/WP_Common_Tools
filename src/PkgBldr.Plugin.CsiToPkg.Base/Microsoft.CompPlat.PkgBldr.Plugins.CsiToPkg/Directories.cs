using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.CsiToPkg
{
	[Export(typeof(IPkgPlugin))]
	internal class Directories : PkgPlugin
	{
		public override void ConvertEntries(XElement ToPkg, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromCsi)
		{
			MyContainter myContainter = (MyContainter)enviorn.arg;
			foreach (XElement item in FromCsi.Elements(FromCsi.Name.Namespace + "directory"))
			{
				string attributeValue = PkgBldrHelpers.GetAttributeValue(item, "destinationPath");
				attributeValue = enviorn.Macros.Resolve(attributeValue);
				if (attributeValue.StartsWith("$(ERROR)", StringComparison.OrdinalIgnoreCase))
				{
					Console.WriteLine("warning: can't resolve {0}", attributeValue);
					continue;
				}
				XElement xElement = item.Element(FromCsi.Name.Namespace + "securityDescriptor");
				if (xElement != null)
				{
					string attributeValue2 = PkgBldrHelpers.GetAttributeValue(xElement, "name");
					SDDL sDDL = myContainter.Security.Lookup(attributeValue2);
					if (sDDL == null)
					{
						Console.WriteLine("error: cant find matching ACE in lookup table");
						break;
					}
					attributeValue = myContainter.Security.Macros.Resolve(attributeValue);
					myContainter.Security.AddDirAce(attributeValue, sDDL);
				}
				else
				{
					string sDDL2 = enviorn.Macros.Resolve("$(build.wrpDirSddl)");
					SDDL sDDL3 = new SDDL();
					sDDL3.Owner = SddlHelpers.GetSddlOwner(sDDL2);
					sDDL3.Group = SddlHelpers.GetSddlGroup(sDDL2);
					sDDL3.Dacl = SddlHelpers.GetSddlDacl(sDDL2);
					sDDL3.Sacl = SddlHelpers.GetSddlSacl(sDDL2);
					attributeValue = myContainter.Security.Macros.Resolve(attributeValue);
					myContainter.Security.AddDirAce(attributeValue, sDDL3);
				}
			}
		}
	}
}
