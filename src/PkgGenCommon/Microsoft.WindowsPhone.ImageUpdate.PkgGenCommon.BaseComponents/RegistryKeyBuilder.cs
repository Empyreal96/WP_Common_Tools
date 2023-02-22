using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents
{
	public sealed class RegistryKeyBuilder
	{
		private RegistryKey key;

		internal RegistryKeyBuilder(string keyName)
		{
			key = new RegistryKey(keyName);
		}

		public RegistryKeyBuilder AddValue(string name, string type, string value)
		{
			RegValueType regValType = RegUtil.RegValueTypeForString(type);
			RegValue regValue = new RegValue();
			regValue.Name = name;
			regValue.RegValType = regValType;
			regValue.Value = value;
			key.Values.Add(regValue);
			return this;
		}

		public RegistryKeyBuilder AddValue(XNode valueElement)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(RegValue));
			using (XmlReader xmlReader = valueElement.CreateReader())
			{
				RegValue item = (RegValue)xmlSerializer.Deserialize(xmlReader);
				key.Values.Add(item);
				return this;
			}
		}

		internal RegistryKey ToPkgObject()
		{
			return key;
		}
	}
}
