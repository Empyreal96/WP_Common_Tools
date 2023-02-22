using System;

namespace Microsoft.WindowsPhone.ImageUpdate.OemCustomizationTool
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			try
			{
				TraceLogger.TraceLevel = TraceLevel.Warn;
				if (!new InputParameters(args).IsInputParamValid)
				{
					TraceLogger.LogMessage(TraceLevel.Error, "Exiting. Try again with the expected parameters.");
					return;
				}
				Customization cust = new Customization(Settings.CustomizationFiles);
				Configuration conf = new Configuration(Settings.ConfigFiles);
				new CustomizationPkgBuilder(cust, conf).GenerateCustomizationPackage();
			}
			catch (Exception ex)
			{
				TraceLogger.LogMessage(TraceLevel.Error, "Customization package creation failed!");
				TraceLogger.LogMessage(TraceLevel.Info, ex.ToString());
			}
		}
	}
}
