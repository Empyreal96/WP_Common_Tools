using System;
using System.Text.RegularExpressions;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents
{
	public class SatelliteId
	{
		private static SatelliteId s_neutralId = new SatelliteId(SatelliteType.Neutral, null);

		public SatelliteType SatType { get; private set; }

		public string Id { get; private set; }

		public string Culture
		{
			get
			{
				if (SatType != SatelliteType.Language)
				{
					return null;
				}
				return Id;
			}
		}

		public string Resolution
		{
			get
			{
				if (SatType != SatelliteType.Resolution)
				{
					return null;
				}
				return Id;
			}
		}

		public string MacroName
		{
			get
			{
				switch (SatType)
				{
				case SatelliteType.Language:
					return "LANGID";
				case SatelliteType.Resolution:
					return "RESID";
				default:
					throw new NotSupportedException(string.Concat("Unsupported satellite type '", SatType, "' for property MacroName"));
				}
			}
		}

		public string MacroString
		{
			get
			{
				switch (SatType)
				{
				case SatelliteType.Language:
					return "$(LANGID)";
				case SatelliteType.Resolution:
					return "$(RESID)";
				default:
					throw new NotSupportedException(string.Concat("Unsupported satellite type '", SatType, "' for property MacroString"));
				}
			}
		}

		public string MacroValue
		{
			get
			{
				if (SatType != 0)
				{
					return Id;
				}
				throw new NotSupportedException(string.Concat("Unsupported satellite type '", SatType, "' for property MacroValue"));
			}
		}

		public string FileSuffix
		{
			get
			{
				switch (SatType)
				{
				case SatelliteType.Neutral:
					return string.Empty;
				case SatelliteType.Language:
					return "lang_" + Id;
				case SatelliteType.Resolution:
					return "res_" + Id;
				default:
					throw new NotSupportedException(string.Concat("Unsupported satellite type '", SatType, "' for property FileSuffix"));
				}
			}
		}

		public static SatelliteId Neutral => s_neutralId;

		private SatelliteId(SatelliteType type, string id)
		{
			SatType = type;
			switch (type)
			{
			case SatelliteType.Neutral:
				Id = string.Empty;
				break;
			case SatelliteType.Language:
				if (id == null || !Regex.Match(id, PkgConstants.c_strCultureStringPattern).Success)
				{
					throw new PkgGenException("Invalid language identifier string: {0}", id);
				}
				Id = id.ToLowerInvariant();
				break;
			case SatelliteType.Resolution:
				if (id == null || !Regex.Match(id, PkgConstants.c_strResolutionStringPattern).Success)
				{
					throw new PkgGenException("Invalid resolution identifier string: {0}", id);
				}
				Id = id.ToLowerInvariant();
				break;
			default:
				throw new PkgGenException("Unexpected satellite type: {0}", type);
			}
		}

		public override string ToString()
		{
			switch (SatType)
			{
			case SatelliteType.Neutral:
				return "Neutral";
			case SatelliteType.Language:
				return "Language_" + Id;
			case SatelliteType.Resolution:
				return "Resolution_" + Id;
			default:
				return string.Concat("Unexpected satellite type '", SatType, "'");
			}
		}

		public override bool Equals(object obj)
		{
			SatelliteId satelliteId = obj as SatelliteId;
			if (this == satelliteId)
			{
				return true;
			}
			if (SatType == satelliteId.SatType && Id == satelliteId.Id)
			{
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return SatType.GetHashCode() ^ Id.GetHashCode();
		}

		public static SatelliteId Create(SatelliteType type, string id)
		{
			if (type == SatelliteType.Neutral)
			{
				return Neutral;
			}
			return new SatelliteId(type, id);
		}
	}
}
