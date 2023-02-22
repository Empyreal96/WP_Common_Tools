using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BasePlugins
{
	[Export(typeof(IPkgPlugin))]
	public class Driver : OSComponent
	{
		public override bool UseSecurityCompilerPassthrough => true;

		public override string XmlSchemaPath => PkgPlugin.BaseComponentSchemaPath;

		public override string XmlElementUniqueXPath => "@InfSource";

		public override void ValidateEntries(IPkgProject packageGenerator, IEnumerable<XElement> componentEntries)
		{
		}

		protected override IEnumerable<PkgObject> ProcessEntry(IPkgProject packageGenerator, XElement componentEntry)
		{
			DriverBuilder driverBuilder = new DriverBuilder(componentEntry.LocalAttribute("InfSource").Value);
			foreach (XElement item in componentEntry.LocalElements("Reference"))
			{
				driverBuilder.AddReference(item);
			}
			foreach (XElement item2 in componentEntry.LocalElements("Security"))
			{
				driverBuilder.AddSecurity(item2);
			}
			ProcessFiles<DriverPkgObject, DriverBuilder>(componentEntry, driverBuilder);
			ProcessRegistry<DriverPkgObject, DriverBuilder>(componentEntry, driverBuilder);
			return new List<PkgObject> { driverBuilder.ToPkgObject() };
		}
	}
}
