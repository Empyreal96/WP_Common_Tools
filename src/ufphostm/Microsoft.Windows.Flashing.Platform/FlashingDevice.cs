using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using FlashingPlatform;
using RAII;

namespace Microsoft.Windows.Flashing.Platform
{
	public class FlashingDevice : ConnectedDevice
	{
		private unsafe IFlashingDevice* m_Device;

		internal new unsafe IFlashingDevice* Native => m_Device;

		internal unsafe FlashingDevice(IFlashingDevice* Device, [In] FlashingPlatform Platform)
		{
			m_Device = Device;
			base._002Ector(null, Platform);
			try
			{
				SetDevice((IConnectedDevice*)Device);
				return;
			}
			catch
			{
				//try-fault
				base.Dispose(true);
				throw;
			}
		}

		private void _007EFlashingDevice()
		{
			_0021FlashingDevice();
		}

		private unsafe void _0021FlashingDevice()
		{
			IFlashingDevice* device = m_Device;
			if (device != null)
			{
				((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, int>*/)(void*)(int)(*(uint*)(*(int*)device + 20)))((IntPtr)device);
				m_Device = null;
			}
		}

		public unsafe string GetDeviceFriendlyName()
		{
			//Discarded unreachable code: IL_009a, IL_00b7, IL_00c9
			IFlashingDevice* device = m_Device;
			ushort* ptr;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ptr);
			int num = ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort**, int>*/)(void*)(int)(*(uint*)(*(int*)device + 24)))((IntPtr)device, &ptr);
			if (num < 0)
			{
				CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E obj;
				System.Runtime.CompilerServices.Unsafe.SkipInit(out obj);
				System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) = (int)global::_003CModule_003E.UfphNativeStrFormat((ushort*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1EG_0040CCFOHDEM_0040_003F_0024AAF_003F_0024AAa_003F_0024AAi_003F_0024AAl_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAt_003F_0024AAo_003F_0024AA_003F5_003F_0024AAg_003F_0024AAe_003F_0024AAt_003F_0024AA_003F5_003F_0024AAd_003F_0024AAe_003F_0024AAv_003F_0024AAi_003F_0024AAc_003F_0024AAe_003F_0024AA_003F5_003F_0024AAf_003F_0024AAr_003F_0024AAi_003F_0024AAe_003F_0024AAn_003F_0024AAd_003F_0024AAl_003F_0024AAy_003F_0024AA_003F5_003F_0024AAn_003F_0024AAa_0040), __arglist());
				*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDeleteArray_0040_0024_0024CBG_0040RAII_0040_00406B_0040);
				try
				{
					IFlashingPlatform* native = m_Platform.Native;
					if (((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)native)))((IntPtr)native) != null)
					{
						IFlashingPlatform* native2 = m_Platform.Native;
						int num2 = (int)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)native2)))((IntPtr)native2);
						((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, global::FlashingPlatform.LogLevel, ushort*, void>*/)(void*)(int)(*(uint*)(*(int*)num2 + 4)))((IntPtr)num2, (global::FlashingPlatform.LogLevel)2, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj)));
					}
					IntPtr ptr2 = (IntPtr)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj));
					throw new Win32Exception(num, Marshal.PtrToStringUni(ptr2));
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj);
					throw;
				}
			}
			return Marshal.PtrToStringUni((IntPtr)ptr);
		}

		public unsafe Guid GetDeviceUniqueID()
		{
			//Discarded unreachable code: IL_009a, IL_00f0, IL_0103
			IFlashingDevice* device = m_Device;
			_GUID gUID;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out gUID);
			int num = ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, _GUID*, int>*/)(void*)(int)(*(uint*)(*(int*)device + 28)))((IntPtr)device, &gUID);
			if (num < 0)
			{
				CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E obj;
				System.Runtime.CompilerServices.Unsafe.SkipInit(out obj);
				System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) = (int)global::_003CModule_003E.UfphNativeStrFormat((ushort*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1DO_0040FGPACCJK_0040_003F_0024AAF_003F_0024AAa_003F_0024AAi_003F_0024AAl_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAt_003F_0024AAo_003F_0024AA_003F5_003F_0024AAg_003F_0024AAe_003F_0024AAt_003F_0024AA_003F5_003F_0024AAd_003F_0024AAe_003F_0024AAv_003F_0024AAi_003F_0024AAc_003F_0024AAe_003F_0024AA_003F5_003F_0024AAu_003F_0024AAn_003F_0024AAi_003F_0024AAq_003F_0024AAu_003F_0024AAe_003F_0024AA_003F5_003F_0024AAI_003F_0024AAD_003F_0024AA_003F_0024AA_0040), __arglist());
				*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDeleteArray_0040_0024_0024CBG_0040RAII_0040_00406B_0040);
				try
				{
					IFlashingPlatform* native = m_Platform.Native;
					if (((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)native)))((IntPtr)native) != null)
					{
						IFlashingPlatform* native2 = m_Platform.Native;
						int num2 = (int)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)native2)))((IntPtr)native2);
						((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, global::FlashingPlatform.LogLevel, ushort*, void>*/)(void*)(int)(*(uint*)(*(int*)num2 + 4)))((IntPtr)num2, (global::FlashingPlatform.LogLevel)2, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj)));
					}
					IntPtr ptr = (IntPtr)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj));
					throw new Win32Exception(num, Marshal.PtrToStringUni(ptr));
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj);
					throw;
				}
			}
			return new Guid(*(uint*)(&gUID), System.Runtime.CompilerServices.Unsafe.As<_GUID, ushort>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gUID, 4)), System.Runtime.CompilerServices.Unsafe.As<_GUID, ushort>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gUID, 6)), System.Runtime.CompilerServices.Unsafe.As<_GUID, byte>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gUID, 8)), System.Runtime.CompilerServices.Unsafe.As<_GUID, byte>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gUID, 9)), System.Runtime.CompilerServices.Unsafe.As<_GUID, byte>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gUID, 10)), System.Runtime.CompilerServices.Unsafe.As<_GUID, byte>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gUID, 11)), System.Runtime.CompilerServices.Unsafe.As<_GUID, byte>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gUID, 12)), System.Runtime.CompilerServices.Unsafe.As<_GUID, byte>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gUID, 13)), System.Runtime.CompilerServices.Unsafe.As<_GUID, byte>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gUID, 14)), System.Runtime.CompilerServices.Unsafe.As<_GUID, byte>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gUID, 15)));
		}

		public unsafe Guid GetDeviceSerialNumber()
		{
			//Discarded unreachable code: IL_009a, IL_00f0, IL_0103
			IFlashingDevice* device = m_Device;
			_GUID gUID;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out gUID);
			int num = ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, _GUID*, int>*/)(void*)(int)(*(uint*)(*(int*)device + 32)))((IntPtr)device, &gUID);
			if (num < 0)
			{
				CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E obj;
				System.Runtime.CompilerServices.Unsafe.SkipInit(out obj);
				System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) = (int)global::_003CModule_003E.UfphNativeStrFormat((ushort*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1EG_0040FNHIMFAE_0040_003F_0024AAF_003F_0024AAa_003F_0024AAi_003F_0024AAl_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAt_003F_0024AAo_003F_0024AA_003F5_003F_0024AAg_003F_0024AAe_003F_0024AAt_003F_0024AA_003F5_003F_0024AAd_003F_0024AAe_003F_0024AAv_003F_0024AAi_003F_0024AAc_003F_0024AAe_003F_0024AA_003F5_003F_0024AAs_003F_0024AAe_003F_0024AAr_003F_0024AAi_003F_0024AAa_003F_0024AAl_003F_0024AA_003F5_003F_0024AAn_003F_0024AAu_003F_0024AAm_003F_0024AAb_0040), __arglist());
				*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDeleteArray_0040_0024_0024CBG_0040RAII_0040_00406B_0040);
				try
				{
					IFlashingPlatform* native = m_Platform.Native;
					if (((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)native)))((IntPtr)native) != null)
					{
						IFlashingPlatform* native2 = m_Platform.Native;
						int num2 = (int)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)native2)))((IntPtr)native2);
						((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, global::FlashingPlatform.LogLevel, ushort*, void>*/)(void*)(int)(*(uint*)(*(int*)num2 + 4)))((IntPtr)num2, (global::FlashingPlatform.LogLevel)2, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj)));
					}
					IntPtr ptr = (IntPtr)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj));
					throw new Win32Exception(num, Marshal.PtrToStringUni(ptr));
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj);
					throw;
				}
			}
			return new Guid(*(uint*)(&gUID), System.Runtime.CompilerServices.Unsafe.As<_GUID, ushort>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gUID, 4)), System.Runtime.CompilerServices.Unsafe.As<_GUID, ushort>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gUID, 6)), System.Runtime.CompilerServices.Unsafe.As<_GUID, byte>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gUID, 8)), System.Runtime.CompilerServices.Unsafe.As<_GUID, byte>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gUID, 9)), System.Runtime.CompilerServices.Unsafe.As<_GUID, byte>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gUID, 10)), System.Runtime.CompilerServices.Unsafe.As<_GUID, byte>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gUID, 11)), System.Runtime.CompilerServices.Unsafe.As<_GUID, byte>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gUID, 12)), System.Runtime.CompilerServices.Unsafe.As<_GUID, byte>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gUID, 13)), System.Runtime.CompilerServices.Unsafe.As<_GUID, byte>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gUID, 14)), System.Runtime.CompilerServices.Unsafe.As<_GUID, byte>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gUID, 15)));
		}

		public unsafe void WriteWim([In] string WimPath, GenericProgress Progress)
		{
			//Discarded unreachable code: IL_0127, IL_0137, IL_017e, IL_018e
			ushort* ptr = (ushort*)((WimPath == null) ? null : Marshal.StringToCoTaskMemUni(WimPath).ToPointer());
			CAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E obj;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj);
			System.Runtime.CompilerServices.Unsafe.As<CAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) = (int)ptr;
			*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoComFree_0040PBG_0040RAII_0040_00406B_0040);
			CAutoDelete_003CCGenericProgressShim_0020_002A_003E obj2;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj2);
			CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E obj3;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj3);
			try
			{
				CGenericProgressShim* ptr2 = (CGenericProgressShim*)global::_003CModule_003E.@new(8u);
				CGenericProgressShim* ptr3;
				try
				{
					if (ptr2 != null)
					{
						*(int*)ptr2 = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7CGenericProgressShim_0040_00406B_0040);
						*(int*)((byte*)ptr2 + 4) = (int)((IntPtr)GCHandle.Alloc(Progress)).ToPointer();
						ptr3 = ptr2;
					}
					else
					{
						ptr3 = null;
					}
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.delete(ptr2);
					throw;
				}
				System.Runtime.CompilerServices.Unsafe.As<CAutoDelete_003CCGenericProgressShim_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj2, 4)) = (int)ptr3;
				*(int*)(&obj2) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDelete_0040PAVCGenericProgressShim_0040_0040_0040RAII_0040_00406B_0040);
				try
				{
					IFlashingDevice* device = m_Device;
					int num = ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*, IGenericProgress*, int>*/)(void*)(int)(*(uint*)(*(int*)device + 36)))((IntPtr)device, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj)), (IGenericProgress*)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, CGenericProgressShim*>*/)(void*)(int)(*(uint*)(*(int*)(&obj2) + 16)))((IntPtr)(&obj2)));
					if (num < 0)
					{
						System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj3, 4)) = (int)global::_003CModule_003E.UfphNativeStrFormat((ushort*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1CI_0040ECIEBNIH_0040_003F_0024AAF_003F_0024AAa_003F_0024AAi_003F_0024AAl_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAt_003F_0024AAo_003F_0024AA_003F5_003F_0024AAw_003F_0024AAr_003F_0024AAi_003F_0024AAt_003F_0024AAe_003F_0024AA_003F5_003F_0024AAW_003F_0024AAI_003F_0024AAM_003F_0024AA_003F_0024AA_0040), __arglist());
						*(int*)(&obj3) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDeleteArray_0040_0024_0024CBG_0040RAII_0040_00406B_0040);
						try
						{
							IFlashingPlatform* native = m_Platform.Native;
							if (((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)native)))((IntPtr)native) != null)
							{
								IFlashingPlatform* native2 = m_Platform.Native;
								int num2 = (int)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)native2)))((IntPtr)native2);
								((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, global::FlashingPlatform.LogLevel, ushort*, void>*/)(void*)(int)(*(uint*)(*(int*)num2 + 4)))((IntPtr)num2, (global::FlashingPlatform.LogLevel)2, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj3) + 16)))((IntPtr)(&obj3)));
							}
							IntPtr ptr4 = (IntPtr)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj3) + 16)))((IntPtr)(&obj3));
							throw new Win32Exception(num, Marshal.PtrToStringUni(ptr4));
						}
						catch
						{
							//try-fault
							global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj3);
							throw;
						}
					}
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDelete<CGenericProgressShim *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDelete_003CCGenericProgressShim_0020_002A_003E_002E_007Bdtor_007D), &obj2);
					throw;
				}
				*(int*)(&obj2) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDelete_0040PAVCGenericProgressShim_0040_0040_0040RAII_0040_00406B_0040);
				global::_003CModule_003E.RAII_002ECAutoDelete_003CCGenericProgressShim_0020_002A_003E_002ERelease(&obj2);
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoComFree<unsigned short const *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
			global::_003CModule_003E.RAII_002ECAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E_002E_007Bdtor_007D(&obj);
			try
			{
				try
				{
					try
					{
						return;
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj3);
						throw;
					}
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDelete<CGenericProgressShim *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDelete_003CCGenericProgressShim_0020_002A_003E_002E_007Bdtor_007D), &obj2);
					throw;
				}
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoComFree<unsigned short const *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
		}

		public unsafe void SkipTransfer()
		{
			//Discarded unreachable code: IL_0094
			IFlashingDevice* device = m_Device;
			int num = ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, int>*/)(void*)(int)(*(uint*)(*(int*)device + 40)))((IntPtr)device);
			CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E obj;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj);
			if (num < 0)
			{
				System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) = (int)global::_003CModule_003E.UfphNativeStrFormat((ushort*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1DA_0040FFAALIKO_0040_003F_0024AAF_003F_0024AAa_003F_0024AAi_003F_0024AAl_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAt_003F_0024AAo_003F_0024AA_003F5_003F_0024AAs_003F_0024AAk_003F_0024AAi_003F_0024AAp_003F_0024AA_003F5_003F_0024AAt_003F_0024AAr_003F_0024AAa_003F_0024AAn_003F_0024AAs_003F_0024AAf_003F_0024AAe_003F_0024AAr_003F_0024AA_003F_0024AA_0040), __arglist());
				*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDeleteArray_0040_0024_0024CBG_0040RAII_0040_00406B_0040);
				try
				{
					IFlashingPlatform* native = m_Platform.Native;
					if (((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)native)))((IntPtr)native) != null)
					{
						IFlashingPlatform* native2 = m_Platform.Native;
						int num2 = (int)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)native2)))((IntPtr)native2);
						((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, global::FlashingPlatform.LogLevel, ushort*, void>*/)(void*)(int)(*(uint*)(*(int*)num2 + 4)))((IntPtr)num2, (global::FlashingPlatform.LogLevel)2, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj)));
					}
					IntPtr ptr = (IntPtr)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj));
					throw new Win32Exception(num, Marshal.PtrToStringUni(ptr));
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj);
					throw;
				}
			}
			try
			{
				return;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
		}

		public unsafe void Reboot()
		{
			//Discarded unreachable code: IL_0094
			IFlashingDevice* device = m_Device;
			int num = ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, int>*/)(void*)(int)(*(uint*)(*(int*)device + 44)))((IntPtr)device);
			CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E obj;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj);
			if (num < 0)
			{
				System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) = (int)global::_003CModule_003E.UfphNativeStrFormat((ushort*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1CC_0040KCIGDDEL_0040_003F_0024AAF_003F_0024AAa_003F_0024AAi_003F_0024AAl_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAt_003F_0024AAo_003F_0024AA_003F5_003F_0024AAr_003F_0024AAe_003F_0024AAb_003F_0024AAo_003F_0024AAo_003F_0024AAt_003F_0024AA_003F_0024AA_0040), __arglist());
				*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDeleteArray_0040_0024_0024CBG_0040RAII_0040_00406B_0040);
				try
				{
					IFlashingPlatform* native = m_Platform.Native;
					if (((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)native)))((IntPtr)native) != null)
					{
						IFlashingPlatform* native2 = m_Platform.Native;
						int num2 = (int)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)native2)))((IntPtr)native2);
						((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, global::FlashingPlatform.LogLevel, ushort*, void>*/)(void*)(int)(*(uint*)(*(int*)num2 + 4)))((IntPtr)num2, (global::FlashingPlatform.LogLevel)2, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj)));
					}
					IntPtr ptr = (IntPtr)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj));
					throw new Win32Exception(num, Marshal.PtrToStringUni(ptr));
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj);
					throw;
				}
			}
			try
			{
				return;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
		}

		public unsafe void EnterMassStorageMode()
		{
			//Discarded unreachable code: IL_0094
			IFlashingDevice* device = m_Device;
			int num = ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, int>*/)(void*)(int)(*(uint*)(*(int*)device + 48)))((IntPtr)device);
			CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E obj;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj);
			if (num < 0)
			{
				System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) = (int)global::_003CModule_003E.UfphNativeStrFormat((ushort*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1EE_0040BNENPODH_0040_003F_0024AAF_003F_0024AAa_003F_0024AAi_003F_0024AAl_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAt_003F_0024AAo_003F_0024AA_003F5_003F_0024AAe_003F_0024AAn_003F_0024AAt_003F_0024AAe_003F_0024AAr_003F_0024AA_003F5_003F_0024AAm_003F_0024AAa_003F_0024AAs_003F_0024AAs_003F_0024AA_003F5_003F_0024AAs_003F_0024AAt_003F_0024AAo_003F_0024AAr_003F_0024AAa_003F_0024AAg_003F_0024AAe_003F_0024AA_003F5_003F_0024AAm_003F_0024AAo_003F_0024AAd_0040), __arglist());
				*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDeleteArray_0040_0024_0024CBG_0040RAII_0040_00406B_0040);
				try
				{
					IFlashingPlatform* native = m_Platform.Native;
					if (((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)native)))((IntPtr)native) != null)
					{
						IFlashingPlatform* native2 = m_Platform.Native;
						int num2 = (int)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)native2)))((IntPtr)native2);
						((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, global::FlashingPlatform.LogLevel, ushort*, void>*/)(void*)(int)(*(uint*)(*(int*)num2 + 4)))((IntPtr)num2, (global::FlashingPlatform.LogLevel)2, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj)));
					}
					IntPtr ptr = (IntPtr)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj));
					throw new Win32Exception(num, Marshal.PtrToStringUni(ptr));
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj);
					throw;
				}
			}
			try
			{
				return;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
		}

		public unsafe void SetBootMode(uint BootMode, string ProfileName)
		{
			//Discarded unreachable code: IL_00ce, IL_00de, IL_0106
			ushort* ptr = (ushort*)((ProfileName == null) ? null : Marshal.StringToCoTaskMemUni(ProfileName).ToPointer());
			CAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E obj;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj);
			System.Runtime.CompilerServices.Unsafe.As<CAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) = (int)ptr;
			*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoComFree_0040PBG_0040RAII_0040_00406B_0040);
			CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E obj2;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj2);
			try
			{
				IFlashingDevice* device = m_Device;
				int num = ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, uint, ushort*, int>*/)(void*)(int)(*(uint*)(*(int*)device + 52)))((IntPtr)device, BootMode, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj)));
				if (num < 0)
				{
					System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj2, 4)) = (int)global::_003CModule_003E.UfphNativeStrFormat((ushort*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1DA_0040MBNNDMOJ_0040_003F_0024AAF_003F_0024AAa_003F_0024AAi_003F_0024AAl_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAt_003F_0024AAo_003F_0024AA_003F5_003F_0024AAs_003F_0024AAe_003F_0024AAt_003F_0024AA_003F5_003F_0024AAb_003F_0024AAo_003F_0024AAo_003F_0024AAt_003F_0024AA_003F5_003F_0024AAm_003F_0024AAo_003F_0024AAd_003F_0024AAe_003F_0024AA_003F_0024AA_0040), __arglist());
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
						IntPtr ptr2 = (IntPtr)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj2) + 16)))((IntPtr)(&obj2));
						throw new Win32Exception(num, Marshal.PtrToStringUni(ptr2));
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj2);
						throw;
					}
				}
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoComFree<unsigned short const *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
			global::_003CModule_003E.RAII_002ECAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E_002E_007Bdtor_007D(&obj);
			try
			{
				try
				{
					return;
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj2);
					throw;
				}
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoComFree<unsigned short const *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
		}

		public unsafe void FlashFFUFile([In] string FFUFilePath, FlashFlags Flags, GenericProgress Progress, HandleRef CancelEvent)
		{
			//Discarded unreachable code: IL_0138, IL_0148, IL_018f, IL_019f
			ushort* ptr = (ushort*)((FFUFilePath == null) ? null : Marshal.StringToCoTaskMemUni(FFUFilePath).ToPointer());
			CAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E obj;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj);
			System.Runtime.CompilerServices.Unsafe.As<CAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) = (int)ptr;
			*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoComFree_0040PBG_0040RAII_0040_00406B_0040);
			CAutoDelete_003CCGenericProgressShim_0020_002A_003E obj2;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj2);
			CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E obj3;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj3);
			try
			{
				CGenericProgressShim* ptr2 = (CGenericProgressShim*)global::_003CModule_003E.@new(8u);
				CGenericProgressShim* ptr3;
				try
				{
					if (ptr2 != null)
					{
						*(int*)ptr2 = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7CGenericProgressShim_0040_00406B_0040);
						*(int*)((byte*)ptr2 + 4) = (int)((IntPtr)GCHandle.Alloc(Progress)).ToPointer();
						ptr3 = ptr2;
					}
					else
					{
						ptr3 = null;
					}
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.delete(ptr2);
					throw;
				}
				System.Runtime.CompilerServices.Unsafe.As<CAutoDelete_003CCGenericProgressShim_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj2, 4)) = (int)ptr3;
				*(int*)(&obj2) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDelete_0040PAVCGenericProgressShim_0040_0040_0040RAII_0040_00406B_0040);
				try
				{
					IntPtr handle = CancelEvent.Handle;
					IFlashingDevice* device = m_Device;
					int num = ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*, uint, IGenericProgress*, void*, int>*/)(void*)(int)(*(uint*)(*(int*)device + 56)))((IntPtr)device, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj)), (uint)Flags, (IGenericProgress*)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, CGenericProgressShim*>*/)(void*)(int)(*(uint*)(*(int*)(&obj2) + 16)))((IntPtr)(&obj2)), (void*)handle);
					if (num < 0)
					{
						System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj3, 4)) = (int)global::_003CModule_003E.UfphNativeStrFormat((ushort*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1DC_0040PGNKAIIP_0040_003F_0024AAF_003F_0024AAa_003F_0024AAi_003F_0024AAl_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAt_003F_0024AAo_003F_0024AA_003F5_003F_0024AAf_003F_0024AAl_003F_0024AAa_003F_0024AAs_003F_0024AAh_003F_0024AA_003F5_003F_0024AAF_003F_0024AAF_003F_0024AAU_003F_0024AA_003F5_003F_0024AAf_003F_0024AAi_003F_0024AAl_003F_0024AAe_003F_0024AA_003F_0024AA_0040), __arglist());
						*(int*)(&obj3) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDeleteArray_0040_0024_0024CBG_0040RAII_0040_00406B_0040);
						try
						{
							IFlashingPlatform* native = m_Platform.Native;
							if (((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)native)))((IntPtr)native) != null)
							{
								IFlashingPlatform* native2 = m_Platform.Native;
								int num2 = (int)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)native2)))((IntPtr)native2);
								((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, global::FlashingPlatform.LogLevel, ushort*, void>*/)(void*)(int)(*(uint*)(*(int*)num2 + 4)))((IntPtr)num2, (global::FlashingPlatform.LogLevel)2, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj3) + 16)))((IntPtr)(&obj3)));
							}
							IntPtr ptr4 = (IntPtr)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj3) + 16)))((IntPtr)(&obj3));
							throw new Win32Exception(num, Marshal.PtrToStringUni(ptr4));
						}
						catch
						{
							//try-fault
							global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj3);
							throw;
						}
					}
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDelete<CGenericProgressShim *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDelete_003CCGenericProgressShim_0020_002A_003E_002E_007Bdtor_007D), &obj2);
					throw;
				}
				*(int*)(&obj2) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDelete_0040PAVCGenericProgressShim_0040_0040_0040RAII_0040_00406B_0040);
				global::_003CModule_003E.RAII_002ECAutoDelete_003CCGenericProgressShim_0020_002A_003E_002ERelease(&obj2);
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoComFree<unsigned short const *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
			global::_003CModule_003E.RAII_002ECAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E_002E_007Bdtor_007D(&obj);
			try
			{
				try
				{
					try
					{
						return;
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj3);
						throw;
					}
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDelete<CGenericProgressShim *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDelete_003CCGenericProgressShim_0020_002A_003E_002E_007Bdtor_007D), &obj2);
					throw;
				}
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoComFree<unsigned short const *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
		}

		[HandleProcessCorruptedStateExceptions]
		protected override void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
		{
			if (A_0)
			{
				try
				{
					_007EFlashingDevice();
					return;
				}
				finally
				{
					base.Dispose(true);
				}
			}
			try
			{
				_0021FlashingDevice();
			}
			finally
			{
				base.Dispose(false);
			}
		}
	}
}
