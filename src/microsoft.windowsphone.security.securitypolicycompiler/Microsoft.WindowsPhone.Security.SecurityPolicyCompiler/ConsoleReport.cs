using System;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public sealed class ConsoleReport : ReportingBase
	{
		internal ConsoleReport()
		{
		}

		public override void ErrorLine(string errorMsg)
		{
			Console.WriteLine(errorMsg);
		}

		public override void Debug(string debugMsg)
		{
			if (ReportingBase.EnableDebugMessage)
			{
				Console.Write("Debug: " + debugMsg);
			}
		}

		public override void DebugLine(string debugMsg)
		{
			if (ReportingBase.EnableDebugMessage)
			{
				Console.WriteLine("Debug: " + debugMsg);
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
