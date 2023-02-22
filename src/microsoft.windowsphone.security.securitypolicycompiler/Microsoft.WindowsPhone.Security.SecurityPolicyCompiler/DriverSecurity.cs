using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class DriverSecurity
	{
		public const string NodeDriverSecurity = "//WP_Policy:Security[@InfSectionName='";

		public const string NodeDriverRule = "//WP_Policy:DriverRule[@Name='";

		public string GetSddlString(string infSectionName, string oldSddl, IXPathNavigable driverPolicyDocument, IXPathNavigable driverRuleTemplateDocument)
		{
			XmlDocument obj = (XmlDocument)driverPolicyDocument;
			StringBuilder stringBuilder = new StringBuilder();
			if (string.IsNullOrEmpty(oldSddl))
			{
				stringBuilder.Append("D:P(A;;GA;;;SY)");
			}
			else
			{
				stringBuilder.Append(oldSddl);
			}
			string xpath = "//WP_Policy:Security[@InfSectionName='" + infSectionName + "']";
			XmlNode xmlNode = obj.SelectSingleNode(xpath, GlobalVariables.NamespaceManager);
			if (xmlNode == null)
			{
				throw new PolicyCompilerInternalException("The driver security element can't be found: " + infSectionName);
			}
			if (xmlNode.ChildNodes.Count > 0)
			{
				XmlNodeList childNodes = xmlNode.ChildNodes;
				AddAccessControlEntries(stringBuilder, childNodes);
			}
			string attribute = ((XmlElement)xmlNode).GetAttribute("RuleTemplate");
			if (!string.IsNullOrEmpty(attribute))
			{
				XmlDocument obj2 = (XmlDocument)driverRuleTemplateDocument;
				new XmlNamespaceManager(obj2.NameTable).AddNamespace("WP_Policy", "urn:Microsoft.WindowsPhone/PackageSchema.v8.00");
				xpath = "//WP_Policy:DriverRule[@Name='" + attribute + "']";
				xmlNode = obj2.SelectSingleNode(xpath, GlobalVariables.NamespaceManager);
				if (xmlNode == null)
				{
					throw new PolicyCompilerInternalException("The driver template rule element can't be found: " + attribute);
				}
				if (xmlNode.ChildNodes.Count > 0)
				{
					XmlNodeList childNodes2 = xmlNode.ChildNodes;
					AddAccessControlEntries(stringBuilder, childNodes2);
				}
			}
			return stringBuilder.ToString();
		}

		private void AddAccessControlEntries(StringBuilder driverSddlString, XmlNodeList rulesXmlElementList)
		{
			foreach (XmlNode rulesXmlElement in rulesXmlElementList)
			{
				string appCapSID = string.Empty;
				string svcCapSID = string.Empty;
				string empty = string.Empty;
				string empty2 = string.Empty;
				string empty3 = string.Empty;
				if (rulesXmlElement.NodeType != XmlNodeType.Element)
				{
					continue;
				}
				XmlElement xmlElement = (XmlElement)rulesXmlElement;
				string localName = xmlElement.LocalName;
				DriverRule driverRule = null;
				switch (localName)
				{
				case "AccessedByCapability":
					empty = xmlElement.GetAttribute("Id");
					if (ConstantStrings.PredefinedServiceCapabilities.Contains(empty))
					{
						svcCapSID = empty;
						appCapSID = null;
					}
					else
					{
						svcCapSID = GlobalVariables.SidMapping[empty];
						appCapSID = SidBuilder.BuildApplicationCapabilitySidString(empty);
					}
					driverRule = new DriverRule(localName);
					break;
				case "AccessedByService":
					svcCapSID = SidBuilder.BuildServiceSidString(xmlElement.GetAttribute("Name"));
					driverRule = new DriverRule(localName);
					break;
				case "AccessedByApplication":
					appCapSID = SidBuilder.BuildApplicationSidString(xmlElement.GetAttribute("Name"));
					driverRule = new DriverRule(localName);
					break;
				default:
					throw new PolicyCompilerInternalException("Internal Error: Driver Security element has a invalid type: " + localName);
				}
				if (driverRule != null)
				{
					driverRule.Add(xmlElement, appCapSID, svcCapSID);
					driverSddlString.Append(driverRule.DACL);
				}
			}
		}
	}
}
