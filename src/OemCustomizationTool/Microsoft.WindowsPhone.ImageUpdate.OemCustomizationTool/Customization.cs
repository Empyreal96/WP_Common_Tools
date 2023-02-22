using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.WindowsPhone.ImageUpdate.OemCustomizationTool
{
	internal class Customization
	{
		public const string c_strOwner = "Owner";

		public const string c_strOwnerType = "OwnerType";

		public const string c_strReleaseType = "ReleaseType";

		public const string c_strOEMCustomizationPackage = "OEMCustomizationPackage";

		public const string c_strComponent = "Component";

		public const string c_strComponentName = "ComponentName";

		public const string c_strSetting = "Setting";

		public const string c_strCustomName = "CustomName";

		public const string c_strKey = "Key";

		private List<XmlFile> xmlFiles;

		private XDocument customizationXmlDoc;

		public List<XmlFile> XmlFiles
		{
			get
			{
				return xmlFiles;
			}
			set
			{
				xmlFiles = value;
			}
		}

		public XDocument CustomizationXmlDoc
		{
			get
			{
				return customizationXmlDoc;
			}
			set
			{
				customizationXmlDoc = value;
			}
		}

		public bool IsCustomizationValid { get; set; }

		public Customization(List<XmlFile> files)
		{
			if (files == null || files.Count == 0)
			{
				TraceLogger.LogMessage(TraceLevel.Error, "Received an empty customization file list.");
				return;
			}
			try
			{
				List<XmlFile> files2 = files;
				customizationXmlDoc = XmlFileHandler.LoadXmlDoc(ref files2);
				XmlFiles = files2;
				ParsePackageAttributes();
				IsCustomizationValid = true;
			}
			catch (Exception ex)
			{
				TraceLogger.LogMessage(TraceLevel.Error, ex.ToString());
			}
		}

		private void ParsePackageAttributes()
		{
			IEnumerable<XElement> enumerable = customizationXmlDoc.Descendants("OEMCustomizationPackage");
			foreach (XElement item in enumerable)
			{
				TraceLogger.LogMessage(TraceLevel.Info, item.ToString());
			}
			TraceLogger.LogMessage(TraceLevel.Info, "Getting package owner names:");
			IEnumerable<string> enumerable2 = from item in enumerable
				where item.Name.LocalName == "OEMCustomizationPackage"
				select item.Attribute("Owner").Value;
			foreach (string item2 in enumerable2)
			{
				TraceLogger.LogMessage(TraceLevel.Info, item2.ToString());
			}
			TraceLogger.LogMessage(TraceLevel.Warn, "Found multiple owner names (likely due to includes). Using '" + enumerable2.Last() + "' to generate package.", enumerable2.Count() > 1);
			Settings.PackageAttributes.Owner = enumerable2.Last();
			TraceLogger.LogMessage(TraceLevel.Info, "Getting package owner types:");
			IEnumerable<string> enumerable3 = from item in enumerable
				where item.Name.LocalName == "OEMCustomizationPackage"
				select item.Attribute("OwnerType").Value;
			foreach (string item3 in enumerable3)
			{
				TraceLogger.LogMessage(TraceLevel.Info, item3.ToString());
			}
			TraceLogger.LogMessage(TraceLevel.Warn, "Found multiple owner types (likely due to includes). Using '" + enumerable3.Last() + "' to generate package.", enumerable3.Count() > 1);
			Settings.PackageAttributes.OwnerTypeString = enumerable3.Last();
			TraceLogger.LogMessage(TraceLevel.Info, "Getting package release types:");
			IEnumerable<string> enumerable4 = from item in enumerable
				where item.Name.LocalName == "OEMCustomizationPackage"
				select item.Attribute("ReleaseType").Value;
			foreach (string item4 in enumerable4)
			{
				TraceLogger.LogMessage(TraceLevel.Info, item4.ToString());
			}
			TraceLogger.LogMessage(TraceLevel.Warn, "Found multiple release types (likely due to includes). Using '" + enumerable4.Last() + "' to generate package.", enumerable4.Count() > 1);
			Settings.PackageAttributes.ReleaseTypeString = enumerable4.Last();
		}
	}
}
