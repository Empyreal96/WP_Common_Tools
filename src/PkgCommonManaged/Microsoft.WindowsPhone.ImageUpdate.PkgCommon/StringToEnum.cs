using System;
using System.Collections.Generic;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public class StringToEnum<T>
	{
		private Dictionary<string, T> _stringToValue = new Dictionary<string, T>(StringComparer.InvariantCultureIgnoreCase);

		public void Add(T value, string name)
		{
			_stringToValue.Add(name, value);
		}

		public T Parse(string name)
		{
			T value;
			if (!_stringToValue.TryGetValue(name, out value))
			{
				throw new PackageException("Incorrect value '{0}' for type '{1}', possbile values are '{2}'", name, typeof(T).Name, string.Join(",", _stringToValue.Keys));
			}
			return value;
		}
	}
}
