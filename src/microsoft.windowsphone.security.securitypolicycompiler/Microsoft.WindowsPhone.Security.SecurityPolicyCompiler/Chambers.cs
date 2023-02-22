using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class Chambers : IPolicyElement
	{
		[XmlElement(ElementName = "Chamber")]
		public List<Chamber> ChamberCollection { get; set; }

		public void Add(IXPathNavigable chambersXmlElement)
		{
			XmlElement chambersXmlElement2 = (XmlElement)chambersXmlElement;
			AddElements(chambersXmlElement2);
		}

		public void AddElements(XmlElement chambersXmlElement)
		{
			XmlNodeList xmlNodeList = chambersXmlElement.SelectNodes("./WP_Policy:Chamber", GlobalVariables.NamespaceManager);
			if (xmlNodeList.Count <= 0)
			{
				return;
			}
			if (ChamberCollection == null)
			{
				ChamberCollection = new List<Chamber>();
			}
			foreach (XmlElement item in xmlNodeList)
			{
				Chamber chamber = new Chamber();
				chamber.Add(item);
				ChamberCollection.Add(chamber);
			}
		}

		public string GetAllChamberProperties()
		{
			string text = string.Empty;
			if (ChamberCollection != null)
			{
				foreach (Chamber item in ChamberCollection)
				{
					text += item.Name;
				}
				return text;
			}
			return text;
		}

		public virtual void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel3, "Chamber");
			foreach (Chamber item in ChamberCollection)
			{
				instance.DebugLine(string.Empty);
				item.Print();
			}
		}
	}
}
