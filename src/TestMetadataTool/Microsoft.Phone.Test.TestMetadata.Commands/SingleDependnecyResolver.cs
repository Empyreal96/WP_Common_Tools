using System.Collections.Generic;
using System.Linq;
using Microsoft.Phone.Test.TestMetadata.Helper;

namespace Microsoft.Phone.Test.TestMetadata.Commands
{
	internal class SingleDependnecyResolver : IBinaryDependencyResolver
	{
		public Package ResolveDependency(PortableExecutableDependency portableExecutableDependency, IList<PackageFile> possibleDependencies, IBinaryDependencyParent referencingFile, out bool continueResolving)
		{
			if (possibleDependencies.Count() != 1)
			{
				continueResolving = true;
				return null;
			}
			continueResolving = false;
			return possibleDependencies[0].Package;
		}
	}
}
