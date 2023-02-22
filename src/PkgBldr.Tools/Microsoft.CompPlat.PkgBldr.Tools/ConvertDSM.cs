using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public class ConvertDSM
	{
		[Flags]
		public enum CONVERTDSM_PARAMETERS_FLAGS
		{
			CONVERTDSM_PARAMETERS_FLAGS_NONE = 0,
			CONVERTDSM_PARAMETERS_FLAGS_MAKE_CAB = 1,
			CONVERTDSM_PARAMETERS_FLAGS_SIGN_OUTPUT = 2,
			CONVERTDSM_PARAMETERS_FLAGS_WOW_MAP_ARCH = 4,
			CONVERTDSM_PARAMETERS_FLAGS_SKIP_POLICY = 8,
			CONVERTDSM_PARAMETERS_FLAGS_USE_FILENAME_AS_NAME = 0x10,
			CONVERTDSM_PARAMETERS_FLAGS_SINGLE_COMPONENT = 0x20,
			CONVERTDSM_PARAMETERS_FLAGS_METADATA_ONLY = 0x40,
			CONVERTDSM_PARAMETERS_FLAGS_LEAVE_SPKG_METADATA = 0x80,
			CONVERTDSM_PARAMETERS_FLAGS_CREATE_RECALL = 0x100,
			CONVERTDSM_PARAMETERS_FLAGS_INCLUDE_CATALOG = 0x200,
			CONVERTDSM_PARAMETERS_MAX_FLAGS = 0x200
		}

		public struct CONVERTDSM_PARAMETERS
		{
			public uint cbSize;

			public uint dwFlags;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszOutDir;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszFullOutName;

			public uint cchFullOutNameSize;

			public uint cchRequired;
		}

		public const string CONVERTDSMDLL_DLL = "ConvertDSMDLL.dll";

		public const int S_OK = 0;

		public static bool FAILED(int hr)
		{
			return hr < 0;
		}

		public static bool SUCCEEDED(int hr)
		{
			return hr >= 0;
		}

		[DllImport("ConvertDSMDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int ConvertOneSPKG(string packages, ref CONVERTDSM_PARAMETERS pParams);

		public static void RunDsmConverter(string input, string output, bool wow, bool ignoreConvertDsmError, uint flags)
		{
			CONVERTDSM_PARAMETERS pParams = default(CONVERTDSM_PARAMETERS);
			pParams.cbSize = (uint)Marshal.SizeOf((object)pParams);
			pParams.dwFlags = flags;
			pParams.pszOutDir = output;
			pParams.pszFullOutName = string.Empty;
			pParams.cchFullOutNameSize = 0u;
			pParams.cchRequired = 0u;
			if (wow)
			{
				pParams.dwFlags |= 4u;
			}
			int num = 0;
			if (FAILED(num = ConvertOneSPKG(input, ref pParams)) && !ignoreConvertDsmError)
			{
				throw new IUException("ConvertDSM failed with error code: " + string.Format(CultureInfo.InvariantCulture, "{0} (0x{0:X})", new object[1] { num }));
			}
		}
	}
}
