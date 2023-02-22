using System;
using System.Diagnostics.Eventing;
using System.Runtime.InteropServices;

namespace FFUComponents
{
	internal class DeviceEventProvider : EventProvider
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

		internal DeviceEventProvider(Guid id)
			: base(id)
		{
		}

		internal unsafe bool TemplateDeviceEvent(ref EventDescriptor eventDescriptor, Guid DeviceUniqueId, string DeviceFriendlyName, string AdditionalInfoString)
		{
			int num = 3;
			bool result = true;
			if (IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* ptr = stackalloc byte[(int)checked(unchecked((ulong)(uint)(sizeof(EventData) * num)) * 1uL)];
				EventData* ptr2 = (EventData*)ptr;
				ptr2->DataPointer = (ulong)(&DeviceUniqueId);
				ptr2->Size = (uint)sizeof(Guid);
				ptr2[1].Size = (uint)((DeviceFriendlyName.Length + 1) * 2);
				ptr2[2].Size = (uint)((AdditionalInfoString.Length + 1) * 2);
				fixed (char* ptr3 = DeviceFriendlyName)
				{
					fixed (char* ptr4 = AdditionalInfoString)
					{
						ptr2[1].DataPointer = (ulong)ptr3;
						ptr2[2].DataPointer = (ulong)ptr4;
						result = WriteEvent(ref eventDescriptor, num, (IntPtr)ptr);
					}
				}
			}
			return result;
		}
	}
}
