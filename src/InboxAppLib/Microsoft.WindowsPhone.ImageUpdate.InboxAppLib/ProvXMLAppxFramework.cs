using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	public class ProvXMLAppxFramework : ProvXMLAppx
	{
		public ProvXMLAppxFramework(InboxAppParameters parameters, AppManifestAppxBase manifest)
			: base(parameters, manifest)
		{
		}

		public override void ReadProvXML()
		{
			_document = XDocument.Load(_parameters.ProvXMLBasePath);
			if (!GetDetailsForFrameworkPackage() && !GetDetailsForAppxInfused() && _originalProvXmlCharacteristic == Characteristic_Appx.Unknown)
			{
				throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "The application package and the provxml do not agree in type, as no <characteristic type=\"{0}\"|\"{1}\"> was found in the provxml file. Please ensure that the provxml file is the correct one for the application package.", new object[2] { "FrameworkPackage", "AppxInfused" }));
			}
		}

		public override void Update(string appInstallDestinationPath, string licenseFileDestinationPath)
		{
			if (string.IsNullOrWhiteSpace(appInstallDestinationPath))
			{
				throw new ArgumentNullException("appInstallDestinationPath", "INTERNAL ERROR: appInstallDestinationPath is null!");
			}
			if (_document == null || !_manifest.IsFramework || _manifestPathElement == null || _productIDElement == null)
			{
				throw new InvalidDataException("INTERNAL ERROR: One or more preconditions for the ProvXMLAppxFramework.Update method are not met.");
			}
			_manifestPathElement.Attribute("name").Value = "AppXManifestPath";
			_manifestPathElement.Attribute("value").Value = Path.Combine(appInstallDestinationPath, "AppxManifest.xml");
			_document.Add(new XComment(string.Format(CultureInfo.InvariantCulture, "Dependency hashes {0}", new object[1] { _packageHash })));
		}

		protected override string DetermineProvXMLDestinationPath()
		{
			string empty = string.Empty;
			string empty2 = string.Empty;
			if (_parameters.InfuseIntoDataPartition)
			{
				throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Infusing framework packages onto the data partition is not supported. Please remove the {0} attribute from the pkg.xml file for all framework packages.", new object[1] { "InfuseIntoDataPartition" }));
			}
			empty = "$(runtime.coldBootProvxmlMS)\\";
			empty2 = "mxipcold_appframework_" + Path.GetFileName(_parameters.ProvXMLBasePath).CleanFileName();
			return Path.Combine(empty, empty2);
		}

		private bool GetDetailsForFrameworkPackage()
		{
			bool result = false;
			IEnumerable<XElement> enumerable = from c in _document.Descendants("characteristic")
				where c.Attribute("type").Value.Equals("FrameworkPackage", StringComparison.OrdinalIgnoreCase)
				select c;
			if (enumerable != null && enumerable.Count() > 0)
			{
				enumerable.First().Attribute("type").Value = "AppxInfused";
				IEnumerable<XElement> enumerable2 = enumerable.Descendants();
				if (enumerable2 != null && enumerable2.Count() > 0)
				{
					_originalProvXmlCharacteristic = Characteristic_Appx.FrameworkPackage;
					_characteristicParamsElements = enumerable2;
					ValidateContents(enumerable2, _manifest);
					result = true;
					LogUtil.Diagnostic("Provxml {0} is of a FrameworkPackage type", _parameters.ProvXMLBasePath);
				}
			}
			return result;
		}
	}
}
