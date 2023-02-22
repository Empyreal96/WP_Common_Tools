using System;
using System.Runtime.InteropServices;

namespace FFUComponents
{
	[StructLayout(LayoutKind.Sequential)]
	public class DEVPROPKEY
	{
		public Guid fmtid;

		public uint pid;

		public DEVPROPKEY(Guid a_fmtid, uint a_pid)
		{
			fmtid = a_fmtid;
			pid = a_pid;
		}
	}
}
