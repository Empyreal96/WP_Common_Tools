namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public enum CompilationState
	{
		CompletedSuccessfully,
		UsageError,
		MacroFileLoadAndValidation,
		GlobalMacroDereferencing,
		PolicyFileLoadAndValidation,
		PolicyMacroDereferencing,
		PolicyElementsDataExtraction,
		PolicyElementsCompilation,
		CompilingCapability,
		CompilingCapabilityRule,
		SaveXmlFile,
		Unknown,
		PolicyFileAddHeaderAttributes,
		PolicyFileAddElements
	}
}
