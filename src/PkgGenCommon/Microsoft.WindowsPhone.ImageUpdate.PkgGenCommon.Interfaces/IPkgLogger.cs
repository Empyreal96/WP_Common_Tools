namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces
{
	public interface IPkgLogger
	{
		void Error(string format, params object[] args);

		void Warning(string format, params object[] args);

		void Message(string format, params object[] args);

		void Diagnostic(string format, params object[] args);
	}
}
