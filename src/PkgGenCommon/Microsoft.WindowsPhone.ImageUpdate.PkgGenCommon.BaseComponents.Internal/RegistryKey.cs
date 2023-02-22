using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot(ElementName = "RegistryKey", Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00")]
	public class RegistryKey : PkgElement
	{
		[XmlAttribute("KeyName")]
		public string KeyName;

		[XmlElement("RegValue")]
		public List<RegValue> Values;

		public RegistryKey()
		{
			Values = new List<RegValue>();
		}

		public RegistryKey(string keyName)
			: this()
		{
			KeyName = keyName;
		}

		public override void Build(IPackageGenerator pkgGen, SatelliteId satelliteId)
		{
			pkgGen.AddRegKey(KeyName, satelliteId);
			Values.ForEach(delegate(RegValue v)
			{
				pkgGen.AddRegValue(KeyName, v.Name, v.RegValType, v.Value, satelliteId);
			});
		}
	}
}
