using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Phone.Test.TestMetadata.Helper;

namespace Microsoft.Phone.Test.TestMetadata.Commands
{
	internal static class BinaryDependencyResolver
	{
		private static readonly IBinaryDependencyResolver[] DependencyResolvers = new IBinaryDependencyResolver[3]
		{
			new NoDpendencyResolver(),
			new SingleDependnecyResolver(),
			new DefaultDependencyResolver()
		};

		public static Package ResolveDependency(PortableExecutableDependency portableExecutableDependency, IBinaryDependencyParent referencingFile)
		{
			List<PackageFile> list = null;
			if (referencingFile.PackageFileRepository.ContainsFile(referencingFile.Partition, portableExecutableDependency.Name))
			{
				list = referencingFile.PackageFileRepository.GetFile(referencingFile.Partition, portableExecutableDependency.Name);
				if (list.Count() > 1 && list.Any((PackageFile x) => x.Package.Name.EndsWith(".guest", StringComparison.OrdinalIgnoreCase)) && list.Any((PackageFile x) => !x.Package.Name.EndsWith(".guest", StringComparison.OrdinalIgnoreCase)))
				{
					list.RemoveAll((PackageFile x) => x.Package.Name.EndsWith(".guest", StringComparison.OrdinalIgnoreCase));
				}
			}
			IBinaryDependencyResolver[] dependencyResolvers = DependencyResolvers;
			for (int i = 0; i < dependencyResolvers.Length; i++)
			{
				bool continueResolving;
				Package result = dependencyResolvers[i].ResolveDependency(portableExecutableDependency, list, referencingFile, out continueResolving);
				if (!continueResolving)
				{
					return result;
				}
			}
			return null;
		}
	}
}
