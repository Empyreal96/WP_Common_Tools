using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public static class LongPathFile
	{
		public static bool Exists(string path)
		{
			return NativeMethods.IU_FileExists(path);
		}

		public static FileAttributes GetAttributes(string path)
		{
			return LongPathCommon.GetAttributes(path);
		}

		public static void SetAttributes(string path, FileAttributes attributes)
		{
			if (!NativeMethods.SetFileAttributes(LongPathCommon.NormalizeLongPath(path), attributes))
			{
				throw LongPathCommon.GetExceptionFromLastWin32Error();
			}
		}

		public static void Delete(string path)
		{
			string lpFileName = LongPathCommon.NormalizeLongPath(path);
			if (Exists(path) && !NativeMethods.DeleteFile(lpFileName))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (lastWin32Error != 2)
				{
					throw LongPathCommon.GetExceptionFromWin32Error(lastWin32Error);
				}
			}
		}

		public static void Move(string sourcePath, string destinationPath)
		{
			string lpPathNameFrom = LongPathCommon.NormalizeLongPath(sourcePath);
			string lpPathNameTo = LongPathCommon.NormalizeLongPath(destinationPath);
			if (!NativeMethods.MoveFile(lpPathNameFrom, lpPathNameTo))
			{
				throw LongPathCommon.GetExceptionFromLastWin32Error();
			}
		}

		public static void Copy(string sourcePath, string destinationPath, bool overwrite)
		{
			string src = LongPathCommon.NormalizeLongPath(sourcePath);
			string dst = LongPathCommon.NormalizeLongPath(destinationPath);
			if (!NativeMethods.CopyFile(src, dst, !overwrite))
			{
				throw LongPathCommon.GetExceptionFromLastWin32Error();
			}
		}

		public static void Copy(string sourcePath, string destinationPath)
		{
			Copy(sourcePath, destinationPath, false);
		}

		public static FileStream Open(string path, FileMode mode, FileAccess access)
		{
			return Open(path, mode, access, FileShare.None, 0, FileOptions.None);
		}

		public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
		{
			return Open(path, mode, access, share, 0, FileOptions.None);
		}

		public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options)
		{
			if (bufferSize == 0)
			{
				bufferSize = 1024;
			}
			FileStream fileStream = new FileStream(GetFileHandle(LongPathCommon.NormalizeLongPath(path), mode, access, share, options), access, bufferSize, (options & FileOptions.Asynchronous) == FileOptions.Asynchronous);
			if (mode == FileMode.Append)
			{
				fileStream.Seek(0L, SeekOrigin.End);
			}
			return fileStream;
		}

		public static FileStream OpenRead(string path)
		{
			return Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		public static FileStream OpenWrite(string path)
		{
			return Open(path, FileMode.Create, FileAccess.ReadWrite);
		}

		public static StreamWriter CreateText(string path)
		{
			return new StreamWriter(Open(path, FileMode.Create, FileAccess.ReadWrite), Encoding.UTF8);
		}

		public static byte[] ReadAllBytes(string path)
		{
			using (FileStream fileStream = OpenRead(path))
			{
				byte[] array = new byte[fileStream.Length];
				fileStream.Read(array, 0, array.Length);
				return array;
			}
		}

		public static void WriteAllBytes(string path, byte[] contents)
		{
			using (FileStream fileStream = OpenWrite(path))
			{
				fileStream.Write(contents, 0, contents.Length);
			}
		}

		public static string ReadAllText(string path, Encoding encoding)
		{
			using (StreamReader streamReader = new StreamReader(OpenRead(path), encoding, true))
			{
				return streamReader.ReadToEnd();
			}
		}

		public static string ReadAllText(string path)
		{
			return ReadAllText(path, Encoding.Default);
		}

		public static void WriteAllText(string path, string contents, Encoding encoding)
		{
			using (StreamWriter streamWriter = new StreamWriter(OpenWrite(path), encoding))
			{
				streamWriter.Write(contents);
			}
		}

		public static void WriteAllText(string path, string contents)
		{
			WriteAllText(path, contents, new UTF8Encoding(false));
		}

		public static string[] ReadAllLines(string path, Encoding encoding)
		{
			using (StreamReader streamReader = new StreamReader(OpenRead(path), encoding, true))
			{
				List<string> list = new List<string>();
				while (!streamReader.EndOfStream)
				{
					list.Add(streamReader.ReadLine());
				}
				return list.ToArray();
			}
		}

		public static string[] ReadAllLines(string path)
		{
			return ReadAllLines(path, Encoding.Default);
		}

		public static void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
		{
			using (StreamWriter streamWriter = new StreamWriter(OpenWrite(path), encoding))
			{
				foreach (string content in contents)
				{
					streamWriter.WriteLine(content);
				}
			}
		}

		public static void WriteAllLines(string path, IEnumerable<string> contents)
		{
			WriteAllLines(path, contents, new UTF8Encoding(false));
		}

		public static void AppendAllText(string path, string contents, Encoding encoding)
		{
			using (StreamWriter streamWriter = new StreamWriter(Open(path, FileMode.Append, FileAccess.ReadWrite), encoding))
			{
				streamWriter.Write(contents);
			}
		}

		public static void AppendAllText(string path, string contents)
		{
			AppendAllText(path, contents, new UTF8Encoding(false));
		}

		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "False positive")]
		private static SafeFileHandle GetFileHandle(string normalizedPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
		{
			NativeMethods.EFileAccess underlyingAccess = GetUnderlyingAccess(access);
			FileMode underlyingMode = GetUnderlyingMode(mode);
			SafeFileHandle safeFileHandle = NativeMethods.CreateFile(normalizedPath, underlyingAccess, (uint)share, IntPtr.Zero, (uint)underlyingMode, (uint)options, IntPtr.Zero);
			if (safeFileHandle.IsInvalid)
			{
				throw LongPathCommon.GetExceptionFromLastWin32Error();
			}
			return safeFileHandle;
		}

		private static FileMode GetUnderlyingMode(FileMode mode)
		{
			if (mode == FileMode.Append)
			{
				return FileMode.OpenOrCreate;
			}
			return mode;
		}

		private static NativeMethods.EFileAccess GetUnderlyingAccess(FileAccess access)
		{
			switch (access)
			{
			case FileAccess.Read:
				return NativeMethods.EFileAccess.GenericRead;
			case FileAccess.Write:
				return NativeMethods.EFileAccess.GenericWrite;
			case FileAccess.ReadWrite:
				return NativeMethods.EFileAccess.GenericRead | NativeMethods.EFileAccess.GenericWrite;
			default:
				throw new ArgumentOutOfRangeException("access");
			}
		}
	}
}
