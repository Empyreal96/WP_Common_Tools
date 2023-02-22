using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using FlashingPlatform;
using RAII;

namespace Microsoft.Windows.Flashing.Platform
{
	public class ConnectedDevice : IDisposable
	{
		private unsafe IConnectedDevice* m_Device;

		private bool m_OwnDevice;

		internal FlashingPlatform m_Platform;

		internal unsafe IConnectedDevice* Native => m_Device;

		internal unsafe ConnectedDevice(IConnectedDevice* Device, [In] FlashingPlatform Platform)
		{
			m_Device = Device;
			m_OwnDevice = ((Device != null) ? true : false);
			m_Platform = Platform;
			base._002Ector();
		}

		internal unsafe void SetDevice(IConnectedDevice* Device)
		{
			m_Device = Device;
		}

		private void _007EConnectedDevice()
		{
			_0021ConnectedDevice();
		}

		private unsafe void _0021ConnectedDevice()
		{
			IConnectedDevice* device = m_Device;
			if (device != null)
			{
				if (m_OwnDevice)
				{
					((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, int>*/)(void*)(int)(*(uint*)(*(int*)device + 20)))((IntPtr)device);
				}
				m_Device = null;
			}
		}

		public unsafe virtual string GetDevicePath()
		{
			//Discarded unreachable code: IL_0097, IL_00b4, IL_00c6
			IConnectedDevice* device = m_Device;
			ushort* ptr;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ptr);
			int num = ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort**, int>*/)(void*)(int)(*(uint*)(int)(*(uint*)device)))((IntPtr)device, &ptr);
			if (num < 0)
			{
				CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E obj;
				System.Runtime.CompilerServices.Unsafe.SkipInit(out obj);
				System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) = (int)global::_003CModule_003E.UfphNativeStrFormat((ushort*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1DE_0040GCEKIMOP_0040_003F_0024AAF_003F_0024AAa_003F_0024AAi_003F_0024AAl_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAt_003F_0024AAo_003F_0024AA_003F5_003F_0024AAg_003F_0024AAe_003F_0024AAt_003F_0024AA_003F5_003F_0024AAd_003F_0024AAe_003F_0024AAv_003F_0024AAi_003F_0024AAc_003F_0024AAe_003F_0024AA_003F5_003F_0024AAp_003F_0024AAa_003F_0024AAt_003F_0024AAh_003F_0024AA_003F_0024AA_0040), __arglist());
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

		public unsafe virtual void SendRawData([In] byte[] Message, uint MessageLength, uint Timeout)
		{
			//Discarded unreachable code: IL_00d9, IL_00e9, IL_0111
			CAutoDeleteArray_003Cunsigned_0020char_003E obj;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj);
			System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020char_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) = (int)global::_003CModule_003E.new_005B_005D(MessageLength);
			*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDeleteArray_0040E_0040RAII_0040_00406B_0040);
			CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E obj2;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj2);
			try
			{
				IntPtr destination = (IntPtr)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, byte*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj));
				Marshal.Copy(Message, 0, destination, (int)MessageLength);
				IConnectedDevice* device = m_Device;
				int num = ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, byte*, uint, uint, int>*/)(void*)(int)(*(uint*)(*(int*)device + 4)))((IntPtr)device, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, byte*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj)), MessageLength, Timeout);
				if (num < 0)
				{
					System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj2, 4)) = (int)global::_003CModule_003E.UfphNativeStrFormat((ushort*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1DA_0040DIJJDJIP_0040_003F_0024AAF_003F_0024AAa_003F_0024AAi_003F_0024AAl_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAt_003F_0024AAo_003F_0024AA_003F5_003F_0024AAs_003F_0024AAe_003F_0024AAn_003F_0024AAd_003F_0024AA_003F5_003F_0024AAr_003F_0024AAa_003F_0024AAw_003F_0024AA_003F5_003F_0024AAd_003F_0024AAa_003F_0024AAt_003F_0024AAa_003F_0024AA_003F_0024AA_0040), __arglist());
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
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned char>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020char_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
			global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020char_003E_002E_007Bdtor_007D(&obj);
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
				global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned char>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020char_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
		}

		public unsafe virtual void ReceiveRawData(out byte[] Message, ref uint MessageLength, uint Timeout)
		{
			//Discarded unreachable code: IL_00c2, IL_011f
			uint num = MessageLength;
			CAutoDeleteArray_003Cunsigned_0020char_003E obj;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj);
			System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020char_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) = (int)global::_003CModule_003E.new_005B_005D(MessageLength);
			*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDeleteArray_0040E_0040RAII_0040_00406B_0040);
			CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E obj2;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj2);
			try
			{
				IConnectedDevice* device = m_Device;
				int num2 = ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, byte*, uint*, uint, int>*/)(void*)(int)(*(uint*)(*(int*)device + 8)))((IntPtr)device, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, byte*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj)), &num, Timeout);
				MessageLength = num;
				if (num2 < 0)
				{
					System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj2, 4)) = (int)global::_003CModule_003E.UfphNativeStrFormat((ushort*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1DG_0040KNJJMPJN_0040_003F_0024AAF_003F_0024AAa_003F_0024AAi_003F_0024AAl_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAt_003F_0024AAo_003F_0024AA_003F5_003F_0024AAr_003F_0024AAe_003F_0024AAc_003F_0024AAe_003F_0024AAi_003F_0024AAv_003F_0024AAe_003F_0024AA_003F5_003F_0024AAr_003F_0024AAa_003F_0024AAw_003F_0024AA_003F5_003F_0024AAd_003F_0024AAa_003F_0024AAt_003F_0024AAa_003F_0024AA_003F_0024AA_0040), __arglist());
					*(int*)(&obj2) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoDeleteArray_0040_0024_0024CBG_0040RAII_0040_00406B_0040);
					try
					{
						IFlashingPlatform* native = m_Platform.Native;
						if (((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)native)))((IntPtr)native) != null)
						{
							IFlashingPlatform* native2 = m_Platform.Native;
							int num3 = (int)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ILogger*>*/)(void*)(int)(*(uint*)(int)(*(uint*)native2)))((IntPtr)native2);
							((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, global::FlashingPlatform.LogLevel, ushort*, void>*/)(void*)(int)(*(uint*)(*(int*)num3 + 4)))((IntPtr)num3, (global::FlashingPlatform.LogLevel)2, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj2) + 16)))((IntPtr)(&obj2)));
						}
						IntPtr ptr = (IntPtr)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, ushort*>*/)(void*)(int)(*(uint*)(*(int*)(&obj2) + 16)))((IntPtr)(&obj2));
						throw new Win32Exception(num2, Marshal.PtrToStringUni(ptr));
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned short const >*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E_002E_007Bdtor_007D), &obj2);
						throw;
					}
				}
				Message = new byte[num];
				Marshal.Copy((IntPtr)((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, byte*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 16)))((IntPtr)(&obj)), Message, 0, (int)MessageLength);
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned char>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020char_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
			global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020char_003E_002E_007Bdtor_007D(&obj);
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
				global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoDeleteArray<unsigned char>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoDeleteArray_003Cunsigned_0020char_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
		}

		[return: MarshalAs(UnmanagedType.U1)]
		public unsafe virtual bool CanFlash()
		{
			IConnectedDevice* device = m_Device;
			return (byte)((((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, int>*/)(void*)(int)(*(uint*)(*(int*)device + 12)))((IntPtr)device) != 0) ? 1u : 0u) != 0;
		}

		public unsafe virtual FlashingDevice CreateFlashingDevice()
		{
			//Discarded unreachable code: IL_00b3, IL_0100, IL_0112, IL_0122
			CAutoRelease_003CFlashingPlatform_003A_003AIFlashingDevice_0020_002A_003E obj;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out obj);
			System.Runtime.CompilerServices.Unsafe.As<CAutoRelease_003CFlashingPlatform_003A_003AIFlashingDevice_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) = 0;
			*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoRelease_0040PAUIFlashingDevice_0040FlashingPlatform_0040_0040_0040RAII_0040_00406B_0040);
			FlashingDevice result;
			try
			{
				IConnectedDevice* device = m_Device;
				int num = ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, IFlashingDevice**, int>*/)(void*)(int)(*(uint*)(*(int*)device + 16)))((IntPtr)device, ((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, IFlashingDevice**>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 4)))((IntPtr)(&obj)));
				if (num < 0)
				{
					CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E obj2;
					System.Runtime.CompilerServices.Unsafe.SkipInit(out obj2);
					System.Runtime.CompilerServices.Unsafe.As<CAutoDeleteArray_003Cunsigned_0020short_0020const_0020_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj2, 4)) = (int)global::_003CModule_003E.UfphNativeStrFormat((ushort*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_1EC_0040LEPNEJDA_0040_003F_0024AAF_003F_0024AAa_003F_0024AAi_003F_0024AAl_003F_0024AAe_003F_0024AAd_003F_0024AA_003F5_003F_0024AAt_003F_0024AAo_003F_0024AA_003F5_003F_0024AAc_003F_0024AAr_003F_0024AAe_003F_0024AAa_003F_0024AAt_003F_0024AAe_003F_0024AA_003F5_003F_0024AAf_003F_0024AAl_003F_0024AAa_003F_0024AAs_003F_0024AAh_003F_0024AAi_003F_0024AAn_003F_0024AAg_003F_0024AA_003F5_003F_0024AAd_003F_0024AAe_003F_0024AAv_003F_0024AAi_003F_0024AAc_003F_0024AAe_0040), __arglist());
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
				result = new FlashingDevice(((UIntPtr/*delegate* unmanaged[Thiscall, Thiscall]<IntPtr, IFlashingDevice*>*/)(void*)(int)(*(uint*)(*(int*)(&obj) + 40)))((IntPtr)(&obj)), m_Platform);
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((UIntPtr/*delegate*<void*, void>*/)(void*)(ulong)(UIntPtr/*delegate*<CAutoRelease<FlashingPlatform::IFlashingDevice *>*, void>*/)(&global::_003CModule_003E.RAII_002ECAutoRelease_003CFlashingPlatform_003A_003AIFlashingDevice_0020_002A_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
			*(int*)(&obj) = (int)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024CAutoRelease_0040PAUIFlashingDevice_0040FlashingPlatform_0040_0040_0040RAII_0040_00406B_0040);
			global::_003CModule_003E.RAII_002ECAutoRelease_003CFlashingPlatform_003A_003AIFlashingDevice_0020_002A_003E_002ERelease(&obj);
			return result;
		}

		[HandleProcessCorruptedStateExceptions]
		protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
		{
			if (A_0)
			{
				_007EConnectedDevice();
				return;
			}
			try
			{
				_0021ConnectedDevice();
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

		~ConnectedDevice()
		{
			Dispose(false);
		}
	}
}
