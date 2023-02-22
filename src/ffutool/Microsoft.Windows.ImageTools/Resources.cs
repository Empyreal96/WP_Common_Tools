using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.Windows.ImageTools
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (resourceMan == null)
				{
					resourceMan = new ResourceManager("Microsoft.Windows.ImageTools.Properties.Resources", typeof(Resources).Assembly);
				}
				return resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return resourceCulture;
			}
			set
			{
				resourceCulture = value;
			}
		}

		internal static string BOOTING_WIM => ResourceManager.GetString("BOOTING_WIM", resourceCulture);

		internal static string DEVICE_ID => ResourceManager.GetString("DEVICE_ID", resourceCulture);

		internal static string DEVICE_NO => ResourceManager.GetString("DEVICE_NO", resourceCulture);

		internal static string DEVICE_TYPE => ResourceManager.GetString("DEVICE_TYPE", resourceCulture);

		internal static string DEVICES_FOUND => ResourceManager.GetString("DEVICES_FOUND", resourceCulture);

		internal static string ERROR_AT_LEAST_ONE_DEVICE_FAILED => ResourceManager.GetString("ERROR_AT_LEAST_ONE_DEVICE_FAILED", resourceCulture);

		internal static string ERROR_BOOT_WIM => ResourceManager.GetString("ERROR_BOOT_WIM", resourceCulture);

		internal static string ERROR_FFU => ResourceManager.GetString("ERROR_FFU", resourceCulture);

		internal static string ERROR_FILE_NOT_FOUND => ResourceManager.GetString("ERROR_FILE_NOT_FOUND", resourceCulture);

		internal static string ERROR_MORE_THAN_ONE_DEVICE => ResourceManager.GetString("ERROR_MORE_THAN_ONE_DEVICE", resourceCulture);

		internal static string ERROR_NO_PLATFORM_ID => ResourceManager.GetString("ERROR_NO_PLATFORM_ID", resourceCulture);

		internal static string ERROR_RESET_BOOT_MODE => ResourceManager.GetString("ERROR_RESET_BOOT_MODE", resourceCulture);

		internal static string ERROR_RESET_MASS_STORAGE_MODE => ResourceManager.GetString("ERROR_RESET_MASS_STORAGE_MODE", resourceCulture);

		internal static string ERROR_SKIP_TRANSFER => ResourceManager.GetString("ERROR_SKIP_TRANSFER", resourceCulture);

		internal static string ERROR_TIMED_OUT => ResourceManager.GetString("ERROR_TIMED_OUT", resourceCulture);

		internal static string ERROR_UNEXPECTED_DEVICESTATUS => ResourceManager.GetString("ERROR_UNEXPECTED_DEVICESTATUS", resourceCulture);

		internal static string ERROR_WIM_BOOT => ResourceManager.GetString("ERROR_WIM_BOOT", resourceCulture);

		internal static string ERRORS => ResourceManager.GetString("ERRORS", resourceCulture);

		internal static string FORCE_OPTION_DEPRECATED => ResourceManager.GetString("FORCE_OPTION_DEPRECATED", resourceCulture);

		internal static string FORMAT_SPEED => ResourceManager.GetString("FORMAT_SPEED", resourceCulture);

		internal static string ID => ResourceManager.GetString("ID", resourceCulture);

		internal static string LOGGING_SIMPLEIO_TO_ETL => ResourceManager.GetString("LOGGING_SIMPLEIO_TO_ETL", resourceCulture);

		internal static string LOGGING_UFP_TO_LOG => ResourceManager.GetString("LOGGING_UFP_TO_LOG", resourceCulture);

		internal static string LOGS_PATH => ResourceManager.GetString("LOGS_PATH", resourceCulture);

		internal static string NAME => ResourceManager.GetString("NAME", resourceCulture);

		internal static string NO_CONNECTED_DEVICES => ResourceManager.GetString("NO_CONNECTED_DEVICES", resourceCulture);

		internal static string REMOVE_PLATFORM_ID => ResourceManager.GetString("REMOVE_PLATFORM_ID", resourceCulture);

		internal static string RESET_BOOT_MODE => ResourceManager.GetString("RESET_BOOT_MODE", resourceCulture);

		internal static string RESET_MASS_STORAGE_MODE => ResourceManager.GetString("RESET_MASS_STORAGE_MODE", resourceCulture);

		internal static string SERIAL_NO => ResourceManager.GetString("SERIAL_NO", resourceCulture);

		internal static string SERIAL_NO_FORMAT => ResourceManager.GetString("SERIAL_NO_FORMAT", resourceCulture);

		internal static string STATUS_BOOTING_TO_WIM => ResourceManager.GetString("STATUS_BOOTING_TO_WIM", resourceCulture);

		internal static string STATUS_CONNECTED => ResourceManager.GetString("STATUS_CONNECTED", resourceCulture);

		internal static string STATUS_DONE => ResourceManager.GetString("STATUS_DONE", resourceCulture);

		internal static string STATUS_ERROR => ResourceManager.GetString("STATUS_ERROR", resourceCulture);

		internal static string STATUS_FLASHING => ResourceManager.GetString("STATUS_FLASHING", resourceCulture);

		internal static string STATUS_LOGS => ResourceManager.GetString("STATUS_LOGS", resourceCulture);

		internal static string STATUS_SKIPPED => ResourceManager.GetString("STATUS_SKIPPED", resourceCulture);

		internal static string STATUS_SKIPPING => ResourceManager.GetString("STATUS_SKIPPING", resourceCulture);

		internal static string STATUS_TRANSFER_WIM => ResourceManager.GetString("STATUS_TRANSFER_WIM", resourceCulture);

		internal static string TRANSFER_STATISTICS => ResourceManager.GetString("TRANSFER_STATISTICS", resourceCulture);

		internal static string USAGE => ResourceManager.GetString("USAGE", resourceCulture);

		internal static string WIM_TRANSFER_RATE => ResourceManager.GetString("WIM_TRANSFER_RATE", resourceCulture);

		internal Resources()
		{
		}
	}
}
