using System;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class DeviceLayoutValidationException : IUException
	{
		public DeviceLayoutValidationError Error { get; private set; }

		public DeviceLayoutValidationException(DeviceLayoutValidationError error, string msg)
			: base(msg)
		{
			Error = error;
		}

		public DeviceLayoutValidationException(DeviceLayoutValidationError error, string message, params object[] args)
			: base(message, args)
		{
			Error = error;
		}

		public DeviceLayoutValidationException(DeviceLayoutValidationError error, Exception inner, string msg)
			: base(inner, msg)
		{
			Error = error;
		}

		public DeviceLayoutValidationException(DeviceLayoutValidationError error, Exception innerException, string message, params object[] args)
			: base(innerException, message, args)
		{
			Error = error;
		}
	}
}
