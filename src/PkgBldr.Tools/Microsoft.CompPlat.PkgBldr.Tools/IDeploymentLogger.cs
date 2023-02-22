using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public interface IDeploymentLogger : Microsoft.WindowsPhone.ImageUpdate.Tools.Common.IDeploymentLogger
	{
		void LogSpkgGenOutput(string spkggenOutput);
	}
}
