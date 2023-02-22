using System;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class FMCollectionItem
	{
		[XmlAttribute]
		public string Path;

		[XmlAttribute("ReleaseType")]
		[DefaultValue(ReleaseType.Production)]
		public ReleaseType releaseType = ReleaseType.Production;

		[XmlAttribute]
		[DefaultValue(false)]
		public bool UserInstallable;

		[XmlAttribute("OwnerType")]
		[DefaultValue(OwnerType.OEM)]
		public OwnerType ownerType = OwnerType.OEM;

		private string _owner;

		[XmlAttribute]
		[DefaultValue(CpuId.Invalid)]
		public CpuId CPUType;

		[XmlAttribute]
		[DefaultValue(false)]
		public bool SkipForPublishing;

		[XmlAttribute]
		[DefaultValue(false)]
		public bool SkipForPRSSigning;

		[XmlAttribute]
		[DefaultValue(false)]
		public bool ValidateAsMicrosoftPhoneFM;

		[XmlAttribute]
		[DefaultValue(false)]
		public bool Critical;

		[XmlAttribute]
		public string ID;

		[XmlAttribute]
		public Guid MicrosoftFMGUID = Guid.Empty;

		[XmlAttribute("OwnerName")]
		[DefaultValue(null)]
		public string Owner
		{
			get
			{
				if (ownerType == OwnerType.Microsoft)
				{
					return OwnerType.Microsoft.ToString();
				}
				return _owner;
			}
			set
			{
				if (ownerType == OwnerType.Microsoft)
				{
					_owner = null;
				}
				else
				{
					_owner = value;
				}
			}
		}

		public bool ShouldSerializeMicrosoftFMGUID()
		{
			return MicrosoftFMGUID != Guid.Empty;
		}

		public override string ToString()
		{
			return Path;
		}

		public string ResolveFMPath(string fmDirectory)
		{
			return Path.ToUpper(CultureInfo.InvariantCulture).Replace("$(FMDIRECTORY)", fmDirectory, StringComparison.OrdinalIgnoreCase);
		}
	}
}
