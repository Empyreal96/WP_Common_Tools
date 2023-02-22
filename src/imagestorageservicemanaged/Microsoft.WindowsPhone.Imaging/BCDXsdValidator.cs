using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BCDXsdValidator
	{
		private bool _fileIsValid = true;

		private IULogger _logger;

		[CLSCompliant(false)]
		public void ValidateXsd(Stream bcdSchemaStream, string xmlFile, IULogger logger)
		{
			if (!File.Exists(xmlFile))
			{
				throw new BCDXsdValidatorException("ImageServices!BCDXsdValidator::ValidateXsd: XML file was not found: " + xmlFile);
			}
			_logger = logger;
			_fileIsValid = true;
			XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
			xmlReaderSettings.ValidationEventHandler += ValidationHandler;
			xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
			xmlReaderSettings.ValidationType = ValidationType.Schema;
			try
			{
				using (XmlReader reader = XmlReader.Create(bcdSchemaStream))
				{
					XmlSchema schema = XmlSchema.Read(reader, ValidationHandler);
					xmlReaderSettings.Schemas.Add(schema);
				}
			}
			catch (XmlSchemaException innerException)
			{
				throw new BCDXsdValidatorException("ImageServices!BCDXsdValidator::ValidateXsd: Unable to use the schema provided", innerException);
			}
			XmlTextReader xmlTextReader = null;
			XmlReader xmlReader = null;
			try
			{
				try
				{
					xmlTextReader = new XmlTextReader(xmlFile);
				}
				catch (Exception innerException2)
				{
					throw new BCDXsdValidatorException("ImageServices!BCDXsdValidator::ValidateXsd: Unable to access the given XML file", innerException2);
				}
				try
				{
					xmlReader = XmlReader.Create(xmlTextReader, xmlReaderSettings);
					while (xmlReader.Read())
					{
					}
				}
				catch (XmlException innerException3)
				{
					throw new BCDXsdValidatorException("ImageServices!BCDXsdValidator::ValidateXsd: There was a problem validating the XML file", innerException3);
				}
			}
			finally
			{
				xmlReader?.Close();
				xmlTextReader?.Close();
			}
			if (!_fileIsValid)
			{
				throw new BCDXsdValidatorException(string.Format(CultureInfo.InvariantCulture, "ImageServices!BCDXsdValidator::ValidateXsd: Validation of {0} failed", new object[1] { xmlFile }));
			}
		}

		private void ValidationHandler(object sender, ValidationEventArgs args)
		{
			string format = string.Format(CultureInfo.InvariantCulture, "\nImageServices!BCDXsdValidator::ValidateXsd: XML Validation {0}: {1}", new object[2] { args.Severity, args.Message });
			if (args.Severity == XmlSeverityType.Error)
			{
				if (_logger != null)
				{
					_logger.LogError(format);
				}
				_fileIsValid = false;
			}
			else if (_logger != null)
			{
				_logger.LogWarning(format);
			}
		}
	}
}
