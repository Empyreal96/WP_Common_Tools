using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class Components : IPolicyElement
	{
		private List<Application> applicationCollection;

		private List<AppBinaries> appBinariesCollection;

		private List<Service> serviceCollection;

		private List<FullTrust> fullTrustCollection;

		private List<Capability> capabilityCollection;

		[XmlElement(ElementName = "Application")]
		public List<Application> ApplicationCollection => applicationCollection;

		[XmlElement(ElementName = "AppBinaries")]
		public List<AppBinaries> AppBinariesCollection => appBinariesCollection;

		[XmlElement(ElementName = "Service")]
		public List<Service> ServiceCollection => serviceCollection;

		[XmlElement(ElementName = "FullTrust")]
		public List<FullTrust> FullTrustCollection => fullTrustCollection;

		public void Add(IXPathNavigable componentXmlElement)
		{
			AddElements((XmlElement)componentXmlElement);
		}

		private void AddElements(XmlElement componentXmlElement)
		{
			AddApplications(componentXmlElement);
			AddAppBinaries(componentXmlElement);
			AddServices(componentXmlElement);
			AddFullTrust(componentXmlElement);
		}

		private void AddApplications(XmlElement componentsXmlElement)
		{
			XmlNodeList xmlNodeList = componentsXmlElement.SelectNodes("./WP_Policy:Application", GlobalVariables.NamespaceManager);
			if (xmlNodeList.Count <= 0)
			{
				return;
			}
			if (applicationCollection == null)
			{
				applicationCollection = new List<Application>();
			}
			foreach (XmlElement item in xmlNodeList)
			{
				Application application = new Application();
				application.Add(item);
				applicationCollection.Add(application);
				if (application.HasPrivateResources())
				{
					if (capabilityCollection == null)
					{
						capabilityCollection = new List<Capability>();
					}
					capabilityCollection.Add(application.PrivateResources);
				}
			}
		}

		private void AddAppBinaries(XmlElement componentsXmlElement)
		{
			XmlNodeList xmlNodeList = componentsXmlElement.SelectNodes("./WP_Policy:AppResource", GlobalVariables.NamespaceManager);
			if (xmlNodeList.Count <= 0)
			{
				return;
			}
			if (appBinariesCollection == null)
			{
				appBinariesCollection = new List<AppBinaries>();
			}
			foreach (XmlElement item in xmlNodeList)
			{
				AppBinaries appBinaries = new AppBinaries();
				appBinaries.Add(item);
				if (appBinaries.ApplicationFileCollection.Count > 0)
				{
					appBinariesCollection.Add(appBinaries);
				}
			}
		}

		private void AddServices(XmlElement componentsXmlElement)
		{
			XmlNodeList xmlNodeList = componentsXmlElement.SelectNodes("./WP_Policy:Service[@Type='Win32OwnProcess' or @Type='Win32ShareProcess']", GlobalVariables.NamespaceManager);
			if (xmlNodeList.Count <= 0)
			{
				return;
			}
			if (serviceCollection == null)
			{
				serviceCollection = new List<Service>();
			}
			foreach (XmlElement item in xmlNodeList)
			{
				Service service = new Service();
				service.Add(item);
				serviceCollection.Add(service);
				if (service.HasPrivateResources())
				{
					if (capabilityCollection == null)
					{
						capabilityCollection = new List<Capability>();
					}
					capabilityCollection.Add(service.PrivateResources);
				}
			}
		}

		private void AddFullTrust(XmlElement componentsXmlElement)
		{
			XmlNodeList xmlNodeList = componentsXmlElement.SelectNodes("./WP_Policy:FullTrust", GlobalVariables.NamespaceManager);
			if (xmlNodeList.Count <= 0)
			{
				return;
			}
			if (fullTrustCollection == null)
			{
				fullTrustCollection = new List<FullTrust>();
			}
			foreach (XmlElement item in xmlNodeList)
			{
				FullTrust fullTrust = new FullTrust();
				fullTrust.Add(item);
				fullTrustCollection.Add(fullTrust);
			}
		}

		public bool HasPrivateCapabilities()
		{
			return capabilityCollection != null;
		}

		public bool HasChild()
		{
			if ((serviceCollection == null || serviceCollection.Count <= 0) && (applicationCollection == null || applicationCollection.Count <= 0))
			{
				if (appBinariesCollection != null)
				{
					return appBinariesCollection.Count > 0;
				}
				return false;
			}
			return true;
		}

		public List<Capability> GetPrivateCapabilities()
		{
			return capabilityCollection;
		}

		public void Print()
		{
			ReportingBase instance = ReportingBase.GetInstance();
			instance.XmlElementLine(ConstantStrings.IndentationLevel1, "Components");
			if (applicationCollection != null)
			{
				foreach (Application item in applicationCollection)
				{
					instance.DebugLine(string.Empty);
					item.Print();
				}
			}
			if (appBinariesCollection != null)
			{
				foreach (AppBinaries item2 in appBinariesCollection)
				{
					instance.DebugLine(string.Empty);
					item2.Print();
				}
			}
			if (serviceCollection != null)
			{
				foreach (Service item3 in serviceCollection)
				{
					instance.DebugLine(string.Empty);
					item3.Print();
				}
			}
			if (fullTrustCollection == null)
			{
				return;
			}
			foreach (FullTrust item4 in fullTrustCollection)
			{
				instance.DebugLine(string.Empty);
				item4.Print();
			}
		}
	}
}
