using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	[XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
	[XmlRoot(ElementName = "UpdateOSOutput", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class UpdateOSOutput
	{
		public string Description;

		public int OverallResult;

		public string UpdateState;

		[XmlArrayItem(ElementName = "Package", Type = typeof(UpdateOSOutputPackage), IsNullable = false)]
		[XmlArray]
		public List<UpdateOSOutputPackage> Packages;

		public static UpdateOSOutput ValidateOutput(string outputFile, IULogger logger)
		{
			UpdateOSOutput updateOSOutput = new UpdateOSOutput();
			XsdValidator xsdValidator = new XsdValidator();
			string text = string.Empty;
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
			foreach (string text2 in manifestResourceNames)
			{
				if (text2.Contains(DevicePaths.UpdateOSOutputSchema))
				{
					text = text2;
					break;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				throw new XsdValidatorException("FeatureAPI!ValidateOutput: XSD resource was not found: " + DevicePaths.UpdateOSOutputSchema);
			}
			using (Stream xsdStream = executingAssembly.GetManifestResourceStream(text))
			{
				try
				{
					xsdValidator.ValidateXsd(xsdStream, outputFile, logger);
				}
				catch (XsdValidatorException innerException)
				{
					throw new FeatureAPIException("FeatureAPI!ValidateOutput: Unable to validate Update OS Output XSD.", innerException);
				}
			}
			logger.LogInfo("FeatureAPI: Successfully validated the Update OS Output XML: {0}", outputFile);
			TextReader textReader = new StreamReader(LongPathFile.OpenRead(outputFile));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(UpdateOSOutput));
			try
			{
				return (UpdateOSOutput)xmlSerializer.Deserialize(textReader);
			}
			catch (Exception innerException2)
			{
				throw new FeatureAPIException("FeatureAPI!ValidateOutput: Unable to parse Update OS Output XML file.", innerException2);
			}
			finally
			{
				textReader.Close();
			}
		}

		public void WriteToFile(string fileName)
		{
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				NewLineOnAttributes = true
			};
			using (XmlWriter xmlWriter = XmlWriter.Create(fileName, settings))
			{
				new XmlSerializer(typeof(UpdateOSOutput)).Serialize(xmlWriter, this);
			}
		}
	}
}
