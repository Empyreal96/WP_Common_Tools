using System.Globalization;

namespace Microsoft.CompPlat.PkgBldr.Base.Security.SecurityPolicy
{
	public static class GlobalVariables
	{
		private static CultureInfo GlobalCulture = new CultureInfo("en-US", false);

		public static CultureInfo Culture => GlobalCulture;
	}
}
