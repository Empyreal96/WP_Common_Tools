using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Base.Security;
using Microsoft.CompPlat.PkgBldr.Base.Security.SecurityPolicy;
using Microsoft.CompPlat.PkgBldr.Interfaces;
using Microsoft.CompPlat.PkgBldr.Tools;

namespace Microsoft.CompPlat.PkgBldr.Plugins
{
	[Export(typeof(IPkgPlugin))]
	internal class Assembly : PkgPlugin
	{
		private IDeploymentLogger logger;

		public override void ConvertEntries(XElement parent, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement component)
		{
			XNamespace @namespace = parent.Name.Namespace;
			GlobalSecurity globalSecurity = enviorn.GlobalSecurity;
			logger = enviorn.Logger;
			bool isNeutral = false;
			if (enviorn.build.satellite.Type == SatelliteType.Neutral)
			{
				isNeutral = true;
			}
			if (globalSecurity.SddlListsAreEmpty())
			{
				return;
			}
			enviorn.Bld.CSI.Root = parent;
			enviorn.Pass = BuildPass.PLUGIN_PASS;
			enviorn.Macros = new MacroResolver();
			enviorn.Macros.Load(XmlReader.Create(PkgGenResources.GetResourceStream("Macros_CsiToWm.xml")));
			XElement xElement = parent.Element(@namespace + "trustInfo");
			XElement xElement2 = null;
			if (xElement == null)
			{
				xElement = new XElement(@namespace + "trustInfo");
				XElement xElement3 = new XElement(@namespace + "security");
				XElement xElement4 = new XElement(@namespace + "accessControl");
				xElement2 = new XElement(@namespace + "securityDescriptorDefinitions");
				xElement.Add(xElement3);
				xElement3.Add(xElement4);
				xElement4.Add(xElement2);
				parent.Add(xElement);
			}
			xElement2 = xElement.Descendants(@namespace + "securityDescriptorDefinitions").First();
			WriteFileSddls(globalSecurity, parent, xElement2);
			if (globalSecurity.DirSddlList.Count > 0)
			{
				XElement xElement5 = parent.Element(@namespace + "directories");
				if (xElement5 == null)
				{
					xElement5 = new XElement(@namespace + "directories");
					parent.Add(xElement5);
				}
				WriteDirectorySddls(globalSecurity, xElement5, xElement2);
			}
			if (globalSecurity.RegKeySddlList.Count > 0)
			{
				XElement xElement6 = parent.Element(@namespace + "registryKeys");
				if (xElement6 == null)
				{
					xElement6 = new XElement(@namespace + "registryKeys");
					parent.Add(xElement6);
				}
				WriteRegKeySddls(globalSecurity, xElement6, xElement2, isNeutral);
			}
			WriteServiceSddls(globalSecurity, parent, xElement2);
			if (globalSecurity.SdRegValuelList.Count > 0)
			{
				XElement registryKeys = PkgBldrHelpers.AddIfNotFound(parent, "registryKeys");
				WriteRegValueSddls(globalSecurity, registryKeys, xElement2);
			}
			if (globalSecurity.WnfValueList.Count > 0)
			{
				foreach (KeyValuePair<string, WnfValue> wnfValue in globalSecurity.WnfValueList)
				{
					if (wnfValue.Value.Name != null)
					{
						logger.LogInfo("WNF security descriptors need to be added to WNF manifests! Please add the following to the WNF notification {0}\nsddl={1}", wnfValue.Value.Name, wnfValue.Value.SecurityDescriptor);
					}
					else
					{
						logger.LogInfo("WNF security descriptors need to be added to WNF manifests! Please add the following to the WNF manifest with Tag={0} for the notification with scope={1} and sequence={2}\nsddl={3}", wnfValue.Value.Tag, wnfValue.Value.Scope, wnfValue.Value.Sequence, wnfValue.Value.SecurityDescriptor);
					}
				}
			}
			if (xElement2.Elements().Count() == 0)
			{
				xElement.Remove();
			}
		}

		private void WriteRegValueSddls(GlobalSecurity gSecurity, XElement registryKeys, XElement securityDescriptorDefinitions)
		{
			foreach (KeyValuePair<string, SdRegValue> sdRegValuel in gSecurity.SdRegValuelList)
			{
				XElement xElement = new XElement(registryKeys.Name.Namespace + "registryKey");
				xElement.Add(new XAttribute("keyName", sdRegValuel.Value.GetRegPath()));
				XElement xElement2 = new XElement(xElement.Name.Namespace + "registryValue");
				xElement2.Add(new XAttribute("name", sdRegValuel.Value.GetRegValueName()));
				xElement2.Add(new XAttribute("valueType", sdRegValuel.Value.RegValueType));
				xElement2.Add(new XAttribute("value", sdRegValuel.Value.GetRegValue()));
				xElement.Add(xElement2);
				if (sdRegValuel.Value.HasAdditionalValue())
				{
					XElement xElement3 = new XElement(xElement.Name.Namespace + "registryValue");
					xElement3.Add(new XAttribute("name", sdRegValuel.Value.GetRegValueName(true)));
					xElement3.Add(new XAttribute("valueType", sdRegValuel.Value.RegValueType));
					xElement3.Add(new XAttribute("value", sdRegValuel.Value.GetRegValue(true)));
					xElement.Add(xElement3);
				}
				registryKeys.Add(xElement);
			}
		}

		private void WriteServiceSddls(GlobalSecurity gSecurity, XElement Parent, XElement securityDescriptorDefinitions)
		{
			foreach (KeyValuePair<string, string> serviceSddl in gSecurity.ServiceSddlList)
			{
				XElement xElement = FindMatchingAttribute(Parent, "serviceData", "name", serviceSddl.Key);
				if (xElement == null)
				{
					logger.LogWarning("TBD: Error, sddl ref to non existing service {0}", serviceSddl.Key);
					continue;
				}
				if (xElement != null && xElement.Element(Parent.Name.Namespace + "securityDescriptor") != null)
				{
					logger.LogInfo("TBD: Merge new Service SDDL with the existing security descriptor value");
					continue;
				}
				string value = HashCalculator.CalculateSha256Hash(serviceSddl.Key);
				xElement.Add(new XElement(Parent.Name.Namespace + "securityDescriptor", new XAttribute("name", value)));
				XElement xElement2 = new XElement(Parent.Name.Namespace + "securityDescriptorDefinition");
				xElement2.Add(new XAttribute("name", value));
				xElement2.Add(new XAttribute("sddl", serviceSddl.Value));
				securityDescriptorDefinitions.Add(xElement2);
			}
		}

		private void WriteFileSddls(GlobalSecurity gSecurity, XElement Parent, XElement securityDescriptorDefinitions)
		{
			foreach (KeyValuePair<string, string> fileSddl in gSecurity.FileSddlList)
			{
				string directoryName = LongPath.GetDirectoryName(fileSddl.Key);
				string fileName = LongPath.GetFileName(fileSddl.Key);
				XElement xElement = FindMatchingChildAttributes(Parent, "file", "destinationPath", directoryName, "name", fileName);
				if (xElement == null)
				{
					logger.LogWarning("TBD: Error, sddl ref to non existing file {0}", fileName);
					continue;
				}
				if (xElement != null && xElement.Element(xElement.Name.Namespace + "securityDescriptor") != null)
				{
					logger.LogInfo("TBD: Merge new File SDDL with the existing security descriptor value");
					continue;
				}
				string value = HashCalculator.CalculateSha256Hash(fileSddl.Key);
				xElement.Add(new XElement(Parent.Name.Namespace + "securityDescriptor", new XAttribute("name", value)));
				XElement xElement2 = new XElement(Parent.Name.Namespace + "securityDescriptorDefinition");
				xElement2.Add(new XAttribute("name", value));
				xElement2.Add(new XAttribute("sddl", fileSddl.Value));
				securityDescriptorDefinitions.Add(xElement2);
			}
		}

		private void WriteDirectorySddls(GlobalSecurity gSecurity, XElement csiDirectories, XElement securityDescriptorDefinitions)
		{
			foreach (KeyValuePair<string, string> dirSddl in gSecurity.DirSddlList)
			{
				XElement xElement = FindMatchingChildAttribute(csiDirectories, "directory", "destinationPath", dirSddl.Key);
				if (xElement != null)
				{
					if (xElement.Element(csiDirectories.Name.Namespace + "securityDescriptor") != null)
					{
						logger.LogInfo("TBD: Merge new Directory SDDL with the existing security descriptor value");
						continue;
					}
				}
				else
				{
					xElement = new XElement(csiDirectories.Name.Namespace + "directory");
					xElement.Add(new XAttribute("destinationPath", dirSddl.Key));
					csiDirectories.Add(xElement);
				}
				string value = HashCalculator.CalculateSha256Hash(dirSddl.Key);
				xElement.Add(new XElement(csiDirectories.Name.Namespace + "securityDescriptor", new XAttribute("name", value)));
				XElement xElement2 = new XElement(csiDirectories.Name.Namespace + "securityDescriptorDefinition");
				xElement2.Add(new XAttribute("name", value));
				xElement2.Add(new XAttribute("sddl", dirSddl.Value));
				securityDescriptorDefinitions.Add(xElement2);
			}
		}

		private void WriteRegKeySddls(GlobalSecurity gSecurity, XElement csiRegKeys, XElement securityDescriptorDefinitions, bool isNeutral)
		{
			foreach (KeyValuePair<string, string> regKeySddl in gSecurity.RegKeySddlList)
			{
				XElement xElement = FindMatchingChildAttribute(csiRegKeys, "registryKey", "keyName", regKeySddl.Key);
				if (xElement != null)
				{
					if (xElement.Element(csiRegKeys.Name.Namespace + "securityDescriptor") != null)
					{
						logger.LogInfo("TBD: Merge new RegKey SDDL with the existing security descriptor value");
						continue;
					}
				}
				else if (isNeutral)
				{
					xElement = new XElement(csiRegKeys.Name.Namespace + "registryKey");
					xElement.Add(new XAttribute("keyName", regKeySddl.Key));
					csiRegKeys.Add(xElement);
				}
				if (xElement != null)
				{
					string value = HashCalculator.CalculateSha256Hash(regKeySddl.Key);
					xElement.Add(new XElement(csiRegKeys.Name.Namespace + "securityDescriptor", new XAttribute("name", value)));
					XElement xElement2 = new XElement(csiRegKeys.Name.Namespace + "securityDescriptorDefinition");
					xElement2.Add(new XAttribute("name", value));
					xElement2.Add(new XAttribute("sddl", regKeySddl.Value));
					securityDescriptorDefinitions.Add(xElement2);
				}
			}
		}

		public XElement FindMatchingChildAttribute(XElement Parent, string ElementName, string AttributeName, string AttributeValue)
		{
			XElement result = null;
			IEnumerable<XElement> enumerable = from el in Parent.Elements(Parent.Name.Namespace + ElementName)
				where el.Attribute(AttributeName).Value.Equals(AttributeValue, StringComparison.OrdinalIgnoreCase)
				select el;
			if (enumerable != null && enumerable.Count() > 0)
			{
				result = enumerable.First();
			}
			return result;
		}

		public XElement FindMatchingAttribute(XElement Parent, string ElementName, string AttributeName, string AttributeValue)
		{
			XElement result = null;
			IEnumerable<XElement> enumerable = from el in Parent.Descendants(Parent.Name.Namespace + ElementName)
				where el.Attribute(AttributeName).Value.Equals(AttributeValue, StringComparison.OrdinalIgnoreCase)
				select el;
			if (enumerable != null && enumerable.Count() > 0)
			{
				result = enumerable.First();
			}
			return result;
		}

		public XElement FindMatchingChildAttributes(XElement Parent, string ElementName, string AttributeName1, string AttributeValue1, string AttributeName2, string AttributeValue2)
		{
			XElement result = null;
			IEnumerable<XElement> enumerable = from el in Parent.Elements(Parent.Name.Namespace + ElementName)
				where el.Attribute(AttributeName1).Value.Equals(AttributeValue1, StringComparison.OrdinalIgnoreCase) && el.Attribute(AttributeName2).Value.Equals(AttributeValue2, StringComparison.OrdinalIgnoreCase)
				select el;
			if (enumerable != null && enumerable.Count() > 0)
			{
				result = enumerable.First();
			}
			return result;
		}
	}
}
