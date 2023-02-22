using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace FFUComponents
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
					resourceMan = new ResourceManager("FFUComponents.Properties.Resources", typeof(Resources).Assembly);
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

		internal static byte[] bootsdi => (byte[])ResourceManager.GetObject("bootsdi", resourceCulture);

		internal static string ERROR_ACQUIRE_MUTEX => ResourceManager.GetString("ERROR_ACQUIRE_MUTEX", resourceCulture);

		internal static string ERROR_ALREADY_RECEIVED_DATA => ResourceManager.GetString("ERROR_ALREADY_RECEIVED_DATA", resourceCulture);

		internal static string ERROR_BINDHANDLE => ResourceManager.GetString("ERROR_BINDHANDLE", resourceCulture);

		internal static string ERROR_CALLBACK_TIMEOUT => ResourceManager.GetString("ERROR_CALLBACK_TIMEOUT", resourceCulture);

		internal static string ERROR_CM_GET_DEVICE_ID => ResourceManager.GetString("ERROR_CM_GET_DEVICE_ID", resourceCulture);

		internal static string ERROR_CM_GET_DEVICE_ID_SIZE => ResourceManager.GetString("ERROR_CM_GET_DEVICE_ID_SIZE", resourceCulture);

		internal static string ERROR_CM_GET_PARENT => ResourceManager.GetString("ERROR_CM_GET_PARENT", resourceCulture);

		internal static string ERROR_DEVICE_IO_CONTROL => ResourceManager.GetString("ERROR_DEVICE_IO_CONTROL", resourceCulture);

		internal static string ERROR_FFUMANAGER_NOT_STARTED => ResourceManager.GetString("ERROR_FFUMANAGER_NOT_STARTED", resourceCulture);

		internal static string ERROR_FILE_CLOSED => ResourceManager.GetString("ERROR_FILE_CLOSED", resourceCulture);

		internal static string ERROR_FLASH => ResourceManager.GetString("ERROR_FLASH", resourceCulture);

		internal static string ERROR_INVALID_DATA => ResourceManager.GetString("ERROR_INVALID_DATA", resourceCulture);

		internal static string ERROR_INVALID_DEVICE_PARAMS => ResourceManager.GetString("ERROR_INVALID_DEVICE_PARAMS", resourceCulture);

		internal static string ERROR_INVALID_ENDPOINT_TYPE => ResourceManager.GetString("ERROR_INVALID_ENDPOINT_TYPE", resourceCulture);

		internal static string ERROR_INVALID_HANDLE => ResourceManager.GetString("ERROR_INVALID_HANDLE", resourceCulture);

		internal static string ERROR_MULTIPE_DISCONNECT_NOTIFICATIONS => ResourceManager.GetString("ERROR_MULTIPE_DISCONNECT_NOTIFICATIONS", resourceCulture);

		internal static string ERROR_NULL_OR_EMPTY_STRING => ResourceManager.GetString("ERROR_NULL_OR_EMPTY_STRING", resourceCulture);

		internal static string ERROR_RECONNECT_TIMEOUT => ResourceManager.GetString("ERROR_RECONNECT_TIMEOUT", resourceCulture);

		internal static string ERROR_RESULT_ALREADY_SET => ResourceManager.GetString("ERROR_RESULT_ALREADY_SET", resourceCulture);

		internal static string ERROR_RESUME_UNEXPECTED_POSITION => ResourceManager.GetString("ERROR_RESUME_UNEXPECTED_POSITION", resourceCulture);

		internal static string ERROR_SETUP_DI_ENUM_DEVICE_INFO => ResourceManager.GetString("ERROR_SETUP_DI_ENUM_DEVICE_INFO", resourceCulture);

		internal static string ERROR_SETUP_DI_ENUM_DEVICE_INTERFACES => ResourceManager.GetString("ERROR_SETUP_DI_ENUM_DEVICE_INTERFACES", resourceCulture);

		internal static string ERROR_SETUP_DI_GET_DEVICE_INTERFACE_DETAIL_W => ResourceManager.GetString("ERROR_SETUP_DI_GET_DEVICE_INTERFACE_DETAIL_W", resourceCulture);

		internal static string ERROR_SETUP_DI_GET_DEVICE_PROPERTY => ResourceManager.GetString("ERROR_SETUP_DI_GET_DEVICE_PROPERTY", resourceCulture);

		internal static string ERROR_UNABLE_TO_COMPLETE_WRITE => ResourceManager.GetString("ERROR_UNABLE_TO_COMPLETE_WRITE", resourceCulture);

		internal static string ERROR_UNABLE_TO_READ_REGION => ResourceManager.GetString("ERROR_UNABLE_TO_READ_REGION", resourceCulture);

		internal static string ERROR_UNRECOGNIZED_COMMAND => ResourceManager.GetString("ERROR_UNRECOGNIZED_COMMAND", resourceCulture);

		internal static string ERROR_USB_TRANSFER => ResourceManager.GetString("ERROR_USB_TRANSFER", resourceCulture);

		internal static string ERROR_WIMBOOT => ResourceManager.GetString("ERROR_WIMBOOT", resourceCulture);

		internal static string ERROR_WINUSB_INITIALIZATION => ResourceManager.GetString("ERROR_WINUSB_INITIALIZATION", resourceCulture);

		internal static string ERROR_WINUSB_QUERY_INTERFACE_SETTING => ResourceManager.GetString("ERROR_WINUSB_QUERY_INTERFACE_SETTING", resourceCulture);

		internal static string ERROR_WINUSB_QUERY_PIPE_INFORMATION => ResourceManager.GetString("ERROR_WINUSB_QUERY_PIPE_INFORMATION", resourceCulture);

		internal static string ERROR_WINUSB_SET_PIPE_POLICY => ResourceManager.GetString("ERROR_WINUSB_SET_PIPE_POLICY", resourceCulture);

		internal static string MODULE_VERSION => ResourceManager.GetString("MODULE_VERSION", resourceCulture);

		internal Resources()
		{
		}
	}
}
