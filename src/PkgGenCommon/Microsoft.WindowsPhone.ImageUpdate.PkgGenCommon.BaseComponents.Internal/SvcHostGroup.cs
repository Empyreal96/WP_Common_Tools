using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot(ElementName = "SvcHostGroup", Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00")]
	public sealed class SvcHostGroup : PkgObject
	{
		private const AuthenticationCapabitities _DEFAULT_CAPABILITIES = AuthenticationCapabitities.NoCustomMarshal | AuthenticationCapabitities.DisableAAA;

		[XmlAttribute("Name")]
		public string Name { get; set; }

		[XmlAttribute("CoInitializeSecurityParam")]
		public bool CoInitializeSecurityParam { get; set; }

		[XmlAttribute("CoInitializeSecurityAllowLowBox")]
		public bool CoInitializeSecurityAllowLowBox { get; set; }

		[XmlAttribute("CoInitializeSecurityAppId")]
		public string CoInitializeSecurityAppId { get; set; }

		[XmlAttribute("DefaultRpcStackSize")]
		public int DefaultRpcStackSize { get; set; }

		[XmlAttribute("SystemCritical")]
		public bool SystemCritical { get; set; }

		[XmlAttribute("AuthenticationLevel")]
		public AuthenticationLevel AuthenticationLevel { get; set; }

		[XmlAttribute("AuthenticationCapabilities")]
		public AuthenticationCapabitities AuthenticationCapabitities { get; set; }

		[XmlAttribute("ImpersonationLevel")]
		public ImpersonationLevel ImpersonationLevel { get; set; }

		public SvcHostGroup()
		{
			AuthenticationCapabitities = AuthenticationCapabitities.NoCustomMarshal | AuthenticationCapabitities.DisableAAA;
		}

		public bool ShouldSerializeAuthenticationCapabilities()
		{
			return AuthenticationCapabitities != (AuthenticationCapabitities.NoCustomMarshal | AuthenticationCapabitities.DisableAAA);
		}

		protected override void DoBuild(IPackageGenerator pkgGen)
		{
			if (pkgGen.BuildPass != 0)
			{
				if (CoInitializeSecurityParam)
				{
					pkgGen.AddRegValue("$(hklm.svchostgroup)", "CoInitializeSecurityParam", RegValueType.DWord, "00000001");
				}
				if (CoInitializeSecurityAllowLowBox)
				{
					pkgGen.AddRegValue("$(hklm.svchostgroup)", "CoInitializeSecurityAllowLowBox", RegValueType.DWord, "00000001");
				}
				if (CoInitializeSecurityAppId != null)
				{
					pkgGen.AddRegValue("$(hklm.svchostgroup)", "CoInitializeSecurityAppId", RegValueType.String, CoInitializeSecurityAppId);
				}
				if (DefaultRpcStackSize != 0)
				{
					pkgGen.AddRegValue("$(hklm.svchostgroup)", "DefaultRpcStackSize", RegValueType.DWord, $"{DefaultRpcStackSize:X8}");
				}
				if (AuthenticationLevel != 0)
				{
					pkgGen.AddRegValue("$(hklm.svchostgroup)", "AuthenticationLevel", RegValueType.DWord, $"{(int)AuthenticationLevel:X8}");
				}
				if (AuthenticationCapabitities != (AuthenticationCapabitities.NoCustomMarshal | AuthenticationCapabitities.DisableAAA))
				{
					pkgGen.AddRegValue("$(hklm.svchostgroup)", "AuthenticationCapabilities", RegValueType.DWord, $"{(int)AuthenticationCapabitities:X8}");
				}
				if (ImpersonationLevel != 0)
				{
					pkgGen.AddRegValue("$(hklm.svchostgroup)", "ImpersonationLevel", RegValueType.DWord, $"{(int)ImpersonationLevel:X8}");
				}
				if (SystemCritical)
				{
					pkgGen.AddRegValue("$(hklm.svchostgroup)", "SystemCritical", RegValueType.DWord, "00000001");
				}
			}
		}
	}
}
