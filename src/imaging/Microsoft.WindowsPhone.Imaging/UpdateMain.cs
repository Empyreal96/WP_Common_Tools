using System;
using System.Runtime.InteropServices;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	internal sealed class UpdateMain : IDisposable
	{
		internal enum IUPhase
		{
			IUPhase_Staging,
			IUPhase_Commit
		}

		public struct OFFLINE_STORE_CREATION_PARAMETERS
		{
			public UIntPtr cbSize;

			public uint dwFlags;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszHostSystemDrivePath;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszHostWindowsDirectoryPath;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszTargetWindowsDirectoryPath;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszHostRegistryMachineSoftwarePath;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszHostRegistryMachineSystemPath;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszHostRegistryMachineSecurityPath;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszHostRegistryMachineSAMPath;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszHostRegistryMachineComponentsPath;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszHostRegistryUserDotDefaultPath;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszHostRegistryDefaultUserPath;

			public uint ulProcessorArchitecture;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string pszHostRegistryMachineOfflineSchemaPath;
		}

		public const int S_OK = 0;

		private IntPtr UpdateContext = IntPtr.Zero;

		private bool _alreadyDisposed;

		public static bool FAILED(int hr)
		{
			return hr < 0;
		}

		public UpdateMain()
		{
			UpdateContext = CreateUpdateContext();
		}

		~UpdateMain()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool isDisposing)
		{
			if (_alreadyDisposed)
			{
				return;
			}
			if (UpdateContext != IntPtr.Zero)
			{
				Deinitialize(UpdateContext);
				if (ReleaseUpdateContext(UpdateContext) == 0)
				{
					UpdateContext = IntPtr.Zero;
				}
			}
			_alreadyDisposed = true;
		}

		public int Initialize(int storeIdsCount, ImageStructures.STORE_ID[] storeIds, string UpdateInputFile, string AlternateStagingLocation, LogUtil.InteropLogString ErrorMsgHandler, LogUtil.InteropLogString WarningMsgHandler, LogUtil.InteropLogString InfoMsgHandler, LogUtil.InteropLogString DebugMsgHandler)
		{
			LogUtil.IULogTo(ErrorMsgHandler, WarningMsgHandler, InfoMsgHandler, DebugMsgHandler);
			IU_LogTo(ErrorMsgHandler, WarningMsgHandler, InfoMsgHandler, DebugMsgHandler);
			return Initialize(UpdateContext, storeIdsCount, storeIds, UpdateInputFile, AlternateStagingLocation);
		}

		public void RegisterProgressCallback(IUPhase Phase)
		{
			IU_InitializeDefaultProgressReporting(Phase);
		}

		public int PrepareUpdate()
		{
			RegisterProgressCallback(IUPhase.IUPhase_Staging);
			return PrepareUpdate(UpdateContext);
		}

		public int ExecuteUpdate()
		{
			RegisterProgressCallback(IUPhase.IUPhase_Commit);
			return ExecuteUpdate(UpdateContext);
		}

		[DllImport("wcp.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
		public static extern int WcpInitialize(out UIntPtr InitCookie);

		[DllImport("Ole32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
		public static extern int CoGetMalloc(uint dwMemContext, out UIntPtr pMalloc);

		[DllImport("wcp.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
		public static extern int SetIsolationIMalloc(UIntPtr IMalloc);

		[DllImport("wcp.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
		public static extern int CreateNewWindows(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string szSystemDrive, ref OFFLINE_STORE_CREATION_PARAMETERS pParameters, UIntPtr ppvKeys, out uint pdwDisposition);

		[DllImport("wcp.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
		public static extern void WcpShutdown(UIntPtr InitCookie);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		private static extern IntPtr CreateUpdateContext();

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		private static extern int ReleaseUpdateContext(IntPtr UpdateContext);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		private static extern int Initialize(IntPtr UpdateContext, int storeIdsCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] ImageStructures.STORE_ID[] storeIds, string UpdateInputFile, string AlternateStagingLocation);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		private static extern void Deinitialize(IntPtr UpdateContext);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		private static extern void IU_InitializeDefaultProgressReporting(IUPhase Phase);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		private static extern int PrepareUpdate(IntPtr UpdateContext);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
		private static extern int ExecuteUpdate(IntPtr UpdateContext);

		[DllImport("IUCore.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void IU_ClearCachedDataPath();

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, SetLastError = true)]
		private static extern void IU_LogTo([MarshalAs(UnmanagedType.FunctionPtr)] LogUtil.InteropLogString ErrorMsgHandler, [MarshalAs(UnmanagedType.FunctionPtr)] LogUtil.InteropLogString WarningMsgHandler, [MarshalAs(UnmanagedType.FunctionPtr)] LogUtil.InteropLogString InfoMsgHandler, [MarshalAs(UnmanagedType.FunctionPtr)] LogUtil.InteropLogString DebugMsgHandler);
	}
}
