using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public abstract class PathElements<T> : IPolicyElement where T : PathElement, new()
	{
		[XmlIgnore]
		public List<T> PathElementCollection { get; set; }

		protected string ElementName { get; set; }

		public PathElements()
		{
		}

		public virtual void Add(IXPathNavigable pathElementsXmlElement)
		{
			XmlElement pathElementsXmlElement2 = (XmlElement)pathElementsXmlElement;
			AddElements(pathElementsXmlElement2);
		}

		public virtual void AddElements(XmlElement pathElementsXmlElement)
		{
			XmlNodeList xmlNodeList = pathElementsXmlElement.SelectNodes("./WP_Policy:" + ElementName, GlobalVariables.NamespaceManager);
			if (xmlNodeList.Count <= 0)
			{
				return;
			}
			if (PathElementCollection == null)
			{
				PathElementCollection = new List<T>();
			}
			foreach (XmlElement item in xmlNodeList)
			{
				T val = new T();
				val.Add(item);
				PathElementCollection.Add(val);
			}
		}

		public string GetAllPaths()
		{
			string text = string.Empty;
			if (PathElementCollection != null)
			{
				foreach (T item in PathElementCollection)
				{
					text += item.Path;
				}
				return text;
			}
			return text;
		}

		public virtual void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel3, ElementName);
			foreach (T item in PathElementCollection)
			{
				instance.DebugLine(string.Empty);
				item.Print();
			}
		}
	}
}
