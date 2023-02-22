using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	[CLSCompliant(false)]
	[Guid("4e1bd148-55a0-4480-a3d1-15544710637c")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAppxManifestReader
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
	}
}
