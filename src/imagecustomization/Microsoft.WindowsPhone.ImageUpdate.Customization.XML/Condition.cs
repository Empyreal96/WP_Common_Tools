using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization.XML
{
	[XmlRoot(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class Condition
	{
		private bool? isWildCard;

		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public string Value { get; set; }

		[XmlAttribute]
		public bool IsWildCard
		{
			get
			{
				if (!isWildCard.HasValue)
				{
					return false;
				}
				return isWildCard.Value;
			}
			set
			{
				isWildCard = value;
			}
		}

		public bool ShouldSerializeValue()
		{
			return !IsWildCard;
		}

		public bool ShouldSerializeIsWildCard()
		{
			return IsWildCard;
		}

		public Condition()
		{
		}

		public Condition(string name, string value)
			: this(name, value, false)
		{
		}

		public Condition(string name, string value, bool IsWildCard)
		{
			Name = name;
			Value = value;
			this.IsWildCard = IsWildCard;
		}
	}
}
