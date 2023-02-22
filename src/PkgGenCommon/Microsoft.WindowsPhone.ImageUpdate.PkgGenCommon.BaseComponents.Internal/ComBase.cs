using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	public abstract class ComBase : PkgObject
	{
		protected string _defaultKey;

		[XmlAttribute("Id")]
		public string Id { get; set; }

		[XmlAttribute("Version")]
		public string Version { get; set; }

		[XmlAttribute("TypeLib")]
		public string TypeLib { get; set; }

		[XmlElement("RegKey")]
		public List<RegistryKey> RegKeys { get; }

		public ComBase()
		{
			RegKeys = new List<RegistryKey>();
		}

		protected override void DoBuild(IPackageGenerator pkgGen)
		{
			base.DoBuild(pkgGen);
			if (TypeLib != null)
			{
				pkgGen.AddRegValue(_defaultKey + "\\TypeLib", "@", RegValueType.String, TypeLib);
			}
			if (RegKeys != null)
			{
				RegKeys.ForEach(delegate(RegistryKey x)
				{
					x.Build(pkgGen);
				});
			}
		}

		protected override void DoPreprocess(PackageProject proj, IMacroResolver macroResolver)
		{
			Id = macroResolver.Resolve(Id, MacroResolveOptions.SkipOnUnknownMacro);
		}
	}
}
