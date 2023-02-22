using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class Authorization : IPolicyElement
	{
		private List<PrincipalClass> principalClassCollection;

		private List<CapabilityClass> capabilityClassCollection;

		private List<ExecuteRule> executeRuleCollection;

		private List<CapabilityRule> capabilityRuleCollection;

		[XmlElement(ElementName = "PrincipalClass")]
		public List<PrincipalClass> PrincipalClassCollection => principalClassCollection;

		[XmlElement(ElementName = "CapabilityClass")]
		public List<CapabilityClass> CapabilityClassCollection => capabilityClassCollection;

		[XmlElement(ElementName = "ExecuteRule")]
		public List<ExecuteRule> ExecuteRuleCollection => executeRuleCollection;

		[XmlElement(ElementName = "CapabilityRule")]
		public List<CapabilityRule> CapabilityRuleCollection => capabilityRuleCollection;

		public void Add(IXPathNavigable authorizationXmlElement)
		{
			AddElements((XmlElement)authorizationXmlElement);
		}

		private void AddElements(XmlElement authorizationXmlElement)
		{
			AddPrincipalClasses(authorizationXmlElement);
			AddCapabilityClasses(authorizationXmlElement);
			AddExecuteRules(authorizationXmlElement);
			AddCapabilityRules(authorizationXmlElement);
		}

		private void AddPrincipalClasses(XmlElement authorizationXmlElement)
		{
			XmlNodeList xmlNodeList = authorizationXmlElement.SelectNodes("./WP_Policy:PrincipalClass", GlobalVariables.NamespaceManager);
			if (xmlNodeList.Count <= 0)
			{
				return;
			}
			if (principalClassCollection == null)
			{
				principalClassCollection = new List<PrincipalClass>();
			}
			foreach (XmlElement item in xmlNodeList)
			{
				PrincipalClass principalClass = new PrincipalClass();
				principalClass.Add(item);
				principalClassCollection.Add(principalClass);
			}
		}

		private void AddCapabilityClasses(XmlElement authorizationXmlElement)
		{
			XmlNodeList xmlNodeList = authorizationXmlElement.SelectNodes("./WP_Policy:CapabilityClass", GlobalVariables.NamespaceManager);
			if (xmlNodeList.Count <= 0)
			{
				return;
			}
			if (capabilityClassCollection == null)
			{
				capabilityClassCollection = new List<CapabilityClass>();
			}
			foreach (XmlElement item in xmlNodeList)
			{
				CapabilityClass capabilityClass = new CapabilityClass();
				capabilityClass.Add(item);
				capabilityClassCollection.Add(capabilityClass);
			}
		}

		private void AddExecuteRules(XmlElement authorizationXmlElement)
		{
			XmlNodeList xmlNodeList = authorizationXmlElement.SelectNodes("./WP_Policy:ExecuteRule", GlobalVariables.NamespaceManager);
			if (xmlNodeList.Count <= 0)
			{
				return;
			}
			if (executeRuleCollection == null)
			{
				executeRuleCollection = new List<ExecuteRule>();
			}
			foreach (XmlElement item in xmlNodeList)
			{
				ExecuteRule executeRule = new ExecuteRule();
				executeRule.Add(item);
				executeRuleCollection.Add(executeRule);
			}
		}

		private void AddCapabilityRules(XmlElement authorizationXmlElement)
		{
			XmlNodeList xmlNodeList = authorizationXmlElement.SelectNodes("./WP_Policy:CapabilityRule", GlobalVariables.NamespaceManager);
			if (xmlNodeList.Count <= 0)
			{
				return;
			}
			if (capabilityRuleCollection == null)
			{
				capabilityRuleCollection = new List<CapabilityRule>();
			}
			foreach (XmlElement item in xmlNodeList)
			{
				CapabilityRule capabilityRule = new CapabilityRule();
				capabilityRule.Add(item);
				capabilityRuleCollection.Add(capabilityRule);
			}
		}

		public bool HasChild()
		{
			if ((principalClassCollection == null || principalClassCollection.Count <= 0) && (capabilityClassCollection == null || capabilityClassCollection.Count <= 0) && (executeRuleCollection == null || executeRuleCollection.Count <= 0))
			{
				if (capabilityRuleCollection != null)
				{
					return capabilityRuleCollection.Count > 0;
				}
				return false;
			}
			return true;
		}

		public void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel1, "Authorization");
			if (principalClassCollection != null)
			{
				foreach (PrincipalClass item in principalClassCollection)
				{
					instance.DebugLine(string.Empty);
					item.Print();
				}
			}
			if (capabilityClassCollection != null)
			{
				foreach (CapabilityClass item2 in capabilityClassCollection)
				{
					instance.DebugLine(string.Empty);
					item2.Print();
				}
			}
			if (executeRuleCollection != null)
			{
				foreach (ExecuteRule item3 in executeRuleCollection)
				{
					instance.DebugLine(string.Empty);
					item3.Print();
				}
			}
			if (capabilityRuleCollection == null)
			{
				return;
			}
			foreach (CapabilityRule item4 in capabilityRuleCollection)
			{
				instance.DebugLine(string.Empty);
				item4.Print();
			}
		}
	}
}
