using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon
{
	public class Macro
	{
		[XmlAttribute("Id")]
		public string Name { get; set; }

		[XmlAttribute("Value")]
		public string StringValue
		{
			get
			{
				return Delegate(Value);
			}
			set
			{
				Value = value;
			}
		}

		[XmlIgnore]
		public object Value { get; set; }

		[XmlIgnore]
		public MacroDelegate Delegate { get; set; }

		public Macro()
		{
			Delegate = (object x) => x.ToString();
		}

		public Macro(string name, object value)
			: this()
		{
			Name = name;
			Value = value;
		}

		public Macro(string name, object value, MacroDelegate del)
			: this(name, value)
		{
			Delegate = del;
		}
	}
}
