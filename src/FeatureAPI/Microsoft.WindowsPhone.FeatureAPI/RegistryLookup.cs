using Microsoft.Win32;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class RegistryLookup
	{
		public static string GetValue(string path, string key)
		{
			string empty = string.Empty;
			using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
			{
				using (RegistryKey registryKey2 = registryKey.OpenSubKey(path, false))
				{
					if (registryKey2 != null)
					{
						return registryKey2.GetValue(key) as string;
					}
					return empty;
				}
			}
		}
	}
}
