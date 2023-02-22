using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using FlashingPlatform;
using RAII;

namespace Microsoft.Windows.Flashing.Platform
{
	public class Logger : IDisposable
	{
		private unsafe ILogger* m_Logger;

		internal FlashingPlatform m_Platform;

		internal unsafe ILogger* Native => m_Logger;

		internal unsafe Logger([In] FlashingPlatform Platform)
		{
			IFlashingPlatform* native = Platform.Native;
			m_Logger = ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)native)))((IntPtr)native);
			m_Platform = Platform;
			base._002Ector();
		}

		private unsafe void _007ELogger()
		{
			m_Logger = null;
		}

		private unsafe void _0021Logger()
		{
			m_Logger = null;
		}

		public unsafe void SetLogLevel(LogLevel Level)
		{
			ILogger* logger = m_Logger;
			if (logger != null)
			{
				((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, global::FlashingPlatform.LogLevel, int>*/)(void*)(int)(*(uint*)(int)(*(uint*)logger)))((IntPtr)logger, (global::FlashingPlatform.LogLevel)Level);
			}
		}

		public unsafe void Log(LogLevel Level, [In] string Message)
		{
			if (m_Logger != null)
			{
				ushort* ptr = (ushort*)((Message == null) ? null : Marshal.StringToCoTaskMemUni(Message).ToPointer());
				CAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E obj;
				System.Runtime.CompilerServices.Unsafe.SkipInit(out obj);
				System.Runtime.CompilerServices.Unsafe.As<CAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) = (int)ptr;
				*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoComFree_0040PBG_0040RAII_0040_00406B_0040);
				try
				{
					ILogger* logger = m_Logger;
					((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, global::FlashingPlatform.LogLevel, ushort*, void>*/)(void*)(int)(*(uint*)(*(int*)logger + 4)))((IntPtr)logger, (global::FlashingPlatform.LogLevel)Level, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj)));
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoComFree<unsigned short const *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E_002E_007Bdtor_007D), &obj);
					throw;
				}
				global::_003CModule_003E.RAII_002ECAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E_002E_007Bdtor_007D(&obj);
			}
		}

		[HandleProcessCorruptedStateExceptions]
		protected unsafe virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
		{
			if (A_0)
			{
				m_Logger = null;
				return;
			}
			try
			{
				m_Logger = null;
			}
			finally
			{
				base.Finalize();
			}
		}

		public virtual sealed void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~Logger()
		{
			Dispose(false);
		}
	}
}
