using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	public class ProvXMLAppxBundle : ProvXMLAppx
	{
		public ProvXMLAppxBundle(InboxAppParameters parameters, AppManifestAppxBase manifest)
			: base(parameters, manifest)
		{
			if (!(manifest is AppManifestAppxBundle))
			{
				throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "INTERNAL ERROR: The manifest passed into ProvXMLAppxBundle is of type {0}. Only AppxManifestBundle2013 types are allowed.", new object[1] { manifest.GetType() }));
			}
		}

		public override void Update(string appInstallDestinationPath, string licenseFileDestinationPath)
		{
			base.Update(appInstallDestinationPath, licenseFileDestinationPath);
			if (_characteristicParamsElements == null || _characteristicParamsElements.First() == null)
			{
				throw new InvalidDataException("INTERNAL ERROR: One or more preconditions for the ProvXMLAppxBundle.Update method are not met.");
			}
			XElement xElement = _characteristicParamsElements.Ancestors().First();
			XElement xElement2 = new XElement("parm");
			xElement2.Add(new XAttribute("name", "IsBundle"));
			xElement2.Add(new XAttribute("value", true.ToString()));
			xElement.Add(xElement2);
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "Appx Bundle ProvXML: (BasePath)=\"{0}\"", new object[1] { _parameters.ProvXMLBasePath });
		}
	}
}
