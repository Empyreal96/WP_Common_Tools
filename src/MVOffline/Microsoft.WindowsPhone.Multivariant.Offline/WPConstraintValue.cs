namespace Microsoft.WindowsPhone.Multivariant.Offline
{
	public class WPConstraintValue
	{
		public string KeyValue { get; set; }

		public bool IsWildCard { get; set; }

		public WPConstraintValue(string value, bool iswildcard)
		{
			KeyValue = value;
			IsWildCard = iswildcard;
		}
	}
}
