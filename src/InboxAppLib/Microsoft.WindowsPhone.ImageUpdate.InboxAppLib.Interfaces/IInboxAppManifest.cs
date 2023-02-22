using System.Collections.Generic;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces
{
	public interface IInboxAppManifest
	{
		string Filename { get; }

		string Title { get; }

		string Description { get; }

		string Publisher { get; }

		List<string> Capabilities { get; }

		string ProductID { get; }

		string Version { get; }

		void ReadManifest();
	}
}
