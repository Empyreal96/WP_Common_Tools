using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.CompDB
{
	public class CompDBPublishingPackageInfo
	{
		[XmlAttribute]
		public string Path;

		[XmlAttribute]
		public string PackageHash;

		[XmlAttribute]
		public string ChunkName;

		[XmlAttribute]
		public string ChunkRelativePath;

		public CompDBPublishingPackageInfo()
		{
		}

		public CompDBPublishingPackageInfo(CompDBPublishingPackageInfo srcPkg)
		{
			Path = srcPkg.Path;
			PackageHash = srcPkg.PackageHash;
			ChunkName = srcPkg.ChunkName;
			ChunkRelativePath = srcPkg.ChunkRelativePath;
		}

		public CompDBPublishingPackageInfo(CompDBPayloadInfo srcPayload)
		{
			Path = srcPayload.Path;
			PackageHash = srcPayload.PayloadHash;
			ChunkName = srcPayload.ChunkName;
			ChunkRelativePath = srcPayload.ChunkPath;
		}

		public override string ToString()
		{
			return Path + " (" + ChunkName + ")";
		}
	}
}
