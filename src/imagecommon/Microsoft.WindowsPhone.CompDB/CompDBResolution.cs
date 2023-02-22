using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.CompDB
{
	public class CompDBResolution
	{
		[XmlAttribute]
		public string Id;

		public CompDBResolution()
		{
		}

		public CompDBResolution(string id)
		{
			Id = id;
		}

		public CompDBResolution(CompDBResolution srcRes)
		{
			Id = srcRes.Id;
		}

		public override string ToString()
		{
			return Id;
		}
	}
}
