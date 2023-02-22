using System;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class UpdateOSOutputIdentity
	{
		public string Owner;

		public string Component;

		public string SubComponent;

		[CLSCompliant(false)]
		public PkgVersion Version;

		public override string ToString()
		{
			string text = Owner + "." + Component;
			if (!string.IsNullOrEmpty(SubComponent))
			{
				text = text + "." + SubComponent;
			}
			return text;
		}
	}
}
