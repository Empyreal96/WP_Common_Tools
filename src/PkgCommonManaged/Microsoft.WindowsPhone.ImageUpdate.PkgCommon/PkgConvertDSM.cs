using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public static class PkgConvertDSM
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
			CONVERTDSM_PARAMETERS_FLAGS_DO_NTSIGN = 0x400,
			CONVERTDSM_PARAMETERS_FLAGS_OUTPUT_NEXT_TO_INPUT = 0x800,
			CONVERTDSM_PARAMETERS_FLAGS_SIGN_TESTONLY = 0x1000,
			CONVERTDSM_PARAMETERS_MAX_FLAGS = 0x1000
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

		private const string CONVERTDSMDLL_DLL = "ConvertDSMDLL.dll";

		public const int S_OK = 0;

		[DllImport("ConvertDSMDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, SetLastError = true)]
		private static extern void ConvertDSM_LogTo([MarshalAs(UnmanagedType.FunctionPtr)] LogUtil.InteropLogString ErrorMsgHandler, [MarshalAs(UnmanagedType.FunctionPtr)] LogUtil.InteropLogString WarningMsgHandler, [MarshalAs(UnmanagedType.FunctionPtr)] LogUtil.InteropLogString InfoMsgHandler, [MarshalAs(UnmanagedType.FunctionPtr)] LogUtil.InteropLogString DebugMsgHandler);

		public static bool FAILED(int hr)
		{
			return hr < 0;
		}

		public static bool SUCCEEDED(int hr)
		{
			return hr >= 0;
		}

		private static CONVERTDSM_PARAMETERS CreateParams(CONVERTDSM_PARAMETERS_FLAGS Flags, string outputDir)
		{
			CONVERTDSM_PARAMETERS cONVERTDSM_PARAMETERS = default(CONVERTDSM_PARAMETERS);
			cONVERTDSM_PARAMETERS.cbSize = (uint)Marshal.SizeOf((object)cONVERTDSM_PARAMETERS);
			cONVERTDSM_PARAMETERS.dwFlags = (uint)Flags;
			cONVERTDSM_PARAMETERS.pszOutDir = outputDir;
			cONVERTDSM_PARAMETERS.pszFullOutName = string.Empty;
			cONVERTDSM_PARAMETERS.cchFullOutNameSize = 0u;
			cONVERTDSM_PARAMETERS.cchRequired = 0u;
			return cONVERTDSM_PARAMETERS;
		}

		[DllImport("ConvertDSMDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int ConvertExpandedPackages(string DsmFolder, string PackageRoot, ref CONVERTDSM_PARAMETERS pParams);

		public static void ConvertExpandedPackage(CONVERTDSM_PARAMETERS_FLAGS Flags, string expandedPackage, string outputDir)
		{
			CONVERTDSM_PARAMETERS pParams = CreateParams(Flags, outputDir);
			int num = 0;
			if (FAILED(num = ConvertExpandedPackages(expandedPackage, expandedPackage, ref pParams)))
			{
				throw new PackageException("ConvertDSM failed with error code: " + string.Format("{0} (0x{0:X})", num));
			}
		}

		[DllImport("ConvertDSMDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int ConvertListOfSPKGs(string[] packagesList, uint cPackageList, ref CONVERTDSM_PARAMETERS pParams);

		public static List<string> ConvertPackagesToCBS(List<string> packageList, string outputDir)
		{
			return ConvertPackagesToCBS(CONVERTDSM_PARAMETERS_FLAGS.CONVERTDSM_PARAMETERS_FLAGS_MAKE_CAB | CONVERTDSM_PARAMETERS_FLAGS.CONVERTDSM_PARAMETERS_FLAGS_SIGN_OUTPUT | CONVERTDSM_PARAMETERS_FLAGS.CONVERTDSM_PARAMETERS_FLAGS_SKIP_POLICY | CONVERTDSM_PARAMETERS_FLAGS.CONVERTDSM_PARAMETERS_FLAGS_USE_FILENAME_AS_NAME, packageList, outputDir);
		}

		public static List<string> ConvertPackagesToCBS(CONVERTDSM_PARAMETERS_FLAGS Flags, List<string> packageList, string outputDir)
		{
			CONVERTDSM_PARAMETERS pParams = CreateParams(Flags, outputDir);
			int num = 0;
			if (FAILED(num = ConvertListOfSPKGs(packageList.ToArray(), (uint)packageList.Count, ref pParams)))
			{
				throw new PackageException("ConvertDSM failed with error code: " + string.Format("{0} (0x{0:X})", num));
			}
			if (string.IsNullOrEmpty(outputDir))
			{
				return packageList.Select((string pkg) => Path.ChangeExtension(pkg, PkgConstants.c_strCBSPackageExtension)).ToList();
			}
			return Directory.GetFiles(outputDir).ToList();
		}
	}
}
