using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.WindowsPhone.MCSF.Offline
{
	internal static class Extensions
	{
		public static XAttribute LocalAttribute<T>(this T source, string localName) where T : XElement
		{
			IEnumerable<XAttribute> enumerable = from a in source.Attributes()
				where a.Name.LocalName == localName
				select a;
			if (enumerable != null && enumerable.Count() > 0)
			{
				return enumerable.First();
			}
			return null;
		}

		public static XElement LocalElement<T>(this T source, string localName) where T : XContainer
		{
			IEnumerable<XElement> enumerable = from e in source.Elements()
				where e.Name.LocalName == localName
				select e;
			if (enumerable != null && enumerable.Count() > 0)
			{
				return enumerable.First();
			}
			return null;
		}

		public static IEnumerable<XElement> LocalElements<T>(this T source, string localName) where T : XContainer
		{
			IEnumerable<XElement> enumerable = from e in source.Elements()
				where e.Name.LocalName == localName
				select e;
			return enumerable ?? new List<XElement>();
		}

		public static uint ParseInt(string intString)
		{
			if (intString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				return uint.Parse(intString.Substring(2), NumberStyles.HexNumber);
			}
			return uint.Parse(intString);
		}

		public static int ParseSignedInt(string intString, int defaultValue)
		{
			if (intString != null)
			{
				return int.Parse(intString);
			}
			return defaultValue;
		}

		public static int ParseSignedInt(string intString)
		{
			if (intString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				return int.Parse(intString.Substring(2), NumberStyles.HexNumber);
			}
			return int.Parse(intString);
		}

		public static long ParseSignedInt64(string intString)
		{
			if (intString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				return long.Parse(intString.Substring(2), NumberStyles.HexNumber);
			}
			return long.Parse(intString);
		}
	}
}
