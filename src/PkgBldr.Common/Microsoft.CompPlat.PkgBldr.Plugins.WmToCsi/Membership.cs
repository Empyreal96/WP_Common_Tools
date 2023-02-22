using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;

namespace Microsoft.CompPlat.PkgBldr.Plugins.WmToCsi
{
	public static class Membership
	{
		public static XElement Add(XElement toCsi, string buildFilter, string subCategory, string name, string version, string publicKeyToken, string typeName)
		{
			XElement xElement = PkgBldrHelpers.AddIfNotFound(toCsi, "memberships");
			XElement xElement2 = new XElement(toCsi.Name.Namespace + "categoryMembership");
			xElement.Add(xElement2);
			if (buildFilter != null)
			{
				xElement2.Add(new XAttribute("buildFilter", buildFilter));
			}
			XElement xElement3 = new XElement(toCsi.Name.Namespace + "id");
			xElement2.Add(xElement3);
			xElement3.Add(new XAttribute("name", name));
			xElement3.Add(new XAttribute("version", version));
			xElement3.Add(new XAttribute("publicKeyToken", publicKeyToken));
			xElement3.Add(new XAttribute("typeName", typeName));
			XElement xElement4 = new XElement(toCsi.Name.Namespace + "categoryInstance");
			if (subCategory != null)
			{
				xElement4.Add(new XAttribute("subcategory", subCategory));
			}
			xElement2.Add(xElement4);
			return xElement4;
		}
	}
}
