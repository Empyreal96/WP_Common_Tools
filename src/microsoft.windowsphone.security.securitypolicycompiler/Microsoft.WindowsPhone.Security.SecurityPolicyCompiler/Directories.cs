using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class Directories : PathElements<Directory>
	{
		[XmlElement(ElementName = "Directory")]
		public new List<Directory> PathElementCollection
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

		public Directories()
		{
			base.ElementName = "Directory";
		}
	}
}
