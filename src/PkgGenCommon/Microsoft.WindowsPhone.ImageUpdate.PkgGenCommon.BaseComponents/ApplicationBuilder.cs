using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents
{
	public sealed class ApplicationBuilder : AppResourceBuilder<ApplicationPkgObject, ApplicationBuilder>
	{
		public ApplicationBuilder SetRequiredCapabilities(IEnumerable<XElement> requiredCapabilities)
		{
			pkgObject.RequiredCapabilities = new XElement(XName.Get("RequiredCapabilities", "urn:Microsoft.WindowsPhone/PackageSchema.v8.00"), requiredCapabilities);
			return this;
		}

		public ApplicationBuilder SetPrivateResources(IEnumerable<XElement> privateResources)
		{
			pkgObject.PrivateResources = new XElement(XName.Get("PrivateResources", "urn:Microsoft.WindowsPhone/PackageSchema.v8.00"), privateResources);
			return this;
		}
	}
}
