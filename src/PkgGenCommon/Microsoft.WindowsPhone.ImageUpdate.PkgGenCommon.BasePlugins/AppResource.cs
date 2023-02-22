using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BasePlugins
{
	[Export(typeof(IPkgPlugin))]
	public class AppResource : OSComponent
	{
		public override bool UseSecurityCompilerPassthrough => true;

		public override string XmlSchemaPath => PkgPlugin.BaseComponentSchemaPath;

		public override string XmlElementUniqueXPath => "@Name";

		protected override void ValidateEntry(IPkgProject packageGenerator, XElement componentEntry)
		{
			XAttribute xAttribute = componentEntry.LocalAttribute("Name");
			if (xAttribute == null || string.IsNullOrEmpty(xAttribute.Value))
			{
				throw new PkgXmlException(componentEntry, "Name needs to be specified for Application or AppResource objects");
			}
		}

		protected override IEnumerable<PkgObject> ProcessEntry(IPkgProject packageGenerator, XElement componentEntry)
		{
			AppResourceBuilder builder = new AppResourceBuilder();
			builder.SetName(componentEntry.LocalAttribute("Name").Value);
			componentEntry.WithLocalAttribute("Suite", delegate(XAttribute x)
			{
				builder.SetSuite(x.Value);
			});
			ProcessFiles<AppResourcePkgObject, AppResourceBuilder>(componentEntry, builder);
			ProcessRegistry<AppResourcePkgObject, AppResourceBuilder>(componentEntry, builder);
			return new List<PkgObject> { builder.ToPkgObject() };
		}
	}
}
