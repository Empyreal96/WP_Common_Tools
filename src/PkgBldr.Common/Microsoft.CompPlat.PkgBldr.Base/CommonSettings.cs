using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.CompPlat.PkgBldr.Base
{
	[CompilerGenerated]
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
	internal sealed class CommonSettings : ApplicationSettingsBase
	{
		private static CommonSettings defaultInstance = (CommonSettings)SettingsBase.Synchronized(new CommonSettings());

		public static CommonSettings Default => defaultInstance;

		[ApplicationScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("True")]
		public bool ErrorOnDeconstructionFailure => (bool)this["ErrorOnDeconstructionFailure"];

		private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e)
		{
		}

		private void SettingsSavingEventHandler(object sender, CancelEventArgs e)
		{
		}
	}
}
