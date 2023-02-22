namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public class PrivilegeNames
	{
		public static TokenPrivilege BackupPrivilege => new TokenPrivilege("SeBackupPrivilege");

		public static TokenPrivilege SecurityPrivilege => new TokenPrivilege("SeSecurityPrivilege");

		public static TokenPrivilege RestorePrivilege => new TokenPrivilege("SeRestorePrivilege");
	}
}
