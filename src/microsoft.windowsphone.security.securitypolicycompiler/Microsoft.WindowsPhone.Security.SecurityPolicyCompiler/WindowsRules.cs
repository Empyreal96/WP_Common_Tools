using System.Text;
using System.Xml;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class WindowsRules : Capability
	{
		public WindowsRules()
		{
			base.OwnerType = CapabilityOwnerType.WindowsRules;
		}

		protected override void AddAttributes(XmlElement windowsRulesXmlElement)
		{
			base.Id = "ID_CAP_WINRULES_" + windowsRulesXmlElement.GetAttribute("Id");
			base.FriendlyName = windowsRulesXmlElement.GetAttribute("FriendlyName");
			base.SvcCapSID = windowsRulesXmlElement.GetAttribute("SID");
			base.Visibility = "Internal";
		}

		protected override void AddElements(XmlElement windowsRulesXmlElement)
		{
			XmlNodeList xmlNodeList = windowsRulesXmlElement.SelectNodes(".", GlobalVariables.NamespaceManager);
			if (xmlNodeList.Count <= 0)
			{
				return;
			}
			if (base.CapabilityRules == null)
			{
				base.CapabilityRules = new CapabilityRules(base.OwnerType);
			}
			foreach (XmlElement item in xmlNodeList)
			{
				base.CapabilityRules.Add(item, null, base.SvcCapSID);
			}
		}

		protected override void CompileAttributes()
		{
			base.ElementId = HashCalculator.CalculateSha256Hash(base.Id, true);
		}

		protected override void CalculateAttributeHash()
		{
			StringBuilder stringBuilder = new StringBuilder(base.Id);
			stringBuilder.Append(base.Visibility);
			stringBuilder.Append(base.SvcCapSID);
			if (base.CapabilityRules != null)
			{
				stringBuilder.Append(base.CapabilityRules.GetAllAttributesString());
			}
			base.AttributeHash = HashCalculator.CalculateSha256Hash(stringBuilder.ToString(), true);
		}
	}
}
