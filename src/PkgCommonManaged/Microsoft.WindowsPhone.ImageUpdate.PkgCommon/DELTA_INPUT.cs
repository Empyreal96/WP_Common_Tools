using System;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	internal struct DELTA_INPUT
	{
		public IntPtr lpStart;

		public UIntPtr uSize;

		public bool Editable;

		public DELTA_INPUT(IntPtr start, UIntPtr size, bool editable)
		{
			lpStart = start;
			uSize = size;
			Editable = editable;
		}
	}
}
