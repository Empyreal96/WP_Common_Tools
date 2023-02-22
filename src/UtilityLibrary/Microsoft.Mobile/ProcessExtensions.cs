using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Mobile
{
	public static class ProcessExtensions
	{
		private class ProcessHelper
		{
			private enum PROCESSINFOCLASS
			{
				ProcessBasicInformation
			}

			private enum NtStatus : uint
			{
				Success
			}

			[StructLayout(LayoutKind.Sequential, Pack = 1)]
			private struct PROCESS_BASIC_INFORMATION
			{
				public IntPtr ExitStatus;

				public IntPtr PebBaseAddress;

				public IntPtr AffinityMask;

				public IntPtr BasePriority;

				public UIntPtr UniqueProcessId;

				public IntPtr InheritedFromUniqueProcessId;

				public int Size => Marshal.SizeOf(typeof(PROCESS_BASIC_INFORMATION));
			}

			public static List<Process> GetProgeny(Process parent)
			{
				Process[] processes = Process.GetProcesses();
				KeyValuePair<Process, int>[] childParentPairs = new KeyValuePair<Process, int>[processes.Length];
				for (int i = 0; i < processes.Length; i++)
				{
					Process process2 = processes[i];
					int value = 0;
					try
					{
						if (process2.Id != parent.Id && process2.StartTime >= parent.StartTime)
						{
							value = GetParentPid(process2.Handle);
						}
					}
					catch
					{
						continue;
					}
					ref KeyValuePair<Process, int> reference = ref childParentPairs[i];
					reference = new KeyValuePair<Process, int>(process2, value);
				}
				List<Process> list = new List<Process>();
				list.Add(parent);
				int count;
				do
				{
					bool flag = true;
					count = list.Count;
					int index;
					for (index = 0; index < childParentPairs.Length; index++)
					{
						if (childParentPairs[index].Value != 0 && list.Any((Process process) => process.Id == childParentPairs[index].Value))
						{
							list.Add(childParentPairs[index].Key);
							ref KeyValuePair<Process, int> reference2 = ref childParentPairs[index];
							reference2 = new KeyValuePair<Process, int>(childParentPairs[index].Key, 0);
						}
					}
				}
				while (count != list.Count);
				return list;
			}

			[DllImport("NTDLL.DLL")]
			private static extern NtStatus NtQueryInformationProcess(IntPtr hProcess, PROCESSINFOCLASS pic, ref PROCESS_BASIC_INFORMATION pbi, int sizePbi, out int pSize);

			private static int GetParentPid(IntPtr hProcess)
			{
				PROCESS_BASIC_INFORMATION pbi = default(PROCESS_BASIC_INFORMATION);
				int pSize;
				if (NtQueryInformationProcess(hProcess, PROCESSINFOCLASS.ProcessBasicInformation, ref pbi, pbi.Size, out pSize) == NtStatus.Success)
				{
					return (int)pbi.InheritedFromUniqueProcessId;
				}
				return 0;
			}
		}

		public static void KillProgeny(this Process process, Action<Process> preKillVisitor = null)
		{
			try
			{
				List<Process> progeny = ProcessHelper.GetProgeny(process);
				foreach (Process item in progeny)
				{
					try
					{
						preKillVisitor?.Invoke(item);
						item.Kill();
					}
					catch
					{
					}
				}
				int millisecondsTimeout = 5000;
				WaitHandle.WaitAll(Enumerable.ToArray(progeny.ConvertAll((Converter<Process, WaitHandle>)((Process p) => new ManualResetEvent(false)
				{
					SafeWaitHandle = new SafeWaitHandle(p.Handle, false)
				}))), millisecondsTimeout);
			}
			catch
			{
			}
		}
	}
}
