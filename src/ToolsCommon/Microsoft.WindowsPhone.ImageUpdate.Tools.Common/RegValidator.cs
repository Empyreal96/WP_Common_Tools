using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public static class RegValidator
	{
		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		public static extern int ValidateRegistryHive(string RegHive);

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
		private static extern int ValidateRegFiles(string[] rgRegFiles, int cRegFiles, string[] rgaFiles, int cRgaFiles);

		public static void Validate(IEnumerable<string> regFiles, IEnumerable<string> rgaFiles)
		{
			string[] array = ((regFiles != null) ? regFiles.ToArray() : new string[0]);
			string[] array2 = ((rgaFiles != null) ? rgaFiles.ToArray() : new string[0]);
			if (array.Length != 0 || array2.Length != 0)
			{
				int num = ValidateRegFiles(array, array.Length, array2, array2.Length);
				if (num != 0)
				{
					throw new IUException("Registry validation failed, check output log for detailed failure information, err '0x{0:X8}'", num);
				}
			}
		}
	}
}
