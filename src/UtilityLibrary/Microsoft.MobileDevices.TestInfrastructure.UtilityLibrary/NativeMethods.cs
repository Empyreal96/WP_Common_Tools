using System;
using System.Runtime.InteropServices;

namespace Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary
{
	internal static class NativeMethods
	{
		[StructLayout(LayoutKind.Sequential)]
		public class SECURITY_ATTRIBUTES
		{
			public int nLength;

			public IntPtr lpSecurityDescriptor;

			public int bInheritHandle;
		}

		internal struct JOBOBJECT_BASIC_LIMIT_INFORMATION
		{
			public long PerProcessUserTimeLimit;

			public long PerJobUserTimeLimit;

			public JOB_OBJECT_LIMIT LimitFlags;

			public UIntPtr MinimumWorkingSetSize;

			public UIntPtr MaximumWorkingSetSize;

			public uint ActiveProcessLimit;

			public long Affinity;

			public uint PriorityClass;

			public uint SchedulingClass;
		}

		internal struct IO_COUNTERS
		{
			public ulong ReadOperationCount;

			public ulong WriteOperationCount;

			public ulong OtherOperationCount;

			public ulong ReadTransferCount;

			public ulong WriteTransferCount;

			public ulong OtherTransferCount;
		}

		internal struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
		{
			public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;

			public IO_COUNTERS IoInfo;

			public UIntPtr ProcessMemoryLimit;

			public UIntPtr JobMemoryLimit;

			public UIntPtr PeakProcessMemoryUsed;

			public UIntPtr PeakJobMemoryUsed;
		}

		internal enum JOBOBJECTINFOCLASS
		{
			AssociateCompletionPortInformation = 7,
			BasicLimitInformation = 2,
			BasicUIRestrictions = 4,
			EndOfJobTimeInformation = 6,
			ExtendedLimitInformation = 9,
			SecurityLimitInformation = 5,
			GroupInformation = 11
		}

		[Flags]
		internal enum JOB_OBJECT_LIMIT : uint
		{
			JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000u
		}

		internal const ulong InvalidHandle = ulong.MaxValue;

		internal const uint EventTraceControlQuery = 0u;

		internal const uint EventTraceControlStop = 1u;

		internal const uint EventControlCodeEnableProvider = 1u;

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "StartTraceW", ExactSpelling = true)]
		internal static extern int StartTrace(out ulong traceHandle, [In][MarshalAs(UnmanagedType.LPWStr)] string sessionName, [In][Out] ref EventTraceProperties eventTraceProperties);

		internal static int EnableTraceEx2(ulong traceHandle, Guid providerId, uint controlCode, TraceLevel traceLevel = TraceLevel.Verbose, ulong matchAnyKeyword = ulong.MaxValue, ulong matchAllKeyword = 0uL, uint timeout = 0u)
		{
			return EnableTraceEx2(traceHandle, ref providerId, controlCode, traceLevel, matchAnyKeyword, matchAllKeyword, timeout, IntPtr.Zero);
		}

		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "ControlTraceW", ExactSpelling = true, SetLastError = true)]
		internal static extern int ControlTrace(ulong SessionHandle, string SessionName, ref EventTraceProperties Properties, uint ControlCode);

		[DllImport("advapi32.dll")]
		private static extern int EnableTraceEx2([In] ulong traceHandle, [In] ref Guid providerId, [In] uint controlCode, [In] TraceLevel traceLevel, [In] ulong matchAnyKeyword, [In] ulong matchAllKeyword, [In] uint timeout, [In] IntPtr enableTraceParameters);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		internal static extern IntPtr CreateJobObject([In] SECURITY_ATTRIBUTES lpJobAttributes, string lpName);

		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

		[DllImport("kernel32.dll")]
		internal static extern bool SetInformationJobObject(IntPtr hJob, JOBOBJECTINFOCLASS JobObjectInfoClass, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

		[DllImport("Kernel32")]
		public static extern bool CloseHandle(IntPtr handle);

		[DllImport("Kernel32")]
		public static extern bool IsProcessInJob(IntPtr hProcess, IntPtr hJob, out bool result);
	}
}
