using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	public class ProvXMLAppx : ProvXMLBase
	{
		protected enum Characteristic_Appx
		{
			Unknown,
			AppxPackage,
			AppxInfused,
			FrameworkPackage
		}

		protected Characteristic_Appx _originalProvXmlCharacteristic;

		protected IEnumerable<XElement> _characteristicParamsElements;

		protected XElement _manifestPathElement;

		protected XElement _licensePathElement;

		protected XElement _instanceIDElement;

		protected XElement _productIDElement;

		protected AppManifestAppxBase _manifest;

		public static ProvXMLAppx CreateAppxProvXML(InboxAppParameters parameters, AppManifestAppxBase manifest)
		{
			ProvXMLAppx provXMLAppx = null;
			if (manifest.IsFramework)
			{
				return new ProvXMLAppxFramework(parameters, manifest);
			}
			if (manifest.IsBundle)
			{
				return new ProvXMLAppxBundle(parameters, manifest);
			}
			return new ProvXMLAppx(parameters, manifest);
		}

		public ProvXMLAppx(InboxAppParameters parameters, AppManifestAppxBase manifest)
			: base(parameters)
		{
			if (manifest == null)
			{
				throw new ArgumentNullException("manifest", "INTERNAL ERROR: The manifest passed into the ProvXMLAppx constructor is null!");
			}
			_manifest = manifest;
			if (!_manifest.IsFramework)
			{
				_licenseDestinationPath = DetermineLicenseDestinationPath();
			}
			_provXMLDestinationPath = DetermineProvXMLDestinationPath();
			if (_parameters.UpdateValue != 0)
			{
				_updateProvXMLDestinationPath = GetMxipFileDestinationPath(_parameters.ProvXMLBasePath, _parameters.Category, _parameters.UpdateValue, _manifest);
			}
		}

		public override void ReadProvXML()
		{
			_document = XDocument.Load(_parameters.ProvXMLBasePath);
			if (!GetDetailsForAppxPackage() && !GetDetailsForAppxInfused() && _originalProvXmlCharacteristic == Characteristic_Appx.Unknown)
			{
				throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "The application package and the provxml do not agree in type, as no <characteristic type=\"{0}\"|\"{1}\"> was found in the provxml file. Please ensure that the provxml file is the correct one for the application package.", new object[2] { "AppxPackage", "AppxInfused" }));
			}
		}

		public override void Update(string appInstallDestinationPath, string licenseFileDestinationPath)
		{
			if (string.IsNullOrWhiteSpace(appInstallDestinationPath))
			{
				throw new ArgumentNullException("appInstallDestinationPath", "INTERNAL ERROR: appInstallDestinationPath is null!");
			}
			if (string.IsNullOrWhiteSpace(licenseFileDestinationPath))
			{
				throw new ArgumentNullException("licenseFileDestinationPath", "INTERNAL ERROR: licenseFileDestinationPath is null!");
			}
			if (_document == null || _manifestPathElement == null || _licensePathElement == null || _productIDElement == null)
			{
				throw new InvalidDataException("INTERNAL ERROR: One or more preconditions for the ProvXMLAppx.Update method are not met.");
			}
			_manifestPathElement.Attribute("name").Value = "AppXManifestPath";
			if (_manifest.IsBundle)
			{
				string path = Path.Combine(appInstallDestinationPath, "AppxMetadata");
				_manifestPathElement.Attribute("value").Value = Path.Combine(path, "AppxBundleManifest.xml");
			}
			else
			{
				_manifestPathElement.Attribute("value").Value = Path.Combine(appInstallDestinationPath, "AppxManifest.xml");
			}
			_document.Add(new XComment(string.Format(CultureInfo.InvariantCulture, "Dependency hashes {0}", new object[1] { _packageHash })));
		}

		protected override string DetermineProvXMLDestinationPath()
		{
			string empty = string.Empty;
			string empty2 = string.Empty;
			empty = ((!_parameters.InfuseIntoDataPartition) ? "$(runtime.commonfiles)\\Provisioning\\" : "$(runtime.data)\\SharedData\\Provisioning\\");
			empty2 = "MPAP_" + Path.GetFileName(_parameters.ProvXMLBasePath).CleanFileName();
			return Path.Combine(empty, base.Category.ToString(), empty2);
		}

		protected override string DetermineLicenseDestinationPath()
		{
			string empty = string.Empty;
			empty = ((!_parameters.InfuseIntoDataPartition) ? "$(runtime.commonfiles)\\Xaps\\" : "$(runtime.data)\\SharedData\\Provisioning\\");
			return Path.Combine(empty, Path.GetFileName(_parameters.LicenseBasePath));
		}

		private bool GetDetailsForAppxPackage()
		{
			bool result = false;
			IEnumerable<XElement> enumerable = from c in _document.Descendants("characteristic")
				where c.Attribute("type").Value.Equals("AppxPackage", StringComparison.OrdinalIgnoreCase)
				select c;
			if (enumerable != null && enumerable.Count() > 0)
			{
				enumerable.First().Attribute("type").Value = "AppxInfused";
				IEnumerable<XElement> enumerable2 = enumerable.Descendants();
				if (enumerable2 != null && enumerable2.Count() > 0)
				{
					_originalProvXmlCharacteristic = Characteristic_Appx.AppxPackage;
					_characteristicParamsElements = enumerable2;
					ValidateContents(enumerable2, _manifest);
					result = true;
					LogUtil.Diagnostic("Provxml {0} is of a AppxPackage type", _parameters.ProvXMLBasePath);
				}
			}
			return result;
		}

		protected bool GetDetailsForAppxInfused()
		{
			bool result = false;
			IEnumerable<XElement> enumerable = (from c in _document.Descendants("characteristic")
				where c.Attribute("type").Value.Equals("AppxInfused", StringComparison.OrdinalIgnoreCase)
				select c).Descendants();
			if (enumerable != null && enumerable.Count() > 0)
			{
				_originalProvXmlCharacteristic = Characteristic_Appx.AppxInfused;
				_characteristicParamsElements = enumerable;
				ValidateContents(enumerable, _manifest);
				result = true;
				LogUtil.Diagnostic("Provxml {0} is of a AppxInfused type", _parameters.ProvXMLBasePath);
			}
			return result;
		}

		protected virtual void ValidateContents(IEnumerable<XElement> characteristicNode, AppManifestAppxBase manifest)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (XElement item in characteristicNode)
			{
				if (!item.HasAttributes)
				{
					continue;
				}
				switch (item.Attribute("name").Value.ToUpper(CultureInfo.InvariantCulture))
				{
				case "PRODUCTID":
				{
					_productIDElement = item;
					if (manifest.IsBundle || manifest.IsResource || string.IsNullOrWhiteSpace(manifest.ProductID))
					{
						break;
					}
					Guid result = default(Guid);
					Guid result2 = default(Guid);
					string value = item.Attribute("value").Value;
					bool flag = Guid.TryParse(value, out result);
					bool flag2 = Guid.TryParse(manifest.ProductID, out result2);
					if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(manifest.ProductID))
					{
						stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "The provXML productID {0} and/or the manifest product ID {1} are blank. Please make sure they are specified.", new object[2] { value, manifest.ProductID }));
					}
					else if (flag2)
					{
						if (flag)
						{
							if (result != result2)
							{
								_productIDElement = null;
								stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Mismatch between manifest product ID: {0}, and provXML product ID: {1}", new object[2] { manifest.ProductID, value }));
							}
						}
						else
						{
							stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "provXML productID = \"{0}\" is not a GUID. It should be the same as the manifest product ID \"{1}\"", new object[2] { value, manifest.ProductID }));
						}
					}
					else if (string.Equals(value, manifest.ProductID, StringComparison.OrdinalIgnoreCase))
					{
						LogUtil.Warning("provXML product \"{0}\" and manifest product ID \"{1}\" match but are not GUIDs.", value, manifest.ProductID);
					}
					else
					{
						_productIDElement = null;
						stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Mismatch between manifest product ID: {0}, and provXML product ID: {1}", new object[2] { manifest.ProductID, value }));
					}
					break;
				}
				case "APPXPATH":
					if (_manifestPathElement != null)
					{
						LogUtil.Warning(string.Format(CultureInfo.InvariantCulture, "This provXML file has an earlier '{0}' attribute with value '{1}'. The '{2}' attribute will be ignored.", new object[2]
						{
							"APPXMANIFESTPATH",
							item.Attribute("value").Value
						}), "APPXPATH");
					}
					else if (!string.IsNullOrWhiteSpace(item.Attribute("value").Value))
					{
						_manifestPathElement = item;
					}
					break;
				case "APPXMANIFESTPATH":
					if (_manifestPathElement != null)
					{
						LogUtil.Warning(string.Format(CultureInfo.InvariantCulture, "This provXML file has an earlier '{0}' attribute with value '{1}'. The '{2}' attribute will be ignored.", new object[2]
						{
							"APPXPATH",
							item.Attribute("value").Value
						}), "APPXMANIFESTPATH");
					}
					else if (string.Compare(Path.GetFileName(item.Attribute("value").Value), manifest.Filename, StringComparison.OrdinalIgnoreCase) == 0)
					{
						_manifestPathElement = item;
					}
					else
					{
						stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Mismatch between manifest file: {0}, and provXML manifest file: {1}", new object[2]
						{
							manifest.Filename,
							item.Attribute("value").Value
						}));
					}
					break;
				case "LICENSEPATH":
				{
					string fileName = Path.GetFileName(item.Attribute("value").Value);
					if (fileName != null && string.Compare(Path.GetFileName(_parameters.LicenseBasePath), fileName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						_licensePathElement = item;
						break;
					}
					stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Mismatch between manifest license file: {0} and provXML license file: {1}", new object[2]
					{
						Path.GetFileName(_parameters.LicenseBasePath),
						fileName
					}));
					break;
				}
				case "INSTANCEID":
					if (string.IsNullOrWhiteSpace(item.Attribute("value").Value))
					{
						stringBuilder.AppendLine("Instance ID is blank or omitted");
					}
					else
					{
						_instanceIDElement = item;
					}
					break;
				default:
					LogUtil.Diagnostic(string.Format(CultureInfo.InvariantCulture, "Unexpected parameter to be ignored: name=\"{0}\", value=\"{1}\"", new object[2]
					{
						item.Attribute("name").Value,
						item.Attribute("value").Value
					}));
					break;
				case "":
				case "OFFERID":
				case "PAYLOADID":
					break;
				}
			}
			if (!string.IsNullOrEmpty(stringBuilder.ToString()))
			{
				LogUtil.Error(stringBuilder.ToString());
				throw new InvalidDataException(stringBuilder.ToString());
			}
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "Appx ProvXML: (BasePath)=\"{0}\"", new object[1] { _parameters.ProvXMLBasePath });
		}
	}
}
