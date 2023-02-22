using System.ComponentModel;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class ConfigCommand
	{
		public string CommandLine { get; set; }

		[DefaultValue(0)]
		public int SuccessExitCode { get; set; }

		[DefaultValue(false)]
		public bool IgnoreExitCode { get; set; }

		[DefaultValue(3)]
		public int TimeOutInMins { get; set; }
	}
}
