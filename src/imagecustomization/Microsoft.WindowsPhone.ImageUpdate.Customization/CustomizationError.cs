using System.Collections.Generic;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization
{
	public class CustomizationError
	{
		public CustomizationErrorSeverity Severity { get; private set; }

		public IEnumerable<IDefinedIn> FilesInvolved { get; private set; }

		public string Message { get; private set; }

		public CustomizationError(CustomizationErrorSeverity severity, IEnumerable<IDefinedIn> filesInvolved, string format, params object[] args)
		{
			Severity = severity;
			FilesInvolved = filesInvolved;
			Message = string.Format(format, args);
		}

		public CustomizationError(CustomizationErrorSeverity severity, IEnumerable<IDefinedIn> filesInvolved, string message)
		{
			Severity = severity;
			FilesInvolved = filesInvolved;
			Message = message;
		}
	}
}
