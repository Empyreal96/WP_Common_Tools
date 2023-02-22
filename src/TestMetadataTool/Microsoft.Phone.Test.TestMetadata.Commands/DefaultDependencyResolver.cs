using System.Collections.Generic;
using System.Linq;
using Microsoft.Phone.Test.TestMetadata.Helper;

namespace Microsoft.Phone.Test.TestMetadata.Commands
{
	internal class DefaultDependencyResolver : IBinaryDependencyResolver
	{
		public Package ResolveDependency(PortableExecutableDependency portableExecutableDependency, IList<PackageFile> possibleDependencies, IBinaryDependencyParent referencingFile, out bool continueResolving)
		{
			int num = possibleDependencies.Count((PackageFile d) => !d.IsProjectFile);
			if (possibleDependencies.Count() != num)
			{
				Log.Error("File {0} [{1}] referenced by {2} found in {3} packages.", portableExecutableDependency.Name, portableExecutableDependency.Type, referencingFile.Name, possibleDependencies.Count);
				foreach (PackageFile possibleDependency in possibleDependencies)
				{
					Log.Error("\t{0}", possibleDependency.Package.Name);
				}
				continueResolving = false;
				return null;
			}
			continueResolving = false;
			return possibleDependencies[0].Package;
		}
	}
}
