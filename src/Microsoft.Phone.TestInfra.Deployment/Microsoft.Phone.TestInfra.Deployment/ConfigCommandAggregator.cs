using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class ConfigCommandAggregator
	{
		[ImportMany(typeof(ConfigActionBase))]
		public IEnumerable<Lazy<ConfigActionBase>> ConfigActionSet { get; set; }

		public List<ConfigCommand> GetConfigCommands(HashSet<string> deployedPackages, string outputPath)
		{
			if (deployedPackages == null || deployedPackages.Count() == 0)
			{
				throw new ArgumentException("cannot be null or empty", "deployedPackages");
			}
			if (string.IsNullOrEmpty(outputPath))
			{
				throw new ArgumentException("cannot be null or empty", "outputPath");
			}
			if (!Directory.Exists(outputPath))
			{
				throw new DirectoryNotFoundException(outputPath);
			}
			List<ConfigCommand> list = new List<ConfigCommand>();
			string path = Path.Combine(outputPath, ConfigActionBase.RelativeConfigFolder);
			Directory.CreateDirectory(path);
			AssemblyCatalog catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
			CompositionContainer compositionService = new CompositionContainer(catalog);
			compositionService.SatisfyImportsOnce(this);
			if (ConfigActionSet != null)
			{
				foreach (Lazy<ConfigActionBase> item in ConfigActionSet)
				{
					list.AddRange(item.Value.GetConfigCommand(deployedPackages, outputPath));
				}
			}
			return list;
		}
	}
}
