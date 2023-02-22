using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class Files : PkgPlugin
	{
		public override void ConvertEntries(XElement toWm, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement fromPkg)
		{
			FileConverter fileConverter = (FileConverter)enviorn.arg;
			if (PkgBldrHelpers.GetAttributeValue(fromPkg, "Resolution") != null)
			{
				enviorn.Logger.LogWarning("resolution content not supported");
				return;
			}
			string attributeValue = PkgBldrHelpers.GetAttributeValue(fromPkg, "Language");
			if (attributeValue != null)
			{
				if (!attributeValue.Equals("*"))
				{
					enviorn.Logger.LogWarning("setting Language={0} to language=*", attributeValue);
				}
				toWm = PkgBldrHelpers.AddIfNotFound(toWm, "language");
				string attributeValue2 = PkgBldrHelpers.GetAttributeValue(fromPkg, "buildFilter");
				if (attributeValue2 != null)
				{
					string value = Helpers.ConvertBuildFilter(attributeValue2);
					toWm.Add(new XAttribute("buildFilter", value));
				}
				fileConverter.IsLangFile = true;
			}
			XElement xElement = new XElement(toWm.Name.Namespace + "files");
			base.ConvertEntries(xElement, plugins, enviorn, fromPkg);
			if (xElement.HasElements)
			{
				string text = Helpers.GenerateWmBuildFilter(fromPkg, enviorn.Logger);
				if (text != null)
				{
					xElement.Add(new XAttribute("buildFilter", text));
				}
				toWm.Add(xElement);
			}
		}
	}
}
