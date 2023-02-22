using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.Imaging
{
	internal static class VhdExtensions
	{
		public static void WriteStruct<T>(this FileStream writer, ref T structure) where T : struct
		{
			int num = Marshal.SizeOf(typeof(T));
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			byte[] array = new byte[num];
			try
			{
				Marshal.StructureToPtr((object)structure, intPtr, false);
				Marshal.Copy(intPtr, array, 0, num);
				writer.Write(array, 0, num);
			}
			finally
			{
				if (IntPtr.Zero != intPtr)
				{
					Marshal.FreeHGlobal(intPtr);
				}
			}
		}

		public static T ReadStruct<T>(this FileStream reader) where T : struct
		{
			int num = Marshal.SizeOf(typeof(T));
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			byte[] array = new byte[num];
			try
			{
				reader.Read(array, 0, num);
				Marshal.Copy(array, 0, intPtr, num);
				return (T)Marshal.PtrToStructure(intPtr, typeof(T));
			}
			finally
			{
				if (IntPtr.Zero != intPtr)
				{
					Marshal.FreeHGlobal(intPtr);
				}
			}
		}
	}
}
