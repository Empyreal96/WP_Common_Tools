using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;
using Microsoft.CompPlat.PkgBldr.Tools;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class RegKey : PkgPlugin
	{
		private IDeploymentLogger m_logger;

		public override void ConvertEntries(XElement ToWm, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromPkg)
		{
			m_logger = enviorn.Logger;
			string text = enviorn.Macros.Resolve(PkgBldrHelpers.GetAttributeValue(FromPkg, "KeyName"));
			string attributeValue = PkgBldrHelpers.GetAttributeValue(FromPkg, "buildFilter");
			string attributeValue2 = PkgBldrHelpers.GetAttributeValue(FromPkg.Parent, "buildFilter");
			if (Regex.IsMatch(text, ".+\\\\WINEVT\\\\Publishers\\\\\\{.+\\}$", RegexOptions.IgnoreCase))
			{
				if (attributeValue != null || attributeValue2 != null)
				{
					string arg = attributeValue;
					if (attributeValue2 != null)
					{
						arg = attributeValue2;
					}
					m_logger.LogWarning("Can't use buildFilter on ETW intrumentation elements until wm.xml build filter pre-proccessing is supported");
					m_logger.LogWarning($"Removing the buildFilter {arg} from {text}");
				}
				string value = Regex.Match(text, "\\{.+\\}$").Value;
				FromPkg.Descendants(FromPkg.Name.Namespace + "RegValue");
				string text2 = null;
				string text3 = null;
				string text4 = null;
				XElement xElement = PkgBldrHelpers.FindMatchingAttribute(FromPkg, "RegValue", "Name", "@");
				if (xElement != null)
				{
					text2 = xElement.Attribute("Value").Value;
				}
				XElement xElement2 = PkgBldrHelpers.FindMatchingAttribute(FromPkg, "RegValue", "Name", "ResourceFileName");
				if (xElement2 != null)
				{
					text3 = xElement2.Attribute("Value").Value;
				}
				XElement xElement3 = PkgBldrHelpers.FindMatchingAttribute(FromPkg, "RegValue", "Name", "MessageFileName");
				if (xElement3 != null)
				{
					text4 = xElement3.Attribute("Value").Value;
				}
				if (string.IsNullOrEmpty(text2) || string.IsNullOrEmpty(text3) || string.IsNullOrEmpty(text4))
				{
					m_logger.LogWarning("Can't convert {0}", text);
					return;
				}
				text4 = RemoveAndReplaceBuildMacros(text4);
				text3 = RemoveAndReplaceBuildMacros(text3);
				XNamespace xNamespace = (XNamespace)"http://manifests.microsoft.com/win/2004/08/windows/events";
				XElement parent = XElement.Parse(string.Format(CultureInfo.InvariantCulture, "\r\n                      <instrumentation\r\n                          xmlns:win=\"http://manifests.microsoft.com/win/2004/08/windows/events\"\r\n                          xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\r\n                          xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\r\n                        >\r\n                        <events xmlns=\"http://schemas.microsoft.com/win/2004/08/events\">\r\n                          <provider \r\n                            name=\"{0}\"\r\n                            guid=\"{1}\"\r\n                            symbol=\"{2}\"\r\n                            resourceFileName=\"{3}\"\r\n                            messageFileName=\"{4}\">\r\n                            <channels/>\r\n                         </provider>\r\n                        </events>\r\n                      </instrumentation>", text2, value, text2.ToUpperInvariant(), text3, text4));
				PkgBldrHelpers.ReplaceDefaultNameSpace(ref parent, parent.Name.Namespace, ToWm.Name.Namespace);
				enviorn.Bld.WM.Root.Add(parent);
			}
			else
			{
				XElement xElement4 = new XElement(ToWm.Name.Namespace + "regKey");
				if (attributeValue != null)
				{
					string value2 = Helpers.ConvertBuildFilter(attributeValue);
					xElement4.Add(new XAttribute("buildFilter", value2));
				}
				xElement4.Add(new XAttribute("keyName", text));
				AddTrustIfNeeded(text, xElement4, enviorn.Bld.WM.Root);
				ToWm.Add(xElement4);
				base.ConvertEntries(xElement4, plugins, enviorn, FromPkg);
			}
		}

		private string RemoveAndReplaceBuildMacros(string etwFileName)
		{
			bool flag = false;
			Match match = Regex.Match(etwFileName, "^.*\\\\");
			if (match.Success)
			{
				if (match.Value.ToLowerInvariant() == "$(runtime.system32)\\")
				{
					etwFileName = etwFileName.Replace(match.Value, "%systemroot%\\system32\\");
				}
				else
				{
					flag = true;
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				m_logger.LogWarning($"Can't convert ETW messageFilename and/or resourceFileName from {etwFileName} to %systemroot%\\system32\\");
				m_logger.LogWarning($"Replacing {match.Value} with ****TBD**** in the output wm.xml");
				etwFileName = ((!match.Success) ? (etwFileName = "****TBD****\\" + etwFileName) : etwFileName.Replace(match.Value, "****TBD****\\"));
			}
			return etwFileName;
		}

		private void AddTrustIfNeeded(string keyName, XElement wmRegKey, XElement root)
		{
			bool flag = false;
			if (keyName.ToLowerInvariant().Contains("$(hkcr.root)\\interface\\"))
			{
				flag = true;
			}
			if (keyName.ToLowerInvariant().Contains("$(hkcr.classes)"))
			{
				flag = true;
			}
			if (keyName.ToLowerInvariant().Contains("$(hklm.software)\\classes"))
			{
				flag = true;
			}
			if (flag)
			{
				XElement xElement = PkgBldrHelpers.AddIfNotFound(root, "trustInfo");
				if ((from e in xElement.Descendants()
					where RegDefaultSddl(e)
					select e).Count() == 0)
				{
					XElement xElement2 = PkgBldrHelpers.AddIfNotFound(PkgBldrHelpers.AddIfNotFound(PkgBldrHelpers.AddIfNotFound(PkgBldrHelpers.AddIfNotFound(xElement, "security"), "accessControl"), "securityDescriptorDefinitions"), "securityDescriptorDefinition");
					xElement2.Add(new XAttribute("name", "WRP_REGKEY_DEFAULT_SDDL"));
					xElement2.Add(new XAttribute("sddl", "$(build.wrpRegKeySddl)"));
				}
				wmRegKey.Add(new XAttribute("securityDescriptor", "WRP_REGKEY_DEFAULT_SDDL"));
			}
		}

		private bool RegDefaultSddl(XElement e)
		{
			if (e.Name.LocalName.Equals("securityDescriptorDefinition") && PkgBldrHelpers.GetAttributeValue(e, "name") == "WRP_REGKEY_DEFAULT_SDDL")
			{
				return true;
			}
			return false;
		}
	}
}
