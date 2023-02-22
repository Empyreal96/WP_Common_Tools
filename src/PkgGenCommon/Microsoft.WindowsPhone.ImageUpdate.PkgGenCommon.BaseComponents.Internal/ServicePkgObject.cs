using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot(ElementName = "Service", Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00")]
	public sealed class ServicePkgObject : OSComponentPkgObject
	{
		[XmlAttribute("Name")]
		public string Name { get; set; }

		[XmlAttribute("DisplayName")]
		public string DisplayName { get; set; }

		[XmlAttribute("Description")]
		public string Description { get; set; }

		[XmlAttribute("Group")]
		public string Group { get; set; }

		[XmlAttribute("SvcHostGroupName")]
		public string SvcHostGroupName { get; set; }

		[XmlAttribute("Start")]
		public ServiceStartMode StartMode { get; set; }

		[XmlAttribute("Type")]
		public ServiceType SvcType { get; set; }

		[XmlAttribute("ErrorControl")]
		public ErrorControlOption ErrorControl { get; set; }

		[XmlAttribute("DependOnGroup")]
		public string DependOnGroup { get; set; }

		[XmlAttribute("DependOnService")]
		public string DependOnService { get; set; }

		[XmlAnyElement("RequiredCapabilities")]
		public XElement RequiredCapabilities { get; set; }

		[XmlAnyElement("PrivateResources")]
		public XElement PrivateResources { get; set; }

		[XmlElement("ServiceDll", typeof(SvcDll))]
		[XmlElement("Executable", typeof(SvcExe))]
		public SvcEntry SvcEntry { get; set; }

		[XmlElement("FailureActions")]
		public FailureActionsPkgObject FailureActions { get; set; }

		public ServicePkgObject()
		{
			SvcEntry = null;
			FailureActions = null;
			ErrorControl = ErrorControlOption.Normal;
		}

		public bool ShouldSerializeErrorControl()
		{
			return ErrorControl != ErrorControlOption.Normal;
		}

		protected override void DoPreprocess(PackageProject proj, IMacroResolver macroResolver)
		{
			SvcDll svcDll = SvcEntry as SvcDll;
			if (svcDll != null)
			{
				svcDll.HostGroupName = SvcHostGroupName;
				svcDll.ServiceName = Name;
			}
			SvcEntry.Preprocess(macroResolver);
			base.DoPreprocess(proj, macroResolver);
		}

		protected override void DoBuild(IPackageGenerator pkgGen)
		{
			base.DoBuild(pkgGen);
			if (pkgGen.BuildPass != 0)
			{
				pkgGen.AddRegValue("$(hklm.service)", "PreshutdownTimeout", RegValueType.DWord, $"{20000u:X8}");
				if (StartMode == ServiceStartMode.DelayedAuto)
				{
					pkgGen.AddRegValue("$(hklm.service)", "Start", RegValueType.DWord, $"{2u:X8}");
					pkgGen.AddRegValue("$(hklm.service)", "DelayedAutoStart", RegValueType.DWord, $"{1u:X8}");
				}
				else
				{
					pkgGen.AddRegValue("$(hklm.service)", "Start", RegValueType.DWord, $"{(uint)StartMode:X8}");
				}
				pkgGen.AddRegValue("$(hklm.service)", "Type", RegValueType.DWord, $"{(uint)SvcType:X8}");
				pkgGen.AddRegValue("$(hklm.service)", "ErrorControl", RegValueType.DWord, $"{(uint)ErrorControl:X8}");
				if (DisplayName != null)
				{
					pkgGen.AddRegValue("$(hklm.service)", "DisplayName", RegValueType.String, DisplayName);
				}
				if (Description != null)
				{
					pkgGen.AddRegValue("$(hklm.service)", "Description", RegValueType.String, Description);
				}
				if (Group != null)
				{
					pkgGen.AddRegValue("$(hklm.service)", "Group", RegValueType.String, Group);
				}
				if (DependOnGroup != null)
				{
					pkgGen.AddRegValue("$(hklm.service)", "DependOnGroup", RegValueType.MultiString, DependOnGroup);
				}
				if (DependOnService != null)
				{
					pkgGen.AddRegValue("$(hklm.service)", "DependOnService", RegValueType.MultiString, DependOnService);
				}
				if (FailureActions != null)
				{
					FailureActions.Build(pkgGen);
				}
			}
			SvcEntry.Build(pkgGen);
		}
	}
}
