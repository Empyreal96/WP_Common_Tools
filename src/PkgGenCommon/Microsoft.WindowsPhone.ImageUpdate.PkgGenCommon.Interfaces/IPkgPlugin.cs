using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces
{
	public interface IPkgPlugin
	{
		string Name { get; }

		string XmlElementName { get; }

		string XmlElementUniqueXPath { get; }

		string XmlSchemaPath { get; }

		bool UseSecurityCompilerPassthrough { get; }

		void ValidateEntries(IPkgProject packageGenerator, IEnumerable<XElement> componentEntries);

		IEnumerable<PkgObject> ProcessEntries(IPkgProject packageGenerator, IEnumerable<XElement> componentEntries);
	}
}
