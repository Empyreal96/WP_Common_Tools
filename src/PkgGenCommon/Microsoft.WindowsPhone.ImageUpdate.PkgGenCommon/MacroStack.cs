using System.Collections.Generic;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon
{
	public class MacroStack
	{
		protected Stack<Dictionary<string, Macro>> _dictionaries = new Stack<Dictionary<string, Macro>>();

		public string GetValue(string name)
		{
			if (name == null)
			{
				throw new PkgGenException("null for parameter name of MacroResolver.GetValue");
			}
			Macro value = null;
			using (Stack<Dictionary<string, Macro>>.Enumerator enumerator = _dictionaries.GetEnumerator())
			{
				while (enumerator.MoveNext() && !enumerator.Current.TryGetValue(name, out value))
				{
				}
			}
			return value?.StringValue;
		}

		public bool RemoveName(string name)
		{
			if (name == null)
			{
				throw new PkgGenException("null for parameter name of MacroResolver.RemoveName");
			}
			foreach (Dictionary<string, Macro> dictionary in _dictionaries)
			{
				if (dictionary.Remove(name))
				{
					return true;
				}
			}
			return false;
		}
	}
}
