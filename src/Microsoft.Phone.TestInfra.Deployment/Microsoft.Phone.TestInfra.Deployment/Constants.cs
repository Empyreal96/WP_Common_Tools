using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public static class Constants
	{
		private static List<string> dependencyProjects;

		private static int numOfLoaders;

		public static string BinaryLocationCacheExtension => ".bin.loc.json";

		public static string PackageLocationCacheExtension => ".pkg.loc.json";

		public static string ManifestFileExtension => ".man.dsm.xml";

		public static string SpkgFileExtension => ".spkg";

		public static string CabFileExtension => ".cab";

		public static string DepFileExtension => ".dep.xml";

		public static List<string> DependencyProjects => dependencyProjects;

		public static int NumOfLoaders => numOfLoaders;

		public static string SupressionFileName => "pkgdep_supress.txt";

		public static string PrebuiltFolderName => "Prebuilt";

		public static string GeneralPackageLocationCacheFileName => "DeployTest.GeneralPackageLocationCache.json";

		public static string GeneralBinaryPackageMappingCacheFileName => "DeployTest.GeneralBinaryLocationCache.json";

		public static string AssemblyDirectory { get; private set; }

		static Constants()
		{
			dependencyProjects = new List<string> { "Windows", "Test", "unittests" };
			try
			{
				string environmentVariable = Environment.GetEnvironmentVariable("NUMBER_OF_PROCESSORS");
				numOfLoaders = int.Parse(environmentVariable);
			}
			catch
			{
				numOfLoaders = 1;
			}
			AssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}
	}
}
