using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot(ElementName = "RegKeys", Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00")]
	public class RegGroup : FilterGroup
	{
		[XmlElement("RegKey")]
		public List<RegistryKey> Keys;

		public RegGroup()
		{
			Keys = new List<RegistryKey>();
		}

		public override void Build(IPackageGenerator pkgGen, SatelliteId satelliteId)
		{
			Keys.ForEach(delegate(RegistryKey x)
			{
				x.Build(pkgGen, satelliteId);
			});
		}

		public static RegGroup Load(XmlReader regFileReader)
		{
			try
			{
				return (RegGroup)new XmlSerializer(typeof(RegGroup), "urn:Microsoft.WindowsPhone/PackageSchema.v8.00").Deserialize(regFileReader);
			}
			catch (InvalidOperationException ex)
			{
				if (ex.InnerException != null)
				{
					throw ex.InnerException;
				}
				throw;
			}
		}
	}
}
