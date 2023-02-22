using System;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents
{
	[Flags]
	public enum AuthenticationCapabitities
	{
		None = 0,
		MutualAuth = 1,
		StaticCloaking = 0x20,
		DynamicCloaking = 0x40,
		AnyAuthority = 0x80,
		MakeFullSIC = 0x100,
		Default = 0x800,
		SecureRefs = 2,
		AccessControl = 4,
		AppId = 8,
		Dynamic = 0x10,
		RequireFullSIC = 0x200,
		AutoImpersonate = 0x400,
		NoCustomMarshal = 0x2000,
		DisableAAA = 0x1000
	}
}
