using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents
{
	public sealed class ServiceBuilder : OSComponentBuilder<ServicePkgObject, ServiceBuilder>
	{
		public class FailureActionsBuilder
		{
			public const int INFINITE_RESET_PERIOD = -1;

			private FailureActionsPkgObject pkgObject;

			public FailureActionsBuilder()
			{
				pkgObject = new FailureActionsPkgObject();
			}

			public FailureActionsBuilder SetResetPeriod(string period)
			{
				int result;
				if (!period.Equals("INFINITE", StringComparison.InvariantCultureIgnoreCase) && !int.TryParse(period, out result))
				{
					throw new ArgumentException("Period must be a number or 'INFINITE'");
				}
				pkgObject.ResetPeriod = period;
				return this;
			}

			public FailureActionsBuilder SetResetPeriod(int period)
			{
				pkgObject.ResetPeriod = ((period < 0) ? "INFINITE" : period.ToString());
				return this;
			}

			public FailureActionsBuilder SetRebootMessage(string value)
			{
				pkgObject.RebootMsg = value;
				return this;
			}

			public FailureActionsBuilder SetCommand(string value)
			{
				pkgObject.Command = value;
				return this;
			}

			public FailureActionsBuilder AddFailureAction(XNode element)
			{
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(FailureAction));
				using (XmlReader xmlReader = element.CreateReader())
				{
					pkgObject.Actions.Add((FailureAction)xmlSerializer.Deserialize(xmlReader));
					return this;
				}
			}

			public FailureActionsBuilder AddFailureAction(FailureActionType type, uint delay)
			{
				pkgObject.Actions.Add(new FailureAction(type, delay));
				return this;
			}

			public FailureActionsPkgObject ToFailureActions()
			{
				return pkgObject;
			}
		}

		public class ServiceExeEntryBuilder : FileBuilder<SvcExe, ServiceExeEntryBuilder>
		{
			public ServiceExeEntryBuilder(XElement element)
				: base(element)
			{
			}

			public ServiceExeEntryBuilder(string source, string destinationDir)
				: base(source, destinationDir)
			{
			}
		}

		public class ServiceDllEntryBuilder : FileBuilder<SvcDll, ServiceDllEntryBuilder>
		{
			public ServiceDllEntryBuilder(XElement element)
				: base(element)
			{
			}

			public ServiceDllEntryBuilder(string source, string destinationDir)
				: base(source, destinationDir)
			{
			}

			public ServiceDllEntryBuilder SetServiceManifest(string value)
			{
				pkgObject.ServiceManifest = value;
				return this;
			}

			public ServiceDllEntryBuilder SetServiceMain(string value)
			{
				pkgObject.ServiceName = value;
				return this;
			}

			public ServiceDllEntryBuilder SetUnloadOnStop(bool value)
			{
				pkgObject.UnloadOnStop = value;
				return this;
			}

			public ServiceDllEntryBuilder SetHostExe(string value)
			{
				pkgObject.HostExe = value;
				return this;
			}
		}

		private object serviceEntry;

		public FailureActionsBuilder FailureActions { get; private set; }

		public ServiceBuilder(string name)
		{
			FailureActions = new FailureActionsBuilder();
			pkgObject = new ServicePkgObject();
			pkgObject.Name = name;
			serviceEntry = null;
		}

		public ServiceBuilder SetDisplayName(string value)
		{
			pkgObject.DisplayName = value;
			return this;
		}

		public ServiceBuilder SetDescription(string value)
		{
			pkgObject.Description = value;
			return this;
		}

		public ServiceBuilder SetGroup(string value)
		{
			pkgObject.Group = value;
			return this;
		}

		public ServiceBuilder SetSvcHostGroupName(string value)
		{
			pkgObject.SvcHostGroupName = value;
			return this;
		}

		public ServiceBuilder SetStartMode(string value)
		{
			return SetStartMode((ServiceStartMode)Enum.Parse(typeof(ServiceStartMode), value));
		}

		public ServiceBuilder SetStartMode(ServiceStartMode value)
		{
			pkgObject.StartMode = value;
			return this;
		}

		public ServiceBuilder SetType(string value)
		{
			return SetType((ServiceType)Enum.Parse(typeof(ServiceType), value));
		}

		public ServiceBuilder SetType(ServiceType value)
		{
			pkgObject.SvcType = value;
			return this;
		}

		public ServiceBuilder SetErrorControl(string value)
		{
			return SetErrorControl((ErrorControlOption)Enum.Parse(typeof(ErrorControlOption), value));
		}

		public ServiceBuilder SetErrorControl(ErrorControlOption value)
		{
			pkgObject.ErrorControl = value;
			return this;
		}

		public ServiceBuilder SetDependOnGroup(string value)
		{
			pkgObject.DependOnGroup = value;
			return this;
		}

		public ServiceBuilder SetDependOnService(string value)
		{
			pkgObject.DependOnService = value;
			return this;
		}

		public ServiceBuilder SetRequiredCapabilities(IEnumerable<XElement> requiredCapabilities)
		{
			pkgObject.RequiredCapabilities = new XElement(XName.Get("RequiredCapabilities", "urn:Microsoft.WindowsPhone/PackageSchema.v8.00"), requiredCapabilities);
			return this;
		}

		public ServiceBuilder SetPrivateResources(IEnumerable<XElement> privateResources)
		{
			pkgObject.PrivateResources = new XElement(XName.Get("PrivateResources", "urn:Microsoft.WindowsPhone/PackageSchema.v8.00"), privateResources);
			return this;
		}

		public ServiceDllEntryBuilder AddServiceDll(XElement element)
		{
			if (serviceEntry != null)
			{
				throw new ArgumentException("Service already has an entry point.");
			}
			serviceEntry = new ServiceDllEntryBuilder(element);
			return (ServiceDllEntryBuilder)serviceEntry;
		}

		public ServiceDllEntryBuilder AddServiceDll(string source, string destinationDir)
		{
			if (serviceEntry != null)
			{
				throw new ArgumentException("Service already has an entry point.");
			}
			serviceEntry = new ServiceDllEntryBuilder(source, destinationDir);
			return (ServiceDllEntryBuilder)serviceEntry;
		}

		public ServiceDllEntryBuilder AddServiceDll(string source)
		{
			return AddServiceDll(source, "$(runtime.default)");
		}

		public ServiceExeEntryBuilder AddExecutable(XElement element)
		{
			if (serviceEntry != null)
			{
				throw new ArgumentException("Service already has an entry point.");
			}
			serviceEntry = new ServiceExeEntryBuilder(element);
			return (ServiceExeEntryBuilder)serviceEntry;
		}

		public ServiceExeEntryBuilder AddExecutable(string source, string destinationDir)
		{
			if (serviceEntry != null)
			{
				throw new ArgumentException("Service already has an entry point.");
			}
			serviceEntry = new ServiceExeEntryBuilder(source, destinationDir);
			return (ServiceExeEntryBuilder)serviceEntry;
		}

		public ServiceExeEntryBuilder AddExecutable(string source)
		{
			return AddExecutable(source, "$(runtime.default)");
		}

		public override ServicePkgObject ToPkgObject()
		{
			RegisterMacro("runtime.default", "$(runtime.system32)");
			RegisterMacro("env.default", "$(env.system32)");
			RegisterMacro("hklm.service", "$(hklm.system)\\controlset001\\services\\" + pkgObject.Name);
			if (serviceEntry == null)
			{
				throw new ArgumentException("Service needs to have an entry point.");
			}
			if (serviceEntry is ServiceExeEntryBuilder)
			{
				if (!string.IsNullOrEmpty(pkgObject.SvcHostGroupName))
				{
					throw new ArgumentException("SvcHostGroupName should not be set when using an ExeEntry");
				}
				pkgObject.SvcEntry = ((ServiceExeEntryBuilder)serviceEntry).ToPkgObject();
			}
			else if (serviceEntry is ServiceDllEntryBuilder)
			{
				if (string.IsNullOrEmpty(pkgObject.SvcHostGroupName))
				{
					throw new ArgumentException("SvcHostGroupName should be set when using an DllEntry");
				}
				pkgObject.SvcEntry = ((ServiceDllEntryBuilder)serviceEntry).ToPkgObject();
			}
			pkgObject.FailureActions = FailureActions.ToFailureActions();
			return base.ToPkgObject();
		}
	}
}
