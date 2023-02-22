using System;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot("ServiceDll", Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00")]
	public class SvcDll : SvcEntry
	{
		[XmlAttribute("ServiceManifest")]
		public string ServiceManifest;

		[XmlAttribute("ServiceMain")]
		public string ServiceMain;

		[XmlAttribute("UnloadOnStop")]
		public bool UnloadOnStop = true;

		[XmlAttribute("BinaryInOneCorePkg")]
		public bool BinaryInOneCorePkg;

		[XmlAttribute("HostExe")]
		public string HostExe = "$(env.system32)\\svchost.exe";

		public string HostGroupName;

		public string ServiceName;

		public bool ShouldSerializeUnloadOnStop()
		{
			return !UnloadOnStop;
		}

		public bool ShouldSerializeHostExe()
		{
			return !HostExe.Equals("$(env.system32)\\svchost.exe", StringComparison.InvariantCulture);
		}

		public override void Build(IPackageGenerator pkgGen)
		{
			base.Build(pkgGen);
			if (pkgGen.BuildPass != 0)
			{
				string keyName = "$(hklm.service)\\Parameters";
				pkgGen.AddRegExpandValue(keyName, "ServiceDll", base.DevicePath);
				if (ServiceManifest != null)
				{
					pkgGen.AddRegValue(keyName, "ServiceManifest", RegValueType.ExpandString, ServiceManifest);
				}
				if (ServiceMain != null)
				{
					pkgGen.AddRegValue(keyName, "ServiceMain", RegValueType.String, ServiceMain);
				}
				if (UnloadOnStop)
				{
					pkgGen.AddRegValue(keyName, "ServiceDllUnloadOnStop", RegValueType.DWord, "00000001");
				}
				pkgGen.AddRegValue("$(hklm.service)", "ImagePath", RegValueType.ExpandString, $"{HostExe} -k {HostGroupName}");
				pkgGen.AddRegMultiSzSegment("$(hklm.svchost)", HostGroupName, ServiceName);
			}
		}
	}
}
