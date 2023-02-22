using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Interfaces;

namespace Microsoft.CompPlat.PkgBldr.Plugins.CsiToPkg
{
	[Export(typeof(IPkgPlugin))]
	internal class Assembly : PkgPlugin
	{
		private static readonly HashAlgorithm Sha256Algorithm = HashAlgorithm.Create("SHA256");

		public override void ConvertEntries(XElement ToPkg, Dictionary<string, IPkgPlugin> plugins, Config enviorn, XElement FromCsi)
		{
			if (enviorn.AutoGenerateOutput)
			{
				Console.WriteLine("error: auto file generation not supported for csi2pkg conversions");
				return;
			}
			MyContainter myContainter = new MyContainter();
			XmlReader macros = XmlReader.Create(PkgGenResources.GetResourceStream("Macros_Policy.xml"));
			myContainter.Security = new Security();
			myContainter.Security.LoadPolicyMacros(macros);
			enviorn.Macros = new MacroResolver();
			enviorn.Macros.Load(XmlReader.Create(PkgGenResources.GetResourceStream("Macros_CsiToPkg.xml")));
			enviorn.Pass = BuildPass.PLUGIN_PASS;
			enviorn.arg = myContainter;
			XElement xElement = new XElement(ToPkg.Name.Namespace + "Components");
			XElement xElement2 = new XElement(ToPkg.Name.Namespace + "OSComponent");
			XElement xElement3 = new XElement(ToPkg.Name.Namespace + "Files");
			XElement xElement4 = new XElement(ToPkg.Name.Namespace + "RegKeys");
			xElement2.Add(xElement3);
			xElement2.Add(xElement4);
			xElement.Add(xElement2);
			ToPkg.Add(xElement);
			myContainter.Files = xElement3;
			myContainter.RegKeys = xElement4;
			BuildPass[] array = (BuildPass[])Enum.GetValues(typeof(BuildPass));
			foreach (BuildPass pass in array)
			{
				enviorn.Pass = pass;
				base.ConvertEntries(ToPkg, plugins, enviorn, FromCsi);
			}
			if (myContainter.Security.HavePolicyData)
			{
				XElement xElement5 = Share.CreatePolicyXmlRoot(myContainter.Security.PolicyID);
				XElement xElement6 = new XElement(xElement5.Name.Namespace + "Rules");
				xElement5.Add(xElement6);
				AddPolicyData(xElement6, "File", myContainter.Security.FileACLs);
				AddPolicyData(xElement6, "Directory", myContainter.Security.DirACLs);
				AddPolicyData(xElement6, "RegKey", myContainter.Security.RegACLs);
				XDocument xDocument = new XDocument(xElement5);
				string output = enviorn.Output;
				output = Regex.Replace(output, ".pkg.xml", ".policy.xml", RegexOptions.IgnoreCase);
				Console.WriteLine("writing {0}", output);
				xDocument.Save(output);
			}
			int num = myContainter.Files.Elements().Count();
			int num2 = myContainter.RegKeys.Elements().Count();
			int num3 = num + num2;
			if (num == 0)
			{
				myContainter.Files.Remove();
			}
			if (num2 == 0)
			{
				myContainter.RegKeys.Remove();
			}
			if (num3 == 0)
			{
				enviorn.ExitStatus = ExitStatus.SKIPPED;
			}
			else
			{
				enviorn.ExitStatus = ExitStatus.SUCCESS;
			}
		}

		private void AddPolicyData(XElement Rules, string RuleType, Dictionary<string, SDDL> SddlTable)
		{
			foreach (KeyValuePair<string, SDDL> item in SddlTable)
			{
				string key = item.Key;
				SDDL value = item.Value;
				string text = null;
				if (key.StartsWith("$(", StringComparison.OrdinalIgnoreCase))
				{
					Console.WriteLine("error: cant resolve policy path {0}", key);
					continue;
				}
				XElement xElement = new XElement(Rules.Name.Namespace + RuleType);
				xElement.Add(new XAttribute("Path", key));
				if (value.Owner != null)
				{
					text = "O:" + value.Owner;
				}
				if (value.Group != null)
				{
					text = text + "G:" + value.Group;
				}
				if (text != null)
				{
					xElement.Add(new XAttribute("Owner", text));
				}
				if (value.Dacl != null)
				{
					xElement.Add(new XAttribute("DACL", value.Dacl));
				}
				if (value.Sacl != null)
				{
					xElement.Add(new XAttribute("SACL", value.Sacl));
				}
				string attributeHash = GetAttributeHash(key, RuleType, value);
				string elementId = GetElementId(key, RuleType);
				xElement.Add(new XAttribute("ElementID", elementId));
				xElement.Add(new XAttribute("AttributeHash", attributeHash));
				Rules.Add(xElement);
			}
		}

		private string GetAttributeHash(string Path, string RuleType, SDDL Ace)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(RuleType);
			stringBuilder.Append(Path);
			if (Ace.Owner != null)
			{
				stringBuilder.Append(Ace.Owner);
			}
			if (Ace.Group != null)
			{
				stringBuilder.Append(Ace.Group);
			}
			if (Ace.Dacl != null)
			{
				stringBuilder.Append(Ace.Dacl);
			}
			return GetSha256Hash(Encoding.Unicode.GetBytes(stringBuilder.ToString()));
		}

		private string GetElementId(string Path, string RuleType)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(RuleType);
			stringBuilder.Append(Path);
			return GetSha256Hash(Encoding.Unicode.GetBytes(stringBuilder.ToString()));
		}

		private static string GetSha256Hash(byte[] buffer)
		{
			return BitConverter.ToString(Sha256Algorithm.ComputeHash(buffer)).Replace("-", string.Empty);
		}
	}
}
