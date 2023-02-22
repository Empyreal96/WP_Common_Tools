using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents
{
	public sealed class ComServerBuilder : OSComponentBuilder<ComPkgObject, ComServerBuilder>
	{
		public class ComDllBuilder : FileBuilder<ComDll, ComDllBuilder>
		{
			public ComDllBuilder(XElement element)
				: base(element)
			{
			}

			public ComDllBuilder(string source, string destinationDir)
				: base(source, destinationDir)
			{
			}
		}

		public abstract class ComBaseBuilder<T, V> : PkgObjectBuilder<T, V> where T : ComBase, new() where V : ComBaseBuilder<T, V>
		{
			private List<RegistryKeyBuilder> regKeys;

			internal ComBaseBuilder()
			{
				regKeys = new List<RegistryKeyBuilder>();
			}

			public V SetTypeLib(string value)
			{
				pkgObject.TypeLib = value;
				return this as V;
			}

			public V SetVersion(string value)
			{
				pkgObject.Version = value;
				return this as V;
			}

			public RegistryKeyBuilder AddRegistryKey(string keyName)
			{
				RegistryKeyBuilder registryKeyBuilder = new RegistryKeyBuilder(keyName);
				regKeys.Add(registryKeyBuilder);
				return registryKeyBuilder;
			}

			public override T ToPkgObject()
			{
				regKeys.ForEach(delegate(RegistryKeyBuilder x)
				{
					pkgObject.RegKeys.Add(x.ToPkgObject());
				});
				return base.ToPkgObject();
			}
		}

		public sealed class ComClassBuilder : ComBaseBuilder<ComClass, ComClassBuilder>
		{
			internal ComClassBuilder(XElement element)
			{
				pkgObject = element.FromXElement<ComClass>();
			}

			internal ComClassBuilder(Guid classId)
			{
				pkgObject = new ComClass();
				pkgObject.Id = classId.ToString();
			}

			public ComClassBuilder SetThreadingModel(ThreadingModel model)
			{
				pkgObject.ThreadingModel = Enum.GetName(typeof(ThreadingModel), model);
				return this;
			}

			public ComClassBuilder SetProgId(string value)
			{
				pkgObject.ProgId = value;
				return this;
			}

			public ComClassBuilder SetVersionIndependentProgId(string value)
			{
				pkgObject.VersionIndependentProgId = value;
				return this;
			}

			public ComClassBuilder SetDescription(string value)
			{
				pkgObject.Description = value;
				return this;
			}

			public ComClassBuilder SetDefaultIcon(string value)
			{
				pkgObject.DefaultIcon = value;
				return this;
			}

			public ComClassBuilder SetAppId(string value)
			{
				pkgObject.AppId = value;
				return this;
			}

			public ComClassBuilder SetSkipInProcServer32(bool flag)
			{
				pkgObject.SkipInProcServer32 = flag;
				return this;
			}

			public override ComClass ToPkgObject()
			{
				RegisterMacro("hkcr.clsid", "$(hkcr.root)\\CLSID\\" + pkgObject.Id);
				return base.ToPkgObject();
			}
		}

		public sealed class ComInterfaceBuilder : ComBaseBuilder<ComInterface, ComInterfaceBuilder>
		{
			internal ComInterfaceBuilder(XElement element)
			{
				pkgObject = element.FromXElement<ComInterface>();
			}

			internal ComInterfaceBuilder(Guid classId)
			{
				pkgObject = new ComInterface();
				pkgObject.Id = classId.ToString();
			}

			public ComInterfaceBuilder SetName(string name)
			{
				pkgObject.Name = name;
				return this;
			}

			public ComInterfaceBuilder SetProxyStubClsId(Guid clsId)
			{
				pkgObject.ProxyStubClsId = clsId.ToString();
				return this;
			}

			public ComInterfaceBuilder SetProxyStubClsId32(Guid clsId)
			{
				pkgObject.ProxyStubClsId32 = clsId.ToString();
				return this;
			}

			public ComInterfaceBuilder SetNumMethods(int numMethods)
			{
				pkgObject.NumMethods = numMethods.ToString();
				return this;
			}

			public override ComInterface ToPkgObject()
			{
				RegisterMacro("hkcr.iid", "$(hkcr.root)\\Interface\\" + pkgObject.Id);
				return base.ToPkgObject();
			}
		}

		private ComDllBuilder dll;

		private List<ComClassBuilder> classes;

		private List<ComInterfaceBuilder> interfaces;

		public ComServerBuilder()
		{
			dll = null;
			classes = new List<ComClassBuilder>();
			interfaces = new List<ComInterfaceBuilder>();
		}

		public ComDllBuilder SetComDll(XElement file)
		{
			dll = new ComDllBuilder(file);
			return dll;
		}

		public ComDllBuilder SetComDll(string source)
		{
			return SetComDll(source, "$(runtime.default)");
		}

		public ComDllBuilder SetComDll(string source, string destinationDir)
		{
			dll = new ComDllBuilder(source, destinationDir);
			return dll;
		}

		public ComClassBuilder AddClass(XElement element)
		{
			ComClassBuilder comClassBuilder = new ComClassBuilder(element);
			classes.Add(comClassBuilder);
			return comClassBuilder;
		}

		public ComClassBuilder AddClass(Guid classId)
		{
			ComClassBuilder comClassBuilder = new ComClassBuilder(classId);
			classes.Add(comClassBuilder);
			return comClassBuilder;
		}

		public ComInterfaceBuilder AddInterface(XElement element)
		{
			ComInterfaceBuilder comInterfaceBuilder = new ComInterfaceBuilder(element);
			interfaces.Add(comInterfaceBuilder);
			return comInterfaceBuilder;
		}

		public ComInterfaceBuilder AddInterface(Guid interfaceId)
		{
			ComInterfaceBuilder comInterfaceBuilder = new ComInterfaceBuilder(interfaceId);
			interfaces.Add(comInterfaceBuilder);
			return comInterfaceBuilder;
		}

		public override ComPkgObject ToPkgObject()
		{
			pkgObject.ComDll = dll.ToPkgObject();
			pkgObject.Classes.Clear();
			pkgObject.Interfaces.Clear();
			classes.ForEach(delegate(ComClassBuilder x)
			{
				pkgObject.Classes.Add(x.ToPkgObject());
			});
			interfaces.ForEach(delegate(ComInterfaceBuilder x)
			{
				pkgObject.Interfaces.Add(x.ToPkgObject());
			});
			return base.ToPkgObject();
		}
	}
}
