using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BasePlugins
{
	[Export(typeof(IPkgPlugin))]
	public class WinRTHost : OSComponent
	{
		public override string XmlSchemaPath => PkgPlugin.BaseComponentSchemaPath;

		public override string XmlElementUniqueXPath => "@Id";

		protected override void ValidateEntry(IPkgProject packageGenerator, XElement componentEntry)
		{
			if (componentEntry.LocalElements("Dll").Count() != 1)
			{
				throw new PkgXmlException(componentEntry, "Only 1 Dll may be specified per WinRTHost object");
			}
			if (componentEntry.LocalElements("WinRTClass").Count() <= 0)
			{
				throw new PkgXmlException(componentEntry, "At least 1 WinRTClass must be specified in a WinRTHost object");
			}
		}

		protected override IEnumerable<PkgObject> ProcessEntry(IPkgProject packageGenerator, XElement componentEntry)
		{
			OSComponentBuilder oSComponentBuilder = new OSComponentBuilder();
			XElement source = componentEntry.LocalElement("Dll");
			string value = source.LocalAttribute("Source").Value;
			string dllDestinationDir = "$(runtime.default)";
			string dllDestinationName = Path.GetFileName(value);
			source.WithLocalAttribute("DestinationDir", delegate(XAttribute x)
			{
				dllDestinationDir = x.Value;
			});
			source.WithLocalAttribute("Name", delegate(XAttribute x)
			{
				dllDestinationName = x.Value;
			});
			string value2 = Path.Combine(dllDestinationDir, dllDestinationName);
			FileBuilder dllFile = oSComponentBuilder.AddFileGroup().AddFile(value, dllDestinationDir).SetName(dllDestinationName);
			source.WithLocalAttribute("Attributes", delegate(XAttribute x)
			{
				dllFile.SetAttributes(x.Value);
			});
			foreach (XElement item in componentEntry.LocalElements("WinRTClass"))
			{
				string value3 = item.LocalAttribute("Id").Value;
				string value4 = item.LocalAttribute("ActivatableId").Value;
				ModernComActivation modernComActivation = (ModernComActivation)Enum.Parse(typeof(ModernComActivation), item.LocalAttribute("ActivationType").Value);
				ModernComTrustLevel modernComTrustLevel = (ModernComTrustLevel)Enum.Parse(typeof(ModernComTrustLevel), item.LocalAttribute("TrustLevel").Value);
				ModernComThreading modernComThreading = (ModernComThreading)Enum.Parse(typeof(ModernComThreading), item.LocalAttribute("ThreadingModel").Value);
				RegistryKeyGroupBuilder registryKeyGroupBuilder = oSComponentBuilder.AddRegistryGroup();
				RegistryKeyBuilder registryKeyBuilder = registryKeyGroupBuilder.AddRegistryKey("$(hklm.software)\\Microsoft\\WindowsRuntime\\ActivatableClassId\\{0}", value4).AddValue("DllPath", "REG_SZ", value2).AddValue("CLSID", "REG_SZ", value3);
				int num = (int)modernComActivation;
				RegistryKeyBuilder registryKeyBuilder2 = registryKeyBuilder.AddValue("ActivationType", "REG_DWORD", num.ToString("X"));
				num = (int)modernComThreading;
				RegistryKeyBuilder registryKeyBuilder3 = registryKeyBuilder2.AddValue("Threading", "REG_DWORD", num.ToString("X"));
				num = (int)modernComTrustLevel;
				registryKeyBuilder3.AddValue("TrustLevel", "REG_DWORD", num.ToString("X"));
				registryKeyGroupBuilder.AddRegistryKey("$(hklm.software)\\Microsoft\\WindowsRuntime\\CLSID\\{0}", value3).AddValue("ActivatableClassId", "REG_SZ", value4);
			}
			ProcessFiles<OSComponentPkgObject, OSComponentBuilder>(componentEntry, oSComponentBuilder);
			ProcessRegistry<OSComponentPkgObject, OSComponentBuilder>(componentEntry, oSComponentBuilder);
			return new List<PkgObject> { oSComponentBuilder.ToPkgObject() };
		}
	}
}
