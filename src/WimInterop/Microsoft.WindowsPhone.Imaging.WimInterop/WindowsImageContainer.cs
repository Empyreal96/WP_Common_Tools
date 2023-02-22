using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.Imaging.WimInterop
{
	public sealed class WindowsImageContainer : IDisposable
	{
		public enum CreateFileAccess
		{
			Read,
			Write
		}

		public enum CreateFileMode
		{
			None,
			CreateNew,
			CreateAlways,
			OpenExisting,
			OpenAlways
		}

		public enum CreateFileCompression
		{
			WIM_COMPRESS_NONE,
			WIM_COMPRESS_XPRESS,
			WIM_COMPRESS_LZX,
			WIM_COMPRESS_LZMS
		}

		private class WindowsImage : IImage, IDisposable
		{
			private IntPtr _parentWindowsImageHandle = IntPtr.Zero;

			private string _parentWindowsImageFilePath;

			private IntPtr _imageHandle = IntPtr.Zero;

			private int _index;

			private string _mountedPath;

			private bool _mounted;

			private const string UNICODE_FILE_MARKER = "\ufeff";

			public string MountedPath
			{
				get
				{
					if (_mountedPath == null)
					{
						return null;
					}
					return _mountedPath;
				}
			}

			public WindowsImage(IntPtr imageContainerHandle, string imageContainerFilePath, int imageIndex)
			{
				_parentWindowsImageHandle = imageContainerHandle;
				_parentWindowsImageFilePath = imageContainerFilePath;
				_index = imageIndex;
				_imageHandle = NativeMethods.LoadImage(imageContainerHandle, imageIndex);
			}

			~WindowsImage()
			{
				DisposeInner();
			}

			public void Dispose()
			{
				DisposeInner();
				GC.SuppressFinalize(this);
			}

			private void DisposeInner()
			{
				if (_mounted)
				{
					DismountImage();
					_mounted = false;
				}
				if (_imageHandle != IntPtr.Zero)
				{
					NativeMethods.CloseHandle(_imageHandle);
					_imageHandle = IntPtr.Zero;
				}
				GC.KeepAlive(this);
			}

			public void Apply(string pathToApplyTo)
			{
				_mountedPath = pathToApplyTo;
				uint applyFlags = 0u;
				NativeMethods.ApplyImage(_imageHandle, pathToApplyTo, applyFlags);
			}

			public void Mount(string pathToMountTo, bool isReadOnly)
			{
				_mountedPath = pathToMountTo;
				uint num = 0u;
				if (isReadOnly)
				{
					num |= 0x200u;
				}
				NativeMethods.MountImage(_imageHandle, pathToMountTo, num);
				_mounted = true;
			}

			public void DismountImage()
			{
				if (_mounted)
				{
					NativeMethods.DismountImage(_imageHandle);
					_mountedPath = null;
					_mounted = false;
				}
			}

			public void DismountImage(bool saveChanges)
			{
				if (_mounted)
				{
					if (saveChanges)
					{
						NativeMethods.WIMCommitImageHandle(_imageHandle, 0u);
					}
					DismountImage();
					_mountedPath = null;
					_mounted = false;
				}
			}
		}

		private class NativeMethods
		{
			public const uint WIM_FLAG_VERIFY = 2u;

			public const uint WIM_FLAG_INDEX = 4u;

			private NativeMethods()
			{
			}

			[DllImport("Wimgapi.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "WIMCreateFile", ExactSpelling = true, SetLastError = true)]
			private static extern IntPtr WimCreateFile([MarshalAs(UnmanagedType.LPWStr)] string WimPath, uint DesiredAccess, uint CreationDisposition, uint FlagsAndAttributes, uint CompressionType, out IntPtr CreationResult);

			public static IntPtr CreateFile(string imageFile, uint access, uint mode, uint compress)
			{
				IntPtr CreationResult = IntPtr.Zero;
				IntPtr zero = IntPtr.Zero;
				uint num = 2u;
				int num2 = -1;
				if (compress == 3)
				{
					num |= 0x20000000u;
				}
				zero = WimCreateFile(imageFile, access, mode, num, compress, out CreationResult);
				num2 = Marshal.GetLastWin32Error();
				if (zero == IntPtr.Zero)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to open/create .wim file {0}. Error = {1}", new object[2] { imageFile, num2 }));
				}
				return zero;
			}

			[DllImport("Wimgapi.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "WIMCloseHandle", ExactSpelling = true, SetLastError = true)]
			private static extern bool WimCloseHandle(IntPtr Handle);

			public static void CloseHandle(IntPtr handle)
			{
				bool num = WimCloseHandle(handle);
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (!num)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to close image handle. Error = {0}", new object[1] { lastWin32Error }));
				}
			}

			[DllImport("Wimgapi.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "WIMSetTemporaryPath", ExactSpelling = true, SetLastError = true)]
			private static extern bool WimSetTemporaryPath(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string TemporaryPath);

			public static void SetTemporaryPath(IntPtr handle, string temporaryPath)
			{
				bool num = WimSetTemporaryPath(handle, temporaryPath);
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (!num)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to set temporary path. Error = {0}", new object[1] { lastWin32Error }));
				}
			}

			[DllImport("Wimgapi.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "WIMLoadImage", ExactSpelling = true, SetLastError = true)]
			private static extern IntPtr WimLoadImage(IntPtr Handle, uint ImageIndex);

			public static IntPtr LoadImage(IntPtr handle, int imageIndex)
			{
				IntPtr intPtr = WimLoadImage(handle, (uint)imageIndex);
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (intPtr == IntPtr.Zero)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to load image. Error = {0}", new object[1] { lastWin32Error }));
				}
				return intPtr;
			}

			[DllImport("Wimgapi.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "WIMCaptureImage", ExactSpelling = true, SetLastError = true)]
			private static extern IntPtr WimCaptureImage(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string Path, uint CaptureFlags);

			public static IntPtr CaptureImage(IntPtr handle, string path)
			{
				IntPtr intPtr = WimCaptureImage(handle, path, 0u);
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (intPtr == IntPtr.Zero)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Failed to capture image from {0}. Error = {1}", new object[2] { path, lastWin32Error }));
				}
				return intPtr;
			}

			[DllImport("Wimgapi.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "WIMSetBootImage", ExactSpelling = true, SetLastError = true)]
			private static extern IntPtr WimSetBootImage(IntPtr Handle, uint index);

			public static IntPtr SetBootImage(IntPtr handle, int index)
			{
				if (index != 0 && (index > GetImageCount(handle) || index < 0))
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Index is out of range.  Current image count is {0}.", new object[1] { GetImageCount(handle) }));
				}
				IntPtr intPtr = WimSetBootImage(handle, (uint)index);
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (intPtr == IntPtr.Zero)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Failed to set boot image with index {0}. Error = {1}", new object[2] { index, lastWin32Error }));
				}
				return intPtr;
			}

			[DllImport("Wimgapi.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "WIMGetImageCount", ExactSpelling = true, SetLastError = true)]
			private static extern int WimGetImageCount(IntPtr Handle);

			public static int GetImageCount(IntPtr windowsImageHandle)
			{
				int num = WimGetImageCount(windowsImageHandle);
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (num == -1)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to get image count. Error = {0}", new object[1] { lastWin32Error }));
				}
				return num;
			}

			[DllImport("Wimgapi.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "WIMMountImage", ExactSpelling = true, SetLastError = true)]
			private static extern bool WimMountImage([MarshalAs(UnmanagedType.LPWStr)] string MountPath, [MarshalAs(UnmanagedType.LPWStr)] string WimFileName, uint ImageIndex, [MarshalAs(UnmanagedType.LPWStr)] string TemporaryPath);

			public static void MountImage(string mountPath, string windowsImageFileName, int imageIndex)
			{
				bool flag = false;
				int lastWin32Error;
				try
				{
					flag = WimMountImage(mountPath, windowsImageFileName, (uint)imageIndex, Environment.GetEnvironmentVariable("temp"));
					lastWin32Error = Marshal.GetLastWin32Error();
				}
				catch (StackOverflowException)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to mount image {0} to {1}.", new object[2] { windowsImageFileName, mountPath }));
				}
				if (!flag)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to mount image {0} to {1}. Error = {2}", new object[3] { windowsImageFileName, mountPath, lastWin32Error }));
				}
			}

			[DllImport("Wimgapi.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "WIMApplyImage", ExactSpelling = true, SetLastError = true)]
			private static extern bool WimApplyImage(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string MountPath, uint ApplyFlags);

			public static void ApplyImage(IntPtr handle, string applyPath, uint applyFlags)
			{
				bool flag = false;
				int lastWin32Error;
				try
				{
					flag = WimApplyImage(handle, applyPath, applyFlags);
					lastWin32Error = Marshal.GetLastWin32Error();
				}
				catch (StackOverflowException)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to mount image from handle to {0}.", new object[1] { applyPath }));
				}
				if (!flag)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to mount image from handle to {0}. Error = {1:X}", new object[2] { applyPath, lastWin32Error }));
				}
			}

			[DllImport("Wimgapi.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "WIMMountImageHandle", ExactSpelling = true, SetLastError = true)]
			private static extern bool WimMountImageHandle(IntPtr Handle, [MarshalAs(UnmanagedType.LPWStr)] string MountPath, uint MountFlags);

			public static void MountImage(IntPtr handle, string mountPath, uint mountFlags)
			{
				bool flag = false;
				int lastWin32Error;
				try
				{
					flag = WimMountImageHandle(handle, mountPath, mountFlags);
					lastWin32Error = Marshal.GetLastWin32Error();
				}
				catch (StackOverflowException)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to mount image from handle to {0}.", new object[1] { mountPath }));
				}
				if (!flag)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to mount image from handle to {0}. Error = {1:X}", new object[2] { mountPath, lastWin32Error }));
				}
			}

			[DllImport("Wimgapi.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "WIMUnmountImage", ExactSpelling = true, SetLastError = true)]
			private static extern bool WimUnmountImage([MarshalAs(UnmanagedType.LPWStr)] string MountPath, [MarshalAs(UnmanagedType.LPWStr)] string WimFileName, uint ImageIndex, bool CommitChanges);

			public static void DismountImage(string mountPath, string wimdowsImageFileName, int imageIndex, bool commitChanges)
			{
				bool flag = false;
				int lastWin32Error;
				try
				{
					flag = WimUnmountImage(mountPath, wimdowsImageFileName, (uint)imageIndex, commitChanges);
					lastWin32Error = Marshal.GetLastWin32Error();
				}
				catch (StackOverflowException ex)
				{
					throw new StackOverflowException(string.Format(CultureInfo.CurrentCulture, "Unable to unmount image {0} from {1}.", new object[2] { wimdowsImageFileName, mountPath }), ex.InnerException);
				}
				if (!flag)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to unmount image {0} from {1}. Error = {2}", new object[3] { wimdowsImageFileName, mountPath, lastWin32Error }));
				}
			}

			[DllImport("Wimgapi.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "WIMUnmountImageHandle", ExactSpelling = true, SetLastError = true)]
			private static extern bool WimUnmountImage(IntPtr ImageHandle, uint UnmountFlags);

			public static void DismountImage(IntPtr imageHandle)
			{
				bool flag = false;
				int lastWin32Error;
				try
				{
					flag = WimUnmountImage(imageHandle, 0u);
					lastWin32Error = Marshal.GetLastWin32Error();
				}
				catch (StackOverflowException ex)
				{
					throw new StackOverflowException(string.Format(CultureInfo.CurrentCulture, "Unable to unmount image from handle {0}.", new object[1] { imageHandle }), ex.InnerException);
				}
				if (!flag)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to unmount image from handle {0}. Error = {1}", new object[2] { imageHandle, lastWin32Error }));
				}
			}

			[DllImport("Wimgapi.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
			private static extern bool WIMCommitImageHandle(IntPtr ImageHandle, uint UnmountFlags, IntPtr phNewImageHandle);

			public static bool WIMCommitImageHandle(IntPtr ImageHandle, uint UnmountFlags)
			{
				return WIMCommitImageHandle(ImageHandle, UnmountFlags, IntPtr.Zero);
			}
		}

		[Flags]
		private enum CreateFileAccessPrivate : uint
		{
			Read = 0x80000000u,
			Write = 0x40000000u,
			Mount = 0x20000000u
		}

		private IntPtr _imageContainerHandle;

		private string _windowsImageFilePath;

		private WindowsImage[] _images;

		private int _imageCount;

		public IImage this[int imageIndex]
		{
			get
			{
				if (_images == null || _images[imageIndex] == null)
				{
					ArrayList arrayList = new ArrayList();
					arrayList.Add(new WindowsImage(_imageContainerHandle, _windowsImageFilePath, imageIndex + 1));
					_images = (WindowsImage[])arrayList.ToArray(typeof(WindowsImage));
				}
				GC.KeepAlive(this);
				return _images[imageIndex];
			}
		}

		public int ImageCount
		{
			get
			{
				if (_imageCount == 0)
				{
					_imageCount = NativeMethods.GetImageCount(_imageContainerHandle);
				}
				GC.KeepAlive(this);
				return _imageCount;
			}
		}

		public WindowsImageContainer(string imageFilePath, CreateFileMode mode, CreateFileAccess access, CreateFileCompression compress)
		{
			CreateFileAccessPrivate mappedFileAccess = GetMappedFileAccess(access);
			if (mappedFileAccess == CreateFileAccessPrivate.Read && (!File.Exists(imageFilePath) || CreateFileMode.OpenExisting != mode))
			{
				throw new UnauthorizedAccessException(string.Format(CultureInfo.CurrentCulture, "Read access can be specified only with OpenExisting mode or OpenAlways mode when the .wim file does not exist."));
			}
			try
			{
				_imageContainerHandle = NativeMethods.CreateFile(imageFilePath, (uint)mappedFileAccess, (uint)mode, (uint)compress);
				_windowsImageFilePath = imageFilePath;
			}
			catch (DllNotFoundException ex)
			{
				throw new DllNotFoundException(string.Format(CultureInfo.CurrentCulture, "Unable to load WIM libraries. Make sure the correct DLLs are present (Wimgapi.dll and Xmlrw.dll)."), ex.InnerException);
			}
			if (_imageContainerHandle.Equals(IntPtr.Zero))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unable to open  the .wim file {0}.", new object[1] { imageFilePath }));
			}
			string environmentVariable = Environment.GetEnvironmentVariable("BUILD_PRODUCT");
			string text = Environment.GetEnvironmentVariable("OBJECT_ROOT");
			if ((!string.IsNullOrEmpty(environmentVariable) && environmentVariable.Equals("nt", StringComparison.OrdinalIgnoreCase)) || string.IsNullOrEmpty(text))
			{
				text = Path.GetTempPath();
			}
			NativeMethods.SetTemporaryPath(_imageContainerHandle, text);
			ArrayList arrayList = new ArrayList();
			for (int i = 0; i < ImageCount; i++)
			{
				arrayList.Add(new WindowsImage(_imageContainerHandle, _windowsImageFilePath, i + 1));
			}
			_images = (WindowsImage[])arrayList.ToArray(typeof(WindowsImage));
		}

		~WindowsImageContainer()
		{
			DisposeInner();
		}

		public void Dispose()
		{
			DisposeInner();
			GC.SuppressFinalize(this);
		}

		private void DisposeInner()
		{
			if (_images != null)
			{
				WindowsImage[] images = _images;
				for (int i = 0; i < images.Length; i++)
				{
					images[i]?.Dispose();
				}
			}
			if (_imageContainerHandle != IntPtr.Zero)
			{
				NativeMethods.CloseHandle(_imageContainerHandle);
				_imageContainerHandle = IntPtr.Zero;
			}
			GC.KeepAlive(this);
		}

		public IEnumerator GetEnumerator()
		{
			if (_images == null)
			{
				return new ArrayList().GetEnumerator();
			}
			return _images.GetEnumerator();
		}

		public void SetBootImage(int imageIndex)
		{
			NativeMethods.SetBootImage(_imageContainerHandle, imageIndex);
			GC.KeepAlive(this);
		}

		public void CaptureImage(string pathToCapture)
		{
			int imageIndex = ImageCount + 1;
			NativeMethods.CloseHandle(NativeMethods.CaptureImage(_imageContainerHandle, pathToCapture));
			GC.KeepAlive(this);
			ArrayList arrayList = new ArrayList();
			if (_images != null)
			{
				WindowsImage[] images = _images;
				foreach (WindowsImage value in images)
				{
					arrayList.Add(value);
				}
			}
			arrayList.Add(new WindowsImage(_imageContainerHandle, _windowsImageFilePath, imageIndex));
			_images = (WindowsImage[])arrayList.ToArray(typeof(WindowsImage));
		}

		private CreateFileAccessPrivate GetMappedFileAccess(CreateFileAccess access)
		{
			switch (access)
			{
			case CreateFileAccess.Read:
				return CreateFileAccessPrivate.Read | CreateFileAccessPrivate.Mount;
			case CreateFileAccess.Write:
				return CreateFileAccessPrivate.Read | CreateFileAccessPrivate.Write | CreateFileAccessPrivate.Mount;
			default:
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "No file access level specified."));
			}
		}
	}
}
