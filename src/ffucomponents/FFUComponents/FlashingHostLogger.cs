using System;
using System.Diagnostics.Eventing;

namespace FFUComponents
{
	public class FlashingHostLogger : IDisposable
	{
		internal EventProviderVersionTwo m_provider = new EventProviderVersionTwo(new Guid("fb961307-bc64-4de4-8828-81d583524da0"));

		private Guid FlashId = new Guid("80ada65c-a7fa-49f8-a2ed-f67790c8f016");

		private Guid DeviceStatusChangeId = new Guid("3a02d575-c63d-4a76-9adf-9b6b736c66dc");

		private Guid TransferId = new Guid("211e6307-fd7c-49f9-a4db-d9ae5a4adb22");

		private Guid BootmeId = new Guid("a0cd9e55-fb70-452f-ac50-2eb82d2984b5");

		private Guid SkipId = new Guid("4979cb5a-17d4-47c6-9ac6-e97446bd74f4");

		private Guid ResetId = new Guid("768fda16-a5c7-44cf-8a47-03580b28538d");

		private Guid RebootId = new Guid("850eedee-9b52-4171-af6f-73c34d84a893");

		private Guid ReconnectId = new Guid("1a80ed37-3a4f-4b81-a466-accb411f96e1");

		private Guid ConnectId = new Guid("bebe24cb-92b1-40ca-843a-f2f9f0cab947");

		private Guid FileReadId = new Guid("d875a842-f690-40bf-880a-16e7d2a88d85");

		private Guid MutexWaitId = new Guid("3120aadc-6b30-4509-bedf-9696c78ddd9c");

		private Guid MassStorageId = new Guid("1b67e5c6-caab-4424-8d24-5c2c258aff5f");

		private Guid StreamClearId = new Guid("d32ce88a-c858-4ed1-86ac-764c58bf2599");

		private Guid ClearIdId = new Guid("3aa9618a-8ac9-4386-b524-c32f4326e59e");

		private Guid WimId = new Guid("0a86e459-1f85-459f-a9da-dca82415c492");

		private Guid WimTransferId = new Guid("53874dd6-905f-4a4c-ac66-5dadb02f4ce8");

		private Guid WimPacketId = new Guid("6f4a3de2-cddd-40d5-829d-861ccbcaff4d");

		private Guid BootModeId = new Guid("07bacab6-769a-4b6c-a68f-3524423291d2");

		protected EventDescriptor Flash_Start;

		protected EventDescriptor Flash_Stop;

		protected EventDescriptor Device_Attach;

		protected EventDescriptor Device_Detach;

		protected EventDescriptor Device_Remove;

		protected EventDescriptor Flash_Error;

		protected EventDescriptor Flash_Timeout;

		protected EventDescriptor TransferException;

		protected EventDescriptor ReconnectIOException;

		protected EventDescriptor ReconnectWin32Exception;

		protected EventDescriptor ReadBootmeIOException;

		protected EventDescriptor ReadBootmeWin32Exception;

		protected EventDescriptor SkipIOException;

		protected EventDescriptor SkipWin32Exception;

		protected EventDescriptor WriteSkipFailed;

		protected EventDescriptor USBResetWin32Exception;

		protected EventDescriptor RebootIOException;

		protected EventDescriptor RebootWin32Exception;

		protected EventDescriptor ConnectWin32Exception;

		protected EventDescriptor ThreadException;

		protected EventDescriptor FileRead_Start;

		protected EventDescriptor FileRead_Stop;

		protected EventDescriptor WaitAbandoned;

		protected EventDescriptor MutexTimeout;

		protected EventDescriptor ConnectNotifyException;

		protected EventDescriptor DisconnectNotifyException;

		protected EventDescriptor InitNotifyException;

		protected EventDescriptor MassStorageIOException;

		protected EventDescriptor MassStorageWin32Exception;

		protected EventDescriptor StreamClearStart;

		protected EventDescriptor StreamClearStop;

		protected EventDescriptor StreamClearPushWin32Exception;

		protected EventDescriptor StreamClearPullWin32Exception;

		protected EventDescriptor StreamClearIOException;

		protected EventDescriptor ClearIdIOException;

		protected EventDescriptor ClearIdWin32Exception;

		protected EventDescriptor WimSuccess;

		protected EventDescriptor WimError;

		protected EventDescriptor WimIOException;

		protected EventDescriptor WimWin32Exception;

		protected EventDescriptor WimTransferStart;

		protected EventDescriptor WimTransferStop;

		protected EventDescriptor WimPacketStart;

		protected EventDescriptor WimPacketStop;

		protected EventDescriptor WimGetStatus;

		protected EventDescriptor BootModeIOException;

		protected EventDescriptor BootModeWin32Exception;

		protected EventDescriptor DeviceFlashParameters;

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				m_provider.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public FlashingHostLogger()
		{
			Flash_Start = new EventDescriptor(0, 0, 0, 4, 1, 1, 0L);
			Flash_Stop = new EventDescriptor(1, 0, 0, 4, 2, 1, 0L);
			Device_Attach = new EventDescriptor(2, 0, 0, 4, 10, 2, 0L);
			Device_Detach = new EventDescriptor(3, 0, 0, 4, 11, 2, 0L);
			Device_Remove = new EventDescriptor(4, 0, 0, 4, 12, 2, 0L);
			Flash_Error = new EventDescriptor(5, 0, 0, 2, 0, 1, 0L);
			Flash_Timeout = new EventDescriptor(6, 0, 0, 2, 0, 1, 0L);
			TransferException = new EventDescriptor(7, 0, 0, 2, 0, 3, 0L);
			ReconnectIOException = new EventDescriptor(8, 0, 0, 2, 13, 8, 0L);
			ReconnectWin32Exception = new EventDescriptor(9, 0, 0, 2, 14, 8, 0L);
			ReadBootmeIOException = new EventDescriptor(10, 0, 0, 2, 13, 4, 0L);
			ReadBootmeWin32Exception = new EventDescriptor(11, 0, 0, 2, 14, 4, 0L);
			SkipIOException = new EventDescriptor(12, 0, 0, 2, 0, 5, 0L);
			SkipWin32Exception = new EventDescriptor(13, 0, 0, 2, 0, 5, 0L);
			WriteSkipFailed = new EventDescriptor(14, 0, 0, 2, 15, 5, 0L);
			USBResetWin32Exception = new EventDescriptor(15, 0, 0, 2, 14, 6, 0L);
			RebootIOException = new EventDescriptor(16, 0, 0, 2, 13, 7, 0L);
			RebootWin32Exception = new EventDescriptor(17, 0, 0, 2, 14, 7, 0L);
			ConnectWin32Exception = new EventDescriptor(18, 0, 0, 2, 14, 9, 0L);
			ThreadException = new EventDescriptor(19, 0, 0, 2, 15, 2, 0L);
			FileRead_Start = new EventDescriptor(20, 0, 0, 4, 1, 10, 0L);
			FileRead_Stop = new EventDescriptor(21, 0, 0, 4, 2, 10, 0L);
			WaitAbandoned = new EventDescriptor(22, 0, 0, 2, 2, 11, 0L);
			MutexTimeout = new EventDescriptor(23, 0, 0, 2, 2, 11, 0L);
			ConnectNotifyException = new EventDescriptor(24, 0, 0, 3, 10, 2, 0L);
			DisconnectNotifyException = new EventDescriptor(25, 0, 0, 3, 12, 2, 0L);
			InitNotifyException = new EventDescriptor(26, 0, 0, 3, 10, 2, 0L);
			MassStorageIOException = new EventDescriptor(27, 0, 0, 2, 13, 12, 0L);
			MassStorageWin32Exception = new EventDescriptor(28, 0, 0, 2, 14, 12, 0L);
			StreamClearStart = new EventDescriptor(29, 0, 0, 4, 1, 13, 0L);
			StreamClearStop = new EventDescriptor(30, 0, 0, 4, 2, 13, 0L);
			StreamClearPushWin32Exception = new EventDescriptor(31, 0, 0, 4, 14, 13, 0L);
			StreamClearPullWin32Exception = new EventDescriptor(32, 0, 0, 4, 14, 13, 0L);
			StreamClearIOException = new EventDescriptor(33, 0, 0, 4, 13, 13, 0L);
			ClearIdIOException = new EventDescriptor(34, 0, 0, 2, 13, 14, 0L);
			ClearIdWin32Exception = new EventDescriptor(35, 0, 0, 2, 14, 14, 0L);
			WimSuccess = new EventDescriptor(36, 0, 0, 4, 16, 15, 0L);
			WimError = new EventDescriptor(37, 0, 0, 2, 16, 15, 0L);
			WimIOException = new EventDescriptor(38, 0, 0, 2, 13, 15, 0L);
			WimWin32Exception = new EventDescriptor(39, 0, 0, 2, 14, 15, 0L);
			WimTransferStart = new EventDescriptor(40, 0, 0, 4, 1, 16, 0L);
			WimTransferStop = new EventDescriptor(41, 0, 0, 4, 2, 16, 0L);
			WimPacketStart = new EventDescriptor(42, 0, 0, 4, 1, 17, 0L);
			WimPacketStop = new EventDescriptor(43, 0, 0, 4, 2, 17, 0L);
			WimGetStatus = new EventDescriptor(44, 0, 0, 4, 1, 15, 0L);
			BootModeIOException = new EventDescriptor(45, 0, 0, 2, 13, 18, 0L);
			BootModeWin32Exception = new EventDescriptor(46, 0, 0, 2, 14, 18, 0L);
			DeviceFlashParameters = new EventDescriptor(47, 0, 0, 4, 0, 1, 0L);
		}

		public bool EventWriteFlash_Start(Guid DeviceId, string DeviceFriendlyName, string AssemblyFileVersion)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEventWithString(ref Flash_Start, DeviceId, DeviceFriendlyName, AssemblyFileVersion);
		}

		public bool EventWriteFlash_Stop(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref Flash_Stop, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteDevice_Attach(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref Device_Attach, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteDevice_Detach(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref Device_Detach, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteDevice_Remove(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref Device_Remove, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteFlash_Error(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref Flash_Error, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteFlash_Timeout(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref Flash_Timeout, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteTransferException(Guid DeviceId, string DeviceFriendlyName, int ErrorCode)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceEventWithErrorCode(ref TransferException, DeviceId, DeviceFriendlyName, ErrorCode);
		}

		public bool EventWriteReconnectIOException(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref ReconnectIOException, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteReconnectWin32Exception(Guid DeviceId, string DeviceFriendlyName, int ErrorCode)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceEventWithErrorCode(ref ReconnectWin32Exception, DeviceId, DeviceFriendlyName, ErrorCode);
		}

		public bool EventWriteReadBootmeIOException(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref ReadBootmeIOException, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteReadBootmeWin32Exception(Guid DeviceId, string DeviceFriendlyName, int ErrorCode)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceEventWithErrorCode(ref ReadBootmeWin32Exception, DeviceId, DeviceFriendlyName, ErrorCode);
		}

		public bool EventWriteSkipIOException(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref SkipIOException, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteSkipWin32Exception(Guid DeviceId, string DeviceFriendlyName, int ErrorCode)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceEventWithErrorCode(ref SkipWin32Exception, DeviceId, DeviceFriendlyName, ErrorCode);
		}

		public bool EventWriteWriteSkipFailed(Guid DeviceId, string DeviceFriendlyName, int ErrorCode)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceEventWithErrorCode(ref WriteSkipFailed, DeviceId, DeviceFriendlyName, ErrorCode);
		}

		public bool EventWriteUSBResetWin32Exception(Guid DeviceId, string DeviceFriendlyName, int ErrorCode)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceEventWithErrorCode(ref USBResetWin32Exception, DeviceId, DeviceFriendlyName, ErrorCode);
		}

		public bool EventWriteRebootIOException(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref RebootIOException, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteRebootWin32Exception(Guid DeviceId, string DeviceFriendlyName, int ErrorCode)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceEventWithErrorCode(ref RebootWin32Exception, DeviceId, DeviceFriendlyName, ErrorCode);
		}

		public bool EventWriteConnectWin32Exception(Guid DeviceId, string DeviceFriendlyName, int ErrorCode)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceEventWithErrorCode(ref ConnectWin32Exception, DeviceId, DeviceFriendlyName, ErrorCode);
		}

		public bool EventWriteThreadException(string String)
		{
			return m_provider.WriteEvent(ref ThreadException, String);
		}

		public bool EventWriteFileRead_Start(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref FileRead_Start, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteFileRead_Stop(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref FileRead_Stop, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteWaitAbandoned(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref WaitAbandoned, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteMutexTimeout(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref MutexTimeout, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteConnectNotifyException(string DevicePath, string Exception)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateNotifyException(ref ConnectNotifyException, DevicePath, Exception);
		}

		public bool EventWriteDisconnectNotifyException(string DevicePath, string Exception)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateNotifyException(ref DisconnectNotifyException, DevicePath, Exception);
		}

		public bool EventWriteInitNotifyException(string DevicePath, string Exception)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateNotifyException(ref InitNotifyException, DevicePath, Exception);
		}

		public bool EventWriteMassStorageIOException(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref MassStorageIOException, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteMassStorageWin32Exception(Guid DeviceId, string DeviceFriendlyName, int ErrorCode)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceEventWithErrorCode(ref MassStorageWin32Exception, DeviceId, DeviceFriendlyName, ErrorCode);
		}

		public bool EventWriteStreamClearStart(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref StreamClearStart, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteStreamClearStop(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref StreamClearStop, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteStreamClearPushWin32Exception(Guid DeviceId, string DeviceFriendlyName, int ErrorCode)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceEventWithErrorCode(ref StreamClearPushWin32Exception, DeviceId, DeviceFriendlyName, ErrorCode);
		}

		public bool EventWriteStreamClearPullWin32Exception(Guid DeviceId, string DeviceFriendlyName, int ErrorCode)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceEventWithErrorCode(ref StreamClearPullWin32Exception, DeviceId, DeviceFriendlyName, ErrorCode);
		}

		public bool EventWriteStreamClearIOException(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref StreamClearIOException, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteClearIdIOException(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref ClearIdIOException, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteClearIdWin32Exception(Guid DeviceId, string DeviceFriendlyName, int ErrorCode)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceEventWithErrorCode(ref ClearIdWin32Exception, DeviceId, DeviceFriendlyName, ErrorCode);
		}

		public bool EventWriteWimSuccess(Guid DeviceId, string DeviceFriendlyName, int ErrorCode)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceEventWithErrorCode(ref WimSuccess, DeviceId, DeviceFriendlyName, ErrorCode);
		}

		public bool EventWriteWimError(Guid DeviceId, string DeviceFriendlyName, int ErrorCode)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceEventWithErrorCode(ref WimError, DeviceId, DeviceFriendlyName, ErrorCode);
		}

		public bool EventWriteWimIOException(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref WimIOException, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteWimWin32Exception(Guid DeviceId, string DeviceFriendlyName, int ErrorCode)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceEventWithErrorCode(ref WimWin32Exception, DeviceId, DeviceFriendlyName, ErrorCode);
		}

		public bool EventWriteWimTransferStart(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref WimTransferStart, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteWimTransferStop(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref WimTransferStop, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteWimPacketStart(Guid DeviceId, string DeviceFriendlyName, int TransferSize)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEventWithSize(ref WimPacketStart, DeviceId, DeviceFriendlyName, TransferSize);
		}

		public bool EventWriteWimPacketStop(Guid DeviceId, string DeviceFriendlyName, int ErrorCode)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceEventWithErrorCode(ref WimPacketStop, DeviceId, DeviceFriendlyName, ErrorCode);
		}

		public bool EventWriteWimGetStatus(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref WimGetStatus, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteBootModeIOException(Guid DeviceId, string DeviceFriendlyName)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceSpecificEvent(ref BootModeIOException, DeviceId, DeviceFriendlyName);
		}

		public bool EventWriteBootModeWin32Exception(Guid DeviceId, string DeviceFriendlyName, int ErrorCode)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceEventWithErrorCode(ref BootModeWin32Exception, DeviceId, DeviceFriendlyName, ErrorCode);
		}

		public bool EventWriteDeviceFlashParameters(int USBTransactionSize, int PacketDataSize)
		{
			if (!m_provider.IsEnabled())
			{
				return true;
			}
			return m_provider.TemplateDeviceFlashParameters(ref DeviceFlashParameters, USBTransactionSize, PacketDataSize);
		}
	}
}
