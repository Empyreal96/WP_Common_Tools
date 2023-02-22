namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces
{
	public interface IInboxProvXML
	{
		string ProvXMLDestinationPath { get; }

		string UpdateProvXMLDestinationPath { get; }

		string LicenseDestinationPath { get; }

		ProvXMLCategory Category { get; }

		string DependencyHash { get; set; }

		void ReadProvXML();

		void Save(string outputBasePath);

		void Update(string installDestinationPath, string licenseFileDestinationPath);
	}
}
