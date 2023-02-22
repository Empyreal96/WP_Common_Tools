using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Interfaces;
using Microsoft.CompPlat.PkgBldr.Tools;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.CompPlat.PkgBldr.Base.BasePlugins
{
	[Export(typeof(IPkgPlugin))]
	internal class BcdStore : PkgPlugin
	{
		public override void ConvertEntries(XElement toCsi, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement fromWm)
		{
			if (enviorn.Convert.Equals(ConversionType.pkg2csi))
			{
				enviorn.Logger.LogInfo("Cannot convert bcdStore elements directly from pkg.xml to csi.");
				return;
			}
			BcdConverter bcdConverter = new BcdConverter(new IULogger());
			string directoryName = Microsoft.CompPlat.PkgBldr.Tools.LongPath.GetDirectoryName(enviorn.Input);
			string attributeValue = PkgBldrHelpers.GetAttributeValue(fromWm, "source");
			attributeValue = directoryName + "\\" + attributeValue;
			using (Stream bcdLayoutSchema = Assembly.LoadFrom(Assembly.GetAssembly(bcdConverter.GetType()).Location).GetManifestResourceStream("BcdLayout.xsd"))
			{
				bcdConverter.ProcessInputXml(attributeValue, bcdLayoutSchema);
			}
			BcdRegData bcdRegData = new BcdRegData();
			bcdConverter.SaveToRegData(bcdRegData);
			Dictionary<string, List<BcdRegValue>> dictionary = bcdRegData.RegKeys();
			XElement xElement = new XElement(toCsi.Name.Namespace + "registryKeys");
			foreach (KeyValuePair<string, List<BcdRegValue>> item in dictionary)
			{
				XElement xElement2 = new XElement(toCsi.Name.Namespace + "registryKey");
				xElement2.Add(new XAttribute("keyName", item.Key));
				foreach (BcdRegValue item2 in item.Value)
				{
					XElement xElement3 = new XElement(toCsi.Name.Namespace + "registryValue");
					xElement3.Add(new XAttribute("name", item2.Name));
					xElement3.Add(new XAttribute("value", item2.Value));
					xElement3.Add(new XAttribute("valueType", item2.Type));
					xElement3.Add(new XAttribute("mutable", "true"));
					switch (item2.Type)
					{
					default:
						throw new PkgGenException("BcdStore can't process invalid reg type {0}", item2.Type);
					case "REG_BINARY":
					case "REG_DWORD":
					case "REG_SZ":
					case "REG_MULTI_SZ":
						break;
					}
					xElement2.Add(xElement3);
				}
				xElement.Add(xElement2);
			}
			string attributeValue2 = PkgBldrHelpers.GetAttributeValue(fromWm, "buildFilter");
			if (attributeValue2 != null)
			{
				xElement.Add(new XAttribute("buildFilter", attributeValue2));
			}
			toCsi.Add(xElement);
		}
	}
}
