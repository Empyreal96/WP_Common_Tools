using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FlashingPlatform;

namespace Microsoft.Windows.Flashing.Platform
{
	internal class NativeFlashingPlatform
	{
		[DllImport("ufphost.dll")]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		public unsafe static extern int GetFlashingPlatformVersion(uint* A_0, uint* A_1);

		[DllImport("ufphost.dll")]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		public unsafe static extern int CreateFlashingPlatform(ushort* A_0, IFlashingPlatform** A_1);
	}
}
