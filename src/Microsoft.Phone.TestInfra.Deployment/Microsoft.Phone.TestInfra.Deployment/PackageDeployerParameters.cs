using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class PackageDeployerParameters
	{
		private TimeSpan expiresIn;

		private string logFile;

		public string RootPaths { get; private set; }

		public string AlternateRoots { get; set; }

		public string Macros { get; set; }

		public string OutputPath { get; private set; }

		public string Packages { get; set; }

		public string PackageFile { get; set; }

		[DefaultValue(TraceLevel.Info)]
		public TraceLevel ConsoleTraceLevel { get; set; }

		[DefaultValue(TraceLevel.Info)]
		public TraceLevel FileTraceLevel { get; set; }

		public string CacheRoot { get; set; }

		[DefaultValue(false)]
		public bool SourceRootIsVolatile { get; set; }

		[DefaultValue(true)]
		public bool Recurse { get; set; }

		public TimeSpan ExpiresIn
		{
			get
			{
				return (expiresIn <= TimeSpan.Zero) ? TimeSpan.FromHours(Settings.Default.DefaultFileExpiresInHours) : expiresIn;
			}
			set
			{
				expiresIn = value;
			}
		}

		public string LogFile
		{
			get
			{
				return string.IsNullOrWhiteSpace(logFile) ? Path.Combine(Settings.Default.DefaultLogDirectory, Settings.Default.DefaultLogName) : logFile;
			}
			set
			{
				logFile = value;
			}
		}

		public PackageDeployerParameters(string outputPath, string rootPath)
		{
			if (string.IsNullOrWhiteSpace(outputPath))
			{
				throw new ArgumentNullException("outputPath");
			}
			if (string.IsNullOrWhiteSpace(rootPath))
			{
				throw new ArgumentNullException("rootPath");
			}
			RootPaths = rootPath;
			OutputPath = outputPath;
		}
	}
}
