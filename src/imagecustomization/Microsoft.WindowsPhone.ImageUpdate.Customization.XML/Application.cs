using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization.XML
{
	[XmlRoot(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class Application : IDefinedIn
	{
		private static readonly string MainOSPartitionRoot;

		private static readonly string DataPartitionRoot;

		private const string VariantMainOSDestinationRoot = "Programs\\CommonFiles\\Multivariant\\Apps";

		private const string VariantDataDestinationRoot = "SharedData\\Multivariant\\Apps";

		private const string StaticLicenseMainOSDestinationRoot = "Programs\\CommonFiles\\xaps";

		private const string StaticLicenseDataDestinationRoot = "SharedData\\Provisioning\\OEM\\Public";

		private const string StaticDeviceMainOSDestinationRoot = "Programs\\CommonFiles\\xaps";

		private const string StaticDeviceDataDestinationRoot = "SharedData\\Provisioning\\OEM\\Public";

		public const string StaticProvXMLMxipupdatePath = "Windows\\System32\\Migrators\\DuMigrationProvisionerMicrosoft\\provxml";

		private const string StaticProvXMLMainOSDestinationRoot = "Programs\\CommonFiles\\Provisioning\\Microsoft";

		private const string StaticProvXMLDataDestinationRoot = "SharedData\\Provisioning\\OEM";

		private const string XapExtension = ".xap";

		private const string AppInstall = "AppInstall";

		private const string AppxPackage = "APPXPackage";

		private const string XapPackage = "XAPPackage";

		private const string InstallInfo = "InstallInfo";

		private const string CharacteristicElementName = "characteristic";

		private const string CharacteristicType = "type";

		private const string LicensePath = "LicensePath";

		private const string XapPath = "XapPath";

		private const string AppxPath = "AppxPath";

		private const string ParmElementName = "parm";

		private const string ParmName = "name";

		private const string ParmValue = "value";

		private const string ProductID = "ProductID";

		private static readonly string DefaultStaticPartition;

		private static readonly string DefaultVariantPartition;

		[XmlIgnore]
		public static readonly string SourceFieldName;

		[XmlIgnore]
		public static readonly string LicenseFieldName;

		[XmlIgnore]
		public static readonly string ProvXMLFieldName;

		private string _targetPartition;

		[XmlIgnore]
		public string DefinedInFile { get; set; }

		[XmlAttribute]
		public string Source { get; set; }

		[XmlIgnore]
		public string ExpandedSourcePath => ImageCustomizations.ExpandPath(Source);

		[XmlAttribute]
		public string License { get; set; }

		[XmlIgnore]
		public string ExpandedLicensePath => ImageCustomizations.ExpandPath(License);

		[XmlAttribute]
		public string ProvXML { get; set; }

		[XmlIgnore]
		public string ExpandedProvXMLPath => ImageCustomizations.ExpandPath(ProvXML);

		[XmlIgnore]
		public string DeviceDestination
		{
			get
			{
				if (Source == null)
				{
					return null;
				}
				string fileName = Path.GetFileName(Source);
				if (TargetPartition.Equals(PkgConstants.c_strMainOsPartition, StringComparison.OrdinalIgnoreCase))
				{
					if (StaticApp)
					{
						return Path.Combine("Programs\\CommonFiles\\xaps", fileName);
					}
					return Path.Combine("Programs\\CommonFiles\\Multivariant\\Apps", fileName);
				}
				if (TargetPartition.Equals(PkgConstants.c_strDataPartition, StringComparison.OrdinalIgnoreCase))
				{
					if (StaticApp)
					{
						return Path.Combine("SharedData\\Provisioning\\OEM\\Public", fileName);
					}
					return Path.Combine("SharedData\\Multivariant\\Apps", fileName);
				}
				throw new InvalidOperationException("Unknown target partition while querying for a static device destination!");
			}
		}

		[XmlIgnore]
		public string DeviceLicense
		{
			get
			{
				string fileName = Path.GetFileName(License);
				if (TargetPartition.Equals(PkgConstants.c_strMainOsPartition, StringComparison.OrdinalIgnoreCase))
				{
					if (StaticApp)
					{
						return Path.Combine("Programs\\CommonFiles\\xaps", fileName);
					}
					return Path.Combine("Programs\\CommonFiles\\Multivariant\\Apps", fileName);
				}
				if (TargetPartition.Equals(PkgConstants.c_strDataPartition, StringComparison.OrdinalIgnoreCase))
				{
					if (StaticApp)
					{
						return Path.Combine("SharedData\\Provisioning\\OEM\\Public", fileName);
					}
					return Path.Combine("SharedData\\Multivariant\\Apps", fileName);
				}
				throw new InvalidOperationException("Unknown target partition while querying for a static device destination!");
			}
		}

		[XmlIgnore]
		public string DeviceProvXML
		{
			get
			{
				string fileName = Path.GetFileName(ProvXML);
				if (StaticApp)
				{
					if (TargetPartition.Equals(PkgConstants.c_strMainOsPartition, StringComparison.OrdinalIgnoreCase))
					{
						return Path.Combine("Programs\\CommonFiles\\Provisioning\\Microsoft", fileName);
					}
					if (TargetPartition.Equals(PkgConstants.c_strDataPartition, StringComparison.OrdinalIgnoreCase))
					{
						return Path.Combine("SharedData\\Provisioning\\OEM", fileName);
					}
					throw new InvalidOperationException("Unknown target partition while querying for a static device provXML path!");
				}
				throw new InvalidOperationException("Non-static applications should not query for a destination path here!");
			}
		}

		[XmlIgnore]
		public bool StaticApp { get; set; }

		[XmlAttribute]
		public string TargetPartition
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_targetPartition))
				{
					if (!StaticApp)
					{
						return DefaultVariantPartition;
					}
					return DefaultStaticPartition;
				}
				return _targetPartition;
			}
			set
			{
				_targetPartition = value;
			}
		}

		[XmlIgnore]
		public static IEnumerable<string> ValidPartitions => new List<string>
		{
			PkgConstants.c_strMainOsPartition,
			PkgConstants.c_strDataPartition
		};

		public KeyValuePair<Guid, XElement> UpdateProvXml(XElement rootNode)
		{
			XElement xElement = new XElement(rootNode);
			XElement xElement2 = xElement.Elements().Single().Elements()
				.First();
			string path;
			if (TargetPartition.Equals(PkgConstants.c_strMainOsPartition, StringComparison.OrdinalIgnoreCase))
			{
				path = MainOSPartitionRoot;
			}
			else
			{
				if (!TargetPartition.Equals(PkgConstants.c_strDataPartition, StringComparison.OrdinalIgnoreCase))
				{
					throw new CustomizationException("Creating provxml to an invalid partition! This should have been caught in verification.");
				}
				path = DataPartitionRoot;
			}
			Guid result;
			if (xElement2.Attribute("type") != null && Guid.TryParse(xElement2.Attribute("type").Value, out result))
			{
				XAttribute xAttribute = xElement2.Elements().SingleOrDefault((XElement x) => x.Attribute("name").Value.Equals("InstallInfo", StringComparison.OrdinalIgnoreCase)).Attribute("value");
				string[] array = xAttribute.Value.Split(';');
				if (DeviceDestination != null)
				{
					array[0] = Path.Combine(path, DeviceDestination);
				}
				array[1] = Path.Combine(path, DeviceLicense);
				xAttribute.Value = string.Join(";", array);
				return new KeyValuePair<Guid, XElement>(result, xElement);
			}
			XElement xElement3 = xElement2.Elements().Single((XElement x) => x.Attribute("name").Value.Equals("ProductID", StringComparison.OrdinalIgnoreCase));
			result = new Guid(xElement3.Attribute("value").Value);
			xElement2.Elements().SingleOrDefault((XElement x) => x.Attribute("name").Value.Equals("LicensePath", StringComparison.OrdinalIgnoreCase))?.Remove();
			XElement xElement4 = new XElement("parm");
			xElement4.Add(new XAttribute("name", "LicensePath"));
			xElement4.Add(new XAttribute("value", Path.Combine(path, DeviceLicense)));
			xElement2.Add(xElement4);
			if (string.IsNullOrWhiteSpace(Source))
			{
				return new KeyValuePair<Guid, XElement>(result, xElement);
			}
			string extension = Path.GetExtension(Source);
			string packageType = (extension.Equals(".xap", StringComparison.OrdinalIgnoreCase) ? "XapPath" : "AppxPath");
			xElement2.Elements().SingleOrDefault((XElement x) => x.Attribute("name").Value.Equals(packageType, StringComparison.OrdinalIgnoreCase))?.Remove();
			XElement xElement5 = new XElement("parm");
			xElement5.Add(new XAttribute("name", packageType));
			string value = Path.Combine(path, DeviceDestination);
			xElement5.Add(new XAttribute("value", value));
			xElement2.Add(xElement5);
			return new KeyValuePair<Guid, XElement>(result, xElement);
		}

		public IEnumerable<CustomizationError> VerifyProvXML()
		{
			List<CustomizationError> list = new List<CustomizationError>();
			XElement xElement;
			try
			{
				xElement = XElement.Load(ExpandedProvXMLPath);
			}
			catch (XmlException ex)
			{
				list.Add(new CustomizationError(CustomizationErrorSeverity.Error, this.ToEnumerable(), "Failed to parse Application ProvXML at {0}: {1}", ProvXML, ex.Message));
				return list;
			}
			IEnumerable<XElement> source = from x in xElement.Elements()
				where x.Attribute("type") != null && x.Attribute("type").Value.Equals("AppInstall", StringComparison.OrdinalIgnoreCase)
				select x;
			if (xElement.Elements().Count() != 1 || source.Count() != 1)
			{
				list.Add(new CustomizationError(CustomizationErrorSeverity.Error, this.ToEnumerable(), "Application ProvXML at '{0}' should only have a single characteristic: AppInstall", ProvXML));
				return list;
			}
			XElement xElement2 = source.Single().Elements().First();
			Guid result;
			if (xElement2.Attribute("type") != null && Guid.TryParse(xElement2.Attribute("type").Value, out result))
			{
				IEnumerable<XElement> source2 = from x in xElement2.Elements()
					where x.Attribute("name") != null && x.Attribute("name").Value.Equals("InstallInfo", StringComparison.OrdinalIgnoreCase)
					select x;
				if (xElement2.Elements().Count() != 1 || source2.Count() != 1)
				{
					list.Add(new CustomizationError(CustomizationErrorSeverity.Error, this.ToEnumerable(), "Application ProvXML at '{0}' should only have a single element under the GUID characteristic: InstallInfo", ProvXML));
					return list;
				}
				XAttribute xAttribute = source2.Single().Attribute("value");
				if (xAttribute == null)
				{
					list.Add(new CustomizationError(CustomizationErrorSeverity.Error, this.ToEnumerable(), "Application ProvXML at '{0}' has an invalid InstallInfo value.", ProvXML));
					return list;
				}
				if (xAttribute.Value.Split(';').Count() < 2)
				{
					list.Add(new CustomizationError(CustomizationErrorSeverity.Error, this.ToEnumerable(), "Application ProvXML at '{0}' has an invalid InstallInfo value.", ProvXML));
					return list;
				}
				return list;
			}
			if (!string.IsNullOrWhiteSpace(Source))
			{
				bool flag = Path.GetExtension(Source).Equals(".xap", StringComparison.OrdinalIgnoreCase);
				string value = ((xElement2.Attribute("type") != null) ? xElement2.Attribute("type").Value : null);
				string text = (flag ? "XAPPackage" : "APPXPackage");
				if (!text.Equals(value, StringComparison.OrdinalIgnoreCase))
				{
					list.Add(new CustomizationError(CustomizationErrorSeverity.Error, this.ToEnumerable(), "Application ProvXML at '{0}' has a missing or mismatched package type (expecting {1})", ProvXML, text));
				}
				string pathType = (flag ? "XapPath" : "AppxPath");
				if (xElement2.Elements().Count((XElement x) => x.Attribute("name") != null && x.Attribute("name").Value.Equals(pathType, StringComparison.OrdinalIgnoreCase)) > 1)
				{
					list.Add(new CustomizationError(CustomizationErrorSeverity.Error, this.ToEnumerable(), "Application ProvXML at '{0}' defines multiple application paths", ProvXML));
				}
			}
			IEnumerable<XElement> source3 = from x in xElement2.Elements()
				where x.Attribute("name") != null && x.Attribute("name").Value.Equals("ProductID", StringComparison.OrdinalIgnoreCase)
				select x;
			if (source3.Count() != 1 || source3.Single().Attribute("value") == null)
			{
				list.Add(new CustomizationError(CustomizationErrorSeverity.Error, this.ToEnumerable(), "Application ProvXML at '{0}' does not define exactly one product ID", ProvXML));
			}
			else if (!Guid.TryParse(source3.Single().Attribute("value").Value, out result))
			{
				list.Add(new CustomizationError(CustomizationErrorSeverity.Error, this.ToEnumerable(), "Application ProvXML at '{0}' does not have a valid Product ID", ProvXML));
			}
			if (xElement2.Elements().Count((XElement x) => x.Attribute("name") != null && x.Attribute("name").Value.Equals("LicensePath", StringComparison.OrdinalIgnoreCase)) > 1)
			{
				list.Add(new CustomizationError(CustomizationErrorSeverity.Error, this.ToEnumerable(), "Application ProvXML at '{0}' defines multiple license paths", ProvXML));
			}
			return list;
		}

		static Application()
		{
			string c_strDefaultDrive = PkgConstants.c_strDefaultDrive;
			char directorySeparatorChar = Path.DirectorySeparatorChar;
			MainOSPartitionRoot = c_strDefaultDrive + directorySeparatorChar;
			DataPartitionRoot = PkgConstants.c_strDefaultDrive + PkgConstants.c_strDataPartitionRoot;
			DefaultStaticPartition = PkgConstants.c_strMainOsPartition;
			DefaultVariantPartition = PkgConstants.c_strDataPartition;
			SourceFieldName = Strings.txtApplicationSource;
			LicenseFieldName = Strings.txtApplicationLicense;
			ProvXMLFieldName = Strings.txtApplicationProvXML;
		}
	}
}
