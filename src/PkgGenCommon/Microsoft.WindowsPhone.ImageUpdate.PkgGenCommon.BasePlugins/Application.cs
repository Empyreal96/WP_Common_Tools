using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BasePlugins
{
	[Export(typeof(IPkgPlugin))]
	public class Application : AppResource
	{
		public override void ValidateEntries(IPkgProject packageGenerator, IEnumerable<XElement> componentEntries)
		{
			foreach (XElement componentEntry in componentEntries)
			{
				ValidateEntry(packageGenerator, componentEntry);
			}
			foreach (IGrouping<string, XElement> item in from x in componentEntries
				group x by string.Format("Name:{0}, Suite:{1}", (string)x.LocalAttribute("Name"), ((string)x.LocalAttribute("Suite")) ?? ""))
			{
				if (item.Count() > 1)
				{
					throw new PkgXmlException(item.First(), "Cannot have more than two Application components with same id:{0}", item.Key);
				}
			}
		}

		protected override IEnumerable<PkgObject> ProcessEntry(IPkgProject packageGenerator, XElement componentEntry)
		{
			ApplicationBuilder builder = new ApplicationBuilder();
			builder.SetName(componentEntry.LocalAttribute("Name").Value);
			componentEntry.WithLocalAttribute("Suite", delegate(XAttribute x)
			{
				builder.SetSuite(x.Value);
			});
			ProcessFiles<ApplicationPkgObject, ApplicationBuilder>(componentEntry, builder);
			ProcessRegistry<ApplicationPkgObject, ApplicationBuilder>(componentEntry, builder);
			return new List<PkgObject> { builder.ToPkgObject() };
		}
	}
}
