using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	[XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
	[XmlRoot(ElementName = "DeviceLayout", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class DeviceLayoutValidationScope
	{
		private string[] _excludedScopes;

		public string Scope { get; set; }

		[XmlArrayItem(ElementName = "Scope", Type = typeof(string), IsNullable = false)]
		[XmlArray]
		public string[] ExcludedScopes
		{
			get
			{
				return _excludedScopes;
			}
			set
			{
				_excludedScopes = value;
			}
		}
	}
}
