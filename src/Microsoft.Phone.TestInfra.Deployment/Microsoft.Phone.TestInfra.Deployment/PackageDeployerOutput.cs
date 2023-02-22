using System.Collections.Generic;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class PackageDeployerOutput
	{
		public bool Success { get; set; }

		public string ErrorMessage { get; set; }

		public List<ConfigCommand> ConfigurationCommands { get; set; }
	}
}
