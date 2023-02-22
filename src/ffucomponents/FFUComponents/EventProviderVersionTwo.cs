using System;
using System.Diagnostics.Eventing;
using System.Runtime.InteropServices;

namespace FFUComponents
{
	internal class EventProviderVersionTwo : EventProvider
	{
		[StructLayout(LayoutKind.Explicit, Size = 16)]
		private struct EventData
		{
			[FieldOffset(0)]
			internal ulong DataPointer;

			[FieldOffset(8)]
			internal uint Size;

			[FieldOffset(12)]
			internal int Reserved;
		}

		internal EventProviderVersionTwo(Guid id)
			: base(id)
		{
		}

		internal unsafe bool TemplateDeviceSpecificEventWithString(ref EventDescriptor eventDescriptor, Guid DeviceId, string DeviceFriendlyName, string AssemblyFileVersion)
		{
			int num = 3;
			bool result = true;
			if (IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* ptr = stackalloc byte[(int)checked(unchecked((ulong)(uint)(sizeof(EventData) * num)) * 1uL)];
				EventData* ptr2 = (EventData*)ptr;
				ptr2->DataPointer = (ulong)(&DeviceId);
				ptr2->Size = (uint)sizeof(Guid);
				ptr2[1].Size = (uint)((DeviceFriendlyName.Length + 1) * 2);
				ptr2[2].Size = (uint)((AssemblyFileVersion.Length + 1) * 2);
				fixed (char* ptr3 = DeviceFriendlyName)
				{
					fixed (char* ptr4 = AssemblyFileVersion)
					{
						ptr2[1].DataPointer = (ulong)ptr3;
						ptr2[2].DataPointer = (ulong)ptr4;
						result = WriteEvent(ref eventDescriptor, num, (IntPtr)ptr);
					}
				}
			}
			return result;
		}

		internal unsafe bool TemplateDeviceSpecificEvent(ref EventDescriptor eventDescriptor, Guid DeviceId, string DeviceFriendlyName)
		{
			int num = 2;
			bool result = true;
			if (IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* ptr = stackalloc byte[(int)checked(unchecked((ulong)(uint)(sizeof(EventData) * num)) * 1uL)];
				EventData* ptr2 = (EventData*)ptr;
				ptr2->DataPointer = (ulong)(&DeviceId);
				ptr2->Size = (uint)sizeof(Guid);
				ptr2[1].Size = (uint)((DeviceFriendlyName.Length + 1) * 2);
				fixed (char* ptr3 = DeviceFriendlyName)
				{
					ptr2[1].DataPointer = (ulong)ptr3;
					result = WriteEvent(ref eventDescriptor, num, (IntPtr)ptr);
				}
			}
			return result;
		}

		internal unsafe bool TemplateDeviceEventWithErrorCode(ref EventDescriptor eventDescriptor, Guid DeviceId, string DeviceFriendlyName, int ErrorCode)
		{
			int num = 3;
			bool result = true;
			if (IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* ptr = stackalloc byte[(int)checked(unchecked((ulong)(uint)(sizeof(EventData) * num)) * 1uL)];
				EventData* ptr2 = (EventData*)ptr;
				ptr2->DataPointer = (ulong)(&DeviceId);
				ptr2->Size = (uint)sizeof(Guid);
				ptr2[1].Size = (uint)((DeviceFriendlyName.Length + 1) * 2);
				ptr2[2].DataPointer = (ulong)(&ErrorCode);
				ptr2[2].Size = 4u;
				fixed (char* ptr3 = DeviceFriendlyName)
				{
					ptr2[1].DataPointer = (ulong)ptr3;
					result = WriteEvent(ref eventDescriptor, num, (IntPtr)ptr);
				}
			}
			return result;
		}

		internal unsafe bool TemplateNotifyException(ref EventDescriptor eventDescriptor, string DevicePath, string Exception)
		{
			int num = 2;
			bool result = true;
			if (IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* ptr = stackalloc byte[(int)checked(unchecked((ulong)(uint)(sizeof(EventData) * num)) * 1uL)];
				EventData* ptr2 = (EventData*)ptr;
				ptr2->Size = (uint)((DevicePath.Length + 1) * 2);
				ptr2[1].Size = (uint)((Exception.Length + 1) * 2);
				fixed (char* ptr3 = DevicePath)
				{
					fixed (char* ptr4 = Exception)
					{
						ptr2->DataPointer = (ulong)ptr3;
						ptr2[1].DataPointer = (ulong)ptr4;
						result = WriteEvent(ref eventDescriptor, num, (IntPtr)ptr);
					}
				}
			}
			return result;
		}

		internal unsafe bool TemplateDeviceSpecificEventWithSize(ref EventDescriptor eventDescriptor, Guid DeviceId, string DeviceFriendlyName, int TransferSize)
		{
			int num = 3;
			bool result = true;
			if (IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* ptr = stackalloc byte[(int)checked(unchecked((ulong)(uint)(sizeof(EventData) * num)) * 1uL)];
				EventData* ptr2 = (EventData*)ptr;
				ptr2->DataPointer = (ulong)(&DeviceId);
				ptr2->Size = (uint)sizeof(Guid);
				ptr2[1].Size = (uint)((DeviceFriendlyName.Length + 1) * 2);
				ptr2[2].DataPointer = (ulong)(&TransferSize);
				ptr2[2].Size = 4u;
				fixed (char* ptr3 = DeviceFriendlyName)
				{
					ptr2[1].DataPointer = (ulong)ptr3;
					result = WriteEvent(ref eventDescriptor, num, (IntPtr)ptr);
				}
			}
			return result;
		}

		internal unsafe bool TemplateDeviceFlashParameters(ref EventDescriptor eventDescriptor, int USBTransactionSize, int PacketDataSize)
		{
			int num = 2;
			bool result = true;
			if (IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* ptr = stackalloc byte[(int)checked(unchecked((ulong)(uint)(sizeof(EventData) * num)) * 1uL)];
				EventData* ptr2 = (EventData*)ptr;
				ptr2->DataPointer = (ulong)(&USBTransactionSize);
				ptr2->Size = 4u;
				ptr2[1].DataPointer = (ulong)(&PacketDataSize);
				ptr2[1].Size = 4u;
				result = WriteEvent(ref eventDescriptor, num, (IntPtr)ptr);
			}
			return result;
		}
	}
}
