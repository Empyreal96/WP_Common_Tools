using System.Globalization;
using System.Security.AccessControl;
using System.Text;

namespace Microsoft.CompPlat.PkgBldr.Base.Security.SecurityPolicy
{
	public class SdRegValue
	{
		private SdRegType m_Type;

		private string m_Name;

		private string m_QualifyingType;

		public string Value { get; set; }

		public string AdditionalValue { get; set; }

		public RegistryValueType RegValueType { get; set; }

		public SdRegValue(SdRegType SdRegValueType, string Name)
		{
			Init(SdRegValueType, Name, null, false);
		}

		public SdRegValue(SdRegType SdRegValueType, string Name, string QualifyingType, bool IsString)
		{
			Init(SdRegValueType, Name, QualifyingType, IsString);
		}

		private void Init(SdRegType SdRegValueType, string Name, string QualifyingType, bool IsString)
		{
			m_Type = SdRegValueType;
			m_Name = Name;
			m_QualifyingType = QualifyingType;
			RegValueType = ((!IsString) ? RegistryValueType.REG_BINARY : RegistryValueType.REG_SZ);
		}

		public string GetRegPath()
		{
			string text = null;
			switch (m_Type)
			{
			case SdRegType.TransientObject:
				return "HKEY_LOCAL_MACHINE\\Software\\Microsoft\\SecurityManager\\TransientObjects\\%5C%5C.%5C" + m_QualifyingType + "%5C" + m_Name.Replace("\\", "%5C");
			case SdRegType.Com:
				return "HKEY_LOCAL_MACHINE\\Software\\Classes\\AppId\\" + m_Name;
			case SdRegType.WinRt:
				return "HKEY_LOCAL_MACHINE\\Software\\Microsoft\\WindowsRuntime\\Server\\" + m_Name;
			case SdRegType.EtwProvider:
				return "HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Control\\WMI\\Security";
			case SdRegType.Generic:
				return m_Name.Substring(0, m_Name.LastIndexOf('\\'));
			default:
				throw new PkgGenException("Invalid SDReg type. Failed to return registry path");
			}
		}

		public string GetUniqueIdentifier()
		{
			string text = null;
			switch (m_Type)
			{
			case SdRegType.TransientObject:
				return m_QualifyingType + m_Name;
			case SdRegType.Com:
				return m_Name;
			case SdRegType.WinRt:
				return m_Name;
			case SdRegType.EtwProvider:
				return m_Name;
			case SdRegType.Generic:
				return m_Name;
			default:
				throw new PkgGenException("Invalid SDReg type. Failed to return unique identifier");
			}
		}

		public bool HasAdditionalValue()
		{
			return m_Type == SdRegType.Com;
		}

		public string GetRegValueName()
		{
			return GetRegValueName(false);
		}

		public string GetRegValueName(bool GetAdditional)
		{
			string text = null;
			switch (m_Type)
			{
			case SdRegType.TransientObject:
				return "SecurityDescriptor";
			case SdRegType.Com:
				return GetAdditional ? "LaunchPermission" : "AccessPermission";
			case SdRegType.WinRt:
				return "Permissions";
			case SdRegType.EtwProvider:
				return m_Name;
			case SdRegType.Generic:
				return m_Name.Substring(m_Name.LastIndexOf('\\') + 1);
			default:
				throw new PkgGenException("Invalid SDReg type. Failed to return registry value name");
			}
		}

		public string GetRegValue()
		{
			return GetRegValue(false);
		}

		public string GetRegValue(bool GetAdditional)
		{
			string text = (GetAdditional ? AdditionalValue : Value);
			if (text != null && RegValueType == RegistryValueType.REG_BINARY)
			{
				RegistrySecurity registrySecurity = new RegistrySecurity();
				registrySecurity.SetSecurityDescriptorSddlForm(text);
				byte[] securityDescriptorBinaryForm = registrySecurity.GetSecurityDescriptorBinaryForm();
				StringBuilder stringBuilder = new StringBuilder();
				byte[] array = securityDescriptorBinaryForm;
				foreach (byte b in array)
				{
					stringBuilder.Append(b.ToString("X2", CultureInfo.InvariantCulture));
				}
				return stringBuilder.ToString();
			}
			return text;
		}
	}
}
