using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	public class PropsFile
	{
		[XmlAttribute("Include")]
		public string Include;

		public string InstallPath;

		public string MC_ARM_FRE;

		public string MC_ARM_CHK;

		public string MC_ARM64_FRE;

		public string MC_ARM64_CHK;

		public string MC_X86_FRE;

		public string MC_X86_CHK;

		public string MC_AMD64_FRE;

		public string MC_AMD64_CHK;

		public string Feature;

		public string Owner;

		public string BusinessReason;

		public override string ToString()
		{
			return Include;
		}
	}
}
