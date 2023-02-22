using System;
using System.Runtime.InteropServices;

namespace Microsoft.Windows.ImageTools
{
	internal static class NativeMethods
	{
		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "StartTraceW", ExactSpelling = true)]
		internal static extern int StartTrace(out ulong traceHandle, [In][MarshalAs(UnmanagedType.LPWStr)] string sessionName, [In][Out] ref EventTraceProperties eventTraceProperties);

		internal static int EnableTraceEx2(ulong traceHandle, Guid providerId, uint controlCode, TraceLevel traceLevel = TraceLevel.Verbose, ulong matchAnyKeyword = ulong.MaxValue, ulong matchAllKeyword = 0uL, uint timeout = 0u)
		{
			return EnableTraceEx2(traceHandle, ref providerId, controlCode, traceLevel, matchAnyKeyword, matchAllKeyword, timeout, IntPtr.Zero);
		}

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "StopTraceW", ExactSpelling = true)]
		internal static extern int StopTrace([In] ulong traceHandle, [In][MarshalAs(UnmanagedType.LPWStr)] string sessionName, out EventTraceProperties eventTraceProperties);

		[DllImport("advapi32.dll")]
		private static extern int EnableTraceEx2([In] ulong traceHandle, [In] ref Guid providerId, [In] uint controlCode, [In] TraceLevel traceLevel, [In] ulong matchAnyKeyword, [In] ulong matchAllKeyword, [In] uint timeout, [In] IntPtr enableTraceParameters);
	}
}
