using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot("Action", Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00")]
	public sealed class FailureAction
	{
		[XmlAttribute("Type")]
		public FailureActionType Type;

		[XmlAttribute("Delay")]
		public uint Delay;

		public FailureAction()
		{
		}

		public FailureAction(FailureActionType type, uint delay)
		{
			Type = type;
			Delay = delay;
		}
	}
}
