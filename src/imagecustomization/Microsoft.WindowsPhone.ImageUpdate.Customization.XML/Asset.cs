using System.IO;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.MCSF.Offline;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization.XML
{
	[XmlRoot(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class Asset : IDefinedIn
	{
		[XmlIgnore]
		public static readonly string SourceFieldName = Strings.txtAssetSource;

		[XmlIgnore]
		public string DefinedInFile { get; set; }

		[XmlAttribute]
		public string Name { get; set; }

		[XmlAttribute]
		public string Source { get; set; }

		[XmlIgnore]
		public string ExpandedSourcePath => ImageCustomizations.ExpandPath(Source);

		[XmlAttribute]
		public string TargetFileName { get; set; }

		[XmlAttribute]
		public string DisplayName { get; set; }

		[XmlAttribute]
		public CustomizationAssetOwner Type { get; set; }

		[XmlIgnore]
		public string Id
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(TargetFileName))
				{
					return TargetFileName;
				}
				return Path.GetFileName(Source);
			}
		}

		public Asset()
		{
			Type = CustomizationAssetOwner.OEM;
		}

		public string GetDevicePath(string deviceRoot)
		{
			return Path.Combine(deviceRoot, Id);
		}

		public string GetDevicePathWithMacros(PolicyAssetInfo policy)
		{
			string text = GetDevicePath(policy.TargetDir);
			if (policy.HasOEMMacros)
			{
				text = new PolicyMacroTable(policy.Name, Name).ReplaceMacros(text);
			}
			return text;
		}
	}
}
