using System.Collections.Generic;
using System.Xml;
using Microsoft.CompPlat.PkgBldr.Base;

namespace Microsoft.CompPlat.PkgBldr.Plugins.CsiToPkg
{
	internal class Security
	{
		private string m_policyId;

		private Dictionary<string, SDDL> m_lookup;

		private Dictionary<string, SDDL> m_fileSddlList;

		private Dictionary<string, SDDL> m_dirSddlList;

		private Dictionary<string, SDDL> m_regSddlList;

		private MacroResolver m_policyMacros;

		private bool m_havePolicyData;

		public bool HavePolicyData => m_havePolicyData;

		public string PolicyID
		{
			get
			{
				return m_policyId;
			}
			set
			{
				m_policyId = value;
			}
		}

		public MacroResolver Macros => m_policyMacros;

		public Dictionary<string, SDDL> FileACLs => m_fileSddlList;

		public Dictionary<string, SDDL> DirACLs => m_dirSddlList;

		public Dictionary<string, SDDL> RegACLs => m_regSddlList;

		public Security()
		{
			m_lookup = new Dictionary<string, SDDL>();
			m_fileSddlList = new Dictionary<string, SDDL>();
			m_dirSddlList = new Dictionary<string, SDDL>();
			m_regSddlList = new Dictionary<string, SDDL>();
			m_policyMacros = new MacroResolver();
		}

		public void LoadPolicyMacros(XmlReader Macros)
		{
			m_policyMacros.Load(Macros);
		}

		public void AddToLookupTable(string name, SDDL ace)
		{
			if (!m_lookup.ContainsKey(name.ToUpperInvariant()))
			{
				m_lookup.Add(name.ToUpperInvariant(), ace);
			}
		}

		public SDDL Lookup(string name)
		{
			name = name.ToUpperInvariant();
			if (m_lookup.ContainsKey(name))
			{
				return m_lookup[name];
			}
			return null;
		}

		public void AddFileAce(string path, SDDL ace)
		{
			if (!m_fileSddlList.ContainsKey(path.ToUpperInvariant()))
			{
				m_fileSddlList.Add(path.ToUpperInvariant(), ace);
				m_havePolicyData = true;
			}
		}

		public void AddDirAce(string path, SDDL ace)
		{
			if (!m_dirSddlList.ContainsKey(path.ToUpperInvariant()))
			{
				m_dirSddlList.Add(path.ToUpperInvariant(), ace);
				m_havePolicyData = true;
			}
		}

		public void AddRegAce(string path, SDDL ace)
		{
			if (!m_regSddlList.ContainsKey(path.ToUpperInvariant()))
			{
				m_regSddlList.Add(path.ToUpperInvariant(), ace);
				m_havePolicyData = true;
			}
		}
	}
}
