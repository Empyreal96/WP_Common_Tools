using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size = 32)]
[NativeCppClass]
internal struct EHExceptionRecord
{
	[StructLayout(LayoutKind.Sequential, Size = 12)]
	[CLSCompliant(false)]
	[NativeCppClass]
	public struct EHParameters
	{
	}
}
