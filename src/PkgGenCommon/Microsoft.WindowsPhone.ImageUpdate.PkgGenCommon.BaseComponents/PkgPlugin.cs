using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents
{
	public abstract class PkgPlugin : IPkgPlugin
	{
		internal static string BaseComponentSchemaPath = "Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BasePlugins.xsd";

		public virtual string Name => XmlElementName;

		public virtual string XmlElementName => GetType().Name;

		public virtual string XmlElementUniqueXPath => null;

		public abstract string XmlSchemaPath { get; }

		public virtual bool UseSecurityCompilerPassthrough => false;

		public virtual void ValidateEntries(IPkgProject packageGenerator, IEnumerable<XElement> componentEntries)
		{
			foreach (XElement componentEntry in componentEntries)
			{
				ValidateEntry(packageGenerator, componentEntry);
			}
		}

		public virtual IEnumerable<PkgObject> ProcessEntries(IPkgProject packageGenerator, IEnumerable<XElement> componentEntries)
		{
			List<PkgObject> list = new List<PkgObject>();
			foreach (XElement componentEntry in componentEntries)
			{
				IEnumerable<PkgObject> enumerable = ProcessEntry(packageGenerator, componentEntry);
				if (enumerable != null)
				{
					list.AddRange(enumerable);
				}
			}
			return list;
		}

		protected virtual void ValidateEntry(IPkgProject packageGenerator, XElement componentEntry)
		{
			throw new NotImplementedException();
		}

		protected virtual IEnumerable<PkgObject> ProcessEntry(IPkgProject packageGenerator, XElement componentEntry)
		{
			throw new NotImplementedException();
		}
	}
}
