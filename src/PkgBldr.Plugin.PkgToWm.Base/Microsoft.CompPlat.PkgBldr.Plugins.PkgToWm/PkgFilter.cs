using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Base.Security.SecurityPolicy;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class PkgFilter : PkgPlugin
	{
		private static List<XElement> m_filterList = new List<XElement>();

		private static List<string> m_langFilterList = new List<string>();

		private static List<string> m_resFilterList = new List<string>();

		private static List<XElement> m_RegKeys = new List<XElement>();

		public override void ConvertEntries(XElement toFilteredPkg, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement fromUnFilteredPkg)
		{
			enviorn.ExitStatus = ExitStatus.SUCCESS;
			if (enviorn.build.wow == Build.WowType.guest)
			{
				AppendGuestToSubCompName(fromUnFilteredPkg);
			}
			toFilteredPkg.Name = fromUnFilteredPkg.Name;
			toFilteredPkg.Add(fromUnFilteredPkg.Attributes());
			XElement xElement = new XElement(fromUnFilteredPkg);
			enviorn.Bld.PKG.Root = xElement;
			DoWowFiltering(xElement, enviorn);
			toFilteredPkg.Attribute("BuildWow").Remove();
			toFilteredPkg.Add(xElement.Elements());
			if (enviorn.build.wow == Build.WowType.guest && toFilteredPkg.Descendants(toFilteredPkg.Name.Namespace + "SettingsGroup").Count() > 0)
			{
				throw new PkgGenException("<SettingsGroup> must be filtered out of guest packages using buildFilter=\"not wow\"");
			}
		}

		private void DoWowFiltering(XElement filteredPkg, Config environ)
		{
			m_filterList.Clear();
			m_langFilterList.Clear();
			m_resFilterList.Clear();
			m_RegKeys.Clear();
			PopulateWowFilterList(filteredPkg);
			foreach (XElement filter in m_filterList)
			{
				string archAsString = environ.Bld.ArchAsString;
				bool state = false;
				if (environ.build.wow == Build.WowType.guest)
				{
					state = true;
				}
				string attributeValue = PkgBldrHelpers.GetAttributeValue(filter, "buildFilter");
				BooleanExpressionEvaluator booleanExpressionEvaluator = new BooleanExpressionEvaluator();
				string expressionPattern = "^([archarmx86amd64notandorwow=\\)\\(\\s]+)$";
				booleanExpressionEvaluator.expressionPattern = expressionPattern;
				booleanExpressionEvaluator.Set("arch", true);
				booleanExpressionEvaluator.Set("wow", state);
				booleanExpressionEvaluator.Set(archAsString, true);
				string text = booleanExpressionEvaluator.Evaluate(attributeValue);
				if (text == null)
				{
					throw new PkgGenException("Invalid build filter {0}", attributeValue);
				}
				filter.Attribute("buildFilter").Remove();
				if (text == "false")
				{
					foreach (XElement langResElement in GetLangResElements(filter))
					{
						string attributeValue2 = PkgBldrHelpers.GetAttributeValue(langResElement, "Resolution");
						string attributeValue3 = PkgBldrHelpers.GetAttributeValue(langResElement, "Language");
						if (attributeValue2 != null)
						{
							AddRootRegResElement(attributeValue2, environ.Bld.PKG.Root);
						}
						if (attributeValue3 != null)
						{
							AddRootRegLangElement(attributeValue3, environ.Bld.PKG.Root);
						}
					}
					filter.Remove();
				}
				Prune(filteredPkg);
			}
			if (m_RegKeys.Count <= 0)
			{
				return;
			}
			XElement xElement = PkgBldrHelpers.AddIfNotFound(PkgBldrHelpers.AddIfNotFound(filteredPkg, "Components"), "OSComponent");
			foreach (XElement regKey in m_RegKeys)
			{
				xElement.Add(regKey);
			}
		}

		private static IEnumerable<XElement> GetLangResElements(XElement element)
		{
			return from ele in element.DescendantsAndSelf(element.Name.Namespace + "Files").Concat(element.DescendantsAndSelf(element.Name.Namespace + "RegKeys"))
				where ele.Attribute("Language") != null || ele.Attribute("Resolution") != null
				select ele;
		}

		private static void AppendGuestToSubCompName(XElement Pkg)
		{
			XAttribute xAttribute = Pkg.Attribute("SubComponent");
			if (xAttribute == null)
			{
				Pkg.Add(new XAttribute("SubComponent", "Guest"));
			}
			else if (xAttribute.Value.Trim().Equals(string.Empty))
			{
				xAttribute.Value = "Guest";
			}
			else
			{
				xAttribute.Value += ".Guest";
			}
		}

		private void AddRootRegLangElement(string languageFilter, XElement pkgRoot)
		{
			if (!m_langFilterList.Contains(languageFilter.ToLowerInvariant()))
			{
				m_langFilterList.Add(languageFilter.ToLowerInvariant());
				XElement xElement = new XElement(pkgRoot.Name.Namespace + "RegKeys");
				xElement.Add(new XAttribute("Language", languageFilter));
				m_RegKeys.Add(xElement);
				XElement xElement2 = new XElement(pkgRoot.Name.Namespace + "RegKey");
				string text = GenerateUniqueKey(pkgRoot, languageFilter);
				xElement2.Add(new XAttribute("KeyName", "$(hklm.microsoft)\\PkgGen\\Lang\\$(langid)\\" + text));
				xElement.Add(xElement2);
			}
		}

		private void AddRootRegResElement(string resFilter, XElement pkgRoot)
		{
			if (!m_resFilterList.Contains(resFilter.ToLowerInvariant()))
			{
				m_resFilterList.Add(resFilter.ToLowerInvariant());
				XElement xElement = new XElement(pkgRoot.Name.Namespace + "RegKeys");
				xElement.Add(new XAttribute("Resolution", resFilter));
				m_RegKeys.Add(xElement);
				XElement xElement2 = new XElement(pkgRoot.Name.Namespace + "RegKey");
				string text = GenerateUniqueKey(pkgRoot, resFilter);
				xElement2.Add(new XAttribute("KeyName", "$(hklm.microsoft)\\PkgGen\\Res\\$(resid)\\" + text));
				xElement.Add(xElement2);
			}
		}

		private string GenerateUniqueKey(XElement pkgElement, string filter)
		{
			string text = filter;
			foreach (XAttribute item in pkgElement.Attributes())
			{
				switch (item.Name.LocalName)
				{
				case "Component":
				case "Owner":
				case "OwnerType":
				case "ReleaseType":
				case "SubComponent":
					text += item.Value.ToLowerInvariant();
					break;
				}
			}
			return HashCalculator.CalculateSha1Hash(text);
		}

		private void PopulateWowFilterList(XElement parent)
		{
			string localName = parent.Name.LocalName;
			if (PkgBldrHelpers.GetAttributeValue(parent, "buildFilter") != null)
			{
				m_filterList.Add(parent);
			}
			foreach (XElement item in parent.Elements())
			{
				PopulateWowFilterList(item);
			}
		}

		private void Prune(XElement root)
		{
			foreach (XElement item in root.Elements())
			{
				Prune(item);
			}
			if (!root.HasElements && !root.HasAttributes)
			{
				root.Remove();
			}
		}
	}
}
