using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.DeviceManagement.MCSF
{
	[Export(typeof(IPkgPlugin))]
	public class SettingsGroup : PkgPlugin
	{
		public override string XmlSchemaPath => "Microsoft.WindowsPhone.DeviceManagement.MCSF.ProjSchema.xsd";

		public override void ValidateEntries(IPkgProject packageGenerator, IEnumerable<XElement> componentEntries)
		{
		}

		public override IEnumerable<PkgObject> ProcessEntries(IPkgProject packageGenerator, IEnumerable<XElement> componentEntries)
		{
			if (packageGenerator == null)
			{
				throw new ArgumentNullException("packageGenerator");
			}
			if (componentEntries == null)
			{
				throw new ArgumentNullException("componentEntries");
			}
			string tempDirectory = packageGenerator.TempDirectory;
			PkgRegWriter pkgRegWriter = new PkgRegWriter(packageGenerator.Attributes["Name"], tempDirectory, packageGenerator.MacroResolver);
			pkgRegWriter.GenerateReadablePolicyXML = true;
			foreach (XElement componentEntry in componentEntries)
			{
				if (componentEntry.Name.LocalName.Equals("SettingsGroup", StringComparison.Ordinal))
				{
					pkgRegWriter.WriteSettingsGroup(componentEntry);
				}
			}
			return new List<PkgObject> { pkgRegWriter.ToPkgObject() };
		}
	}
}
