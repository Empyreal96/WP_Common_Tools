using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;

namespace Microsoft.CompPlat.PkgBldr.Interfaces
{
	public interface IPkgPlugin
	{
		string Name { get; }

		string XmlElementName { get; }

		string XmlElementUniqueXPath { get; }

		string XmlSchemaPath { get; }

		string XmlSchemaNameSpace { get; }

		void ConvertEntries(XElement parent, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement component);

		bool Pass(BuildPass pass);
	}
}
