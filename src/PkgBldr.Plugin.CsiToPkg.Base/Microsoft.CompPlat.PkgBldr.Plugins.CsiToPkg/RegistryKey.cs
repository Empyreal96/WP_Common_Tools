using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.CsiToPkg
{
	[Export(typeof(IPkgPlugin))]
	internal class RegistryKey : PkgPlugin
	{
		public override void ConvertEntries(XElement ToPkg, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromCsi)
		{
			string attributeValue = PkgBldrHelpers.GetAttributeValue(FromCsi, "keyName");
			MyContainter myContainter = (MyContainter)enviorn.arg;
			XElement regKeys = myContainter.RegKeys;
			string input = attributeValue.TrimEnd("\\".ToCharArray());
			input = enviorn.Macros.Resolve(input);
			input = RegHelpers.RegKeyNameToMacro(input);
			if (input == null)
			{
				Console.WriteLine("warning: ignoring key", attributeValue);
				return;
			}
			XElement xElement = new XElement(ToPkg.Name.Namespace + "RegKey");
			xElement.Add(new XAttribute("KeyName", input));
			base.ConvertEntries(xElement, plugins, enviorn, FromCsi);
			if (xElement.Elements().Count() > 0)
			{
				Share.MergeNewPkgRegKey(regKeys, xElement);
			}
			XElement xElement2 = FromCsi.Element(FromCsi.Name.Namespace + "securityDescriptor");
			if (xElement2 != null)
			{
				string name = xElement2.Attribute("name").Value.ToUpperInvariant();
				SDDL sDDL = myContainter.Security.Lookup(name);
				if (sDDL == null)
				{
					Console.WriteLine("error: cant find matching ACE in lookup table");
					return;
				}
				string path = RegHelpers.RegMacroToKeyName(input);
				myContainter.Security.AddRegAce(path, sDDL);
			}
		}
	}
}
