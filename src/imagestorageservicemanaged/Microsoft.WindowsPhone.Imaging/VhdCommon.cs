using System;
using System.Net;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.Imaging
{
	internal static class VhdCommon
	{
		public static uint VHDSectorSize = 512u;

		public static uint DynamicVHDBlockSize = 2097152u;

		public static uint Swap32(uint data)
		{
			return (uint)IPAddress.HostToNetworkOrder((int)data);
		}

		public static ulong Swap64(ulong data)
		{
			return (ulong)IPAddress.HostToNetworkOrder((long)data);
		}

		public static uint CalculateChecksum<T>(ref T type) where T : struct
		{
			uint num = 0u;
			int num2 = Marshal.SizeOf(typeof(T));
			IntPtr intPtr = Marshal.AllocHGlobal(num2);
			byte[] array = new byte[num2];
			try
			{
				Marshal.StructureToPtr((object)type, intPtr, false);
				Marshal.Copy(intPtr, array, 0, num2);
				byte[] array2 = array;
				foreach (byte b in array2)
				{
					num += b;
				}
				return ~num;
			}
			finally
			{
				if (IntPtr.Zero != intPtr)
				{
					Marshal.FreeHGlobal(intPtr);
				}
			}
		}

		public static uint Round(uint number, uint roundTo)
		{
			return (number + roundTo - 1) / roundTo * roundTo;
		}
	}
}
