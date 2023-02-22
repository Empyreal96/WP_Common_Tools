using System;

namespace Microsoft.Windows.Flashing.Platform
{
	[Flags]
	public enum FlashFlags : uint
	{
		Normal = 0u,
		SkipPlatformIDCheck = 1u,
		SkipSignatureCheck = 2u,
		SkipRootKeyHashCheck = 4u,
		SkipHash = 8u,
		VerifyWrite = 0x10u
	}
}
