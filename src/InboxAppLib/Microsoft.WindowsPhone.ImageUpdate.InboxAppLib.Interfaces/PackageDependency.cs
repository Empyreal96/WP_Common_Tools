using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces
{
	public sealed class PackageDependency : IComparable, IComparer
	{
		public string Name { get; set; }

		public string MinVersion { get; set; }

		public PackageDependency()
		{
			Name = string.Empty;
			MinVersion = string.Empty;
		}

		public PackageDependency(PackageDependency packageDependency)
		{
			Name = packageDependency.Name;
			MinVersion = packageDependency.MinVersion;
		}

		public PackageDependency(string name, string minVersion)
		{
			Name = name;
			MinVersion = minVersion;
		}

		public bool IsValid()
		{
			Regex regex = new Regex("(0|[1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])(\\.(0|[1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])){3}", RegexOptions.Singleline);
			if (!string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(MinVersion))
			{
				return regex.IsMatch(MinVersion);
			}
			return false;
		}

		public bool MeetsVersionRequirements(string version)
		{
			if (string.IsNullOrWhiteSpace(version))
			{
				throw new ArgumentNullException("version", "The version parameter is null!");
			}
			if (!IsValid())
			{
				throw new InvalidDataException("INTERNAL ERROR: The PackageDependency object is not a valid object!");
			}
			if (!version.Contains("."))
			{
				throw new ArgumentException("The passed in version value does not look like a version string!", "version");
			}
			string[] array = MinVersion.Split('.');
			string[] array2 = version.Split('.');
			bool result = true;
			int num = array.Length;
			int num2 = array2.Length;
			int num3 = Math.Min(num, num2);
			int num4 = Math.Max(num, num2);
			if (num < num4)
			{
				StringBuilder stringBuilder = new StringBuilder(MinVersion);
				for (int i = 0; i < num4 - num; i++)
				{
					stringBuilder.Append(".0");
				}
				array = stringBuilder.ToString().Split('.');
				LogUtil.Diagnostic("PackageDependency: Padded the shorter MinVersion from {0} to {1}", MinVersion, string.Join(".", array));
			}
			else if (num2 < num4)
			{
				StringBuilder stringBuilder2 = new StringBuilder(version);
				for (int j = 0; j < num4 - num2; j++)
				{
					stringBuilder2.Append(".0");
				}
				array2 = stringBuilder2.ToString().Split('.');
				LogUtil.Diagnostic("PackageDependency: Padded the shorter version from {0} to {1}", version, string.Join(".", array2));
			}
			num3 = Math.Min(array.Length, array2.Length);
			for (int k = 0; k < num3; k++)
			{
				uint result2 = 0u;
				uint result3 = 0u;
				if (uint.TryParse(array2[k], out result3) && uint.TryParse(array[k], out result2))
				{
					if (result3 != result2)
					{
						result = result3 > result2;
						break;
					}
					continue;
				}
				throw new ArgumentOutOfRangeException(string.Format(CultureInfo.InvariantCulture, "Internal error: {0} and {1} cannot be compared due to non-numeric fields", new object[2] { version, MinVersion }));
			}
			return result;
		}

		public int CompareTo(object obj)
		{
			return Compare(this, obj);
		}

		public int Compare(object obj1, object obj2)
		{
			if ((obj1 is string || obj1 is PackageDependency) && (obj2 is string || obj2 is PackageDependency))
			{
				PackageDependency obj3 = ((obj1 is string) ? new PackageDependency(obj1 as string, "") : (obj1 as PackageDependency));
				return string.Compare(strB: ((obj2 is string) ? new PackageDependency(obj2 as string, "") : (obj2 as PackageDependency)).Name, strA: obj3.Name, comparisonType: StringComparison.OrdinalIgnoreCase);
			}
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "cannot compare objects of type {0} against type {1}", new object[2]
			{
				obj1.GetType(),
				obj2.GetType()
			}));
		}

		public override bool Equals(object obj)
		{
			return Compare(this, obj) == 0;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "(Name)=\"{0}\", (MinVersion)=\"{1}\"", new object[2] { Name, MinVersion });
		}
	}
}
