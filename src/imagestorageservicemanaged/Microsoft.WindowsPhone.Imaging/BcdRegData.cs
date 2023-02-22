using System;
using System.Collections.Generic;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BcdRegData
	{
		private Dictionary<string, List<BcdRegValue>> _regKeys = new Dictionary<string, List<BcdRegValue>>(StringComparer.OrdinalIgnoreCase);

		public void AddRegKey(string regKey)
		{
			if (!_regKeys.ContainsKey(regKey))
			{
				_regKeys.Add(regKey, new List<BcdRegValue>());
			}
		}

		public Dictionary<string, List<BcdRegValue>> RegKeys()
		{
			return _regKeys;
		}

		public void AddRegValue(string regKey, string name, string value, string type)
		{
			if (!_regKeys.ContainsKey(regKey))
			{
				AddRegKey(regKey);
			}
			BcdRegValue item = new BcdRegValue(name, value, type);
			_regKeys[regKey].Add(item);
		}
	}
}
