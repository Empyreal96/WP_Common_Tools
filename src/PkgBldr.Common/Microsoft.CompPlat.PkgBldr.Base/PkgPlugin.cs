using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Base
{
	public abstract class PkgPlugin : IPkgPlugin
	{
		internal static string BaseComponentSchemaPath = "PkgBldr.WM.XSD\\BasePlugins.xsd";

		public virtual string Name => XmlElementName;

		public virtual string XmlElementName => char.ToLowerInvariant(GetType().Name[0]) + GetType().Name.Substring(1);

		public virtual string XmlElementUniqueXPath => XmlSchemaPath;

		public virtual string XmlSchemaPath => BaseComponentSchemaPath;

		public virtual string XmlSchemaNameSpace => null;

		public virtual bool Pass(BuildPass pass)
		{
			return pass == BuildPass.PLUGIN_PASS;
		}

		public virtual void ConvertEntries(XElement parent, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement component)
		{
			if (plugins == null)
			{
				throw new ArgumentNullException("plugins");
			}
			if (enviorn == null)
			{
				throw new ArgumentNullException("enviorn");
			}
			if (component == null)
			{
				throw new ArgumentNullException("component");
			}
			foreach (XElement item in component.Elements())
			{
				if (plugins.ContainsKey(item.Name.LocalName) && plugins[item.Name.LocalName].Pass(enviorn.Pass))
				{
					plugins[item.Name.LocalName].ConvertEntries(parent, plugins, enviorn, item);
				}
			}
		}
	}
}
