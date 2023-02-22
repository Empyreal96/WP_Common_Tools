using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct ManifestConstants_XAP
	{
		public const string ManifestFileName = "WMAppManifest.xml";

		public const string SLLLightupManifestFileName = "appxmanifest.xml";

		public const string AttributeValueXAPManifestPath = "XAPMANIFESTPATH";

		public const string AttributeValueXAPPath = "XAPPATH";

		public const string Xap2009Schema = "http://schemas.microsoft.com/windowsphone/2009/deployment";

		public const string Xap2012Schema = "http://schemas.microsoft.com/windowsphone/2012/deployment";

		public const string AppNode = "Deployment/App";

		public const string CapabilityNode = "Deployment/App/Capabilities/Capability";
	}
}
