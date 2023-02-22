using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxApp
{
	[Export(typeof(IPkgPlugin))]
	public class InboxApp : PkgPlugin
	{
		private interface IPkgPluginAdapter
		{
			void ValidateEntry(IPkgProject packageGenerator, XElement componentEntry);

			IEnumerable<PkgObject> ProcessEntry(IPkgProject packageGenerator, XElement componentEntry);
		}

		internal class InboxAppLibAdapter : IPkgPluginAdapter
		{
			private static readonly ReadOnlyCollection<string> validInfuseIntoDataPartitionAttrValues = PkgGenConstants.ValidAttrInfuseIntoDataPartitionValues;

			private static readonly ReadOnlyCollection<string> validUpdateAttrValues = PkgGenConstants.ValidAttrUpdateValues;

			private string _sourceBasePath = string.Empty;

			private string _licenseBasePath = string.Empty;

			private string _provXMLBasePath = string.Empty;

			private bool _infuseIntoDataPartition;

			private UpdateType _updateValue;

			private ProvXMLCategory _category;

			private InboxAppParameters _inboxAppParameters;

			public void ValidateEntry(IPkgProject packageGenerator, XElement componentEntry)
			{
				ValidateRequiredAttribute(componentEntry, "Source", "Source is required for InboxApp objects");
				ValidateRequiredAttribute(componentEntry, "ProvXML", "A provisioning xml file is required for InboxApp objects");
				ValidateAttributeValues(componentEntry, "InfuseIntoDataPartition", validInfuseIntoDataPartitionAttrValues, "The 'InfuseIntoDataPartition' attribute must be either 'true' or 'false'");
				ValidateAttributeValues(componentEntry, "Update", validUpdateAttrValues, "The 'Update' attribute must be either 'early' or 'normal'");
			}

			public IEnumerable<PkgObject> ProcessEntry(IPkgProject packageGenerator, XElement componentEntry)
			{
				GetInboxAttributes(packageGenerator, componentEntry);
				IInboxAppPackage inboxAppPackage = AppPackageFactory.CreateAppPackage(_inboxAppParameters);
				LogUtil.Message("[InboxApp.ProcessEntry] Processing: {0}", _inboxAppParameters.ToString());
				LogUtil.Diagnostic("[InboxApp.ProcessEntry] app package type: \"{0}\" ", inboxAppPackage.GetType());
				inboxAppPackage.OpenPackage();
				LogUtil.Diagnostic("[InboxApp.ProcessEntry] Successfully opened \"{0}\" ({1})", _sourceBasePath, inboxAppPackage.ToString());
				IInboxAppManifest manifest = inboxAppPackage.GetManifest();
				LogUtil.Diagnostic("[InboxApp.ProcessEntry] app manifest type: \"{0}\" ", manifest.GetType());
				LogUtil.Diagnostic("[InboxApp.ProcessEntry] appManifest.Title: \"{0}\" ", manifest.Title);
				LogUtil.Diagnostic("[InboxApp.ProcessEntry] appManifest.Description: \"{0}\" ", manifest.Description);
				LogUtil.Diagnostic("[InboxApp.ProcessEntry] appManifest.Publisher: \"{0}\" ", manifest.Publisher);
				IInboxAppToPkgObjectsMappingStrategy pkgObjectsMappingStrategy = inboxAppPackage.GetPkgObjectsMappingStrategy();
				LogUtil.Diagnostic("[InboxApp.ProcessEntry] mapping strategy type: \"{0}\" ", pkgObjectsMappingStrategy.GetType());
				OSComponentBuilder osComponent = new OSComponentBuilder();
				List<PkgObject> list = pkgObjectsMappingStrategy.Map(inboxAppPackage, packageGenerator, osComponent);
				foreach (PkgObject item in list)
				{
					LogUtil.Diagnostic("[InboxApp.ProcessEntry] Added to package: {0}", item.ToString());
				}
				LogUtil.Diagnostic("[InboxApp.ProcessEntry] PkgObject count: {0}", list.Count);
				return list;
			}

			private void GetInboxAttributes(IPkgProject packageGenerator, XElement componentEntry)
			{
				XAttribute xAttribute = componentEntry.LocalAttribute("Source");
				_sourceBasePath = packageGenerator.MacroResolver.Resolve(xAttribute.Value, MacroResolveOptions.ErrorOnUnknownMacro);
				LogUtil.Diagnostic("[InboxApp.ValidateEntry] {0}(BasePath) = \"{1}\"", "Source", _sourceBasePath);
				XAttribute xAttribute2 = componentEntry.LocalAttribute("ProvXML");
				_provXMLBasePath = packageGenerator.MacroResolver.Resolve(xAttribute2.Value, MacroResolveOptions.ErrorOnUnknownMacro);
				LogUtil.Diagnostic("[InboxApp.ValidateEntry] {0}(BasePath) = \"{1}\"", "ProvXML", _provXMLBasePath);
				XAttribute xAttribute3 = componentEntry.LocalAttribute("License");
				if (xAttribute3 != null)
				{
					_licenseBasePath = packageGenerator.MacroResolver.Resolve(xAttribute3.Value, MacroResolveOptions.ErrorOnUnknownMacro);
					LogUtil.Diagnostic("[InboxApp.ValidateEntry] {0}(BasePath) = \"{1}\"", "License", _licenseBasePath);
				}
				XAttribute xAttribute4 = componentEntry.LocalAttribute("InfuseIntoDataPartition");
				if (xAttribute4 != null)
				{
					int num = validInfuseIntoDataPartitionAttrValues.IndexOf(xAttribute4.Value.ToLower(CultureInfo.InvariantCulture).Trim());
					_infuseIntoDataPartition = num == 0;
					LogUtil.Diagnostic("[InboxApp.ValidateEntry] (OPTIONAL) {0} = \"{1}\"", "InfuseIntoDataPartition", _infuseIntoDataPartition.ToString());
				}
				XAttribute xAttribute5 = componentEntry.LocalAttribute("Update");
				if (xAttribute5 != null)
				{
					switch (validUpdateAttrValues.IndexOf(xAttribute5.Value.ToLower(CultureInfo.InvariantCulture).Trim()))
					{
					case 0:
						_updateValue = UpdateType.UpdateEarly;
						break;
					case 1:
						_updateValue = UpdateType.UpdateNormal;
						break;
					default:
						_updateValue = UpdateType.UpdateNotNeeded;
						break;
					}
					LogUtil.Diagnostic("[InboxApp.ValidateEntry] (OPTIONAL) {0} = \"{1}\"", "Update", _updateValue.ToString());
				}
				if (_infuseIntoDataPartition && _updateValue != 0)
				{
					LogUtil.Warning(string.Format(CultureInfo.InvariantCulture, "Update is not supported while infusing to the Data Partition. Ignoring the Update attribute."));
					_updateValue = UpdateType.UpdateNotNeeded;
				}
				string value = packageGenerator.MacroResolver.GetValue("PROVXMLTYPE");
				if (!string.IsNullOrWhiteSpace(value))
				{
					value = value.ToUpper(CultureInfo.InvariantCulture);
					if (value == "Microsoft".ToUpper(CultureInfo.InvariantCulture))
					{
						_category = ProvXMLCategory.Microsoft;
					}
					else if (value == "Test".ToUpper(CultureInfo.InvariantCulture))
					{
						_category = ProvXMLCategory.Test;
					}
					else if (!(value == "OEM".ToUpper(CultureInfo.InvariantCulture)))
					{
						throw new ArgumentOutOfRangeException(string.Format(CultureInfo.InvariantCulture, "The value \"{0}\" is an invalid value for the parameter {1}. Valid values are: {2}", new object[3]
						{
							value,
							"PROVXMLTYPE",
							string.Join(",", PkgGenConstants.ValidVariablePROVXMLTYPEValues)
						}));
					}
				}
				string workingBaseDir = Path.Combine(packageGenerator.TempDirectory, Path.GetFileName(Path.GetRandomFileName()));
				_inboxAppParameters = new InboxAppParameters(_sourceBasePath, _licenseBasePath, _provXMLBasePath, _infuseIntoDataPartition, _updateValue, _category, workingBaseDir);
			}

			private void ValidateRequiredAttribute(XElement componentEntry, string attributeName, string message)
			{
				XAttribute xAttribute = componentEntry.LocalAttribute(attributeName);
				if (xAttribute == null || string.IsNullOrWhiteSpace(xAttribute.Value))
				{
					throw new PkgXmlException(componentEntry, message);
				}
			}

			private void ValidateAttributeValues(XElement componentEntry, string attributeName, ReadOnlyCollection<string> validValues, string message)
			{
				XAttribute xAttribute = componentEntry.LocalAttribute(attributeName);
				if (xAttribute != null && validValues != null && validValues.Count > 0)
				{
					string value = xAttribute.Value.ToLower(CultureInfo.InvariantCulture);
					if (!string.IsNullOrWhiteSpace(value) && !validValues.Contains(value))
					{
						throw new PkgXmlException(componentEntry, message);
					}
				}
			}
		}

		private IPkgPluginAdapter adapter;

		public override string Name => "Inbox Application Component";

		public override string XmlElementUniqueXPath => "@Source";

		public override string XmlSchemaPath => "Microsoft.WindowsPhone.ImageUpdate.InboxApp.InboxApp.Resources.Schema.xsd";

		public InboxApp()
		{
			adapter = new InboxAppLibAdapter();
		}

		protected override void ValidateEntry(IPkgProject packageGenerator, XElement componentEntry)
		{
			adapter.ValidateEntry(packageGenerator, componentEntry);
		}

		protected override IEnumerable<PkgObject> ProcessEntry(IPkgProject packageGenerator, XElement componentEntry)
		{
			return adapter.ProcessEntry(packageGenerator, componentEntry);
		}
	}
}
