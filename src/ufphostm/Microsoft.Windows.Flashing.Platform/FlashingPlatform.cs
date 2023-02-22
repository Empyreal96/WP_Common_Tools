using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using FlashingPlatform;
using RAII;

namespace Microsoft.Windows.Flashing.Platform
{
	public class FlashingPlatform : IDisposable
	{
		private unsafe IFlashingPlatform* m_Platform;

		private DeviceNotificationCallback m_DeviceNotificationCallback;

		private unsafe CDeviceNotificationCallbackShim* m_DeviceNotificationCallbackShim;

		public static uint MajorVerion = 0u;

		public static uint MinorVerion = 2u;

		internal unsafe IFlashingPlatform* Native => m_Platform;

		public unsafe FlashingPlatform(string LogFile)
		{
			//Discarded unreachable code: IL_004e, IL_00be, IL_0160, IL_01d9, IL_01e9
			uint num;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out num);
			uint num2;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out num2);
			int num3 = NativeFlashingPlatform.GetFlashingPlatformVersion(&num, &num2);
			if (num3 < 0)
			{
				CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E obj;
				System.Runtime.CompilerServices.Unsafe.SkipInit(out obj);
				System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) = (int)global::_003CModule_003E.UfphNativeStrFormat((ushort*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1FA_0040DFPBFODB_0040_003F_0024AAF_003F_0024AAa_003F_0024AAi_003F_0024AAl_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAt_003F_0024AAo_003F_0024AA_003F5_003F_0024AAg_003F_0024AAe_003F_0024AAt_003F_0024AA_003F5_003F_0024AAf_003F_0024AAl_003F_0024AAa_003F_0024AAs_003F_0024AAh_003F_0024AAi_003F_0024AAn_003F_0024AAg_003F_0024AA_003F5_003F_0024AAp_003F_0024AAl_003F_0024AAa_003F_0024AAt_003F_0024AAf_003F_0024AAo_003F_0024AAr_003F_0024AAm_003F_0024AA_003F5_0040), __arglist());
				*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDeleteArray_0040_0024_0024CBG_0040RAII_0040_00406B_0040);
				try
				{
					IntPtr ptr = (IntPtr)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj));
					throw new Win32Exception(num3, Marshal.PtrToStringUni(ptr));
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj);
					throw;
				}
			}
			if (num != MajorVerion || num2 != MinorVerion)
			{
				num3 = -2147019873;
			}
			if (num3 < 0)
			{
				CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E obj2;
				System.Runtime.CompilerServices.Unsafe.SkipInit(out obj2);
				System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj2, 4)) = (int)global::_003CModule_003E.UfphNativeStrFormat((ushort*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1KE_0040NICNNOJH_0040_003F_0024AAM_003F_0024AAi_003F_0024AAs_003F_0024AAm_003F_0024AAa_003F_0024AAt_003F_0024AAc_003F_0024AAh_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAv_003F_0024AAe_003F_0024AAr_003F_0024AAs_003F_0024AAi_003F_0024AAo_003F_0024AAn_003F_0024AA_003F5_003F_0024AAo_003F_0024AAf_003F_0024AA_003F5_003F_0024AAn_003F_0024AAa_003F_0024AAt_003F_0024AAi_003F_0024AAv_003F_0024AAe_003F_0024AA_003F5_003F_0024AAf_003F_0024AAl_003F_0024AAa_0040), __arglist(MajorVerion, MinorVerion, num, num2));
				*(int*)(&obj2) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDeleteArray_0040_0024_0024CBG_0040RAII_0040_00406B_0040);
				try
				{
					IntPtr ptr2 = (IntPtr)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj2) + 16)))((IntPtr)(&obj2));
					throw new Win32Exception(num3, Marshal.PtrToStringUni(ptr2));
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj2);
					throw;
				}
			}
			CAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E obj3;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj3);
			System.Runtime.CompilerServices.Unsafe.As<CAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj3, 4)) = (int)((LogFile == null) ? null : Marshal.StringToCoTaskMemUni(LogFile).ToPointer());
			*(int*)(&obj3) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoComFree_0040PBG_0040RAII_0040_00406B_0040);
			CAutoRelease_003CFlashingPlatform_003A_003AIFlashingPlatform_0020_002A_003E obj4;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj4);
			CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E obj5;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj5);
			try
			{
				System.Runtime.CompilerServices.Unsafe.As<CAutoRelease_003CFlashingPlatform_003A_003AIFlashingPlatform_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj4, 4)) = 0;
				*(int*)(&obj4) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoRelease_0040PAUIFlashingPlatform_0040FlashingPlatform_0040_0040_0040RAII_0040_00406B_0040);
				try
				{
					num3 = NativeFlashingPlatform.CreateFlashingPlatform(((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj3) + 16)))((IntPtr)(&obj3)), ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, IFlashingPlatform**>*/)(void*)(int)(*(uint*)(*(int*)(&obj4) + 4)))((IntPtr)(&obj4)));
					if (num3 < 0)
					{
						System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj5, 4)) = (int)global::_003CModule_003E.UfphNativeStrFormat((ushort*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1EG_0040HDIJANGN_0040_003F_0024AAF_003F_0024AAa_003F_0024AAi_003F_0024AAl_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAt_003F_0024AAo_003F_0024AA_003F5_003F_0024AAc_003F_0024AAr_003F_0024AAe_003F_0024AAa_003F_0024AAt_003F_0024AAe_003F_0024AA_003F5_003F_0024AAf_003F_0024AAl_003F_0024AAa_003F_0024AAs_003F_0024AAh_003F_0024AAi_003F_0024AAn_003F_0024AAg_003F_0024AA_003F5_003F_0024AAp_003F_0024AAl_003F_0024AAa_003F_0024AAt_003F_0024AAf_003F_0024AAo_0040), __arglist());
						*(int*)(&obj5) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDeleteArray_0040_0024_0024CBG_0040RAII_0040_00406B_0040);
						try
						{
							IntPtr ptr3 = (IntPtr)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj5) + 16)))((IntPtr)(&obj5));
							throw new Win32Exception(num3, Marshal.PtrToStringUni(ptr3));
						}
						catch
						{
							//try-fault
							global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj5);
							throw;
						}
					}
					m_Platform = ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, IFlashingPlatform*>*/)(void*)(int)(*(uint*)(*(int*)(&obj4) + 40)))((IntPtr)(&obj4));
					m_DeviceNotificationCallbackShim = null;
					m_DeviceNotificationCallback = null;
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoRelease<FlashingPlatform::IFlashingPlatform *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoRelease_003CFlashingPlatform_003A_003AIFlashingPlatform_0020_002A_003E_002E_007Bdtor_007D), &obj4);
					throw;
				}
				*(int*)(&obj4) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoRelease_0040PAUIFlashingPlatform_0040FlashingPlatform_0040_0040_0040RAII_0040_00406B_0040);
				global::_003CModule_003E.RAII_002ECAutoRelease_003CFlashingPlatform_003A_003AIFlashingPlatform_0020_002A_003E_002ERelease(&obj4);
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoComFree<unsigned short const *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E_002E_007Bdtor_007D), &obj3);
				throw;
			}
			global::_003CModule_003E.RAII_002ECAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E_002E_007Bdtor_007D(&obj3);
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
						global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj5);
						throw;
					}
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoRelease<FlashingPlatform::IFlashingPlatform *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoRelease_003CFlashingPlatform_003A_003AIFlashingPlatform_0020_002A_003E_002E_007Bdtor_007D), &obj4);
					throw;
				}
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoComFree<unsigned short const *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E_002E_007Bdtor_007D), &obj3);
				throw;
			}
		}

		private void _007EFlashingPlatform()
		{
			_0021FlashingPlatform();
		}

		private unsafe void _0021FlashingPlatform()
		{
			CDeviceNotificationCallbackShim* deviceNotificationCallbackShim = m_DeviceNotificationCallbackShim;
			if (deviceNotificationCallbackShim != null)
			{
				CDeviceNotificationCallbackShim* ptr = deviceNotificationCallbackShim;
				global::_003CModule_003E.gcroot_003CMicrosoft_003A_003AWindows_003A_003AFlashing_003A_003APlatform_003A_003ADeviceNotificationCallback_0020_005E_003E_002E_007Bdtor_007D((gcroot_003CMicrosoft_003A_003AWindows_003A_003AFlashing_003A_003APlatform_003A_003ADeviceNotificationCallback_0020_005E_003E*)((byte*)ptr + 4));
				global::_003CModule_003E.delete(ptr);
				m_DeviceNotificationCallbackShim = null;
			}
			IFlashingPlatform* platform = m_Platform;
			if (platform != null)
			{
				((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, int>*/)(void*)(int)(*(uint*)(*(int*)platform + 28)))((IntPtr)platform);
				m_Platform = null;
			}
			m_DeviceNotificationCallback = null;
		}

		public void GetVersion(out uint Major, out uint Minor)
		{
			Major = MajorVerion;
			Minor = MinorVerion;
		}

		public Logger GetLogger()
		{
			return new Logger(this);
		}

		public unsafe ConnectedDevice CreateConnectedDevice([In] string DevicePath)
		{
			//Discarded unreachable code: IL_00df, IL_013e, IL_0151, IL_0161, IL_0171
			ushort* ptr = (ushort*)((DevicePath == null) ? null : Marshal.StringToCoTaskMemUni(DevicePath).ToPointer());
			CAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E obj;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj);
			System.Runtime.CompilerServices.Unsafe.As<CAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) = (int)ptr;
			*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoComFree_0040PBG_0040RAII_0040_00406B_0040);
			ConnectedDevice result;
			try
			{
				CAutoRelease_003CFlashingPlatform_003A_003AIConnectedDevice_0020_002A_003E obj2;
				System.Runtime.CompilerServices.Unsafe.SkipInit(out obj2);
				System.Runtime.CompilerServices.Unsafe.As<CAutoRelease_003CFlashingPlatform_003A_003AIConnectedDevice_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj2, 4)) = 0;
				*(int*)(&obj2) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoRelease_0040PAUIConnectedDevice_0040FlashingPlatform_0040_0040_0040RAII_0040_00406B_0040);
				try
				{
					IFlashingPlatform* platform = m_Platform;
					int num = ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*, IConnectedDevice**, int>*/)(void*)(int)(*(uint*)(*(int*)platform + 4)))((IntPtr)platform, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj)), ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, IConnectedDevice**>*/)(void*)(int)(*(uint*)(*(int*)(&obj2) + 4)))((IntPtr)(&obj2)));
					if (num < 0)
					{
						CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E obj3;
						System.Runtime.CompilerServices.Unsafe.SkipInit(out obj3);
						System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj3, 4)) = (int)global::_003CModule_003E.UfphNativeStrFormat((ushort*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1EE_0040GEIPDOOK_0040_003F_0024AAF_003F_0024AAa_003F_0024AAi_003F_0024AAl_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAt_003F_0024AAo_003F_0024AA_003F5_003F_0024AAc_003F_0024AAr_003F_0024AAe_003F_0024AAa_003F_0024AAt_003F_0024AAe_003F_0024AA_003F5_003F_0024AAc_003F_0024AAo_003F_0024AAn_003F_0024AAn_003F_0024AAe_003F_0024AAc_003F_0024AAt_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAd_003F_0024AAe_003F_0024AAv_003F_0024AAi_003F_0024AAc_0040), __arglist());
						*(int*)(&obj3) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDeleteArray_0040_0024_0024CBG_0040RAII_0040_00406B_0040);
						try
						{
							platform = m_Platform;
							IFlashingPlatform* intPtr = platform;
							if (((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)intPtr)))((IntPtr)intPtr) != null)
							{
								platform = m_Platform;
								IFlashingPlatform* intPtr2 = platform;
								int num2 = (int)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)intPtr2)))((IntPtr)intPtr2);
								((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, global::FlashingPlatform.LogLevel, ushort*, void>*/)(void*)(int)(*(uint*)(*(int*)num2 + 4)))((IntPtr)num2, (global::FlashingPlatform.LogLevel)2, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj3) + 16)))((IntPtr)(&obj3)));
							}
							IntPtr ptr2 = (IntPtr)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj3) + 16)))((IntPtr)(&obj3));
							throw new Win32Exception(num, Marshal.PtrToStringUni(ptr2));
						}
						catch
						{
							//try-fault
							global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj3);
							throw;
						}
					}
					result = new ConnectedDevice(((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, IConnectedDevice*>*/)(void*)(int)(*(uint*)(*(int*)(&obj2) + 40)))((IntPtr)(&obj2)), this);
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoRelease<FlashingPlatform::IConnectedDevice *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoRelease_003CFlashingPlatform_003A_003AIConnectedDevice_0020_002A_003E_002E_007Bdtor_007D), &obj2);
					throw;
				}
				*(int*)(&obj2) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoRelease_0040PAUIConnectedDevice_0040FlashingPlatform_0040_0040_0040RAII_0040_00406B_0040);
				global::_003CModule_003E.RAII_002ECAutoRelease_003CFlashingPlatform_003A_003AIConnectedDevice_0020_002A_003E_002ERelease(&obj2);
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoComFree<unsigned short const *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
			global::_003CModule_003E.RAII_002ECAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E_002E_007Bdtor_007D(&obj);
			return result;
		}

		public unsafe FlashingDevice CreateFlashingDevice([In] string DevicePath)
		{
			//Discarded unreachable code: IL_00df, IL_013e, IL_0151, IL_0161, IL_0171
			ushort* ptr = (ushort*)((DevicePath == null) ? null : Marshal.StringToCoTaskMemUni(DevicePath).ToPointer());
			CAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E obj;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj);
			System.Runtime.CompilerServices.Unsafe.As<CAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) = (int)ptr;
			*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoComFree_0040PBG_0040RAII_0040_00406B_0040);
			FlashingDevice result;
			try
			{
				CAutoRelease_003CFlashingPlatform_003A_003AIFlashingDevice_0020_002A_003E obj2;
				System.Runtime.CompilerServices.Unsafe.SkipInit(out obj2);
				System.Runtime.CompilerServices.Unsafe.As<CAutoRelease_003CFlashingPlatform_003A_003AIFlashingDevice_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj2, 4)) = 0;
				*(int*)(&obj2) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoRelease_0040PAUIFlashingDevice_0040FlashingPlatform_0040_0040_0040RAII_0040_00406B_0040);
				try
				{
					IFlashingPlatform* platform = m_Platform;
					int num = ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*, IFlashingDevice**, int>*/)(void*)(int)(*(uint*)(*(int*)platform + 8)))((IntPtr)platform, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj)), ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, IFlashingDevice**>*/)(void*)(int)(*(uint*)(*(int*)(&obj2) + 4)))((IntPtr)(&obj2)));
					if (num < 0)
					{
						CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E obj3;
						System.Runtime.CompilerServices.Unsafe.SkipInit(out obj3);
						System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj3, 4)) = (int)global::_003CModule_003E.UfphNativeStrFormat((ushort*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1EC_0040LEPNEJDA_0040_003F_0024AAF_003F_0024AAa_003F_0024AAi_003F_0024AAl_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAt_003F_0024AAo_003F_0024AA_003F5_003F_0024AAc_003F_0024AAr_003F_0024AAe_003F_0024AAa_003F_0024AAt_003F_0024AAe_003F_0024AA_003F5_003F_0024AAf_003F_0024AAl_003F_0024AAa_003F_0024AAs_003F_0024AAh_003F_0024AAi_003F_0024AAn_003F_0024AAg_003F_0024AA_003F5_003F_0024AAd_003F_0024AAe_003F_0024AAv_003F_0024AAi_003F_0024AAc_003F_0024AAe_0040), __arglist());
						*(int*)(&obj3) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDeleteArray_0040_0024_0024CBG_0040RAII_0040_00406B_0040);
						try
						{
							platform = m_Platform;
							IFlashingPlatform* intPtr = platform;
							if (((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)intPtr)))((IntPtr)intPtr) != null)
							{
								platform = m_Platform;
								IFlashingPlatform* intPtr2 = platform;
								int num2 = (int)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)intPtr2)))((IntPtr)intPtr2);
								((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, global::FlashingPlatform.LogLevel, ushort*, void>*/)(void*)(int)(*(uint*)(*(int*)num2 + 4)))((IntPtr)num2, (global::FlashingPlatform.LogLevel)2, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj3) + 16)))((IntPtr)(&obj3)));
							}
							IntPtr ptr2 = (IntPtr)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj3) + 16)))((IntPtr)(&obj3));
							throw new Win32Exception(num, Marshal.PtrToStringUni(ptr2));
						}
						catch
						{
							//try-fault
							global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj3);
							throw;
						}
					}
					result = new FlashingDevice(((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, IFlashingDevice*>*/)(void*)(int)(*(uint*)(*(int*)(&obj2) + 40)))((IntPtr)(&obj2)), this);
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoRelease<FlashingPlatform::IFlashingDevice *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoRelease_003CFlashingPlatform_003A_003AIFlashingDevice_0020_002A_003E_002E_007Bdtor_007D), &obj2);
					throw;
				}
				*(int*)(&obj2) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoRelease_0040PAUIFlashingDevice_0040FlashingPlatform_0040_0040_0040RAII_0040_00406B_0040);
				global::_003CModule_003E.RAII_002ECAutoRelease_003CFlashingPlatform_003A_003AIFlashingDevice_0020_002A_003E_002ERelease(&obj2);
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoComFree<unsigned short const *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
			global::_003CModule_003E.RAII_002ECAutoComFree_003Cunsigned_0020short_0020const_0020_002A_003E_002E_007Bdtor_007D(&obj);
			return result;
		}

		public unsafe ConnectedDeviceCollection GetConnectedDeviceCollection()
		{
			//Discarded unreachable code: IL_00ad, IL_00f5, IL_0107, IL_0117
			CAutoRelease_003CFlashingPlatform_003A_003AIConnectedDeviceCollection_0020_002A_003E obj;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj);
			System.Runtime.CompilerServices.Unsafe.As<CAutoRelease_003CFlashingPlatform_003A_003AIConnectedDeviceCollection_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) = 0;
			*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoRelease_0040PAUIConnectedDeviceCollection_0040FlashingPlatform_0040_0040_0040RAII_0040_00406B_0040);
			ConnectedDeviceCollection result;
			try
			{
				IFlashingPlatform* platform = m_Platform;
				int num = ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, IConnectedDeviceCollection**, int>*/)(void*)(int)(*(uint*)(*(int*)platform + 12)))((IntPtr)platform, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, IConnectedDeviceCollection**>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 4)))((IntPtr)(&obj)));
				if (num < 0)
				{
					CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E obj2;
					System.Runtime.CompilerServices.Unsafe.SkipInit(out obj2);
					System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj2, 4)) = (int)global::_003CModule_003E.UfphNativeStrFormat((ushort*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1FE_0040BCHEDBFP_0040_003F_0024AAF_003F_0024AAa_003F_0024AAi_003F_0024AAl_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAt_003F_0024AAo_003F_0024AA_003F5_003F_0024AAg_003F_0024AAe_003F_0024AAt_003F_0024AA_003F5_003F_0024AAc_003F_0024AAo_003F_0024AAn_003F_0024AAn_003F_0024AAe_003F_0024AAc_003F_0024AAt_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAd_003F_0024AAe_003F_0024AAv_003F_0024AAi_003F_0024AAc_003F_0024AAe_003F_0024AA_003F5_003F_0024AAc_0040), __arglist());
					*(int*)(&obj2) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDeleteArray_0040_0024_0024CBG_0040RAII_0040_00406B_0040);
					try
					{
						platform = m_Platform;
						IFlashingPlatform* intPtr = platform;
						if (((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)intPtr)))((IntPtr)intPtr) != null)
						{
							platform = m_Platform;
							IFlashingPlatform* intPtr2 = platform;
							int num2 = (int)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)intPtr2)))((IntPtr)intPtr2);
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
				result = new ConnectedDeviceCollection(((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, IConnectedDeviceCollection*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 40)))((IntPtr)(&obj)), this);
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoRelease<FlashingPlatform::IConnectedDeviceCollection *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoRelease_003CFlashingPlatform_003A_003AIConnectedDeviceCollection_0020_002A_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
			*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoRelease_0040PAUIConnectedDeviceCollection_0040FlashingPlatform_0040_0040_0040RAII_0040_00406B_0040);
			global::_003CModule_003E.RAII_002ECAutoRelease_003CFlashingPlatform_003A_003AIConnectedDeviceCollection_0020_002A_003E_002ERelease(&obj);
			return result;
		}

		public unsafe void RegisterDeviceNotificationCallback(DeviceNotificationCallback Callback, ref DeviceNotificationCallback OldCallback)
		{
			//Discarded unreachable code: IL_00dd, IL_0144
			CDeviceNotificationCallbackShim* ptr3;
			if (Callback != null)
			{
				CDeviceNotificationCallbackShim* ptr = (CDeviceNotificationCallbackShim*)global::_003CModule_003E.@new(8u);
				CDeviceNotificationCallbackShim* ptr2;
				try
				{
					ptr2 = ((ptr == null) ? null : global::_003CModule_003E.CDeviceNotificationCallbackShim_002E_007Bctor_007D(ptr, Callback));
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.delete(ptr);
					throw;
				}
				ptr3 = ptr2;
			}
			else
			{
				ptr3 = null;
			}
			CAutoDelete_003CCDeviceNotificationCallbackShim_0020_002A_003E obj;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj);
			System.Runtime.CompilerServices.Unsafe.As<CAutoDelete_003CCDeviceNotificationCallbackShim_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) = (int)ptr3;
			*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDelete_0040PAVCDeviceNotificationCallbackShim_0040_0040_0040RAII_0040_00406B_0040);
			CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E obj2;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj2);
			try
			{
				IFlashingPlatform* platform = m_Platform;
				int num = ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, IDeviceNotificationCallback*, IDeviceNotificationCallback**, int>*/)(void*)(int)(*(uint*)(*(int*)platform + 16)))((IntPtr)platform, (IDeviceNotificationCallback*)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, CDeviceNotificationCallbackShim*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj)), null);
				if (num < 0)
				{
					System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj2, 4)) = (int)global::_003CModule_003E.UfphNativeStrFormat((ushort*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1GA_0040LFCCFCBB_0040_003F_0024AAF_003F_0024AAa_003F_0024AAi_003F_0024AAl_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAt_003F_0024AAo_003F_0024AA_003F5_003F_0024AAr_003F_0024AAe_003F_0024AAg_003F_0024AAi_003F_0024AAs_003F_0024AAt_003F_0024AAe_003F_0024AAr_003F_0024AA_003F5_003F_0024AAd_003F_0024AAe_003F_0024AAv_003F_0024AAi_003F_0024AAc_003F_0024AAe_003F_0024AA_003F5_003F_0024AAn_003F_0024AAo_003F_0024AAt_003F_0024AAi_003F_0024AAf_003F_0024AAi_0040), __arglist());
					*(int*)(&obj2) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDeleteArray_0040_0024_0024CBG_0040RAII_0040_00406B_0040);
					try
					{
						platform = m_Platform;
						IFlashingPlatform* intPtr = platform;
						if (((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)intPtr)))((IntPtr)intPtr) != null)
						{
							platform = m_Platform;
							IFlashingPlatform* intPtr2 = platform;
							int num2 = (int)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)intPtr2)))((IntPtr)intPtr2);
							((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, global::FlashingPlatform.LogLevel, ushort*, void>*/)(void*)(int)(*(uint*)(*(int*)num2 + 4)))((IntPtr)num2, (global::FlashingPlatform.LogLevel)2, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj2) + 16)))((IntPtr)(&obj2)));
						}
						IntPtr ptr4 = (IntPtr)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj2) + 16)))((IntPtr)(&obj2));
						throw new Win32Exception(num, Marshal.PtrToStringUni(ptr4));
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj2);
						throw;
					}
				}
				if (OldCallback != null)
				{
					OldCallback = m_DeviceNotificationCallback;
				}
				m_DeviceNotificationCallback = Callback;
				m_DeviceNotificationCallbackShim = ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, CDeviceNotificationCallbackShim*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 40)))((IntPtr)(&obj));
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDelete<CDeviceNotificationCallbackShim *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDelete_003CCDeviceNotificationCallbackShim_0020_002A_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
			*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDelete_0040PAVCDeviceNotificationCallbackShim_0040_0040_0040RAII_0040_00406B_0040);
			global::_003CModule_003E.RAII_002ECAutoDelete_003CCDeviceNotificationCallbackShim_0020_002A_003E_002ERelease(&obj);
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
				global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDelete<CDeviceNotificationCallbackShim *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDelete_003CCDeviceNotificationCallbackShim_0020_002A_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
		}

		public unsafe string GetErrorMessage(int HResult)
		{
			IFlashingPlatform* platform = m_Platform;
			ushort* ptr = ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, int, ushort*>*/)(void*)(int)(*(uint*)(*(int*)platform + 20)))((IntPtr)platform, HResult);
			return (ptr == null) ? null : Marshal.PtrToStringUni((IntPtr)ptr);
		}

		public unsafe int Thor2ResultFromHResult(int HResult)
		{
			IFlashingPlatform* platform = m_Platform;
			return ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, int, int>*/)(void*)(int)(*(uint*)(*(int*)platform + 24)))((IntPtr)platform, HResult);
		}

		[HandleProcessCorruptedStateExceptions]
		protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
		{
			if (A_0)
			{
				_007EFlashingPlatform();
				return;
			}
			try
			{
				_0021FlashingPlatform();
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

		~FlashingPlatform()
		{
			Dispose(false);
		}
	}
}
