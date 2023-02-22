using System;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public static class ProcessPrivilege
	{
		public static void Adjust(TokenPrivilege privilege, bool enablePrivilege)
		{
			if (NativeSecurityMethods.IU_AdjustProcessPrivilege(privilege.Value, enablePrivilege) != 0)
			{
				throw new Exception($"Failed to adjust privilege with name {privilege.Value} and value {enablePrivilege}");
			}
		}
	}
}
