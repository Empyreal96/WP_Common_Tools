using System.Collections.Generic;
using Microsoft.WindowsPhone.MCSF.Offline;

namespace Microsoft.WindowsPhone.Multivariant.Offline
{
	public class MVSettingGroup
	{
		private string _policyPath;

		public string Path { get; private set; }

		public string PolicyPath
		{
			get
			{
				return _policyPath;
			}
			private set
			{
				_policyPath = PolicyMacroTable.MacroTildeToDollar(value);
			}
		}

		public List<MVSetting> Settings { get; private set; }

		public MVSettingGroup(string path, string policyPath)
		{
			Path = path;
			PolicyPath = policyPath;
			Settings = new List<MVSetting>();
		}
	}
}
