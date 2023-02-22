using System.Xml;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class CapabilityRulesInstallationFolder : CapabilityRulesDirectory
	{
		public CapabilityRulesInstallationFolder()
		{
		}

		public CapabilityRulesInstallationFolder(CapabilityOwnerType value)
			: this()
		{
			base.OwnerType = value;
		}

		protected sealed override void CompileAttributes(string appCapSID, string svcCapSID)
		{
			string empty = string.Empty;
			string empty2 = string.Empty;
			base.Rights = "0x001200A9";
			base.CompileAttributes(appCapSID, svcCapSID);
			base.DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { base.Inheritance, base.Rights, "OW" });
			base.DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { "CIOI", "FA", "SY" });
			base.DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { "CIOI", "FA", "S-1-5-80-956008885-3418522649-1831038044-1853292631-2271478464" });
			base.DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { "CIOI", "0x001200A9", "OW" });
			base.DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { "CIOI", "0x001200A9", "S-1-5-21-2702878673-795188819-444038987-2781" });
			base.DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { "CIOI", "0x001200A9", appCapSID });
			empty = SidBuilder.BuildApplicationCapabilitySidString("ID_CAP_CHAMBER_PROFILE_CODE_R");
			empty2 = GlobalVariables.SidMapping["ID_CAP_CHAMBER_PROFILE_CODE_R"];
			base.DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { "CIOI", "0x001200A9", empty2 });
			base.DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { "CIOI", "0x001200A9", empty });
			base.DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { "CIOI", "0x001200A9", "S-1-5-21-2702878673-795188819-444038987-2781" });
			empty = SidBuilder.BuildApplicationCapabilitySidString("ID_CAP_CHAMBER_PROFILE_CODE_RW");
			empty2 = GlobalVariables.SidMapping["ID_CAP_CHAMBER_PROFILE_CODE_RW"];
			base.DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { "CIOI", "0x001201bf", empty2 });
			base.DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { "CIOI", "0x001201bf", empty });
			base.DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { "CIOI", "0x001201bf", "S-1-5-21-2702878673-795188819-444038987-2781" });
		}

		protected sealed override void ValidateOutPath()
		{
			ValidateFileOutPath(true);
		}

		protected override void AddAttributes(XmlElement baseRuleXmlElement)
		{
			base.AddAttributes(baseRuleXmlElement);
			base.Flags |= 2147483648u;
		}
	}
}
