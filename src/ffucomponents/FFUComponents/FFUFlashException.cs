using System;
using System.Runtime.Serialization;

namespace FFUComponents
{
	[Serializable]
	public class FFUFlashException : FFUException
	{
		public enum ErrorCode
		{
			None = 0,
			FlashError = 2,
			InvalidStoreHeader = 8,
			DescriptorAllocationFailed = 9,
			DescriptorReadFailed = 11,
			BlockReadFailed = 12,
			BlockWriteFailed = 13,
			CrcError = 14,
			SecureHeaderReadFailed = 15,
			InvalidSecureHeader = 16,
			InsufficientSecurityPadding = 17,
			InvalidImageHeader = 18,
			InsufficientImagePadding = 19,
			BufferingFailed = 20,
			ExcessBlocks = 21,
			InvalidPlatformId = 22,
			HashCheckFailed = 23,
			SignatureCheckFailed = 24,
			DesyncFailed = 26,
			FailedBcdQuery = 27,
			InvalidWriteDescriptors = 28,
			AntiTheftCheckFailed = 29,
			RemoveableMediaCheckFailed = 32,
			UseOptimizedSettingsFailed = 33
		}

		public ErrorCode Error { get; private set; }

		public FFUFlashException()
		{
		}

		public FFUFlashException(string message)
			: base(message)
		{
		}

		public FFUFlashException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public FFUFlashException(string deviceName, Guid deviceId, ErrorCode error, string message)
			: base(deviceName, deviceId, message)
		{
			Error = error;
		}

		protected FFUFlashException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			info.AddValue("Error", Error);
		}

		protected new virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Error", Error);
		}
	}
}
