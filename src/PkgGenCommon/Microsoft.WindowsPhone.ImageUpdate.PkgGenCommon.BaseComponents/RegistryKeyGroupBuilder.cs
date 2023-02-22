using System.Collections.Generic;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents
{
	public sealed class RegistryKeyGroupBuilder : FilterGroupBuilder<RegGroup, RegistryKeyGroupBuilder>
	{
		private List<RegistryKeyBuilder> regKeys;

		public RegistryKeyGroupBuilder()
		{
			regKeys = new List<RegistryKeyBuilder>();
		}

		public RegistryKeyBuilder AddRegistryKey(string keyName, params object[] args)
		{
			return AddRegistryKey(string.Format(keyName, args));
		}

		public RegistryKeyBuilder AddRegistryKey(string keyName)
		{
			RegistryKeyBuilder registryKeyBuilder = new RegistryKeyBuilder(keyName);
			regKeys.Add(registryKeyBuilder);
			return registryKeyBuilder;
		}

		public override RegGroup ToPkgObject()
		{
			filterGroup.Keys.Clear();
			regKeys.ForEach(delegate(RegistryKeyBuilder x)
			{
				filterGroup.Keys.Add(x.ToPkgObject());
			});
			return base.ToPkgObject();
		}
	}
}
