using System;
using System.Configuration;

namespace Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary.Factories
{
	public class ClassFactory<I, P> where P : I, new()
	{
		private static Type publishedInterface;

		private static Type providerType;

		private static object providerLock;

		private static bool haveCheckedConfiguration;

		private static string ProviderTypeNameKey { get; set; }

		static ClassFactory()
		{
			publishedInterface = typeof(I);
			providerType = typeof(P);
			providerLock = new object();
			haveCheckedConfiguration = false;
			if (!publishedInterface.IsInterface)
			{
				throw new ArgumentException($"Type {typeof(I).Name} must be an interface");
			}
			ProviderTypeNameKey = publishedInterface.Name + "ProviderType";
		}

		public static Type GetProviderType()
		{
			return providerType;
		}

		public static void SetProviderType(Type value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("Provider Type must not be null");
			}
			if (!publishedInterface.IsAssignableFrom(value))
			{
				throw new ArgumentException($"Type {value.FullName} does not implement {publishedInterface.FullName}.");
			}
			lock (providerLock)
			{
				providerType = value;
			}
		}

		public static void CreateFactoryConfigFile(string exeFileName, Type providerType)
		{
			Configuration configuration = ConfigurationManager.OpenExeConfiguration(exeFileName);
			if (configuration.AppSettings.Settings[ProviderTypeNameKey] == null)
			{
				configuration.AppSettings.Settings.Add(ProviderTypeNameKey, providerType.AssemblyQualifiedName);
			}
			else
			{
				configuration.AppSettings.Settings[ProviderTypeNameKey].Value = providerType.AssemblyQualifiedName;
			}
			configuration.Save();
		}

		public static I Create()
		{
			if (!haveCheckedConfiguration)
			{
				haveCheckedConfiguration = true;
				string text = ConfigurationManager.AppSettings[ProviderTypeNameKey];
				if (!string.IsNullOrEmpty(text))
				{
					Type type = Type.GetType(text, true);
					SetProviderType(type);
				}
			}
			return (I)Activator.CreateInstance(providerType, true);
		}
	}
}
