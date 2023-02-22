using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.CompDB
{
	public class CompDBChunkMapItem
	{
		[XmlAttribute]
		public string ChunkName;

		[XmlAttribute]
		public string Path;

		[XmlAttribute]
		public DesktopCompDBGen.PackageTypes Type;

		public CompDBChunkMapItem()
		{
		}

		public CompDBChunkMapItem(CompDBChunkMapItem src)
		{
			ChunkName = src.ChunkName;
			Path = src.Path;
			Type = src.Type;
		}

		public override string ToString()
		{
			return Path + " (" + ChunkName + "\\" + Type.ToString() + ")";
		}
	}
}
