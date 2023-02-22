using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization.XML
{
	[XmlRoot(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class Import : IDefinedIn
	{
		[XmlIgnore]
		public const string SourceFieldName = "Import source";

		[XmlIgnore]
		public string DefinedInFile { get; set; }

		[XmlAttribute]
		public string Source { get; set; }

		[XmlIgnore]
		public string ExpandedSourcePath => ImageCustomizations.ExpandPath(Source);

		public Import()
		{
		}

		public Import(string source)
		{
			Source = source;
		}
	}
}
