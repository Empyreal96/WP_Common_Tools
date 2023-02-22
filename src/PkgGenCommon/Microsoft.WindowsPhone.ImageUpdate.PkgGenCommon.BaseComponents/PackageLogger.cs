using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents
{
	internal class PackageLogger : IPkgLogger
	{
		public void Error(string format, params object[] args)
		{
			LogUtil.Error(format, args);
		}

		public void Warning(string format, params object[] args)
		{
			LogUtil.Warning(format, args);
		}

		public void Message(string format, params object[] args)
		{
			LogUtil.Message(format, args);
		}

		public void Diagnostic(string format, params object[] args)
		{
			LogUtil.Diagnostic(format, args);
		}
	}
}
