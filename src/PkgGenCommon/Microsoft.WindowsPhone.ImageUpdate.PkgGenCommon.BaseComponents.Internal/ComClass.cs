using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot(ElementName = "Class", Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00")]
	public sealed class ComClass : ComBase
	{
		[XmlAttribute("ThreadingModel")]
		public string ThreadingModel { get; set; }

		[XmlAttribute("ProgId")]
		public string ProgId { get; set; }

		[XmlAttribute("VersionIndependentProgId")]
		public string VersionIndependentProgId { get; set; }

		[XmlAttribute("Description")]
		public string Description { get; set; }

		[XmlAttribute("DefaultIcon")]
		public string DefaultIcon { get; set; }

		[XmlAttribute("AppId")]
		public string AppId { get; set; }

		[XmlAttribute("SkipInProcServer32")]
		public bool SkipInProcServer32 { get; set; }

		[XmlIgnore]
		public PkgFile Dll { get; set; }

		public ComClass()
		{
			ThreadingModel = "Both";
			_defaultKey = "$(hkcr.clsid)";
		}

		protected override void DoBuild(IPackageGenerator pkgGen)
		{
			base.DoBuild(pkgGen);
			if (Description != null)
			{
				pkgGen.AddRegValue(_defaultKey, "@", RegValueType.String, Description);
			}
			if (AppId != null)
			{
				pkgGen.AddRegValue(_defaultKey, "AppId", RegValueType.String, AppId);
			}
			if (ProgId != null)
			{
				pkgGen.AddRegValue(_defaultKey + "\\ProgId", "@", RegValueType.String, ProgId);
			}
			if (VersionIndependentProgId != null)
			{
				pkgGen.AddRegValue(_defaultKey + "\\VersionIndependentProgId", "@", RegValueType.String, VersionIndependentProgId);
			}
			if (DefaultIcon != null)
			{
				pkgGen.AddRegValue(_defaultKey + "\\DefaultIcon", "@", RegValueType.ExpandString, DefaultIcon);
			}
			if (base.Version != null)
			{
				pkgGen.AddRegValue(_defaultKey + "\\Version", "@", RegValueType.String, base.Version);
			}
			if (!SkipInProcServer32)
			{
				pkgGen.AddRegExpandValue(_defaultKey + "\\InProcServer32", "@", Dll.DevicePath);
				pkgGen.AddRegValue(_defaultKey + "\\InProcServer32", "ThreadingModel", RegValueType.String, ThreadingModel);
			}
		}
	}
}
