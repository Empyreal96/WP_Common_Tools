using System.Collections.Generic;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces
{
	public interface IInboxAppToPkgObjectsMappingStrategy
	{
		List<PkgObject> Map(IInboxAppPackage appPackage, IPkgProject packageGenerator, OSComponentBuilder osComponent);
	}
}
