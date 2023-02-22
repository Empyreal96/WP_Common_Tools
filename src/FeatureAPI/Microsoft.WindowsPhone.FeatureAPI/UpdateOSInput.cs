using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	[XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
	[XmlRoot(ElementName = "UpdateOSInput", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class UpdateOSInput
	{
		public string Description;

		public string DateTime;

		[XmlArrayItem(ElementName = "PackageFile", Type = typeof(string), IsNullable = false)]
		[XmlArray]
		public List<string> PackageFiles;

		public static UpdateOSInput ValidateInput(string inputFile, IULogger logger)
		{
			UpdateOSInput updateOSInput = new UpdateOSInput();
			string text = string.Empty;
			string updateOSInputSchema = DevicePaths.UpdateOSInputSchema;
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
			foreach (string text2 in manifestResourceNames)
			{
				if (text2.Contains(updateOSInputSchema))
				{
					text = text2;
					break;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				throw new XsdValidatorException("ToolsCommon!XsdValidator::ValidateXsd: XSD resource was not found: " + updateOSInputSchema);
			}
			using (Stream xsdStream = executingAssembly.GetManifestResourceStream(text))
			{
				XsdValidator xsdValidator = new XsdValidator();
				try
				{
					xsdValidator.ValidateXsd(xsdStream, inputFile, logger);
				}
				catch (XsdValidatorException innerException)
				{
					throw new FeatureAPIException("FeatureAPI!ValidateInput: Unable to validate update input XSD.", innerException);
				}
			}
			logger.LogInfo("FeatureAPI: Successfully validated the update input XML: {0}", inputFile);
			TextReader textReader = new StreamReader(LongPathFile.OpenRead(inputFile));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(UpdateOSInput));
			try
			{
				updateOSInput = (UpdateOSInput)xmlSerializer.Deserialize(textReader);
				for (int j = 0; j < updateOSInput.PackageFiles.Count; j++)
				{
					updateOSInput.PackageFiles[j] = Environment.ExpandEnvironmentVariables(updateOSInput.PackageFiles[j]);
					updateOSInput.PackageFiles[j] = updateOSInput.PackageFiles[j].Trim();
				}
				return updateOSInput;
			}
			catch (Exception innerException2)
			{
				throw new FeatureAPIException("FeatureAPI!ValidateInput: Unable to parse Update OS Input XML file.", innerException2);
			}
			finally
			{
				textReader.Close();
			}
		}

		public void WriteToFile(string fileName)
		{
			TextWriter textWriter = new StreamWriter(LongPathFile.OpenWrite(fileName));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(UpdateOSInput));
			try
			{
				xmlSerializer.Serialize(textWriter, this);
			}
			catch (Exception innerException)
			{
				throw new FeatureAPIException("FeatureAPI!WriteToFile: Unable to write Update OS Input XML file '" + fileName + "'", innerException);
			}
			finally
			{
				textWriter.Close();
			}
		}
	}
}
