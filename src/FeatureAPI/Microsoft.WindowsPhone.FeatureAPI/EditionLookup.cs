using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class EditionLookup
	{
		public enum LookupMethod
		{
			Registry,
			EnvironmentVariable,
			HardCodedPath
		}

		[XmlAttribute]
		public LookupMethod Method;

		[XmlAttribute]
		public string Path;

		[XmlAttribute]
		public string Key;

		[XmlAttribute]
		public string RelativePath;

		[XmlAttribute]
		public string MSPackageDirectoryName;

		[XmlIgnore]
		public string InstallPath
		{
			get
			{
				string text = string.Empty;
				switch (Method)
				{
				case LookupMethod.Registry:
					text = RegistryLookup.GetValue(Path, Key);
					break;
				case LookupMethod.EnvironmentVariable:
					text = Environment.GetEnvironmentVariable(Key);
					break;
				case LookupMethod.HardCodedPath:
					text = Environment.ExpandEnvironmentVariables(Path);
					break;
				}
				if (!string.IsNullOrWhiteSpace(text))
				{
					if (!string.IsNullOrWhiteSpace(RelativePath))
					{
						text = System.IO.Path.Combine(text, RelativePath);
					}
					text = Environment.ExpandEnvironmentVariables(text);
					if (!LongPathDirectory.Exists(text))
					{
						text = string.Empty;
					}
				}
				return text;
			}
		}

		public override string ToString()
		{
			string text = Method.ToString() + " ";
			switch (Method)
			{
			case LookupMethod.Registry:
			case LookupMethod.EnvironmentVariable:
				text += Key;
				break;
			case LookupMethod.HardCodedPath:
				text += Path;
				break;
			}
			return text;
		}
	}
}
