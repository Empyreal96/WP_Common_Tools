using System;
using System.Collections.Generic;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class PackageManagerConfiguration : ICloneable
	{
		public TimeSpan ExpiresIn { get; set; }

		public string OutputPath { get; set; }

		public IEnumerable<string> RootPaths { get; set; }

		public IEnumerable<string> AlternateRootPaths { get; set; }

		public string CachePath { get; set; }

		public string PackagesExtractionCachePath { get; set; }

		public bool SourceRootIsVolatile { get; set; }

		public bool RecursiveDeployment { get; set; }

		public IDictionary<string, string> Macros { get; set; }

		public object Clone()
		{
			PackageManagerConfiguration packageManagerConfiguration = new PackageManagerConfiguration();
			packageManagerConfiguration.OutputPath = OutputPath;
			packageManagerConfiguration.RootPaths = RootPaths;
			packageManagerConfiguration.AlternateRootPaths = AlternateRootPaths;
			packageManagerConfiguration.CachePath = CachePath;
			packageManagerConfiguration.PackagesExtractionCachePath = PackagesExtractionCachePath;
			packageManagerConfiguration.SourceRootIsVolatile = SourceRootIsVolatile;
			packageManagerConfiguration.RecursiveDeployment = RecursiveDeployment;
			packageManagerConfiguration.ExpiresIn = ExpiresIn;
			return packageManagerConfiguration;
		}
	}
}
