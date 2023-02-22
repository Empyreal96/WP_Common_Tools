using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Phone.Test.TestMetadata.Helper;
using Microsoft.Phone.Test.TestMetadata.ObjectModel;
using Microsoft.Tools.IO;

namespace Microsoft.Phone.Test.TestMetadata.Commands
{
	internal static class ExplicitDependency
	{
		public static void Load(string metadataFile, HashSet<Dependency> dependencies)
		{
			Metadata metadata;
			try
			{
				metadata = Metadata.Load(metadataFile);
			}
			catch (Exception ex)
			{
				Log.Error("Unable to Loading metadata file {0}", metadataFile);
				Log.Error(ex.Message);
				if (ex.InnerException != null)
				{
					Log.Error(ex.InnerException.Message);
				}
				Log.Error("{0}", ex);
				return;
			}
			foreach (Dependency dependency in metadata.Dependencies)
			{
				if (dependency is PackageDependency)
				{
					PackageDependency obj = dependency as PackageDependency;
					obj.Name = obj.Name.ToLowerInvariant();
				}
				else if (dependency is BinaryDependency)
				{
					BinaryDependency obj2 = dependency as BinaryDependency;
					obj2.Name = obj2.Name.ToLowerInvariant();
				}
				dependencies.Add(dependency);
			}
		}

		public static IEnumerable<PortableExecutableDependency> FileDependcyList(HashSet<Dependency> dependencies)
		{
			return from fileDependency in dependencies.OfType<BinaryDependency>()
				select new PortableExecutableDependency
				{
					Name = LongPathPath.GetFileName(fileDependency.Name),
					Type = BinaryDependencyType.Explicit
				};
		}

		public static IEnumerable<RemoteFileDependency> RemoteFileDependencyList(HashSet<Dependency> dependencie)
		{
			return dependencie.OfType<RemoteFileDependency>();
		}

		public static IEnumerable<PackageDependency> PackageDependencyList(HashSet<Dependency> dependencie)
		{
			return dependencie.OfType<PackageDependency>();
		}
	}
}
