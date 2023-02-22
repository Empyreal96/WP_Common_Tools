using System.Collections.Generic;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization
{
	public class CustomContent
	{
		public IEnumerable<CustomizationError> CustomizationErrors { get; internal set; }

		public IEnumerable<string> PackagePaths { get; internal set; }

		public List<KeyValuePair<string, string>> DataContent { get; internal set; }
	}
}
