using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public class SecurityUtils
	{
		private const int ERROR_INSUFFICIENT_BUFFER = 122;

		private static readonly Regex regexExtractMIL = new Regex("(?<MIL>\\(ML[^\\)]*\\))", RegexOptions.Compiled);

		public static string GetFileSystemMandatoryLevel(string resourcePath)
		{
			string result = string.Empty;
			string text = ConvertSDToStringSD(GetSecurityDescriptor(resourcePath, SecurityInformationFlags.MANDATORY_ACCESS_LABEL), SecurityInformationFlags.MANDATORY_ACCESS_LABEL);
			if (!string.IsNullOrEmpty(text))
			{
				text = text.TrimEnd(default(char));
				Match match = regexExtractMIL.Match(text);
				if (match.Success)
				{
					result = match.Groups["MIL"].Value;
				}
			}
			return result;
		}

		[CLSCompliant(false)]
		public static byte[] GetSecurityDescriptor(string resourcePath, SecurityInformationFlags flags)
		{
			byte[] array = null;
			int lpnLengthNeeded = 0;
			NativeSecurityMethods.GetFileSecurity(resourcePath, flags, IntPtr.Zero, 0, ref lpnLengthNeeded);
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (lastWin32Error != 122)
			{
				Console.WriteLine("Error {0} Calling GetFileSecurity() on {1}", lastWin32Error, resourcePath);
				throw new Win32Exception(lastWin32Error);
			}
			int num = lpnLengthNeeded;
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			try
			{
				if (!NativeSecurityMethods.GetFileSecurity(resourcePath, flags, intPtr, num, ref lpnLengthNeeded))
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}
				array = new byte[lpnLengthNeeded];
				Marshal.Copy(intPtr, array, 0, lpnLengthNeeded);
				return array;
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}

		[CLSCompliant(false)]
		public static string ConvertSDToStringSD(byte[] securityDescriptor, SecurityInformationFlags flags)
		{
			string empty = string.Empty;
			int StringSecurityDescriptorLen;
			IntPtr StringSecurityDescriptor;
			bool flag = NativeSecurityMethods.ConvertSecurityDescriptorToStringSecurityDescriptor(securityDescriptor, 1, flags, out StringSecurityDescriptor, out StringSecurityDescriptorLen);
			try
			{
				if (!flag)
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}
				return Marshal.PtrToStringUni(StringSecurityDescriptor, StringSecurityDescriptorLen);
			}
			finally
			{
				if (StringSecurityDescriptor != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(StringSecurityDescriptor);
				}
				StringSecurityDescriptor = IntPtr.Zero;
			}
		}

		public static AclCollection GetFileSystemACLs(string rootDir)
		{
			if (rootDir == null)
			{
				throw new ArgumentNullException("rootDir");
			}
			if (!LongPathDirectory.Exists(rootDir))
			{
				throw new ArgumentException($"Directory {rootDir} does not exist");
			}
			AclCollection aclCollection = new AclCollection();
			DirectoryInfo directoryInfo = new DirectoryInfo(rootDir);
			DirectoryAcl directoryAcl = new DirectoryAcl(directoryInfo, rootDir);
			if (!directoryAcl.IsEmpty)
			{
				aclCollection.Add(directoryAcl);
			}
			GetFileSystemACLsRecursive(directoryInfo, rootDir, aclCollection);
			return aclCollection;
		}

		public static AclCollection GetRegistryACLs(string hiveRoot)
		{
			if (hiveRoot == null)
			{
				throw new ArgumentNullException("hiveRoot");
			}
			if (!LongPathDirectory.Exists(hiveRoot))
			{
				throw new ArgumentException($"Directory {hiveRoot} does not exist");
			}
			AclCollection aclCollection = new AclCollection();
			foreach (SystemRegistryHiveFiles value in Enum.GetValues(typeof(SystemRegistryHiveFiles)))
			{
				string hivefile = Path.Combine(hiveRoot, Enum.GetName(typeof(SystemRegistryHiveFiles), value));
				string prefix = RegistryUtils.MapHiveToMountPoint(value);
				using (ORRegistryKey parent = ORRegistryKey.OpenHive(hivefile, prefix))
				{
					GetRegistryACLsRecursive(parent, aclCollection);
				}
			}
			return aclCollection;
		}

		private static void GetFileSystemACLsRecursive(DirectoryInfo rootdi, string rootDir, AclCollection accesslist)
		{
			DirectoryInfo[] directories = rootdi.GetDirectories();
			foreach (DirectoryInfo obj in directories)
			{
				GetFileSystemACLsRecursive(obj, rootDir, accesslist);
				DirectoryAcl directoryAcl = new DirectoryAcl(obj, rootDir);
				if (!directoryAcl.IsEmpty)
				{
					accesslist.Add(directoryAcl);
				}
			}
			FileInfo[] files = rootdi.GetFiles();
			for (int i = 0; i < files.Length; i++)
			{
				FileAcl fileAcl = new FileAcl(files[i], rootDir);
				if (!fileAcl.IsEmpty)
				{
					accesslist.Add(fileAcl);
				}
			}
		}

		public static void GetRegistryACLsRecursive(ORRegistryKey parent, AclCollection accesslist)
		{
			string[] subKeys = parent.SubKeys;
			foreach (string subkeyname in subKeys)
			{
				using (ORRegistryKey oRRegistryKey = parent.OpenSubKey(subkeyname))
				{
					GetRegistryACLsRecursive(oRRegistryKey, accesslist);
					RegistryAcl registryAcl = new RegistryAcl(oRRegistryKey);
					if (!registryAcl.IsEmpty)
					{
						accesslist.Add(registryAcl);
					}
				}
			}
		}
	}
}
