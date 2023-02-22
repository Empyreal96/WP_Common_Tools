using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public class Logger : IULogger, IDeploymentLogger, Microsoft.WindowsPhone.ImageUpdate.Tools.Common.IDeploymentLogger
	{
		public void LogSpkgGenOutput(string spkggenOutput)
		{
			LogDebug(spkggenOutput);
		}
	}
}
