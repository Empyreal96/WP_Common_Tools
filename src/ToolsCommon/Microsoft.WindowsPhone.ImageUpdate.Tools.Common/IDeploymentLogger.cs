using System;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public interface IDeploymentLogger
	{
		void Log(LoggingLevel level, string format, params object[] list);

		void LogException(Exception exp);

		void LogException(Exception exp, LoggingLevel level);

		void LogDebug(string format, params object[] list);

		void LogInfo(string format, params object[] list);

		void LogWarning(string format, params object[] list);

		void LogError(string format, params object[] list);
	}
}
