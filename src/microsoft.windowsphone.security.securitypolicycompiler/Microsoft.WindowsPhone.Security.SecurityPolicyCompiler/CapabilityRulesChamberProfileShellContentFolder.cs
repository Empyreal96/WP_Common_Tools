namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class CapabilityRulesChamberProfileShellContentFolder : CapabilityRulesChamberProfileDefaultDataFolder
	{
		public CapabilityRulesChamberProfileShellContentFolder()
		{
		}

		public CapabilityRulesChamberProfileShellContentFolder(CapabilityOwnerType value)
			: this()
		{
			base.OwnerType = value;
		}

		protected sealed override void CompileAttributes(string appCapSID, string svcCapSID)
		{
			string empty = string.Empty;
			string empty2 = string.Empty;
			base.CompileAttributes(appCapSID, svcCapSID);
			base.Rights = "0x001200A9";
			empty = SidBuilder.BuildApplicationCapabilitySidString("ID_CAP_CHAMBER_PROFILE_DATA_SHELLCONTENT_R");
			empty2 = GlobalVariables.SidMapping["ID_CAP_CHAMBER_PROFILE_DATA_SHELLCONTENT_R"];
			base.DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { "CIOI", base.Rights, empty });
			base.DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { "CIOI", base.Rights, empty2 });
			base.DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { "CIOI", base.Rights, "S-1-5-21-2702878673-795188819-444038987-2781" });
			empty = SidBuilder.BuildApplicationCapabilitySidString("ID_CAP_CHAMBER_PROFILE_DATA_SHELLCONTENT_RWD");
			empty2 = GlobalVariables.SidMapping["ID_CAP_CHAMBER_PROFILE_DATA_SHELLCONTENT_RWD"];
			base.DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { "CIOI", "0x001301bf", empty });
			base.DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { "CIOI", "0x001301bf", empty2 });
			base.DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { "CIOI", "0x001301bf", "S-1-5-21-2702878673-795188819-444038987-2781" });
		}
	}
}
