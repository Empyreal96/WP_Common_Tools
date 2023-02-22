namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public class CapabilityRulesChamberProfileMediaFolder : CapabilityRulesChamberProfileDefaultDataFolder
	{
		public CapabilityRulesChamberProfileMediaFolder()
		{
		}

		public CapabilityRulesChamberProfileMediaFolder(CapabilityOwnerType value)
			: this()
		{
			base.OwnerType = value;
		}

		protected sealed override void CompileAttributes(string appCapSID, string svcCapSID)
		{
			string empty = string.Empty;
			string empty2 = string.Empty;
			base.CompileAttributes(appCapSID, svcCapSID);
			base.Rights = "0x001301bf";
			empty = SidBuilder.BuildApplicationCapabilitySidString("ID_CAP_CHAMBER_PROFILE_DATA_MEDIA_RWD");
			empty2 = GlobalVariables.SidMapping["ID_CAP_CHAMBER_PROFILE_DATA_MEDIA_RWD"];
			base.DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { "CIOI", base.Rights, empty });
			base.DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { "CIOI", base.Rights, empty2 });
			base.DACL += string.Format(GlobalVariables.Culture, "(A;{0};{1};;;{2})", new object[3] { "CIOI", base.Rights, "S-1-5-21-2702878673-795188819-444038987-2781" });
		}
	}
}
