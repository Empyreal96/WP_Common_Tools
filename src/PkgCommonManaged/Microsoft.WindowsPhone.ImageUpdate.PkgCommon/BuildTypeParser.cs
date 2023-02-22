namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public static class BuildTypeParser
	{
		private static StringToEnum<BuildType> parser;

		static BuildTypeParser()
		{
			parser = new StringToEnum<BuildType>();
			parser.Add(BuildType.Retail, "Retail");
			parser.Add(BuildType.Retail, "fre");
			parser.Add(BuildType.Checked, "Checked");
			parser.Add(BuildType.Checked, "chk");
			parser.Add(BuildType.Debug, "Debug");
			parser.Add(BuildType.Debug, "dbg");
		}

		public static BuildType Parse(string value)
		{
			return parser.Parse(value);
		}
	}
}
