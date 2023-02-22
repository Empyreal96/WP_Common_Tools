using Microsoft.Win32.SafeHandles;

namespace Microsoft.Tools.IO.Interop
{
	internal sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal SafeFindHandle()
			: base(true)
		{
		}

		protected override bool ReleaseHandle()
		{
			return NativeMethods.FindClose(handle);
		}
	}
}
