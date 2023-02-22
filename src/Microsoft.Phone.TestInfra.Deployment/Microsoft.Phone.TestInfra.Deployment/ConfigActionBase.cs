using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Microsoft.Phone.TestInfra.Deployment
{
	[InheritedExport(typeof(ConfigActionBase))]
	public abstract class ConfigActionBase
	{
		public static readonly string RelativeConfigFolder = "Files\\ConfigActions";

		public static readonly string RelativeFilesFolder = "Files";

		public abstract List<ConfigCommand> GetConfigCommand(HashSet<string> deployedPackages, string outputPath);
	}
}
