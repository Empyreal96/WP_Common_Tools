using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class Executables : PathElements<Executable>
	{
		[XmlElement(ElementName = "Executable")]
		public new List<Executable> PathElementCollection
		{
			get
			{
				return base.PathElementCollection;
			}
			set
			{
				base.PathElementCollection = value;
			}
		}

		public Executables()
		{
			base.ElementName = "Executable";
		}
	}
}
