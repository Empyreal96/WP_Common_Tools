using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	[Export(typeof(IPkgPlugin))]
	internal class ComServer : PkgPlugin
	{
		public override void ConvertEntries(XElement toWm, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement fromPkg)
		{
			XElement parent = PkgBldrHelpers.AddIfNotFound(toWm, "COM");
			XElement xElement = PkgBldrHelpers.AddIfNotFound(parent, "servers");
			XElement xElement2 = PkgBldrHelpers.AddIfNotFound(parent, "interfaces");
			XElement xElement3 = new XElement(toWm.Name.Namespace + "inProcServer");
			xElement.Add(xElement3);
			ComData comData = new ComData();
			comData.InProcServer = xElement3;
			comData.Interfaces = xElement2;
			comData.InProcServerClasses = PkgBldrHelpers.AddIfNotFound(xElement3, "classes");
			comData.RegKeys = new XElement(toWm.Name.Namespace + "regKeys");
			comData.Files = new XElement(toWm.Name.Namespace + "files");
			object arg = enviorn.arg;
			enviorn.arg = comData;
			base.ConvertEntries(toWm, plugins, enviorn, fromPkg);
			if (comData.Files.HasElements)
			{
				enviorn.Bld.WM.Root.Add(comData.Files);
			}
			if (comData.RegKeys.HasElements)
			{
				enviorn.Bld.WM.Root.Add(comData.RegKeys);
			}
			AddInterfaces(toWm, enviorn, fromPkg);
			if (!xElement2.HasElements)
			{
				xElement2.Remove();
			}
			if (!xElement.HasElements)
			{
				xElement.Remove();
			}
			enviorn.arg = arg;
		}

		private void AddInterfaces(XElement toWm, Config env, XElement fromPkg)
		{
			ComData comData = (ComData)env.arg;
			XElement xElement = fromPkg.Element(fromPkg.Name.Namespace + "Interfaces");
			if (xElement == null)
			{
				return;
			}
			foreach (XElement item in xElement.Elements(fromPkg.Name.Namespace + "Interface"))
			{
				XElement xElement2 = new XElement(toWm.Name.Namespace + "interface");
				foreach (XAttribute item2 in item.Attributes())
				{
					item2.Value = env.Macros.Resolve(item2.Value);
					switch (item2.Name.LocalName)
					{
					case "Name":
						xElement2.Add(new XAttribute("name", item2.Value));
						break;
					case "ProxyStubClsId":
					case "ProxyStubClsId32":
						xElement2.Add(new XAttribute("proxyStubClsId", item2.Value));
						break;
					case "Id":
						xElement2.Add(new XAttribute("id", item2.Value));
						break;
					case "TypeLib":
						PkgBldrHelpers.AddIfNotFound(xElement2, "typeLib").Add(new XAttribute("id", item2.Value));
						break;
					case "Version":
						PkgBldrHelpers.AddIfNotFound(xElement2, "typeLib").Add(new XAttribute("version", item2.Value));
						break;
					default:
						env.Logger.LogWarning("invalid COM interface attribute {0}", item2.Name.LocalName);
						break;
					case "NumMethods":
						break;
					}
				}
				comData.Interfaces.Add(xElement2);
			}
		}
	}
}
