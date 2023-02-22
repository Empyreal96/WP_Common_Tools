using System;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public struct VersionInfo
	{
		public static readonly VersionInfo Empty;

		[XmlAttribute]
		public ushort Major;

		[XmlAttribute]
		public ushort Minor;

		[XmlAttribute]
		public ushort QFE;

		[XmlAttribute]
		public ushort Build;

		public VersionInfo(ushort major, ushort minor, ushort qfe, ushort build)
		{
			Major = major;
			Minor = minor;
			QFE = qfe;
			Build = build;
		}

		public override string ToString()
		{
			return $"{Major}.{Minor}.{QFE}.{Build}";
		}

		public override int GetHashCode()
		{
			return Major.GetHashCode() ^ Minor.GetHashCode() ^ QFE.GetHashCode() ^ Build.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is VersionInfo)
			{
				return this == (VersionInfo)obj;
			}
			return false;
		}

		public static bool operator ==(VersionInfo versionInfo1, VersionInfo versionInfo2)
		{
			if (versionInfo1.Major == versionInfo2.Major && versionInfo1.Minor == versionInfo2.Minor && versionInfo1.QFE == versionInfo2.QFE)
			{
				return versionInfo1.Build == versionInfo2.Build;
			}
			return false;
		}

		public static bool operator !=(VersionInfo versionInfo1, VersionInfo versionInfo2)
		{
			return !(versionInfo1 == versionInfo2);
		}

		public static bool operator <(VersionInfo versionInfo1, VersionInfo versionInfo2)
		{
			int[,] array = new int[4, 2]
			{
				{ versionInfo1.Major, versionInfo2.Major },
				{ versionInfo1.Minor, versionInfo2.Minor },
				{ versionInfo1.QFE, versionInfo2.QFE },
				{ versionInfo1.Build, versionInfo2.Build }
			};
			for (int i = 0; i < array.GetLength(0); i++)
			{
				if (array[i, 0] < array[i, 1])
				{
					return true;
				}
				if (array[i, 0] > array[i, 1])
				{
					return false;
				}
			}
			return false;
		}

		public static bool operator <=(VersionInfo versionInfo1, VersionInfo versionInfo2)
		{
			if (!(versionInfo1 < versionInfo2))
			{
				return versionInfo1 == versionInfo2;
			}
			return true;
		}

		public static bool operator >(VersionInfo versionInfo1, VersionInfo versionInfo2)
		{
			return versionInfo2 < versionInfo1;
		}

		public static bool operator >=(VersionInfo versionInfo1, VersionInfo versionInfo2)
		{
			if (!(versionInfo1 > versionInfo2))
			{
				return versionInfo1 == versionInfo2;
			}
			return true;
		}

		public static int Compare(VersionInfo versionInfo1, VersionInfo versionInfo2)
		{
			if (versionInfo1 < versionInfo2)
			{
				return -1;
			}
			if (versionInfo1 == versionInfo2)
			{
				return 0;
			}
			return 1;
		}

		public static bool TryParse(string versionInfoString, out VersionInfo versionInfo)
		{
			try
			{
				versionInfo = Parse(versionInfoString);
				return true;
			}
			catch (Exception)
			{
				versionInfo = default(VersionInfo);
				return false;
			}
		}

		public static VersionInfo Parse(string versionInfoString)
		{
			if (string.IsNullOrEmpty(versionInfoString))
			{
				throw new ArgumentNullException("versionInfoString is null or empty", (Exception)null);
			}
			ArgumentException ex = new ArgumentException("The version info string must be in the form 'UINT16.UINT16.UINT16.UINT16'", versionInfoString);
			ex.Data.Add("versionInfoString", versionInfoString);
			string[] array = versionInfoString.Split('.');
			if (array == null || array.Length != 4)
			{
				throw ex;
			}
			VersionInfo result = default(VersionInfo);
			if (!ushort.TryParse(array[0], out result.Major))
			{
				throw ex;
			}
			if (!ushort.TryParse(array[1], out result.Minor))
			{
				throw ex;
			}
			if (!ushort.TryParse(array[2], out result.QFE))
			{
				throw ex;
			}
			if (!ushort.TryParse(array[3], out result.Build))
			{
				throw ex;
			}
			return result;
		}
	}
}
