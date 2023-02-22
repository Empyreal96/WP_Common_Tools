using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary;

namespace Microsoft.TestInfra.UtilityLibrary
{
	public class WindowsJob : IDisposable
	{
		private IntPtr jobObjectHandle;

		public WindowsJob()
		{
			jobObjectHandle = NativeMethods.CreateJobObject(null, null);
			NativeMethods.JOBOBJECT_BASIC_LIMIT_INFORMATION basicLimitInformation = new NativeMethods.JOBOBJECT_BASIC_LIMIT_INFORMATION
			{
				LimitFlags = NativeMethods.JOB_OBJECT_LIMIT.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
			};
			NativeMethods.JOBOBJECT_EXTENDED_LIMIT_INFORMATION jOBOBJECT_EXTENDED_LIMIT_INFORMATION = new NativeMethods.JOBOBJECT_EXTENDED_LIMIT_INFORMATION
			{
				BasicLimitInformation = basicLimitInformation
			};
			int num = Marshal.SizeOf(typeof(NativeMethods.JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			Marshal.StructureToPtr((object)jOBOBJECT_EXTENDED_LIMIT_INFORMATION, intPtr, false);
			if (!NativeMethods.SetInformationJobObject(jobObjectHandle, NativeMethods.JOBOBJECTINFOCLASS.ExtendedLimitInformation, intPtr, (uint)num))
			{
				throw new TypicalException<WindowsJob>($"Unable to set information.  Error: {Marshal.GetLastWin32Error()}");
			}
		}

		~WindowsJob()
		{
			if (jobObjectHandle != IntPtr.Zero)
			{
				throw new InvalidOperationException("This IDisposable was not Disposed before the finalizer was called.  This is almost certainly a bug.  Please examine the code that instantiated the object and ensure it calls Dispose() or uses a using statement.");
			}
		}

		public void Dispose()
		{
			if (jobObjectHandle != IntPtr.Zero)
			{
				NativeMethods.CloseHandle(jobObjectHandle);
				jobObjectHandle = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}

		public void AssignProcess(Process process)
		{
			if (!NativeMethods.AssignProcessToJobObject(jobObjectHandle, process.Handle))
			{
				throw new TypicalException<WindowsJob>($"AssignProcessToJobObject() failed for process '{process.ProcessName}'. Error: {Marshal.GetLastWin32Error()}");
			}
		}

		public static bool IsProcessInJob(Process process, IntPtr job)
		{
			bool result = false;
			if (!NativeMethods.IsProcessInJob(process.Handle, job, out result))
			{
				throw new TypicalException<WindowsJob>($"IsProcessInJob() failed for process '{process.ProcessName}'. Error: {Marshal.GetLastWin32Error()}");
			}
			return result;
		}

		public static bool IsProcessInAnyJob(Process process)
		{
			return IsProcessInJob(process, IntPtr.Zero);
		}

		public bool IsProcessInJob(Process process)
		{
			return IsProcessInJob(process, jobObjectHandle);
		}
	}
}
