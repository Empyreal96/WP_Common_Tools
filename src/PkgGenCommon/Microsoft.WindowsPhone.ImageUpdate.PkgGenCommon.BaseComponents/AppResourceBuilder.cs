using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents
{
	public sealed class AppResourceBuilder : AppResourceBuilder<AppResourcePkgObject, AppResourceBuilder>
	{
	}
	public abstract class AppResourceBuilder<T, V> : OSComponentBuilder<T, V> where T : AppResourcePkgObject, new() where V : AppResourceBuilder<T, V>
	{
		public AppResourceBuilder()
		{
		}

		public V SetName(string name)
		{
			pkgObject.Name = name;
			return (V)this;
		}

		public V SetSuite(string suite)
		{
			pkgObject.Suite = suite;
			return (V)this;
		}

		public override T ToPkgObject()
		{
			RegisterMacro("runtime.default", "$(runtime.apps)\\" + pkgObject.Name);
			RegisterMacro("env.default", "$(env.apps)\\" + pkgObject.Name);
			return base.ToPkgObject();
		}
	}
}
