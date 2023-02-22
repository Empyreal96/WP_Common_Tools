using System.Collections.Generic;
using System.Linq;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	public abstract class PkgObjectBuilder<T, V> where T : PkgObject, new() where V : PkgObjectBuilder<T, V>
	{
		private Dictionary<string, Macro> localMacros;

		protected T pkgObject;

		public PkgObjectBuilder()
		{
			localMacros = new Dictionary<string, Macro>();
			pkgObject = new T();
		}

		public V RegisterMacro(string name, string value)
		{
			if (!localMacros.Keys.Contains(name))
			{
				localMacros.Add(name, new Macro(name, value));
			}
			return (V)this;
		}

		public V RegisterMacro(string name, object value, MacroDelegate del)
		{
			if (!localMacros.Keys.Contains(name))
			{
				localMacros.Add(name, new Macro(name, value, del));
			}
			return (V)this;
		}

		public virtual T ToPkgObject()
		{
			pkgObject.LocalMacros = new MacroTable();
			pkgObject.LocalMacros.Macros.AddRange(localMacros.Values);
			return pkgObject;
		}
	}
}
