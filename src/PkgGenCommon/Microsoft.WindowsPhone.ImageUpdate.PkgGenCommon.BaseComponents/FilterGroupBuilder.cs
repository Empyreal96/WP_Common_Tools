using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents
{
	public abstract class FilterGroupBuilder<T, V> where T : FilterGroup, new() where V : FilterGroupBuilder<T, V>
	{
		protected T filterGroup;

		public FilterGroupBuilder()
		{
			filterGroup = new T();
		}

		public V SetCpuId(string value)
		{
			return SetCpuId(CpuIdParser.Parse(value));
		}

		public V SetCpuId(CpuId value)
		{
			filterGroup.CpuFilter = value;
			return (V)this;
		}

		public V SetResolution(string value)
		{
			filterGroup.Resolution = value;
			return (V)this;
		}

		public V SetLanguage(string value)
		{
			filterGroup.Language = value;
			return (V)this;
		}

		public virtual T ToPkgObject()
		{
			return filterGroup;
		}
	}
}
