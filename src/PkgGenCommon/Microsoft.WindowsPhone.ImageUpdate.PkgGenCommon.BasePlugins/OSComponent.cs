using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BasePlugins
{
	[Export(typeof(IPkgPlugin))]
	public class OSComponent : PkgPlugin
	{
		public override bool UseSecurityCompilerPassthrough => true;

		public override string XmlSchemaPath => PkgPlugin.BaseComponentSchemaPath;

		protected override void ValidateEntry(IPkgProject packageGenerator, XElement componentEntry)
		{
		}

		protected override IEnumerable<PkgObject> ProcessEntry(IPkgProject packageGenerator, XElement componentEntry)
		{
			OSComponentBuilder oSComponentBuilder = new OSComponentBuilder();
			ProcessFiles<OSComponentPkgObject, OSComponentBuilder>(componentEntry, oSComponentBuilder);
			ProcessRegistry<OSComponentPkgObject, OSComponentBuilder>(componentEntry, oSComponentBuilder);
			return new List<PkgObject> { oSComponentBuilder.ToPkgObject() };
		}

		protected void ProcessFiles<T, V>(XElement componentEntry, V builder) where T : OSComponentPkgObject, new() where V : OSComponentBuilder<T, V>
		{
			foreach (XElement item in componentEntry.LocalElements("Files"))
			{
				FileGroupBuilder groupBuilder = builder.AddFileGroup();
				item.WithLocalAttribute("Language", delegate(XAttribute x)
				{
					groupBuilder.SetLanguage(x.Value);
				});
				item.WithLocalAttribute("Resolution", delegate(XAttribute x)
				{
					groupBuilder.SetResolution(x.Value);
				});
				item.WithLocalAttribute("CpuFilter", delegate(XAttribute x)
				{
					groupBuilder.SetCpuId(x.Value);
				});
				foreach (XElement item2 in item.Elements())
				{
					groupBuilder.AddFile(item2);
				}
			}
		}

		protected void ProcessRegistry<T, V>(XElement componentEntry, V builder) where T : OSComponentPkgObject, new() where V : OSComponentBuilder<T, V>
		{
			foreach (XElement item in componentEntry.LocalDescendants("RegImport"))
			{
				builder.AddRegistryImport(item.Attribute("Source").Value);
			}
			foreach (XElement item2 in componentEntry.LocalElements("RegKeys"))
			{
				RegistryKeyGroupBuilder groupBuilder = builder.AddRegistryGroup();
				item2.WithLocalAttribute("Language", delegate(XAttribute x)
				{
					groupBuilder.SetLanguage(x.Value);
				});
				item2.WithLocalAttribute("Resolution", delegate(XAttribute x)
				{
					groupBuilder.SetResolution(x.Value);
				});
				item2.WithLocalAttribute("CpuFilter", delegate(XAttribute x)
				{
					groupBuilder.SetCpuId(x.Value);
				});
				foreach (XElement item3 in item2.Elements())
				{
					RegistryKeyBuilder registryKeyBuilder = groupBuilder.AddRegistryKey(item3.Attribute("KeyName").Value);
					foreach (XElement item4 in item3.LocalElements("RegValue"))
					{
						registryKeyBuilder.AddValue(item4);
					}
				}
			}
		}
	}
}
