using System.Collections.Generic;
using Microsoft.Phone.Test.TestMetadata.Helper;

namespace Microsoft.Phone.Test.TestMetadata.Commands
{
	internal class NoDpendencyResolver : IBinaryDependencyResolver
	{
		public Package ResolveDependency(PortableExecutableDependency portableExecutableDependency, IList<PackageFile> possibleDependencies, IBinaryDependencyParent referencingFile, out bool continueResolving)
		{
			if (possibleDependencies == null || possibleDependencies.Count == 0)
			{
				ValidateSuppressedFile(portableExecutableDependency, referencingFile);
				continueResolving = false;
				return null;
			}
			continueResolving = true;
			return null;
		}

		private static void ValidateSuppressedFile(PortableExecutableDependency portableExecutableDependency, IBinaryDependencyParent referencingFile)
		{
			if (!referencingFile.PackageFileRepository.IsFileSupressed(referencingFile.Partition, referencingFile.Name, portableExecutableDependency.Name))
			{
				Log.Warning("File {0} [{1}] referenced by {2} in partition {3} not found in any package.", portableExecutableDependency.Name, portableExecutableDependency.Type, referencingFile.Name, referencingFile.Partition);
			}
		}
	}
}
