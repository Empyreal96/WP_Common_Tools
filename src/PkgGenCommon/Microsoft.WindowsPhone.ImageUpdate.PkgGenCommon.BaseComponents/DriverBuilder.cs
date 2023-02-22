using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents
{
	public sealed class DriverBuilder : OSComponentBuilder<DriverPkgObject, DriverBuilder>
	{
		private List<Reference> references;

		private List<Security> security;

		public DriverBuilder(string infSource)
		{
			pkgObject.InfSource = infSource;
			references = new List<Reference>();
			security = new List<Security>();
		}

		public DriverBuilder AddReference(string source, string stagingSubDir)
		{
			references.Add(new Reference(source, stagingSubDir));
			return this;
		}

		public DriverBuilder AddReference(XElement reference)
		{
			references.Add(reference.FromXElement<Reference>());
			return this;
		}

		public DriverBuilder AddSecurity(string infSectionName)
		{
			security.Add(new Security(infSectionName));
			return this;
		}

		public DriverBuilder AddSecurity(XElement security)
		{
			this.security.Add(security.FromXElement<Security>());
			return this;
		}

		public override DriverPkgObject ToPkgObject()
		{
			RegisterMacro("runtime.default", "$(runtime.drivers)");
			RegisterMacro("env.default", "$(env.drivers)");
			pkgObject.References.AddRange(references);
			pkgObject.InfSecurity.AddRange(security);
			return base.ToPkgObject();
		}
	}
}
