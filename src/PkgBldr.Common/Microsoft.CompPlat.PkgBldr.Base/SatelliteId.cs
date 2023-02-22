using System.Text.RegularExpressions;

namespace Microsoft.CompPlat.PkgBldr.Base
{
	public class SatelliteId
	{
		private static SatelliteId s_neutralId = new SatelliteId(SatelliteType.Neutral, null);

		public SatelliteType Type { get; private set; }

		public string Id { get; private set; }

		public string Culture
		{
			get
			{
				if (Type != SatelliteType.Language)
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
				if (Type != SatelliteType.Resolution)
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
				switch (Type)
				{
				case SatelliteType.Language:
					return "LANGID";
				case SatelliteType.Resolution:
					return "RESID";
				default:
					return "NEUTRAL";
				}
			}
		}

		public string MacroString
		{
			get
			{
				switch (Type)
				{
				case SatelliteType.Language:
					return "$(LANGID)";
				case SatelliteType.Resolution:
					return "$(RESID)";
				default:
					return "NEUTRAL";
				}
			}
		}

		public string MacroValue => Id;

		public string FileSuffix
		{
			get
			{
				switch (Type)
				{
				default:
					return string.Empty;
				case SatelliteType.Language:
					if (Id.Equals("*"))
					{
						return "Resources";
					}
					return "lang_" + Id;
				case SatelliteType.Resolution:
					return "res_" + Id;
				}
			}
		}

		public static SatelliteId Neutral => s_neutralId;

		private SatelliteId(SatelliteType type, string id)
		{
			Type = type;
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
			switch (Type)
			{
			default:
				return "Neutral";
			case SatelliteType.Language:
				return "Language_" + Id;
			case SatelliteType.Resolution:
				return "Resolution_" + Id;
			}
		}

		public override bool Equals(object obj)
		{
			SatelliteId satelliteId = obj as SatelliteId;
			if (this == satelliteId)
			{
				return true;
			}
			if (Type == satelliteId.Type && Id == satelliteId.Id)
			{
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Type.GetHashCode() ^ Id.GetHashCode();
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
