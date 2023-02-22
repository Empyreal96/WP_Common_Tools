using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public class XmlValidator
	{
		protected ValidationEventHandler _validationEventHandler;

		protected XmlReaderSettings _xmlReaderSettings;

		private static void OnSchemaValidationEvent(object sender, ValidationEventArgs e)
		{
			Console.WriteLine(e.Message);
		}

		public XmlValidator()
			: this(OnSchemaValidationEvent)
		{
		}

		public XmlValidator(ValidationEventHandler eventHandler)
		{
			_validationEventHandler = eventHandler;
			_xmlReaderSettings = new XmlReaderSettings();
			_xmlReaderSettings.ValidationType = ValidationType.Schema;
			_xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings | XmlSchemaValidationFlags.ProcessIdentityConstraints;
			_xmlReaderSettings.ValidationEventHandler += _validationEventHandler;
		}

		public void AddSchema(XmlSchema schema)
		{
			_xmlReaderSettings.Schemas.Add(schema);
		}

		public void AddSchema(string xsdFile)
		{
			using (Stream xsdStream = LongPathFile.OpenRead(xsdFile))
			{
				AddSchema(xsdStream);
			}
		}

		public void AddSchema(Stream xsdStream)
		{
			using (XmlReader reader = XmlReader.Create(xsdStream))
			{
				AddSchema(XmlSchema.Read(reader, _validationEventHandler));
			}
		}

		public void Validate(string xmlFile)
		{
			using (Stream xmlStream = LongPathFile.OpenRead(xmlFile))
			{
				Validate(xmlStream);
			}
		}

		public void Validate(Stream xmlStream)
		{
			XmlReader xmlReader = XmlReader.Create(xmlStream, _xmlReaderSettings);
			while (xmlReader.Read())
			{
			}
		}

		public void Validate(XElement element)
		{
			while (element != null && element.GetSchemaInfo() == null)
			{
				element = element.Parent;
			}
			if (element == null)
			{
				throw new ArgumentException("Argument has no SchemaInfo anywhere in the document. Validate the XDocument first.");
			}
			IXmlSchemaInfo schemaInfo = element.GetSchemaInfo();
			element.Validate(schemaInfo.SchemaElement, _xmlReaderSettings.Schemas, _validationEventHandler, true);
		}

		public void Validate(XDocument document)
		{
			document.Validate(_xmlReaderSettings.Schemas, _validationEventHandler, true);
		}

		public XmlReader GetXmlReader(string xmlFile)
		{
			return XmlReader.Create(LongPathFile.OpenRead(xmlFile), _xmlReaderSettings);
		}

		public XmlReader GetXmlReader(Stream xmlStream)
		{
			return XmlReader.Create(xmlStream, _xmlReaderSettings);
		}
	}
}
