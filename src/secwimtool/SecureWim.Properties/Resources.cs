using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace SecureWim.Properties
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (resourceMan == null)
				{
					resourceMan = new ResourceManager("SecureWim.Properties.Resources", typeof(Resources).Assembly);
				}
				return resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return resourceCulture;
			}
			set
			{
				resourceCulture = value;
			}
		}

		internal static string BuildUsageString => ResourceManager.GetString("BuildUsageString", resourceCulture);

		internal static string ExtractUsageString => ResourceManager.GetString("ExtractUsageString", resourceCulture);

		internal static string ReplaceUsageString => ResourceManager.GetString("ReplaceUsageString", resourceCulture);

		internal static byte[] sdiData => (byte[])ResourceManager.GetObject("sdiData", resourceCulture);

		internal Resources()
		{
		}
	}
}
