using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using Microsoft.CompPlat.PkgBldr.Base;
using Microsoft.CompPlat.PkgBldr.Tools;

namespace Microsoft.CompPlat.PkgBldr.Plugins.PkgToWm
{
	public static class Helpers
	{
		public static string lowerCamel(string s)
		{
			switch (s)
			{
			case "WNF":
				return "wnf";
			case "COM":
				return "com";
			case "ETWProvider":
				return "etwProvider";
			case "SDRegValue":
				return "sdRegValue";
			default:
				return char.ToLowerInvariant(s[0]) + s.Substring(1);
			}
		}

		public static string ConvertBuildFilter(string pkgBuildFilter)
		{
			return pkgBuildFilter.Replace("wow", "build.isWow").Replace("arch", "build.arch");
		}

		public static string ConvertMulitSz(string pkgValue)
		{
			string text = null;
			text = pkgValue.Replace(";", "&quot;,&quot;");
			return "&quot;" + text + "&quot;";
		}

		[SuppressMessage("Microsoft.Design", "CA1026")]
		public static string ComConvertThreading(string pkgThreading, IDeploymentLogger passedInLogger = null)
		{
			IDeploymentLogger deploymentLogger = passedInLogger ?? new Logger();
			string result = null;
			switch (pkgThreading.ToLowerInvariant())
			{
			case "both":
				result = "Both";
				break;
			case "apartment":
				result = "STA";
				break;
			case "free":
				result = "MTA";
				break;
			case "neutral":
				result = "Neutral";
				break;
			default:
				deploymentLogger.LogWarning("unknown COM threading {0}", pkgThreading);
				break;
			}
			return result;
		}

		internal static string GenerateWmBuildFilter(XElement pkgElement, IDeploymentLogger logger)
		{
			string result = null;
			string attributeValue = PkgBldrHelpers.GetAttributeValue(pkgElement, "buildFilter");
			string attributeValue2 = PkgBldrHelpers.GetAttributeValue(pkgElement, "CpuFilter");
			if (attributeValue != null)
			{
				result = ConvertBuildFilter(attributeValue);
			}
			if (attributeValue2 != null)
			{
				if (attributeValue == null)
				{
					result = "build.arch = " + attributeValue2.ToLowerInvariant();
				}
				else
				{
					logger.LogWarning("Pkg.xml contains both a CpuFilter and a buildFilter. Ignoring the CpuFilter.");
				}
			}
			return result;
		}
	}
}
