using System.Collections.Generic;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces
{
	public interface IInboxAppPackage
	{
		void OpenPackage();

		List<string> GetCapabilities();

		IInboxAppManifest GetManifest();

		IInboxAppToPkgObjectsMappingStrategy GetPkgObjectsMappingStrategy();

		string GetInstallDestinationPath(bool isTopLevelPackage);
	}
}
