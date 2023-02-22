using System;
using System.Globalization;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces
{
	public sealed class AppPackageFactory
	{
		private AppPackageFactory()
		{
			throw new NotSupportedException("The 'AppPackageFactory' class should never be constructed on its own. Please use only the static methods.");
		}

		public static IInboxAppPackage CreateAppPackage(InboxAppParameters parameters)
		{
			if (parameters == null)
			{
				throw new ArgumentNullException("parameters", "Internal error: The InboxAppParameters is null.");
			}
			string packageBasePath = parameters.PackageBasePath;
			IInboxAppPackage inboxAppPackage = null;
			if (InboxAppUtils.ExtensionMatches(packageBasePath, ".xap"))
			{
				throw new ArgumentException("This tool does not support XAP packages for infusion. Try using an appx or appxbundle.");
			}
			if (InboxAppUtils.ExtensionMatches(packageBasePath, ".appxbundle"))
			{
				inboxAppPackage = new AppPackageAppxBundle(parameters);
			}
			else if (InboxAppUtils.ExtensionMatches(packageBasePath, ".appx"))
			{
				inboxAppPackage = new AppPackageAppx(parameters);
			}
			if (inboxAppPackage == null)
			{
				LogUtil.Error(string.Format(CultureInfo.InvariantCulture, "The packageBasePath \"{0}\" is of a package type that is not supported.", new object[1] { packageBasePath }));
			}
			return inboxAppPackage;
		}
	}
}
