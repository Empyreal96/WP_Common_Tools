using System;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools
{
	public class CabException : ApplicationException
	{
		private const string STR_CABERROR = "Cab operation failed with hr = 0x{0:X8} [{1}], CAB Operation {2}, Params: {3}";

		public uint CabHResult { get; private set; }

		public CabException(uint cabHR)
			: base(CabErrorMapper.Instance.MapError(cabHR))
		{
			CabHResult = cabHR;
		}

		public CabException(uint cabHR, string cabMethod, params string[] args)
			: base(FormatMessage(cabHR, cabMethod, args))
		{
			CabHResult = cabHR;
		}

		public CabException(string msg)
			: base(msg)
		{
		}

		public CabException(string message, params object[] args)
			: base(string.Format(message, args))
		{
		}

		public CabException(string msg, Exception inner)
			: base(msg, inner)
		{
		}

		private static string FormatMessage(uint cabHR, string cabMethod, params string[] list)
		{
			string text = string.Join(",", list);
			return $"Cab operation failed with hr = 0x{cabHR:X8} [{CabErrorMapper.Instance.MapError(cabHR)}], CAB Operation {cabMethod}, Params: {text}";
		}
	}
}
