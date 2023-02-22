using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.CsiToPkg
{
	[Export(typeof(IPkgPlugin))]
	internal class FailureActions : PkgPlugin
	{
		public override void ConvertEntries(XElement ToPkg, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromCsi)
		{
			IEnumerable<XElement> enumerable = FromCsi.Descendants(FromCsi.Name.Namespace + "action");
			if (enumerable.Count() == 0)
			{
				return;
			}
			string attributeValue = PkgBldrHelpers.GetAttributeValue(FromCsi, "resetPeriod");
			List<byte> list = new List<byte>();
			int value = 0;
			int value2 = 0;
			int value3 = Convert.ToInt32(attributeValue, CultureInfo.InvariantCulture);
			list.AddRange(BitConverter.GetBytes(value3));
			list.AddRange(BitConverter.GetBytes(value));
			list.AddRange(BitConverter.GetBytes(value2));
			list.AddRange(BitConverter.GetBytes(enumerable.Count()));
			list.AddRange(BitConverter.GetBytes(20));
			foreach (XElement item in enumerable)
			{
				int num = 0;
				string attributeValue2 = PkgBldrHelpers.GetAttributeValue(item, "type");
				string attributeValue3 = PkgBldrHelpers.GetAttributeValue(item, "delay");
				switch (attributeValue2.ToLowerInvariant())
				{
				case "none":
					num = 0;
					break;
				case "rebootmachine":
					num = 1;
					break;
				case "restartservice":
					num = 2;
					break;
				case "runcommand":
					num = 3;
					break;
				default:
					Console.WriteLine("warning: unknow service action type {0}", attributeValue2);
					continue;
				}
				list.AddRange(BitConverter.GetBytes(num));
				uint value4 = Convert.ToUInt32(attributeValue3, CultureInfo.InvariantCulture);
				list.AddRange(BitConverter.GetBytes(value4));
			}
			string value5 = BitConverter.ToString(list.ToArray()).Replace('-', ',');
			XElement content = RegHelpers.PkgRegValue("FailureActions", "REG_BINARY", value5);
			ToPkg.Add(content);
		}
	}
}
