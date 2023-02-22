using System.Collections.Generic;
using Microsoft.Phone.Test.TestMetadata.Helper;

namespace Microsoft.Phone.Test.TestMetadata.Commands
{
	internal interface IBinaryDependencyResolver
	{
		Package ResolveDependency(PortableExecutableDependency portableExecutableDependency, IList<PackageFile> possibleDependencies, IBinaryDependencyParent referencingFile, out bool continueResolving);
	}
}
