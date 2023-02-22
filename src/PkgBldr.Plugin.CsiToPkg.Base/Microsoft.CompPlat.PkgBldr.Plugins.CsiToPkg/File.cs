using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.CsiToPkg
{
	[Export(typeof(IPkgPlugin))]
	internal class File : PkgPlugin
	{
		public override void ConvertEntries(XElement ToPkg, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromCsi)
		{
			MyContainter myContainter = (MyContainter)enviorn.arg;
			XElement files = myContainter.Files;
			XNamespace @namespace = ToPkg.Name.Namespace;
			string attributeValue = PkgBldrHelpers.GetAttributeValue(FromCsi, "name");
			string attributeValue2 = PkgBldrHelpers.GetAttributeValue(FromCsi, "destinationPath");
			string attributeValue3 = PkgBldrHelpers.GetAttributeValue(FromCsi, "sourceName");
			PkgBldrHelpers.GetAttributeValue(FromCsi, "sourcePath");
			string attributeValue4 = PkgBldrHelpers.GetAttributeValue(FromCsi, "importPath");
			PkgBldrHelpers.GetAttributeValue(FromCsi, "buildType");
			XElement xElement = new XElement(files.Name.Namespace + "File");
			string input = attributeValue4.TrimEnd("\\".ToCharArray()) + "\\" + attributeValue3;
			input = enviorn.Macros.Resolve(input);
			xElement.Add(new XAttribute("Source", input));
			string text = null;
			string text2 = null;
			if (attributeValue2 != null)
			{
				text = attributeValue2.TrimEnd("\\".ToCharArray());
				text = enviorn.Macros.Resolve(text);
				if (text.StartsWith("$(ERROR)", StringComparison.OrdinalIgnoreCase))
				{
					Console.WriteLine("warning: can't resolve {0}", attributeValue2);
					return;
				}
				xElement.Add(new XAttribute("DestinationDir", text));
			}
			if (attributeValue != null)
			{
				text2 = attributeValue.TrimEnd("\\".ToCharArray());
				xElement.Add(new XAttribute("Name", text2));
			}
			files.Add(xElement);
			XElement firstDecendant = PkgBldrHelpers.GetFirstDecendant(FromCsi, FromCsi.Name.Namespace + "securityDescriptor");
			if (firstDecendant != null)
			{
				string name = firstDecendant.Attribute("name").Value.ToUpperInvariant();
				SDDL sDDL = myContainter.Security.Lookup(name);
				if (sDDL == null)
				{
					Console.WriteLine("error: cant find matching ACE in lookup table");
					return;
				}
				text = myContainter.Security.Macros.Resolve(text);
				string path = (text + "\\" + text2).ToUpperInvariant();
				myContainter.Security.AddFileAce(path, sDDL);
			}
		}
	}
}
