using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public sealed class LogUtilityReport : ReportingBase
	{
		internal LogUtilityReport()
		{
		}

		public override void ErrorLine(string errorMsg)
		{
			LogUtil.Error(errorMsg);
		}

		public override void Debug(string debugMsg)
		{
			if (ReportingBase.EnableDebugMessage)
			{
				LogUtil.Diagnostic(debugMsg);
			}
		}

		public override void DebugLine(string debugMsg)
		{
			if (ReportingBase.EnableDebugMessage)
			{
				LogUtil.Diagnostic(debugMsg);
			}
		}

		public override void XmlElementLine(string indentation, string elememt)
		{
			DebugLine(string.Format(GlobalVariables.Culture, "{0}{1}", new object[2] { indentation, elememt }));
		}

		public override void XmlAttributeLine(string indentation, string elememt, string value)
		{
			DebugLine(string.Format(GlobalVariables.Culture, "{0}{1}=\"{2}\"", new object[3] { indentation, elememt, value }));
		}
	}
}
