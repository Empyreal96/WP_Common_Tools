using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public class XsdValidator
	{
		private bool _fileIsValid = true;

		private IULogger _logger;

		public void ValidateXsd(string xsdFile, string xmlFile, IULogger logger)
		{
			if (!LongPathFile.Exists(xmlFile))
			{
				throw new XsdValidatorException("ToolsCommon!XsdValidator::ValidateXsd: XML file was not found: " + xmlFile);
			}
			using (FileStream xmlStream = LongPathFile.OpenRead(xmlFile))
			{
				string text = string.Empty;
				Assembly executingAssembly = Assembly.GetExecutingAssembly();
				string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
				foreach (string text2 in manifestResourceNames)
				{
					if (text2.Contains(xsdFile))
					{
						text = text2;
						break;
					}
				}
				if (string.IsNullOrEmpty(text))
				{
					throw new XsdValidatorException("ToolsCommon!XsdValidator::ValidateXsd: XSD resource was not found: " + xsdFile);
				}
				using (Stream xsdStream = executingAssembly.GetManifestResourceStream(text))
				{
					ValidateXsd(xsdStream, xmlStream, xmlFile, logger);
				}
			}
		}

		public void ValidateXsd(Stream xsdStream, string xmlFile, IULogger logger)
		{
			if (!LongPathFile.Exists(xmlFile))
			{
				throw new XsdValidatorException("ToolsCommon!XsdValidator::ValidateXsd: XML file was not found: " + xmlFile);
			}
			using (FileStream xmlStream = LongPathFile.OpenRead(xmlFile))
			{
				ValidateXsd(xsdStream, xmlStream, xmlFile, logger);
			}
		}

		public void ValidateXsd(Stream xsdStream, Stream xmlStream, string xmlName, IULogger logger)
		{
			_logger = logger;
			_fileIsValid = true;
			if (xsdStream == null)
			{
				throw new XsdValidatorException("ToolsCommon!XsdValidator::ValidateXsd: Failed to load the embeded schema file for xml: " + xmlName);
			}
			XmlDocument xmlDocument;
			try
			{
				XmlSchema schema = null;
				using (XmlReader reader = XmlReader.Create(xsdStream))
				{
					schema = XmlSchema.Read(reader, ValidationHandler);
				}
				xmlDocument = new XmlDocument();
				xmlDocument.Schemas.Add(schema);
			}
			catch (XmlSchemaException innerException)
			{
				throw new XsdValidatorException("ToolsCommon!XsdValidator::ValidateXsd: Unable to use the schema provided for xml: " + xmlName, innerException);
			}
			try
			{
				xmlDocument.Load(xmlStream);
				xmlDocument.Validate(ValidationHandler);
			}
			catch (Exception innerException2)
			{
				throw new XsdValidatorException("ToolsCommon!XsdValidator::ValidateXsd: There was a problem validating the XML file " + xmlName, innerException2);
			}
			if (!_fileIsValid)
			{
				throw new XsdValidatorException(string.Format(CultureInfo.InvariantCulture, "ToolsCommon!XsdValidator::ValidateXsd: Validation of {0} failed", new object[1] { xmlName }));
			}
		}

		private void ValidationHandler(object sender, ValidationEventArgs args)
		{
			string format = string.Format(CultureInfo.InvariantCulture, "\nToolsCommon!XsdValidator::ValidateXsd: XML Validation {0}: {1}", new object[2] { args.Severity, args.Message });
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
