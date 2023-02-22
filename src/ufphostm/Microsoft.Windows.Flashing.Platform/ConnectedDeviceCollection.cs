using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using FlashingPlatform;
using RAII;

namespace Microsoft.Windows.Flashing.Platform
{
	public class ConnectedDeviceCollection : IEnumerable<ConnectedDevice>, IEnumerator<ConnectedDevice>
	{
		private unsafe IConnectedDeviceCollection* m_Collection;

		private int m_Index;

		internal FlashingPlatform m_Platform;

		public unsafe virtual ConnectedDevice CurrentT
		{
			get
			{
				if (m_Index < 0)
				{
					throw new InvalidOperationException(Marshal.PtrToStringUni((IntPtr)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1DC_0040CMEHKPEC_0040_003F_0024AAY_003F_0024AAo_003F_0024AAu_003F_0024AA_003F5_003F_0024AAm_003F_0024AAu_003F_0024AAs_003F_0024AAt_003F_0024AA_003F5_003F_0024AAc_003F_0024AAa_003F_0024AAl_003F_0024AAl_003F_0024AA_003F5_003F_0024AAM_003F_0024AAo_003F_0024AAv_003F_0024AAe_003F_0024AAN_003F_0024AAe_003F_0024AAx_003F_0024AAt_003F_0024AA_003F_0024CI_003F_0024AA_003F_0024CJ_003F_0024AA_003F_0024AA_0040)));
				}
				int index = m_Index;
				if (index >= Count)
				{
					throw new InvalidOperationException(Marshal.PtrToStringUni((IntPtr)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1CM_0040MEAAIPAK_0040_003F_0024AAE_003F_0024AAn_003F_0024AAu_003F_0024AAm_003F_0024AAe_003F_0024AAr_003F_0024AAa_003F_0024AAt_003F_0024AAi_003F_0024AAo_003F_0024AAn_003F_0024AA_003F5_003F_0024AAh_003F_0024AAa_003F_0024AAs_003F_0024AA_003F5_003F_0024AAe_003F_0024AAn_003F_0024AAd_003F_0024AAe_003F_0024AAd_003F_0024AA_003F_0024AA_0040)));
				}
				return GetConnectedDeviceAt((uint)index);
			}
		}

		public virtual object Current => CurrentT;

		public unsafe virtual int Count
		{
			get
			{
				IConnectedDeviceCollection* collection = m_Collection;
				return (int)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, uint>*/)(void*)(int)(*(uint*)(int)(*(uint*)collection)))((IntPtr)collection);
			}
		}

		internal unsafe IConnectedDeviceCollection* Native => m_Collection;

		internal unsafe ConnectedDeviceCollection(IConnectedDeviceCollection* Collection, [In] FlashingPlatform Platform)
		{
			m_Collection = Collection;
			m_Index = -1;
			m_Platform = Platform;
			base._002Ector();
		}

		private void _007EConnectedDeviceCollection()
		{
			_0021ConnectedDeviceCollection();
		}

		private unsafe void _0021ConnectedDeviceCollection()
		{
			IConnectedDeviceCollection* collection = m_Collection;
			if (collection != null)
			{
				((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, int>*/)(void*)(int)(*(uint*)(*(int*)collection + 8)))((IntPtr)collection);
				m_Collection = null;
			}
		}

		public virtual IEnumerator GetEnumerator()
		{
			return GetEnumeratorT();
		}

		public virtual IEnumerator<ConnectedDevice> GetEnumeratorT()
		{
			return this;
		}

		IEnumerator<ConnectedDevice> IEnumerable<ConnectedDevice>.GetEnumerator()
		{
			//ILSpy generated this explicit interface implementation from .override directive in GetEnumeratorT
			return this.GetEnumeratorT();
		}

		[return: MarshalAs(UnmanagedType.U1)]
		public virtual bool MoveNext()
		{
			int index = m_Index;
			if (index < Count)
			{
				m_Index = index + 1;
			}
			return m_Index < Count;
		}

		public virtual void Reset()
		{
			m_Index = -1;
		}

		public unsafe uint GetCount()
		{
			IConnectedDeviceCollection* collection = m_Collection;
			return ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, uint>*/)(void*)(int)(*(uint*)(int)(*(uint*)collection)))((IntPtr)collection);
		}

		public unsafe ConnectedDevice GetConnectedDeviceAt(uint Index)
		{
			//Discarded unreachable code: IL_00b4, IL_0101, IL_0113, IL_0123
			CAutoRelease_003CFlashingPlatform_003A_003AIConnectedDevice_0020_002A_003E obj;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj);
			System.Runtime.CompilerServices.Unsafe.As<CAutoRelease_003CFlashingPlatform_003A_003AIConnectedDevice_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) = 0;
			*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoRelease_0040PAUIConnectedDevice_0040FlashingPlatform_0040_0040_0040RAII_0040_00406B_0040);
			ConnectedDevice result;
			try
			{
				IConnectedDeviceCollection* collection = m_Collection;
				int num = ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, uint, IConnectedDevice**, int>*/)(void*)(int)(*(uint*)(*(int*)collection + 4)))((IntPtr)collection, Index, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, IConnectedDevice**>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 4)))((IntPtr)(&obj)));
				if (num < 0)
				{
					CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E obj2;
					System.Runtime.CompilerServices.Unsafe.SkipInit(out obj2);
					System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj2, 4)) = (int)global::_003CModule_003E.UfphNativeStrFormat((ushort*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1FG_0040NPJPCCED_0040_003F_0024AAF_003F_0024AAa_003F_0024AAi_003F_0024AAl_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAt_003F_0024AAo_003F_0024AA_003F5_003F_0024AAg_003F_0024AAe_003F_0024AAt_003F_0024AA_003F5_003F_0024AAc_003F_0024AAo_003F_0024AAn_003F_0024AAn_003F_0024AAe_003F_0024AAc_003F_0024AAt_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAd_003F_0024AAe_003F_0024AAv_003F_0024AAi_003F_0024AAc_003F_0024AAe_003F_0024AA_003F5_003F_0024AAa_0040), __arglist(Index));
					*(int*)(&obj2) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDeleteArray_0040_0024_0024CBG_0040RAII_0040_00406B_0040);
					try
					{
						IFlashingPlatform* native = m_Platform.Native;
						if (((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)native)))((IntPtr)native) != null)
						{
							IFlashingPlatform* native2 = m_Platform.Native;
							int num2 = (int)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)native2)))((IntPtr)native2);
							((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, global::FlashingPlatform.LogLevel, ushort*, void>*/)(void*)(int)(*(uint*)(*(int*)num2 + 4)))((IntPtr)num2, (global::FlashingPlatform.LogLevel)2, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj2) + 16)))((IntPtr)(&obj2)));
						}
						IntPtr ptr = (IntPtr)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj2) + 16)))((IntPtr)(&obj2));
						throw new Win32Exception(num, Marshal.PtrToStringUni(ptr));
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj2);
						throw;
					}
				}
				result = new ConnectedDevice(((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, IConnectedDevice*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 40)))((IntPtr)(&obj)), m_Platform);
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoRelease<FlashingPlatform::IConnectedDevice *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoRelease_003CFlashingPlatform_003A_003AIConnectedDevice_0020_002A_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
			*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoRelease_0040PAUIConnectedDevice_0040FlashingPlatform_0040_0040_0040RAII_0040_00406B_0040);
			global::_003CModule_003E.RAII_002ECAutoRelease_003CFlashingPlatform_003A_003AIConnectedDevice_0020_002A_003E_002ERelease(&obj);
			return result;
		}

		[HandleProcessCorruptedStateExceptions]
		protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
		{
			if (A_0)
			{
				_007EConnectedDeviceCollection();
				return;
			}
			try
			{
				_0021ConnectedDeviceCollection();
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

		~ConnectedDeviceCollection()
		{
			Dispose(false);
		}
	}
}
