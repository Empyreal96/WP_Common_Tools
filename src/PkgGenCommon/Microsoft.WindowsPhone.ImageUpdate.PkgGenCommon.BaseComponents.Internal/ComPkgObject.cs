using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot(ElementName = "ComServer", Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00")]
	public sealed class ComPkgObject : OSComponentPkgObject
	{
		[XmlElement("Dll")]
		public ComDll ComDll { get; set; }

		[XmlArray("Classes")]
		[XmlArrayItem(typeof(ComClass), ElementName = "Class")]
		public List<ComClass> Classes { get; }

		[XmlArray("Interfaces")]
		[XmlArrayItem(typeof(ComInterface), ElementName = "Interface")]
		public List<ComInterface> Interfaces { get; }

		public ComPkgObject()
		{
			Classes = new List<ComClass>();
			Interfaces = new List<ComInterface>();
		}

		public bool ShouldSerializeClasses()
		{
			if (Classes != null)
			{
				return Classes.Count > 0;
			}
			return false;
		}

		public bool ShouldSerializeInterfaces()
		{
			if (Interfaces != null)
			{
				return Interfaces.Count > 0;
			}
			return false;
		}

		protected override void DoPreprocess(PackageProject proj, IMacroResolver macroResolver)
		{
			ComDll.Preprocess(macroResolver);
			Classes.ForEach(delegate(ComClass x)
			{
				x.Preprocess(proj, macroResolver);
			});
			Interfaces.ForEach(delegate(ComInterface x)
			{
				x.Preprocess(proj, macroResolver);
			});
			base.DoPreprocess(proj, macroResolver);
		}

		protected override void DoBuild(IPackageGenerator pkgGen)
		{
			base.DoBuild(pkgGen);
			ComDll.Build(pkgGen);
			if (pkgGen.BuildPass == BuildPass.BuildTOC)
			{
				return;
			}
			if (Classes != null)
			{
				Classes.ForEach(delegate(ComClass x)
				{
					x.Dll = ComDll;
				});
				Classes.ForEach(delegate(ComClass x)
				{
					x.Build(pkgGen);
				});
			}
			if (Interfaces != null)
			{
				Interfaces.ForEach(delegate(ComInterface x)
				{
					x.Build(pkgGen);
				});
			}
		}
	}
}
