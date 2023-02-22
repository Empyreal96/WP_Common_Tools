namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class Directory : PathElement
	{
		public Directory()
		{
			base.ElementName = "Directory";
			base.WildcardSupport = true;
		}
	}
}
