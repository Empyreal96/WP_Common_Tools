using System.Collections.Generic;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents
{
	public sealed class OSComponentBuilder : OSComponentBuilder<OSComponentPkgObject, OSComponentBuilder>
	{
	}
	public abstract class OSComponentBuilder<T, V> : PkgObjectBuilder<T, V> where T : OSComponentPkgObject, new() where V : OSComponentBuilder<T, V>
	{
		private List<FileGroupBuilder> fileGroups;

		private List<RegistryKeyGroupBuilder> registryGroups;

		private List<string> registryImports;

		internal OSComponentBuilder()
		{
			fileGroups = new List<FileGroupBuilder>();
			registryGroups = new List<RegistryKeyGroupBuilder>();
			registryImports = new List<string>();
		}

		public FileGroupBuilder AddFileGroup()
		{
			FileGroupBuilder fileGroupBuilder = new FileGroupBuilder();
			fileGroups.Add(fileGroupBuilder);
			return fileGroupBuilder;
		}

		public V AddRegistryImport(string source)
		{
			registryImports.Add(source);
			return (V)this;
		}

		public RegistryKeyGroupBuilder AddRegistryGroup()
		{
			RegistryKeyGroupBuilder registryKeyGroupBuilder = new RegistryKeyGroupBuilder();
			registryGroups.Add(registryKeyGroupBuilder);
			return registryKeyGroupBuilder;
		}

		public override T ToPkgObject()
		{
			RegisterMacro("runtime.default", "$(runtime.system32)");
			RegisterMacro("env.default", "$(env.system32)");
			pkgObject.FileGroups.Clear();
			pkgObject.KeyGroups.Clear();
			pkgObject.RegImports.Clear();
			fileGroups.ForEach(delegate(FileGroupBuilder file)
			{
				pkgObject.FileGroups.Add(file.ToPkgObject());
			});
			registryGroups.ForEach(delegate(RegistryKeyGroupBuilder regKey)
			{
				pkgObject.KeyGroups.Add(regKey.ToPkgObject());
			});
			registryImports.ForEach(delegate(string import)
			{
				pkgObject.RegImports.Add(new RegImport(import));
			});
			return base.ToPkgObject();
		}
	}
}
