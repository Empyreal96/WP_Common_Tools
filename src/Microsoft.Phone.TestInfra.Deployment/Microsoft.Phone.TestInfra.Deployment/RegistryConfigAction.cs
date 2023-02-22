using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class RegistryConfigAction : ConfigActionBase
	{
		public override List<ConfigCommand> GetConfigCommand(HashSet<string> deployedPackages, string outputPath)
		{
			if (deployedPackages == null || deployedPackages.Count == 0)
			{
				throw new ArgumentNullException("deployedPackages");
			}
			if (string.IsNullOrWhiteSpace(outputPath))
			{
				throw new ArgumentNullException("outputPath");
			}
			List<ConfigCommand> list = new List<ConfigCommand>();
			string path = Path.Combine(outputPath, ConfigActionBase.RelativeConfigFolder);
			string path2 = Path.Combine(outputPath, ConfigActionBase.RelativeFilesFolder);
			if (!Directory.Exists(path2))
			{
				throw new InvalidDataException("Could not find the files folder in the deployment output.");
			}
			path2 = Path.GetFullPath(path2);
			string[] files = Directory.GetFiles(path2, "*.reg", SearchOption.AllDirectories);
			string[] array = files;
			foreach (string regFile in array)
			{
				string fileName = Path.GetFileName(regFile);
				if (deployedPackages.Any((string x) => string.Compare(PathHelper.GetFileNameWithoutExtension(x, ".spkg"), Path.GetFileNameWithoutExtension(regFile), true) == 0))
				{
					string text = Path.Combine(path, fileName);
					if (string.Compare(regFile, text, true) != 0)
					{
						File.Copy(regFile, text, true);
					}
					string commandLine = $"reg.exe import \"{text}\"";
					list.Add(new ConfigCommand
					{
						CommandLine = commandLine
					});
					Logger.Info("Added command line to import {0}.", fileName);
				}
			}
			return list;
		}
	}
}
