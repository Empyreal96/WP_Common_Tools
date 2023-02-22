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
	public class Service : OSComponent
	{
		public override bool UseSecurityCompilerPassthrough => true;

		public override string XmlSchemaPath => PkgPlugin.BaseComponentSchemaPath;

		public override string XmlElementUniqueXPath => "@Name";

		protected override void ValidateEntry(IPkgProject packageGenerator, XElement componentEntry)
		{
			IEnumerable<XElement> source = componentEntry.LocalElements("Executable") ?? new XElement[0];
			IEnumerable<XElement> source2 = componentEntry.LocalElements("ServiceDll") ?? new XElement[0];
			string text = (string)componentEntry.LocalAttribute("Name");
			if (source.Count() + source2.Count() == 0)
			{
				throw new PkgXmlException(componentEntry, "Service '{0}': Executable or ServiceDll is missing.", text);
			}
			if (source.Count() + source2.Count() > 1)
			{
				throw new PkgXmlException(componentEntry, "Service '{0}': Can only specify one entry file type. Excutable or ServiceDll.", text);
			}
			if (source2.Count() > 0 && componentEntry.LocalAttribute("SvcHostGroupName") == null)
			{
				throw new PkgXmlException(componentEntry, "Service '{0}': SvcHostGroupName is required when when ServiceDll is declared.", text);
			}
			if (source.Count() > 0 && componentEntry.LocalAttribute("SvcHostGroupName") != null)
			{
				throw new PkgXmlException(componentEntry, "Service '{0}': SvcHostGroupName can only be set when ServiceDll is declared.", text);
			}
			if (componentEntry.LocalElements("FailureActions").Count() != 1)
			{
				throw new PkgXmlException(componentEntry, "'FailureActions' element is required for 'Service' object.");
			}
		}

		protected override IEnumerable<PkgObject> ProcessEntry(IPkgProject packageGenerator, XElement componentEntry)
		{
			ServiceBuilder builder = new ServiceBuilder(componentEntry.LocalAttribute("Name").Value);
			componentEntry.WithLocalAttribute("DisplayName", delegate(XAttribute x)
			{
				builder.SetDisplayName(x.Value);
			});
			componentEntry.WithLocalAttribute("Description", delegate(XAttribute x)
			{
				builder.SetDescription(x.Value);
			});
			componentEntry.WithLocalAttribute("Group", delegate(XAttribute x)
			{
				builder.SetGroup(x.Value);
			});
			componentEntry.WithLocalAttribute("DependOnGroup", delegate(XAttribute x)
			{
				builder.SetDependOnGroup(x.Value);
			});
			componentEntry.WithLocalAttribute("DependOnService", delegate(XAttribute x)
			{
				builder.SetDependOnService(x.Value);
			});
			componentEntry.WithLocalAttribute("Start", delegate(XAttribute x)
			{
				builder.SetStartMode(x.Value);
			});
			componentEntry.WithLocalAttribute("Type", delegate(XAttribute x)
			{
				builder.SetType(x.Value);
			});
			componentEntry.WithLocalAttribute("ErrorControl", delegate(XAttribute x)
			{
				builder.SetErrorControl(x.Value);
			});
			XElement xElement = componentEntry.LocalElement("FailureActions");
			builder.FailureActions.SetResetPeriod((string)xElement.LocalAttribute("ResetPeriod"));
			xElement.WithLocalAttribute("Command", delegate(XAttribute x)
			{
				builder.FailureActions.SetCommand((string)x);
			});
			xElement.WithLocalAttribute("RebootMessage", delegate(XAttribute x)
			{
				builder.FailureActions.SetRebootMessage((string)x);
			});
			foreach (XElement item in xElement.Elements())
			{
				builder.FailureActions.AddFailureAction(item);
			}
			if (componentEntry.LocalElement("Executable") != null)
			{
				builder.AddExecutable(componentEntry.LocalElement("Executable"));
			}
			else
			{
				builder.SetSvcHostGroupName(componentEntry.LocalAttribute("SvcHostGroupName").Value);
				builder.AddServiceDll(componentEntry.LocalElement("ServiceDll"));
			}
			ProcessFiles<ServicePkgObject, ServiceBuilder>(componentEntry, builder);
			ProcessRegistry<ServicePkgObject, ServiceBuilder>(componentEntry, builder);
			return new List<PkgObject> { builder.ToPkgObject() };
		}
	}
}
