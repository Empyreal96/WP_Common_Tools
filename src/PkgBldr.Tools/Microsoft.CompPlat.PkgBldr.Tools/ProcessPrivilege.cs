using System;
using System.Globalization;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public static class ProcessPrivilege
	{
		public static void Adjust(ConstValue<string> privilege, bool enablePrivilege)
		{
			if (NativeSecurityMethods.IU_AdjustProcessPrivilege(privilege.Value, enablePrivilege) != 0)
			{
				throw new Exception(string.Format(CultureInfo.InvariantCulture, "Failed to adjust privilege with name {0} and value {1}", new object[2] { privilege.Value, enablePrivilege }));
			}
		}
	}
}
