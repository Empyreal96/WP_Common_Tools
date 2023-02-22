using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization.XML
{
	[XmlRoot(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class DataAsset : IDefinedIn
	{
		[XmlIgnore]
		public static readonly string SourceFieldName = Strings.txtDataAssetSource;

		[XmlIgnore]
		public string DefinedInFile { get; set; }

		[XmlAttribute]
		public string Source { get; set; }

		[XmlIgnore]
		public string ExpandedSourcePath => ImageCustomizations.ExpandPath(Source);
	}
}
