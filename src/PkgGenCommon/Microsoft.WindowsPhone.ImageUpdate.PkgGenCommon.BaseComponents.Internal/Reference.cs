using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot(ElementName = "Reference", Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00")]
	public sealed class Reference
	{
		[XmlAttribute("Source")]
		public string Source { get; set; }

		[XmlAttribute("StagingSubDir")]
		public string StagingSubDir { get; set; }

		internal Reference()
		{
		}

		internal Reference(string source, string stagingSubDir)
		{
			Source = source;
			StagingSubDir = stagingSubDir;
		}
	}
}
