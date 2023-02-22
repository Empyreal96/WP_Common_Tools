using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class XmlValidator
	{
		private bool _fileIsValid = true;

		private IULogger _logger;

		public void ValidateXmlAndAddDefaults(string xsdFile, string xmlFile, IULogger logger)
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
			string text = string.Empty;
			if (!File.Exists(xmlFile))
			{
				throw new XmlValidatorException("XmlValidator::ValidateXml: XML file was not found: " + xmlFile);
			}
			_logger = logger;
			string[] array = manifestResourceNames;
			foreach (string text2 in array)
			{
				if (text2.Contains(xsdFile))
				{
					text = text2;
					break;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				throw new XmlValidatorException("XmlValidator::ValidateXml: XSD resource was not found: " + xsdFile);
			}
			_fileIsValid = true;
			using (Stream stream = executingAssembly.GetManifestResourceStream(text))
			{
				if (stream == null)
				{
					throw new XmlValidatorException("XmlValidator::ValidateXml: Failed to load the embeded schema file: " + text);
				}
				try
				{
					XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
					XmlSchema schema = XmlSchema.Read(stream, ValidationHandler);
					xmlSchemaSet.Add(schema);
					XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
					xmlReaderSettings.ValidationType = ValidationType.Schema;
					xmlReaderSettings.Schemas = xmlSchemaSet;
					xmlReaderSettings.ValidationEventHandler += ValidationHandler;
					using (XmlReader reader = XmlReader.Create(xmlFile, xmlReaderSettings))
					{
						XmlDocument xmlDocument = new XmlDocument();
						xmlDocument.Load(reader);
						xmlDocument.Schemas = xmlSchemaSet;
						xmlDocument.Validate(ValidationHandler);
					}
				}
				catch (XmlSchemaException innerException)
				{
					throw new XmlValidatorException("XmlValidator::ValidateXml: Unable to use the schema provided " + text, innerException);
				}
				catch (Exception innerException2)
				{
					throw new XmlValidatorException("XmlValidator::ValidateXml: There was a problem validating the XML file " + xmlFile, innerException2);
				}
			}
			if (!_fileIsValid)
			{
				throw new XmlValidatorException(string.Format(CultureInfo.InvariantCulture, "XmlValidator::ValidateXml: Validation of {0} failed", new object[1] { xmlFile }));
			}
		}

		private void ValidationHandler(object sender, ValidationEventArgs args)
		{
			string format = string.Format(CultureInfo.InvariantCulture, "\nXmlValidator::ValidateXml: XML Validation {0}: {1}", new object[2] { args.Severity, args.Message });
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
