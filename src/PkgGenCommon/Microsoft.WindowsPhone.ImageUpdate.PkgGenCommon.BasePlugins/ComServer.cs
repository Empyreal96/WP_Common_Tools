using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BasePlugins
{
	[Export(typeof(IPkgPlugin))]
	public class ComServer : OSComponent
	{
		public override bool UseSecurityCompilerPassthrough => true;

		public override string XmlSchemaPath => PkgPlugin.BaseComponentSchemaPath;

		protected override void ValidateEntry(IPkgProject packageGenerator, XElement componentEntry)
		{
			if (componentEntry.LocalElements("Dll").Count() != 1)
			{
				throw new PkgXmlException(componentEntry, "ComServers must have one and only one Dll element.");
			}
			foreach (XElement item in componentEntry.LocalDescendants("Class"))
			{
				string text = packageGenerator.MacroResolver.Resolve(item.LocalAttribute("Id").Value);
				Guid result;
				if (!Guid.TryParseExact(text, "B", out result))
				{
					throw new PkgXmlException(item, "Invalid COM class ID:'{0}'", text);
				}
			}
			foreach (XElement item2 in componentEntry.LocalDescendants("Interface"))
			{
				string text2 = packageGenerator.MacroResolver.Resolve(item2.LocalAttribute("Id").Value);
				Guid result2;
				if (!Guid.TryParseExact(text2, "B", out result2))
				{
					throw new PkgXmlException(item2, "Invalid COM interface ID:'{0}'", text2);
				}
			}
		}

		protected override IEnumerable<PkgObject> ProcessEntry(IPkgProject packageGenerator, XElement componentEntry)
		{
			ComServerBuilder comServerBuilder = new ComServerBuilder();
			comServerBuilder.SetComDll(componentEntry.LocalElement("Dll"));
			foreach (XElement item in componentEntry.LocalDescendants("Class"))
			{
				comServerBuilder.AddClass(item);
			}
			foreach (XElement item2 in componentEntry.LocalDescendants("Interface"))
			{
				comServerBuilder.AddInterface(item2);
			}
			ProcessFiles<ComPkgObject, ComServerBuilder>(componentEntry, comServerBuilder);
			ProcessRegistry<ComPkgObject, ComServerBuilder>(componentEntry, comServerBuilder);
			return new List<PkgObject> { comServerBuilder.ToPkgObject() };
		}
	}
}
