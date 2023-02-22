using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	[XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
	[XmlRoot(ElementName = "UpdateHistory", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class UpdateHistory
	{
		[XmlArrayItem(ElementName = "UpdateEvent", Type = typeof(UpdateEvent), IsNullable = false)]
		[XmlArray]
		public List<UpdateEvent> UpdateEvents;

		public static UpdateHistory ValidateUpdateHistory(string xmlFile, IULogger logger)
		{
			UpdateHistory updateHistory = new UpdateHistory();
			string text = string.Empty;
			string updateHistorySchema = DevicePaths.UpdateHistorySchema;
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
			foreach (string text2 in manifestResourceNames)
			{
				if (text2.Contains(updateHistorySchema))
				{
					text = text2;
					break;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				throw new XsdValidatorException("FeatureAPI!ValidateUpdateHistory: XSD resource was not found: " + updateHistorySchema);
			}
			using (Stream xsdStream = executingAssembly.GetManifestResourceStream(text))
			{
				try
				{
					new XsdValidator().ValidateXsd(xsdStream, xmlFile, logger);
				}
				catch (XsdValidatorException innerException)
				{
					throw new FeatureAPIException("FeatureAPI!ValidateUpdateHistory: Unable to validate Update History XSD.", innerException);
				}
			}
			logger.LogInfo("FeatureAPI: Successfully validated the Update History XML");
			TextReader textReader = new StreamReader(LongPathFile.OpenRead(xmlFile));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(UpdateHistory));
			try
			{
				updateHistory = (UpdateHistory)xmlSerializer.Deserialize(textReader);
				foreach (UpdateEvent updateEvent in updateHistory.UpdateEvents)
				{
					foreach (UpdateOSOutputPackage package in updateEvent.UpdateResults.Packages)
					{
						package.CpuType = CpuIdParser.Parse(package.CpuTypeStr);
					}
				}
				return updateHistory;
			}
			catch (Exception innerException2)
			{
				throw new FeatureAPIException("FeatureAPI!ValidateUpdateHistory: Unable to parse Update History XML file.", innerException2);
			}
			finally
			{
				textReader.Close();
			}
		}

		public UpdateOSOutput GetPackageList()
		{
			UpdateOSOutput updateOSOutput = new UpdateOSOutput();
			List<UpdateEvent> list = (from ue in UpdateEvents
				where ue.UpdateResults.UpdateState.Equals("Update Complete", StringComparison.OrdinalIgnoreCase)
				orderby ue.Sequence
				select ue).ToList();
			Dictionary<string, UpdateOSOutputPackage> dictionary = new Dictionary<string, UpdateOSOutputPackage>(StringComparer.OrdinalIgnoreCase);
			foreach (UpdateEvent item in list)
			{
				foreach (UpdateOSOutputPackage package in item.UpdateResults.Packages)
				{
					if (package.IsRemoval)
					{
						dictionary.Remove(package.Name);
					}
					else
					{
						dictionary[package.Name] = package;
					}
				}
				updateOSOutput.OverallResult = item.UpdateResults.OverallResult;
				updateOSOutput.UpdateState = item.UpdateResults.UpdateState;
			}
			updateOSOutput.Packages = new List<UpdateOSOutputPackage>(dictionary.Values);
			updateOSOutput.Description = "List of packages currently in this image or on this device";
			return updateOSOutput;
		}
	}
}
