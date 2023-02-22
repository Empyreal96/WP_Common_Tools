using System.Collections.Generic;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon
{
	public interface IMacroResolver
	{
		void BeginLocal();

		void Register(string name, string value);

		void Register(string name, object value, MacroDelegate del);

		void Register(IEnumerable<KeyValuePair<string, Macro>> macros);

		void EndLocal();

		string GetValue(string name);

		string Resolve(string input);

		string Resolve(string input, MacroResolveOptions option);
	}
}
