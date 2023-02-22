using System.Xml;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class PrivateResources : Capability
	{
		public void SetPrivateResourcesOwner(string name, CapabilityOwnerType ownerTypeValue, string componentSid)
		{
			base.OwnerType = ownerTypeValue;
			base.Id = "ID_CAP_PRIV_" + name;
			base.FriendlyName = name + " private capability";
			base.Visibility = "Private";
			switch (base.OwnerType)
			{
			case CapabilityOwnerType.Application:
				base.AppCapSID = componentSid;
				break;
			case CapabilityOwnerType.Service:
				base.SvcCapSID = componentSid;
				break;
			}
		}

		protected override void AddAttributes(XmlElement privateResourcesXmlElement)
		{
		}

		protected override void AddElements(XmlElement privateResourcesXmlElement)
		{
			if (base.CapabilityRules == null)
			{
				base.CapabilityRules = new CapabilityRules(base.OwnerType);
			}
			base.CapabilityRules.Add(privateResourcesXmlElement, base.AppCapSID, base.SvcCapSID);
		}

		protected override void CompileAttributes()
		{
			base.ElementId = HashCalculator.CalculateSha256Hash(base.Id, true);
		}
	}
}
