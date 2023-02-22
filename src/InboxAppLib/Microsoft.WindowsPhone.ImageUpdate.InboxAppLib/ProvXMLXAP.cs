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
	public class ProvXMLXAP : ProvXMLBase
	{
		protected enum Characteristic_XAP
		{
			Unknown,
			XapLegacy,
			XapPackage,
			XapInfused
		}

		protected Characteristic_XAP _originalProvXmlCharacteristic;

		protected XElement _manifestPathElement;

		protected XElement _licensePathElement;

		protected AppManifestXAP _manifest;

		public ProvXMLXAP(InboxAppParameters parameters, AppManifestXAP manifest)
			: base(parameters)
		{
			_manifest = manifest;
			_licenseDestinationPath = DetermineLicenseDestinationPath();
			_provXMLDestinationPath = DetermineProvXMLDestinationPath();
			if (_parameters.UpdateValue != 0)
			{
				_updateProvXMLDestinationPath = GetMxipFileDestinationPath(_parameters.ProvXMLBasePath, _parameters.Category, _parameters.UpdateValue, null);
			}
		}

		public override void ReadProvXML()
		{
			_document = XDocument.Load(_parameters.ProvXMLBasePath);
			if (!GetDetailsForXapPackage() && !GetDetailsForXapInfused() && !GetDetailsForXapLegacy() && _originalProvXmlCharacteristic == Characteristic_XAP.Unknown)
			{
				throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "The application package and the provxml do not agree in type, as no <characteristic type=\"{0}\"|\"{1}\"> was found in the provxml file. Please ensure that the provxml file is the correct one for the application package.", new object[2] { "XapPackage", "XapInfused" }));
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
			if (_document == null || _manifestPathElement == null || _licensePathElement == null)
			{
				throw new InvalidDataException("INTERNAL ERROR: One or more preconditions for the ProvXmlXAP.Update method are not met.");
			}
			_manifestPathElement.Attribute("name").Value = "XapManifestPath";
			_manifestPathElement.Attribute("value").Value = Path.Combine(appInstallDestinationPath, "WMAppManifest.xml");
			_licensePathElement.Attribute("value").Value = licenseFileDestinationPath;
		}

		protected override string DetermineProvXMLDestinationPath()
		{
			string empty = string.Empty;
			empty = ((!_parameters.InfuseIntoDataPartition) ? "$(runtime.commonfiles)\\Provisioning\\" : "$(runtime.data)\\SharedData\\Provisioning\\");
			empty = Path.Combine(empty, base.Category.ToString());
			return Path.Combine(empty, Path.GetFileName(_parameters.ProvXMLBasePath));
		}

		protected override string DetermineLicenseDestinationPath()
		{
			string empty = string.Empty;
			empty = ((!_parameters.InfuseIntoDataPartition) ? "$(runtime.commonfiles)\\Provisioning\\" : "$(runtime.data)\\SharedData\\Provisioning\\");
			string path = Path.GetFileNameWithoutExtension(_parameters.ProvXMLBasePath) + "_" + Path.GetFileName(_parameters.LicenseBasePath);
			empty = Path.Combine(empty, base.Category.ToString());
			return Path.Combine(empty, path);
		}

		private bool GetDetailsForXapPackage()
		{
			bool result = false;
			IEnumerable<XElement> enumerable = from c in _document.Descendants("characteristic")
				where c.Attribute("type").Value.Equals("XapPackage", StringComparison.OrdinalIgnoreCase)
				select c;
			if (enumerable != null && enumerable.Count() > 0)
			{
				XElement xElement = enumerable.First();
				xElement.Attribute("type").Value = "XapInfused";
				IEnumerable<XElement> enumerable2 = xElement.Descendants();
				if (enumerable2 != null && enumerable2.Count() > 0)
				{
					_originalProvXmlCharacteristic = Characteristic_XAP.XapPackage;
					ValidateContents(enumerable2, _manifest, _originalProvXmlCharacteristic);
					result = true;
					LogUtil.Diagnostic("Provxml {0} is of a XapPackage type", _parameters.ProvXMLBasePath);
				}
			}
			return result;
		}

		private bool GetDetailsForXapInfused()
		{
			bool result = false;
			IEnumerable<XElement> enumerable = (from c in _document.Descendants("characteristic")
				where c.Attribute("type").Value.Equals("XapInfused", StringComparison.OrdinalIgnoreCase)
				select c).Descendants();
			if (enumerable != null && enumerable.Count() > 0)
			{
				_originalProvXmlCharacteristic = Characteristic_XAP.XapInfused;
				ValidateContents(enumerable, _manifest, _originalProvXmlCharacteristic);
				result = true;
				LogUtil.Diagnostic("Provxml {0} is of a XapInfused type", _parameters.ProvXMLBasePath);
			}
			return result;
		}

		private bool GetDetailsForXapLegacy()
		{
			bool result = false;
			IEnumerable<XElement> enumerable = (from c in _document.Descendants("characteristic")
				where c.Attribute("type").Value.Equals("AppInstall", StringComparison.OrdinalIgnoreCase)
				select c).Descendants();
			if (enumerable != null && enumerable.Count() > 0)
			{
				IEnumerable<XElement> enumerable2 = from c in enumerable.Descendants("parm")
					where c.Attribute("name").Value.Equals("INSTALLINFO", StringComparison.OrdinalIgnoreCase)
					select c;
				if (enumerable2 != null && enumerable2.Count() > 0)
				{
					_originalProvXmlCharacteristic = Characteristic_XAP.XapLegacy;
					ValidateAndRearrangeDocumentForXapInfused(enumerable, _manifest, _originalProvXmlCharacteristic);
					result = true;
					LogUtil.Diagnostic("Provxml {0} is of a XapLegacy type", _parameters.ProvXMLBasePath);
				}
			}
			return result;
		}

		private void ValidateContents(IEnumerable<XElement> characteristicNode, AppManifestXAP manifest, Characteristic_XAP characteristic)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (characteristic == Characteristic_XAP.XapPackage || characteristic == Characteristic_XAP.XapInfused)
			{
				foreach (XElement item in characteristicNode)
				{
					if (!item.HasAttributes)
					{
						continue;
					}
					string text = item.Attribute("name").Value.ToUpper(CultureInfo.InvariantCulture);
					if (text != null && text.Length == 0)
					{
						continue;
					}
					switch (text)
					{
					case "PRODUCTID":
						if (!item.Attribute("value").Value.Equals(manifest.ProductID, StringComparison.OrdinalIgnoreCase))
						{
							stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Mismatch between manifest product ID: {0}, and provXML product ID: {1}", new object[2]
							{
								manifest.ProductID,
								item.Attribute("value").Value
							}));
						}
						break;
					case "XAPPATH":
						if (string.Compare(Path.GetFileName(item.Attribute("value").Value), Path.GetFileName(_parameters.PackageBasePath), StringComparison.OrdinalIgnoreCase) == 0)
						{
							_manifestPathElement = item;
							break;
						}
						stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Mismatch between XAP filename: {0}, and provXML XAP filename: {1}", new object[2]
						{
							_parameters.PackageBasePath,
							item.Attribute("value").Value
						}));
						break;
					case "XAPMANIFESTPATH":
						if (string.Compare(Path.GetFileName(item.Attribute("value").Value), manifest.Filename, StringComparison.OrdinalIgnoreCase) == 0)
						{
							_manifestPathElement = item;
							break;
						}
						stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Mismatch between manifest file: {0}, and provXML manifest file: {1}", new object[2]
						{
							manifest.Filename,
							item.Attribute("value").Value
						}));
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
						break;
					default:
						LogUtil.Diagnostic(string.Format(CultureInfo.InvariantCulture, "Unexpected parameter to be ignored: name=\"{0}\", value=\"{1}\"", new object[2]
						{
							item.Attribute("name").Value,
							item.Attribute("value").Value
						}));
						break;
					}
				}
				if (!string.IsNullOrEmpty(stringBuilder.ToString()))
				{
					LogUtil.Error(stringBuilder.ToString());
					throw new InvalidDataException(stringBuilder.ToString());
				}
				return;
			}
			throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "INTERNAL ERROR: Characteristic \"{0}\" is not supported by ValidateContents", new object[1] { _originalProvXmlCharacteristic.ToString() }));
		}

		private void ValidateAndRearrangeDocumentForXapInfused(IEnumerable<XElement> characteristicNode, AppManifestXAP manifest, Characteristic_XAP characteristic)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string text = "";
			string value = "";
			string text2 = "";
			string value2 = "";
			string value3 = "";
			string text3 = "";
			if (characteristic != Characteristic_XAP.XapLegacy)
			{
				throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "INTERNAL ERROR: Characteristic \"{0}\" is not supported by ValidateAndRearrangeDocumentForXapInfused", new object[1] { _originalProvXmlCharacteristic.ToString() }));
			}
			foreach (XElement item in characteristicNode)
			{
				if (item.Name.LocalName.Equals("characteristic", StringComparison.OrdinalIgnoreCase))
				{
					IEnumerable<XAttribute> enumerable = item.Attributes("type");
					if (enumerable != null && enumerable.Count() > 0)
					{
						text = enumerable.First().Value;
						if (!text.Equals(manifest.ProductID, StringComparison.OrdinalIgnoreCase))
						{
							stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Mismatch between manifest product ID: {0}, and provXML product ID: {1}", new object[2] { manifest.ProductID, text }));
						}
					}
				}
				else
				{
					if (!item.Name.LocalName.Equals("parm", StringComparison.OrdinalIgnoreCase) || item.Attribute("name") == null || item.Attribute("value") == null)
					{
						continue;
					}
					string value4 = item.Attribute("value").Value;
					string[] array = value4.Split(new char[1] { ';' }, 5, StringSplitOptions.RemoveEmptyEntries);
					LogUtil.Diagnostic("installInfos.Length = {0}, \"{1}\"", array.Length, value4);
					if (array.Length >= 1)
					{
						value = array[0];
					}
					if (array.Length >= 2)
					{
						text2 = array[1];
						string fileName = Path.GetFileName(text2);
						if (fileName != null && !Path.GetFileName(_parameters.LicenseBasePath).Equals(fileName, StringComparison.OrdinalIgnoreCase))
						{
							stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Mismatch between manifest license file: {0} and provXML license file: {1}", new object[2]
							{
								Path.GetFileName(_parameters.LicenseBasePath),
								fileName
							}));
						}
					}
					if (array.Length >= 3)
					{
						value2 = array[2];
					}
					if (array.Length >= 4)
					{
						value3 = array[3];
					}
					if (array.Length >= 5)
					{
						text3 = array[4];
					}
					if (string.IsNullOrWhiteSpace(value))
					{
						stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "XapPath is blank in the provXML file \"{0}\"", new object[1] { _parameters.ProvXMLBasePath }));
					}
					if (string.IsNullOrWhiteSpace(text2))
					{
						stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "LicensePath is blank in the provXML file \"{0}\"", new object[1] { _parameters.ProvXMLBasePath }));
					}
					if (string.IsNullOrWhiteSpace(value2))
					{
						stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "InstanceID is blank in the provXML file \"{0}\"", new object[1] { _parameters.ProvXMLBasePath }));
					}
					if (string.IsNullOrWhiteSpace(value3))
					{
						stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "OfferID is blank in the provXML file \"{0}\"", new object[1] { _parameters.ProvXMLBasePath }));
					}
				}
			}
			_document.RemoveNodes();
			XElement xElement = new XElement("characteristic");
			xElement.Add(new XAttribute("type", "AppInstall"));
			_document.Add(xElement);
			XElement xElement2 = new XElement("characteristic");
			xElement2.Add(new XAttribute("type", "XapInfused"));
			xElement.Add(xElement2);
			XElement xElement3 = new XElement("parm");
			xElement3.Add(new XAttribute("name", "ProductID"));
			xElement3.Add(new XAttribute("value", text));
			xElement2.Add(xElement3);
			XElement xElement4 = new XElement("parm");
			xElement4.Add(new XAttribute("name", "XapManifestPath"));
			xElement4.Add(new XAttribute("value", value));
			xElement2.Add(xElement4);
			_manifestPathElement = xElement4;
			XElement xElement5 = new XElement("parm");
			xElement5.Add(new XAttribute("name", "LicensePath"));
			xElement5.Add(new XAttribute("value", text2));
			xElement2.Add(xElement5);
			_licensePathElement = xElement5;
			XElement xElement6 = new XElement("parm");
			xElement6.Add(new XAttribute("name", "InstanceID"));
			xElement6.Add(new XAttribute("value", value2));
			xElement2.Add(xElement6);
			XElement xElement7 = new XElement("parm");
			xElement7.Add(new XAttribute("name", "OfferID"));
			xElement7.Add(new XAttribute("value", value3));
			xElement2.Add(xElement7);
			if (!string.IsNullOrWhiteSpace(text3) && text3.Equals(true.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				XElement xElement8 = new XElement("parm");
				xElement8.Add(new XAttribute("name", "UninstallDisabled"));
				xElement8.Add(new XAttribute("value", true.ToString()));
				xElement2.Add(xElement8);
			}
			if (!string.IsNullOrEmpty(stringBuilder.ToString()))
			{
				LogUtil.Error(stringBuilder.ToString());
				throw new InvalidDataException();
			}
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "XAP ProvXML: (BasePath)=\"{0}\"", new object[1] { _parameters.ProvXMLBasePath });
		}
	}
}
