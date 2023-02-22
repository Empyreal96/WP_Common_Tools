using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class NewDepXmlGenerator
	{
		private bool updateOrCreate;

		private PackageInfo pkgInfo;

		private HashSet<PkgDepResolve> pkgDepSet = new HashSet<PkgDepResolve>();

		private HashSet<Dependency> resolvedDependencies;

		private PackageLocator packageLocator;

		private BinaryLocator binaryLocator;

		private HashSet<string> includedBinaries = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

		public string OutputPath { get; private set; }

		public NewDepXmlGenerator(string outputPath, PackageInfo pkgInfo, bool updateOrCreate, PackageLocator packageLocator)
		{
			if (string.IsNullOrEmpty(outputPath))
			{
				throw new ArgumentNullException("outputPath");
			}
			if (pkgInfo == null)
			{
				throw new ArgumentNullException("pkgInfo");
			}
			if (packageLocator == null)
			{
				throw new ArgumentNullException("packageLocator");
			}
			OutputPath = outputPath;
			this.pkgInfo = pkgInfo;
			this.updateOrCreate = updateOrCreate;
			this.packageLocator = packageLocator;
			binaryLocator = new BinaryLocator(packageLocator);
		}

		public string GetDepXml()
		{
			ResolveDependency();
			return SaveDepXml();
		}

		private void ResolveDependency()
		{
			string pkgName = pkgInfo.PackageName.ToLowerInvariant();
			pkgDepSet.Add(new PkgDepResolve
			{
				PkgInfo = pkgInfo,
				IsProcessed = false
			});
			resolvedDependencies = new HashSet<Dependency>
			{
				new PackageDependency
				{
					PkgName = pkgName,
					RelativePath = pkgInfo.RelativePath
				}
			};
			ResolveDependency(pkgDepSet);
		}

		private void ResolveDependency(HashSet<PkgDepResolve> pkgsWorkingSet)
		{
			IEnumerable<PkgDepResolve> enumerable = pkgsWorkingSet.Where((PkgDepResolve x) => !x.IsProcessed);
			if (enumerable.Count() == 0)
			{
				return;
			}
			HashSet<PkgDepResolve> hashSet = new HashSet<PkgDepResolve>();
			foreach (PkgDepResolve item2 in enumerable)
			{
				item2.IsProcessed = true;
				string text = Path.ChangeExtension(item2.PkgInfo.AbsolutePath, Constants.ManifestFileExtension);
				if (!ReliableFile.Exists(text, Settings.Default.ShareAccessRetryCount, TimeSpan.FromMilliseconds(Settings.Default.ShareAccessRetryDelayInMs)))
				{
					continue;
				}
				PackageDescription packageDescription = BinaryLocator.ReadPackageDescriptionFromManifestFile(text, item2.PkgInfo.RootPath);
				if (packageDescription == null)
				{
					continue;
				}
				includedBinaries.UnionWith(packageDescription.Binaries);
				foreach (Dependency dependency in packageDescription.Dependencies)
				{
					if (dependency is PackageDependency)
					{
						PackageInfo packageInfo = packageLocator.FindPackage((dependency as PackageDependency).PkgName);
						if (packageInfo == null)
						{
							Logger.Error("The excplicitly dependent package {0} was not found.", (dependency as PackageDependency).PkgName);
							continue;
						}
						hashSet.Add(new PkgDepResolve
						{
							PkgInfo = packageInfo,
							IsProcessed = false
						});
						(dependency as PackageDependency).AbsolutePath = packageInfo.AbsolutePath;
						(dependency as PackageDependency).RelativePath = packageInfo.RelativePath;
						resolvedDependencies.Add(dependency);
					}
					else if (dependency is BinaryDependency)
					{
						string text2 = (dependency as BinaryDependency).FileName.ToLowerInvariant();
						if (!includedBinaries.Contains(text2))
						{
							PackageInfo packageInfo2 = binaryLocator.FindContainingPackage(text2);
							if (packageInfo2 != null)
							{
								hashSet.Add(new PkgDepResolve
								{
									PkgInfo = packageInfo2,
									IsProcessed = false
								});
								PackageDependency packageDependency = new PackageDependency();
								packageDependency.PkgName = packageInfo2.PackageName;
								packageDependency.AbsolutePath = packageInfo2.AbsolutePath;
								packageDependency.RelativePath = packageInfo2.RelativePath;
								PackageDependency item = packageDependency;
								resolvedDependencies.Add(item);
							}
						}
					}
					else if (dependency is RemoteFileDependency || dependency is EnvironmentPathDependency)
					{
						resolvedDependencies.Add(dependency);
					}
				}
			}
			pkgsWorkingSet.UnionWith(hashSet);
			ResolveDependency(pkgsWorkingSet);
		}

		private string SaveDepXml()
		{
			if (!Directory.Exists(OutputPath))
			{
				Directory.CreateDirectory(OutputPath);
			}
			string text = Path.Combine(OutputPath, pkgInfo.PackageName + Constants.DepFileExtension);
			ResolvedDependency.Save(text, resolvedDependencies);
			return text;
		}
	}
}
