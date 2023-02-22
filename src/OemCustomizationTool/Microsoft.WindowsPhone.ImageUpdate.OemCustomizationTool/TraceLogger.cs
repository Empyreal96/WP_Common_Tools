using System.Diagnostics;
using System.Globalization;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.OemCustomizationTool
{
	internal class TraceLogger
	{
		private static TraceLevel traceLevel = TraceLevel.Warn;

		public static TraceLevel TraceLevel
		{
			get
			{
				return traceLevel;
			}
			set
			{
				traceLevel = value;
				LogUtil.SetVerbosity(value == TraceLevel.Info);
			}
		}

		public static void LogMessage(TraceLevel reqLevel, string msg, bool condition = true)
		{
			if (condition)
			{
				StackFrame stackFrame = new StackFrame(1);
				string name = stackFrame.GetMethod().Name;
				string name2 = stackFrame.GetMethod().DeclaringType.Name;
				string text = "[OCT|" + name2 + "." + name + "()]: ";
				switch (reqLevel)
				{
				case TraceLevel.Error:
					LogUtil.Error(text + msg);
					break;
				case TraceLevel.Warn:
					LogUtil.Warning(text + msg);
					break;
				case TraceLevel.Info:
					LogUtil.Diagnostic(text + msg);
					break;
				}
				if (reqLevel <= TraceLevel)
				{
					string text2 = "[OCT|" + reqLevel.ToString().ToUpper(CultureInfo.InvariantCulture) + "|" + name2 + "." + name + "()]: ";
				}
			}
		}
	}
}
