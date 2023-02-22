using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	[CLSCompliant(false)]
	[Guid("d06f67bc-b31d-4eba-a8af-638e73e77b4d")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAppxManifestReader2
	{
		IAppxManifestPackageId GetPackageId();

		IAppxManifestProperties GetProperties();

		IAppxManifestPackageDependenciesEnumerator GetPackageDependencies();

		APPX_CAPABILITIES GetCapabilities();

		IAppxManifestResourcesEnumerator GetResources();

		IAppxManifestDeviceCapabilitiesEnumerator GetDeviceCapabilities();

		ulong GetPrerequisite([In][MarshalAs(UnmanagedType.LPWStr)] string name);

		IAppxManifestApplicationsEnumerator GetApplications();

		IStream GetStream();

		IAppxManifestQualifiedResourcesEnumerator GetQualifiedResources();
	}
}
