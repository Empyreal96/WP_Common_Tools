namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public static class NormalizedString
	{
		public static string Get(string value)
		{
			return value.ToUpper(GlobalVariables.Culture);
		}
	}
}
