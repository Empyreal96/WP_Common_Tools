using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	public class AppManifestXAP : IInboxAppManifest
	{
		private string _title = string.Empty;

		private string _description = string.Empty;

		private string _publisher = string.Empty;

		private List<string> _capabilities = new List<string>();

		private string _manifestBasePath = string.Empty;

		private string _manifestDestinationPath = string.Empty;

		private string _productID = string.Empty;

		private string _version = string.Empty;

		public string Filename => Path.GetFileName(_manifestBasePath);

		public string Title => _title;

		public string Description => _description;

		public string Publisher => _publisher;

		public List<string> Capabilities => _capabilities;

		public string ProductID => _productID;

		public string Version => _version;

		public AppManifestXAP(string manifestBasePath)
		{
			_manifestBasePath = InboxAppUtils.ValidateFileOrDir(manifestBasePath, false);
		}

		public void ReadManifest()
		{
			XDocument node = XDocument.Load(_manifestBasePath);
			LogUtil.Message("Parsing XAP Manifest: {0}", _manifestBasePath);
			XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(new NameTable());
			xmlNamespaceManager.AddNamespace("xap2009", "http://schemas.microsoft.com/windowsphone/2009/deployment");
			xmlNamespaceManager.AddNamespace("xap2012", "http://schemas.microsoft.com/windowsphone/2012/deployment");
			StringBuilder stringBuilder = new StringBuilder();
			string text = "/xap2009:";
			IEnumerable<XElement> source = node.XPathSelectElements(text + "Deployment/App", xmlNamespaceManager);
			if (source.Count() == 0)
			{
				text = "/xap2012:";
				source = node.XPathSelectElements(text + "Deployment/App", xmlNamespaceManager);
			}
			if (source.Count() > 0)
			{
				if (source.Count() > 1)
				{
					LogUtil.Warning("XAP Manifest has {0} App nodes, only the first will be processed", source.Count());
				}
				foreach (XAttribute item in source.First().Attributes())
				{
					string text2 = item.Name.ToString();
					string value = item.Value;
					switch (text2.ToString().ToUpper(CultureInfo.InvariantCulture))
					{
					case "PRODUCTID":
						try
						{
							new Guid(value).ToString();
							_productID = value;
							LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "Setting productID: '{0}'", new object[1] { _productID }));
						}
						catch (FormatException)
						{
							stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "ProductID \"{0}\" is in an invalid GUID format.", new object[1] { value }));
						}
						catch (OverflowException)
						{
							stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "ProductID \"{0}\" is in an invalid GUID format.", new object[1] { value }));
						}
						catch (ArgumentNullException)
						{
							stringBuilder.AppendLine("ProductID is null or blank.");
						}
						break;
					case "TITLE":
						_title = item.Value;
						LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "Setting title: '{0}'", new object[1] { _title }));
						break;
					case "VERSION":
						_version = item.Value;
						LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "Setting version: '{0}'", new object[1] { _version }));
						break;
					case "PUBLISHER":
						_publisher = item.Value;
						LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "Setting publisher: '{0}'", new object[1] { _publisher }));
						break;
					case "DESCRIPTION":
						_description = item.Value;
						LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "Setting description: '{0}'", new object[1] { _description }));
						break;
					default:
						LogUtil.Diagnostic(string.Format(CultureInfo.InvariantCulture, "Ignoring '{0}'='{1}'", new object[2] { item.Name, item.Value }));
						break;
					}
				}
				foreach (XElement item2 in node.XPathSelectElements(text + "Deployment/App/Capabilities/Capability", xmlNamespaceManager))
				{
					foreach (XAttribute item3 in item2.Attributes())
					{
						if (item3.Name.ToString().ToUpper(CultureInfo.InvariantCulture).Equals("NAME"))
						{
							if (!_capabilities.Contains(item3.Value))
							{
								LogUtil.Message(string.Format(CultureInfo.InvariantCulture, "Adding capability: {0}", new object[1] { item3.Value }));
								_capabilities.Add(item3.Value);
							}
							else
							{
								LogUtil.Diagnostic(string.Format(CultureInfo.InvariantCulture, "Ignoring capability attribute named: {0}, value: {1}", new object[2] { item3.Name, item3.Value }));
							}
						}
					}
				}
			}
			if (string.IsNullOrEmpty(_productID))
			{
				stringBuilder.AppendLine("ProductID not defined in the manifest");
			}
			if (string.IsNullOrEmpty(_title))
			{
				stringBuilder.AppendLine("Title is not defined in the manifest");
			}
			if (string.IsNullOrEmpty(_version))
			{
				stringBuilder.AppendLine("Version is not defined in the manifest");
			}
			if (string.IsNullOrEmpty(_publisher))
			{
				stringBuilder.AppendLine("Publisher is not defined in the manifest");
			}
			if (!string.IsNullOrEmpty(stringBuilder.ToString()))
			{
				LogUtil.Error(stringBuilder.ToString());
				throw new InvalidDataException(stringBuilder.ToString());
			}
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "XAP manifest: (Filename): \"{0}\", (Title): \"{1}\", (ProductID): \"{2}\" ", new object[3] { Filename, Title, ProductID });
		}
	}
}
