using System;
using System.Globalization;
using System.Text;
using System.Xml;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon;

namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public static class GlobalVariables
	{
		private static XmlNamespaceManager namespaceManager;

		private static CompilationState currentCompilationState = CompilationState.Unknown;

		private static CultureInfo culture = new CultureInfo("en-US", false);

		private static StringComparison stringComparison = StringComparison.Ordinal;

		private const int maxReferenceLevel = 100;

		public static XmlNamespaceManager NamespaceManager
		{
			get
			{
				return namespaceManager;
			}
			set
			{
				namespaceManager = value;
			}
		}

		public static IMacroResolver MacroResolver { get; set; }

		internal static SidMapping SidMapping { get; set; }

		internal static bool IsInPackageAllowList { get; set; }

		public static CompilationState CurrentCompilationState
		{
			get
			{
				return currentCompilationState;
			}
			set
			{
				currentCompilationState = value;
			}
		}

		public static CultureInfo Culture => culture;

		public static StringComparison GlobalStringComparison => stringComparison;

		public static string ResolveMacroReference(string valueWithMacro, string errorExtraInfo)
		{
			string empty = string.Empty;
			try
			{
				return MacroResolver.Resolve(valueWithMacro);
			}
			catch (PkgGenException originalException)
			{
				throw new PolicyCompilerInternalException(string.Format(Culture, "Macro Referencing Error: {0}, Value= {1}", new object[2] { errorExtraInfo, valueWithMacro }), originalException);
			}
		}

		public static string GetPhoneSDDL(string sidMappingFilePath, string capId, string rights)
		{
			SidMapping sidMapping = SidMapping.CreateInstance(sidMappingFilePath);
			StringBuilder stringBuilder = new StringBuilder("D:P(A;;GA;;;SY)");
			DriverRule driverRule = new DriverRule("AccessedByCapability");
			string svcCapSID = sidMapping[capId];
			string appCapSID = SidBuilder.BuildApplicationCapabilitySidString(capId);
			driverRule.Add(appCapSID, svcCapSID, rights);
			stringBuilder.Append(driverRule.DACL);
			return stringBuilder.ToString();
		}
	}
}
