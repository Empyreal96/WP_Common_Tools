using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace FFUComponents
{
	internal class DTSFUsbStream : Stream
	{
		private const byte UsbEndpointDirectionMask = 128;

		private string deviceName;

		private SafeFileHandle deviceHandle;

		private IntPtr usbHandle;

		private byte bulkInPipeId;

		private byte bulkOutPipeId;

		private bool isDisposed;

		private const int retryCount = 10;

		private TimeSpan completionTimeout = TimeSpan.FromSeconds(5.0);

		public override bool CanRead => true;

		public override bool CanSeek => false;

		public override bool CanWrite => true;

		public override long Length
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override long Position
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public DTSFUsbStream(string deviceName, FileShare shareMode, TimeSpan transferTimeout)
		{
			if (string.IsNullOrEmpty(deviceName))
			{
				throw new ArgumentException("Invalid Argument", "deviceName");
			}
			isDisposed = false;
			this.deviceName = deviceName;
			try
			{
				int lastError = 0;
				deviceHandle = CreateDeviceHandle(this.deviceName, shareMode, ref lastError);
				if (deviceHandle.IsInvalid)
				{
					throw new IOException(string.Format(CultureInfo.CurrentCulture, Resources.ERROR_INVALID_HANDLE, new object[2] { deviceName, lastError }));
				}
				InitializeDevice();
				SetTransferTimeout(transferTimeout);
				if (!ThreadPool.BindHandle(deviceHandle))
				{
					throw new IOException(string.Format(CultureInfo.CurrentCulture, Resources.ERROR_BINDHANDLE, new object[1] { deviceName }));
				}
				Connect();
			}
			catch (Exception)
			{
				CloseDeviceHandle();
				throw;
			}
		}

		public DTSFUsbStream(string deviceName, TimeSpan transferTimeout)
			: this(deviceName, FileShare.None, transferTimeout)
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Flush()
		{
		}

		private void HandleAsyncTimeout(IAsyncResult asyncResult)
		{
			if (NativeMethods.CancelIo(deviceHandle))
			{
				asyncResult.AsyncWaitHandle.WaitOne(completionTimeout, false);
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			IAsyncResult asyncResult = BeginRead(buffer, offset, count, null, null);
			try
			{
				return EndRead(asyncResult);
			}
			catch (TimeoutException innerException)
			{
				HandleAsyncTimeout(asyncResult);
				throw new Win32Exception(Resources.ERROR_CALLBACK_TIMEOUT, innerException);
			}
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			IAsyncResult asyncResult = BeginWrite(buffer, offset, count, null, null);
			try
			{
				EndWrite(asyncResult);
			}
			catch (TimeoutException innerException)
			{
				HandleAsyncTimeout(asyncResult);
				throw new Win32Exception(Resources.ERROR_CALLBACK_TIMEOUT, innerException);
			}
		}

		private void RetryRead(uint errorCode, DTSFUsbStreamReadAsyncResult asyncResult, out Exception exception)
		{
			exception = null;
			if (IsDeviceDisconnected(errorCode))
			{
				exception = new Win32Exception((int)errorCode);
				return;
			}
			if (asyncResult.RetryCount > 10)
			{
				exception = new Win32Exception((int)errorCode);
				return;
			}
			int errorCode2 = 0;
			ClearPipeStall(bulkInPipeId, out errorCode2);
			if (errorCode2 != 0)
			{
				exception = new Win32Exception(errorCode2);
				return;
			}
			try
			{
				BeginReadInternal(asyncResult.Buffer, asyncResult.Offset, asyncResult.Count, asyncResult.RetryCount++, asyncResult.AsyncCallback, asyncResult.AsyncState);
			}
			catch (Exception ex)
			{
				exception = ex;
			}
		}

		private unsafe void ReadIOCompletionCallback(uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped)
		{
			try
			{
				DTSFUsbStreamReadAsyncResult dTSFUsbStreamReadAsyncResult = (DTSFUsbStreamReadAsyncResult)Overlapped.Unpack(nativeOverlapped).AsyncResult;
				Overlapped.Free(nativeOverlapped);
				Exception exception = null;
				if (errorCode != 0)
				{
					RetryRead(errorCode, dTSFUsbStreamReadAsyncResult, out exception);
					if (exception != null)
					{
						dTSFUsbStreamReadAsyncResult.SetAsCompleted(exception, false);
					}
				}
				else
				{
					dTSFUsbStreamReadAsyncResult.SetAsCompleted((int)numBytes, false);
				}
			}
			catch (Exception)
			{
			}
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback userCallback, object stateObject)
		{
			if (deviceHandle.IsClosed)
			{
				throw new ObjectDisposedException(Resources.ERROR_FILE_CLOSED);
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", "ArgumentOutOfRange_NeedNonNegNum");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_NeedNonNegNum");
			}
			if (buffer.Length - offset < count)
			{
				throw new ArgumentException("Argument_InvalidOffLen");
			}
			return BeginReadInternal(buffer, offset, count, 10, userCallback, stateObject);
		}

		private unsafe IAsyncResult BeginReadInternal(byte[] buffer, int offset, int count, int retryCount, AsyncCallback userCallback, object stateObject)
		{
			NativeOverlapped* ptr = null;
			DTSFUsbStreamReadAsyncResult dTSFUsbStreamReadAsyncResult = new DTSFUsbStreamReadAsyncResult(userCallback, stateObject)
			{
				Buffer = buffer,
				Offset = offset,
				Count = count,
				RetryCount = retryCount
			};
			ptr = new Overlapped(0, 0, IntPtr.Zero, dTSFUsbStreamReadAsyncResult).Pack(ReadIOCompletionCallback, buffer);
			fixed (byte* ptr2 = buffer)
			{
				if (!NativeMethods.WinUsbReadPipe(usbHandle, bulkInPipeId, ptr2 + offset, (uint)count, IntPtr.Zero, ptr))
				{
					int lastWin32Error = Marshal.GetLastWin32Error();
					if (997 != lastWin32Error)
					{
						Overlapped.Unpack(ptr);
						Overlapped.Free(ptr);
						throw new Win32Exception(lastWin32Error);
					}
				}
			}
			return dTSFUsbStreamReadAsyncResult;
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			return ((DTSFUsbStreamReadAsyncResult)asyncResult).EndInvoke();
		}

		private void RetryWrite(uint errorCode, DTSFUsbStreamWriteAsyncResult asyncResult, out Exception exception)
		{
			exception = null;
			if (IsDeviceDisconnected(errorCode))
			{
				exception = new Win32Exception((int)errorCode);
				return;
			}
			if (asyncResult.RetryCount > 10)
			{
				exception = new Win32Exception((int)errorCode);
				return;
			}
			int errorCode2 = 0;
			ClearPipeStall(bulkOutPipeId, out errorCode2);
			if (errorCode2 != 0)
			{
				exception = new Win32Exception(errorCode2);
				return;
			}
			try
			{
				BeginWriteInternal(asyncResult.Buffer, asyncResult.Offset, asyncResult.Count, asyncResult.RetryCount++, asyncResult.AsyncCallback, asyncResult.AsyncState);
			}
			catch (Exception ex)
			{
				exception = ex;
			}
		}

		private unsafe void WriteIOCompletionCallback(uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped)
		{
			DTSFUsbStreamWriteAsyncResult dTSFUsbStreamWriteAsyncResult = (DTSFUsbStreamWriteAsyncResult)Overlapped.Unpack(nativeOverlapped).AsyncResult;
			Overlapped.Free(nativeOverlapped);
			Exception exception = null;
			try
			{
				if (errorCode != 0)
				{
					RetryWrite(errorCode, dTSFUsbStreamWriteAsyncResult, out exception);
					if (exception != null)
					{
						dTSFUsbStreamWriteAsyncResult.SetAsCompleted(exception, false);
					}
				}
				else
				{
					dTSFUsbStreamWriteAsyncResult.SetAsCompleted(exception, false);
				}
			}
			catch (Exception)
			{
			}
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback userCallback, object stateObject)
		{
			if (deviceHandle.IsClosed)
			{
				throw new ObjectDisposedException(Resources.ERROR_FILE_CLOSED);
			}
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", "ArgumentOutOfRange_NeedNonNegNum");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_NeedNonNegNum");
			}
			if (buffer.Length - offset < count)
			{
				throw new ArgumentException("Argument_InvalidOffLen");
			}
			return BeginWriteInternal(buffer, offset, count, 0, userCallback, stateObject);
		}

		private unsafe IAsyncResult BeginWriteInternal(byte[] buffer, int offset, int count, int retryCount, AsyncCallback userCallback, object stateObject)
		{
			NativeOverlapped* ptr = null;
			DTSFUsbStreamWriteAsyncResult dTSFUsbStreamWriteAsyncResult = new DTSFUsbStreamWriteAsyncResult(userCallback, stateObject)
			{
				Buffer = buffer,
				Offset = offset,
				Count = count,
				RetryCount = retryCount
			};
			ptr = new Overlapped(0, 0, IntPtr.Zero, dTSFUsbStreamWriteAsyncResult).Pack(WriteIOCompletionCallback, buffer);
			fixed (byte* ptr2 = buffer)
			{
				if (!NativeMethods.WinUsbWritePipe(usbHandle, bulkOutPipeId, ptr2 + offset, (uint)count, IntPtr.Zero, ptr))
				{
					int lastWin32Error = Marshal.GetLastWin32Error();
					if (997 != lastWin32Error)
					{
						Overlapped.Unpack(ptr);
						Overlapped.Free(ptr);
						throw new Win32Exception(lastWin32Error);
					}
				}
			}
			return dTSFUsbStreamWriteAsyncResult;
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			((DTSFUsbStreamWriteAsyncResult)asyncResult).EndInvoke();
		}

		private static SafeFileHandle CreateDeviceHandle(string deviceName, FileShare shareMode, ref int lastError)
		{
			SafeFileHandle result = NativeMethods.CreateFile(deviceName, 3221225472u, (uint)shareMode, IntPtr.Zero, 3u, 1073741952u, IntPtr.Zero);
			lastError = Marshal.GetLastWin32Error();
			return result;
		}

		private void CloseDeviceHandle()
		{
			if (IntPtr.Zero != usbHandle)
			{
				NativeMethods.WinUsbFree(usbHandle);
				usbHandle = IntPtr.Zero;
			}
			if (!deviceHandle.IsInvalid && !deviceHandle.IsClosed)
			{
				deviceHandle.Close();
				deviceHandle.SetHandleAsInvalid();
			}
		}

		private void InitializeDevice()
		{
			WinUsbInterfaceDescriptor usbAltInterfaceDescriptor = default(WinUsbInterfaceDescriptor);
			WinUsbPipeInformation pipeInformation = default(WinUsbPipeInformation);
			if (!NativeMethods.WinUsbInitialize(deviceHandle, ref usbHandle))
			{
				throw new IOException(Resources.ERROR_WINUSB_INITIALIZATION);
			}
			if (!NativeMethods.WinUsbQueryInterfaceSettings(usbHandle, 0, ref usbAltInterfaceDescriptor))
			{
				throw new IOException(Resources.ERROR_WINUSB_QUERY_INTERFACE_SETTING);
			}
			for (byte b = 0; b < usbAltInterfaceDescriptor.NumEndpoints; b = (byte)(b + 1))
			{
				if (!NativeMethods.WinUsbQueryPipe(usbHandle, 0, b, ref pipeInformation))
				{
					throw new IOException(Resources.ERROR_WINUSB_QUERY_PIPE_INFORMATION);
				}
				WinUsbPipeType pipeType = pipeInformation.PipeType;
				if (pipeType == WinUsbPipeType.Bulk)
				{
					if (IsBulkInEndpoint(pipeInformation.PipeId))
					{
						SetupBulkInEndpoint(pipeInformation.PipeId);
					}
					else
					{
						if (!IsBulkOutEndpoint(pipeInformation.PipeId))
						{
							throw new IOException(Resources.ERROR_INVALID_ENDPOINT_TYPE);
						}
						SetupBulkOutEndpoint(pipeInformation.PipeId);
					}
				}
			}
		}

		private bool IsBulkInEndpoint(byte pipeId)
		{
			return (pipeId & 0x80) == 128;
		}

		private bool IsBulkOutEndpoint(byte pipeId)
		{
			return (pipeId & 0x80) == 0;
		}

		public void PipeReset()
		{
			int errorCode;
			ClearPipeStall(bulkInPipeId, out errorCode);
			ClearPipeStall(bulkOutPipeId, out errorCode);
		}

		public void SetTransferTimeout(TimeSpan transferTimeout)
		{
			uint value = (uint)transferTimeout.TotalMilliseconds;
			SetPipePolicy(bulkInPipeId, 3u, value);
			SetPipePolicy(bulkOutPipeId, 3u, value);
		}

		public void SetShortPacketTerminate()
		{
			SetPipePolicy(bulkOutPipeId, 1u, true);
		}

		private void SetupBulkInEndpoint(byte pipeId)
		{
			bulkInPipeId = pipeId;
		}

		private void SetupBulkOutEndpoint(byte pipeId)
		{
			bulkOutPipeId = pipeId;
		}

		private void SetPipePolicy(byte pipeId, uint policyType, uint value)
		{
			if (!NativeMethods.WinUsbSetPipePolicy(usbHandle, pipeId, policyType, (uint)Marshal.SizeOf(typeof(uint)), ref value))
			{
				throw new IOException(Resources.ERROR_WINUSB_SET_PIPE_POLICY);
			}
		}

		private void SetPipePolicy(byte pipeId, uint policyType, bool value)
		{
			if (!NativeMethods.WinUsbSetPipePolicy(usbHandle, pipeId, policyType, (uint)Marshal.SizeOf(typeof(bool)), ref value))
			{
				throw new IOException(Resources.ERROR_WINUSB_SET_PIPE_POLICY);
			}
		}

		private unsafe void ControlTransferSetData(UsbControlRequest request, ushort value)
		{
			WinUsbSetupPacket setupPacket = default(WinUsbSetupPacket);
			setupPacket.RequestType = 33;
			setupPacket.Request = (byte)request;
			setupPacket.Value = value;
			setupPacket.Index = 0;
			setupPacket.Length = 0;
			uint lengthTransferred = 0u;
			byte[] data = null;
			fixed (byte* buffer = data)
			{
				if (!NativeMethods.WinUsbControlTransfer(usbHandle, setupPacket, buffer, setupPacket.Length, ref lengthTransferred, IntPtr.Zero))
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}
			}
		}

		private unsafe void ControlTransferGetData(UsbControlRequest request, byte[] buffer)
		{
			WinUsbSetupPacket setupPacket = default(WinUsbSetupPacket);
			setupPacket.RequestType = 161;
			setupPacket.Request = (byte)request;
			setupPacket.Value = 0;
			setupPacket.Index = 0;
			setupPacket.Length = (ushort)((buffer != null) ? ((ushort)buffer.Length) : 0);
			uint lengthTransferred = 0u;
			fixed (byte* buffer2 = buffer)
			{
				if (!NativeMethods.WinUsbControlTransfer(usbHandle, setupPacket, buffer2, setupPacket.Length, ref lengthTransferred, IntPtr.Zero))
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}
			}
		}

		private void ClearPipeStall(byte pipeId, out int errorCode)
		{
			errorCode = 0;
			if (!NativeMethods.WinUsbAbortPipe(usbHandle, pipeId))
			{
				errorCode = Marshal.GetLastWin32Error();
			}
			if (!NativeMethods.WinUsbResetPipe(usbHandle, pipeId))
			{
				errorCode = Marshal.GetLastWin32Error();
			}
		}

		private bool IsDeviceDisconnected(uint errorCode)
		{
			if (errorCode != 2 && errorCode != 1167 && errorCode != 31 && errorCode != 121)
			{
				return errorCode == 995;
			}
			return true;
		}

		protected override void Dispose(bool disposing)
		{
			if (!(!isDisposed && disposing))
			{
				return;
			}
			try
			{
				CloseDeviceHandle();
			}
			catch (Exception)
			{
			}
			finally
			{
				base.Dispose(disposing);
				isDisposed = true;
			}
		}

		~DTSFUsbStream()
		{
			Dispose(false);
		}

		private void Connect()
		{
			ControlTransferSetData(UsbControlRequest.LineStateSet, 1);
		}
	}
}
