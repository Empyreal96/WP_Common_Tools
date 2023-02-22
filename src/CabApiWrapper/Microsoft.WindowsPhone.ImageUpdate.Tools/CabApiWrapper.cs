using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools
{
	public class CabApiWrapper
	{
		internal sealed class NativeMethods
		{
			private const string STRING_CABAPI_DLL = "CabApi.dll";

			private const CallingConvention CALLING_CONVENTION = CallingConvention.Cdecl;

			private const bool SET_LAST_ERROR = true;

			private const CharSet CHAR_SET = CharSet.Auto;

			public static string CAB_API_NAME => "CabApi.dll";

			private NativeMethods()
			{
			}

			[DllImport("CabApi.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, SetLastError = true)]
			public static extern uint Cab_Extract(string filename, string outputDir);

			[DllImport("CabApi.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, SetLastError = true)]
			public static extern uint Cab_ExtractOne(string filename, string outputDir, string fileToExtract);

			[DllImport("CabApi.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, SetLastError = true)]
			public static extern uint Cab_ExtractSelected(string filename, string outputDir, string[] filesToExtract, uint cFilesToExtract);

			[DllImport("CabApi.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = "Cab_ExtractSelectedToTarget", SetLastError = true)]
			public static extern uint Cab_ExtractSelected(string filename, string[] filesToExtract, uint cFilesToExtract, string[] targetPaths, uint cTargetPaths);

			[DllImport("CabApi.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, SetLastError = true)]
			public static extern uint Cab_GetFileSizeList(string filename, out IntPtr fileList, out IntPtr sizeList, out uint cFileList);

			[DllImport("CabApi.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, SetLastError = true)]
			public static extern uint Cab_FreeFileSizeList(IntPtr fileList, IntPtr sizeList, uint cFileList);

			[DllImport("CabApi.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, SetLastError = true)]
			public static extern uint Cab_GetFileList(string filename, out IntPtr fileList, out uint cFileList);

			[DllImport("CabApi.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, SetLastError = true)]
			public static extern uint Cab_FreeFileList(IntPtr fileList, uint cFileList);

			[DllImport("CabApi.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, SetLastError = true)]
			public static extern uint Cab_CheckIsCabinet(string filename, out bool isCabinet);

			[DllImport("CabApi.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, SetLastError = true)]
			public static extern uint Cab_CreateCab(string filename, string rootDirectory, string tempWorkingFolder, string filterToSelectFiles, CompressionType compressionType);

			[DllImport("CabApi.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, SetLastError = true)]
			public static extern uint Cab_CreateCabSelected(string filename, string[] files, uint cFiles, string[] targetfiles, uint cTargetFiles, string tempWorkingFolder, string prefixToTrim, CompressionType compressionType);
		}

		private const string STR_COMMA = ",";

		private CabApiWrapper()
		{
		}

		public static void Extract(string filename, string outputDir)
		{
			if (string.IsNullOrEmpty(filename))
			{
				throw new ArgumentNullException("filename");
			}
			filename = LongPath.GetFullPathUNC(filename);
			if (string.IsNullOrEmpty(outputDir))
			{
				throw new ArgumentNullException("outputDir");
			}
			outputDir = LongPath.GetFullPathUNC(outputDir);
			if (!LongPathFile.Exists(filename))
			{
				throw new FileNotFoundException($"CAB file {filename} not found", filename);
			}
			uint num = NativeMethods.Cab_Extract(filename, outputDir);
			if (num != 0)
			{
				throw new CabException(num, "Cab_Extract", filename, outputDir);
			}
		}

		public static void ExtractOne(string filename, string outputDir, string fileToExtract)
		{
			if (string.IsNullOrEmpty(filename))
			{
				throw new ArgumentNullException("filename");
			}
			filename = LongPath.GetFullPathUNC(filename);
			if (string.IsNullOrEmpty(outputDir))
			{
				throw new ArgumentNullException("outputDir");
			}
			outputDir = LongPath.GetFullPathUNC(outputDir);
			if (string.IsNullOrEmpty(fileToExtract))
			{
				throw new ArgumentNullException("fileToExtract");
			}
			if (!LongPathFile.Exists(filename))
			{
				throw new FileNotFoundException($"CAB file {filename} not found", filename);
			}
			uint num = NativeMethods.Cab_ExtractOne(filename, outputDir, fileToExtract);
			if (num != 0)
			{
				throw new CabException(num, "Cab_ExtractOne", filename, outputDir, fileToExtract);
			}
		}

		public static void ExtractSelected(string filename, string outputDir, IEnumerable<string> filesToExtract)
		{
			if (string.IsNullOrEmpty(filename))
			{
				throw new ArgumentNullException("filename");
			}
			filename = LongPath.GetFullPathUNC(filename);
			if (string.IsNullOrEmpty(outputDir))
			{
				throw new ArgumentNullException("outputDir");
			}
			outputDir = LongPath.GetFullPathUNC(outputDir);
			if (filesToExtract == null)
			{
				throw new ArgumentNullException("fileToExtract");
			}
			string[] array = filesToExtract.ToArray();
			uint num = (uint)array.Length;
			if (num == 0)
			{
				throw new ArgumentException("Parameter 'filesToExtract' cannot be empty");
			}
			if (!LongPathFile.Exists(filename))
			{
				throw new FileNotFoundException($"CAB file {filename} not found", filename);
			}
			uint num2 = NativeMethods.Cab_ExtractSelected(filename, outputDir, array, num);
			if (num2 != 0)
			{
				throw new CabException(num2, "Cab_ExtractSelected", filename, outputDir, string.Join(",", array));
			}
		}

		public static void ExtractSelected(string filename, IEnumerable<string> filesToExtract, IEnumerable<string> targetPaths)
		{
			if (string.IsNullOrEmpty(filename))
			{
				throw new ArgumentNullException("filename");
			}
			filename = LongPath.GetFullPathUNC(filename);
			if (filesToExtract == null)
			{
				throw new ArgumentNullException("fileToExtract");
			}
			if (targetPaths == null)
			{
				throw new ArgumentNullException("targetPaths");
			}
			string[] array = filesToExtract.ToArray();
			string[] array2 = targetPaths.ToArray();
			uint num = (uint)array.Length;
			uint num2 = (uint)array2.Length;
			if (num == 0)
			{
				throw new ArgumentException("'filesToExtract' parameter cannot be empty");
			}
			if (num2 == 0)
			{
				throw new ArgumentException("'targetPaths' parameter cannot be empty");
			}
			if (num != num2)
			{
				throw new ArgumentException("'filesToExtract' and 'targetPaths' should have the same number of elements");
			}
			uint num3 = NativeMethods.Cab_ExtractSelected(filename, array, num, array2, num2);
			if (num3 != 0)
			{
				throw new CabException(num3, "Cab_ExtractSelected", filename, string.Join(",", array), string.Join(",", array2));
			}
		}

		public static string[] GetFileList(string filename, out ulong[] fileSizes)
		{
			if (string.IsNullOrEmpty(filename))
			{
				throw new ArgumentNullException("filename");
			}
			filename = LongPath.GetFullPathUNC(filename);
			if (!LongPathFile.Exists(filename))
			{
				throw new FileNotFoundException($"CAB file {filename} not found", filename);
			}
			IntPtr fileList = IntPtr.Zero;
			IntPtr sizeList = IntPtr.Zero;
			uint cFileList = 0u;
			try
			{
				uint num = NativeMethods.Cab_GetFileSizeList(filename, out fileList, out sizeList, out cFileList);
				if (num != 0)
				{
					throw new CabException(num, "Cab_GetFileSizeList", filename);
				}
				fileSizes = new ulong[cFileList];
				long[] array = new long[cFileList];
				Marshal.Copy(sizeList, array, 0, (int)cFileList);
				fileSizes = (ulong[])(object)array;
				IntPtr[] array2 = new IntPtr[cFileList];
				Marshal.Copy(fileList, array2, 0, (int)cFileList);
				string[] array3 = new string[cFileList];
				for (int i = 0; i < cFileList; i++)
				{
					array3[i] = Marshal.PtrToStringUni(array2[i]);
				}
				return array3;
			}
			finally
			{
				if (fileList != IntPtr.Zero && cFileList != 0)
				{
					NativeMethods.Cab_FreeFileSizeList(fileList, sizeList, cFileList);
				}
			}
		}

		public static string[] GetFileList(string filename)
		{
			ulong[] fileSizes;
			return GetFileList(filename, out fileSizes);
		}

		public static bool IsCabinet(string filename)
		{
			if (string.IsNullOrEmpty(filename))
			{
				throw new ArgumentNullException("filename");
			}
			filename = LongPath.GetFullPathUNC(filename);
			if (!LongPathFile.Exists(filename))
			{
				throw new FileNotFoundException($"CAB file {filename} not found", filename);
			}
			bool isCabinet;
			uint num = NativeMethods.Cab_CheckIsCabinet(filename, out isCabinet);
			if (num != 0)
			{
				throw new CabException(num, "Cab_CheckIsCabinet", filename);
			}
			return isCabinet;
		}

		public static void CreateCab(string filename, string rootDirectory, string tempWorkingFolder, string filterToSelectFiles, CompressionType compressionType = CompressionType.None)
		{
			if (string.IsNullOrEmpty(filename))
			{
				throw new ArgumentNullException("filename");
			}
			filename = LongPath.GetFullPathUNC(filename);
			if (string.IsNullOrEmpty(rootDirectory))
			{
				throw new ArgumentNullException("rootDirectory");
			}
			rootDirectory = LongPath.GetFullPathUNC(rootDirectory);
			if (string.IsNullOrEmpty(tempWorkingFolder))
			{
				throw new ArgumentNullException("tempWorkingFolder");
			}
			tempWorkingFolder = LongPath.GetFullPathUNC(tempWorkingFolder);
			if (!LongPathDirectory.Exists(rootDirectory))
			{
				throw new DirectoryNotFoundException($"'rootDirectory' folder {rootDirectory} does not exist");
			}
			if (!LongPathDirectory.Exists(tempWorkingFolder))
			{
				throw new DirectoryNotFoundException($"'tempWorkingFolder' folder {tempWorkingFolder} does not exist");
			}
			uint num = NativeMethods.Cab_CreateCab(filename, rootDirectory, tempWorkingFolder, filterToSelectFiles, compressionType);
			if (num != 0)
			{
				throw new CabException(num, "Cab_CreateCab", filename, rootDirectory, tempWorkingFolder, filterToSelectFiles);
			}
		}

		public static void CreateCabSelected(string filename, string[] files, string tempWorkingFolder, string prefixToTrim, CompressionType compressionType = CompressionType.None)
		{
			if (string.IsNullOrEmpty(filename))
			{
				throw new ArgumentNullException("filename");
			}
			if (files == null)
			{
				throw new ArgumentNullException("files");
			}
			if (string.IsNullOrEmpty(tempWorkingFolder))
			{
				throw new ArgumentNullException("tempWorkingFolder");
			}
			if (files.Length == 0)
			{
				throw new ArgumentException("'files' parameter cannot be empty");
			}
			if (!LongPathDirectory.Exists(tempWorkingFolder))
			{
				throw new DirectoryNotFoundException($"'tempWorkingFolder' folder {tempWorkingFolder} does not exist");
			}
			string[] array = files.Where((string x) => !LongPathFile.Exists(x)).ToArray();
			if (array.Length != 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendFormat("Error when adding files to cab file '{0}'. The following files specified in 'files' don't exist:", filename);
				stringBuilder.AppendLine();
				string[] array2 = array;
				foreach (string arg in array2)
				{
					stringBuilder.AppendFormat("\t{0}", arg);
					stringBuilder.AppendLine();
				}
				throw new FileNotFoundException(stringBuilder.ToString());
			}
			uint num = NativeMethods.Cab_CreateCabSelected(filename, files, (uint)files.Length, null, 0u, tempWorkingFolder, prefixToTrim, compressionType);
			if (num != 0)
			{
				throw new CabException(num, "Cab_CreateCabSelected", filename, tempWorkingFolder, prefixToTrim);
			}
		}

		public static void CreateCabSelected(string filename, string[] sourceFiles, string[] targetFiles, string tempWorkingFolder, CompressionType compressionType = CompressionType.None)
		{
			if (string.IsNullOrEmpty(filename))
			{
				throw new ArgumentNullException("filename");
			}
			if (sourceFiles == null)
			{
				throw new ArgumentNullException("sourceFiles");
			}
			if (targetFiles == null)
			{
				throw new ArgumentNullException("targetFiles");
			}
			if (string.IsNullOrEmpty(tempWorkingFolder))
			{
				throw new ArgumentNullException("tempWorkingFolder");
			}
			if (sourceFiles.Length == 0)
			{
				throw new ArgumentException("'sourceFiles' parameter cannot be empty");
			}
			if (targetFiles.Length == 0)
			{
				throw new ArgumentException("'targetFiles' parameter cannot be empty");
			}
			if (sourceFiles.Length != targetFiles.Length)
			{
				throw new ArgumentException("'sourceFiles' and 'targetFiles' should have the same number of elements");
			}
			if (!LongPathDirectory.Exists(tempWorkingFolder))
			{
				throw new DirectoryNotFoundException($"'tempWorkingFolder' folder {tempWorkingFolder} does not exist");
			}
			string[] array = sourceFiles.Where((string x) => !LongPathFile.Exists(x)).ToArray();
			if (array.Length != 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendFormat("Error when adding files to cab file '{0}'. The following files specified in 'sourceFiles' don't exist:", filename);
				stringBuilder.AppendLine();
				string[] array2 = array;
				foreach (string arg in array2)
				{
					stringBuilder.AppendFormat("\t{0}", arg);
					stringBuilder.AppendLine();
				}
				throw new FileNotFoundException(stringBuilder.ToString());
			}
			uint num = NativeMethods.Cab_CreateCabSelected(filename, sourceFiles, (uint)sourceFiles.Length, targetFiles, (uint)targetFiles.Length, tempWorkingFolder, null, compressionType);
			if (num != 0)
			{
				throw new CabException(num, "Cab_CreateCabSelected", filename, tempWorkingFolder);
			}
		}
	}
}
