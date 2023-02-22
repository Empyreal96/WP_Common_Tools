using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public class RgaBuilder
	{
		private class KeyValuePairComparer : IEqualityComparer<KeyValuePair<string, string>>
		{
			public bool Equals(KeyValuePair<string, string> x, KeyValuePair<string, string> y)
			{
				if (x.Key.Equals(y.Key, StringComparison.InvariantCultureIgnoreCase))
				{
					return x.Value.Equals(y.Value, StringComparison.InvariantCultureIgnoreCase);
				}
				return false;
			}

			public int GetHashCode(KeyValuePair<string, string> obj)
			{
				return obj.Key.GetHashCode() ^ obj.Value.GetHashCode();
			}
		}

		private Dictionary<KeyValuePair<string, string>, List<string>> _rgaValues = new Dictionary<KeyValuePair<string, string>, List<string>>(new KeyValuePairComparer());

		public bool HasContent => _rgaValues.Count > 0;

		public void AddRgaValue(string keyName, string valueName, params string[] values)
		{
			KeyValuePair<string, string> key = new KeyValuePair<string, string>(keyName, valueName);
			List<string> value = null;
			if (!_rgaValues.TryGetValue(key, out value))
			{
				value = new List<string>();
				_rgaValues.Add(key, value);
			}
			value.AddRange(values);
		}

		public void Save(string outputFile)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Windows Registry Editor Version 5.00");
			foreach (IGrouping<string, KeyValuePair<KeyValuePair<string, string>, List<string>>> item in from x in _rgaValues
				group x by x.Key.Key)
			{
				stringBuilder.AppendFormat("[{0}]", item.Key);
				stringBuilder.AppendLine();
				foreach (KeyValuePair<KeyValuePair<string, string>, List<string>> item2 in item)
				{
					RegUtil.RegOutput(stringBuilder, item2.Key.Value, item2.Value);
				}
				stringBuilder.AppendLine();
			}
			LongPathFile.WriteAllText(outputFile, stringBuilder.ToString(), Encoding.Unicode);
		}
	}
}
