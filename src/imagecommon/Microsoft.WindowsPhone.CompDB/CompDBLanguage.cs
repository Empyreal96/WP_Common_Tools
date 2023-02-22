using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.CompDB
{
	public class CompDBLanguage
	{
		[XmlAttribute]
		public string Id;

		public CompDBLanguage()
		{
		}

		public CompDBLanguage(string id)
		{
			Id = id;
		}

		public CompDBLanguage(CompDBLanguage srcLang)
		{
			Id = srcLang.Id;
		}

		public override string ToString()
		{
			return Id;
		}
	}
}
