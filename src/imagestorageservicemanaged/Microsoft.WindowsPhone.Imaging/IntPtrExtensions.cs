using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.Imaging
{
	public static class IntPtrExtensions
	{
		public static IntPtr Increment(this IntPtr ptr, int cbSize)
		{
			return new IntPtr(ptr.ToInt64() + cbSize);
		}

		public static IntPtr Increment<T>(this IntPtr ptr)
		{
			return ptr.Increment(Marshal.SizeOf(typeof(T)));
		}

		public static T ElementAt<T>(this IntPtr ptr, int index)
		{
			int cbSize = Marshal.SizeOf(typeof(T)) * index;
			return (T)Marshal.PtrToStructure(ptr.Increment(cbSize), typeof(T));
		}
	}
}
