using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;

namespace Microsoft.CompPlat.PkgBldr.Plugins.CsiToPkg
{
	public static class Share
	{
		public class PhoneIdentity
		{
			public string Owner { get; set; }

			public string OwnerType { get; set; }

			public string Component { get; set; }

			public string SubComponent { get; set; }
		}

		public static XElement CreatePolicyXmlRoot(string PolicyID)
		{
			XElement xElement = new XElement((XNamespace)"urn:Microsoft.WindowsPhone/PhoneSecurityPolicyInternal.v8.00" + "PhoneSecurityPolicy");
			xElement.Add(new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"));
			xElement.Add(new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"));
			xElement.Add(new XAttribute("PackageID", PolicyID));
			xElement.Add(new XAttribute("Description", "Mobile Core Policy"));
			xElement.Add(new XAttribute("Vendor", "Microsoft"));
			xElement.Add(new XAttribute("RequiredOSVersion", "8.00"));
			xElement.Add(new XAttribute("FileVersion", "8.00"));
			xElement.Add(new XAttribute("HashType", "Sha2"));
			return xElement;
		}

		public static void MergeNewPkgRegKey(XElement RegKeys, XElement RegKey)
		{
			string attributeValue = PkgBldrHelpers.GetAttributeValue(RegKey, "KeyName");
			XElement xElement = PkgBldrHelpers.FindMatchingAttribute(RegKeys, "RegKey", "KeyName", attributeValue);
			if (xElement != null)
			{
				foreach (XElement item in RegKey.Elements(RegKeys.Name.Namespace + "RegValue"))
				{
					string attributeValue2 = PkgBldrHelpers.GetAttributeValue(item, "Name");
					if (PkgBldrHelpers.FindMatchingAttribute(xElement, "RegValue", "Name", attributeValue2) != null)
					{
						Console.WriteLine("error: duplicate RegValue Name {0}", attributeValue2);
					}
					else
					{
						xElement.Add(item);
					}
				}
				return;
			}
			RegKeys.Add(RegKey);
		}

		public static PhoneIdentity CsiNameToPhoneIdentity(string csiName)
		{
			PhoneIdentity phoneIdentity = new PhoneIdentity();
			string text = null;
			Regex regex = new Regex("_lang_([A-Za-z]+\\-[A-Za-z]+)", RegexOptions.IgnoreCase);
			Match match = regex.Match(csiName);
			if (match.Success)
			{
				text = match.Value;
				csiName = regex.Replace(csiName, "");
			}
			csiName = Regex.Replace(csiName, "-+", ".");
			csiName = Regex.Replace(csiName, "_+", ".");
			csiName = Regex.Replace(csiName, "microsoft", "", RegexOptions.IgnoreCase);
			csiName = Regex.Replace(csiName, "windows", "", RegexOptions.IgnoreCase);
			csiName = Regex.Replace(csiName, "package", "", RegexOptions.IgnoreCase);
			csiName = Regex.Replace(csiName, "deployment", "", RegexOptions.IgnoreCase);
			csiName = Regex.Replace(csiName, "product", "", RegexOptions.IgnoreCase);
			csiName = csiName.Trim('.');
			csiName = Regex.Replace(csiName, "\\.+", ".");
			phoneIdentity.Owner = "Microsoft";
			phoneIdentity.OwnerType = "Microsoft";
			string[] array = csiName.Split('.');
			if (array.Length < 2)
			{
				phoneIdentity.SubComponent = csiName;
				phoneIdentity.Component = "OneCore";
				return phoneIdentity;
			}
			string text2 = "";
			phoneIdentity.Component = array[0];
			for (int i = 1; i < array.Length; i++)
			{
				text2 = text2 + array[i] + ".";
			}
			phoneIdentity.SubComponent = text2.Trim('.');
			if (text != null)
			{
				phoneIdentity.SubComponent += text;
			}
			return phoneIdentity;
		}
	}
}
