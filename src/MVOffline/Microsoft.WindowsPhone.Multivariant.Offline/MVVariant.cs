using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Microsoft.WindowsPhone.Multivariant.Offline
{
	public class MVVariant
	{
		public string Name { get; private set; }

		public List<MVCondition> Conditions { get; private set; }

		public List<MVSettingGroup> SettingsGroups { get; private set; }

		public List<KeyValuePair<Guid, XElement>> Applications { get; private set; }

		public MVVariant(string name)
		{
			Name = name;
			Conditions = new List<MVCondition>();
			SettingsGroups = new List<MVSettingGroup>();
			Applications = new List<KeyValuePair<Guid, XElement>>();
		}
	}
}
