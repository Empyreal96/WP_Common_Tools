using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.CompPlat.PkgBldr.Base.Tools;
using Microsoft.CompPlat.PkgBldr.Interfaces;
using Microsoft.CompPlat.PkgBldr.Tools;

namespace Microsoft.CompPlat.PkgBldr.Base
{
	internal class SchemaSet
	{
		private List<XElement> m_externalXmlElements;

		private IDeploymentLogger m_logger;

		private PkgBldrCmd m_pkgBldrArgs;

		private List<XmlSchema> m_mergedSchemaList { get; set; }

		public SchemaSet(IDeploymentLogger logger = null, PkgBldrCmd pkgBldrArgs = null)
		{
			m_logger = logger ?? new Logger();
			m_pkgBldrArgs = pkgBldrArgs ?? new PkgBldrCmd();
		}

		private void getExternalElements(XElement root, XNamespace rootns)
		{
			if (root.Name.Namespace != rootns)
			{
				if (SchemaIsExternal(root.Name.Namespace))
				{
					m_externalXmlElements.Add(root);
				}
				return;
			}
			foreach (XElement item in root.Elements())
			{
				getExternalElements(item, rootns);
			}
		}

		private void validateStream(Stream xmlStream, XmlSchema schema)
		{
			XmlReaderSettings xmlReaderSettings = new XmlReaderSettings
			{
				ValidationType = ValidationType.Schema
			};
			xmlReaderSettings.ValidationEventHandler += ValidationEventHandler;
			xmlReaderSettings.Schemas.Add(schema);
			XmlReader xmlReader = XmlReader.Create(xmlStream, xmlReaderSettings);
			try
			{
				while (xmlReader.Read())
				{
				}
			}
			catch (XmlException ex)
			{
				throw new PkgGenException("Line:{0} Position {1} schema validation error \n {2}", ex.LineNumber.ToString(CultureInfo.InvariantCulture), ex.LinePosition.ToString(CultureInfo.InvariantCulture), ex.Message);
			}
		}

		private XmlSchema GetSchema(XNamespace target)
		{
			foreach (XmlSchema mergedSchema in m_mergedSchemaList)
			{
				if (mergedSchema.TargetNamespace == target)
				{
					return mergedSchema;
				}
			}
			throw new PkgGenException("no schema found for {0}", target.ToString());
		}

		private bool SchemaIsExternal(XNamespace target)
		{
			foreach (XmlSchema mergedSchema in m_mergedSchemaList)
			{
				if (mergedSchema.TargetNamespace == target)
				{
					return true;
				}
			}
			return false;
		}

		private void validateElement(XElement element, XmlSchema schema)
		{
			Stream stream = new MemoryStream();
			element.Save(stream);
			stream.Position = 0L;
			validateStream(stream, schema);
		}

		public void ValidateXmlFile(string xmlFile)
		{
			if (LongPathFile.Exists(xmlFile))
			{
				throw new PkgGenException("xml file not found {0}", xmlFile);
			}
			using (FileStream xmlStream = LongPathFile.OpenRead(xmlFile))
			{
				ValidateStream(xmlStream);
			}
		}

		private void ValidationEventHandler(object sender, ValidationEventArgs e)
		{
			string text = null;
			switch (e.Severity)
			{
			case XmlSeverityType.Error:
				text = string.Format(CultureInfo.InvariantCulture, "Line:{0} Postition:{1} error {2}", new object[3]
				{
					e.Exception.LineNumber.ToString(CultureInfo.InvariantCulture),
					e.Exception.LinePosition.ToString(CultureInfo.InvariantCulture),
					e.Message
				});
				m_logger.LogError(text);
				throw new PkgGenException("PkgBldrCommon schema validation failed");
			case XmlSeverityType.Warning:
				text = string.Format(CultureInfo.InvariantCulture, "Line:{0} Postition:{1} warning {2}", new object[3]
				{
					e.Exception.LineNumber.ToString(CultureInfo.InvariantCulture),
					e.Exception.LinePosition.ToString(CultureInfo.InvariantCulture),
					e.Message
				});
				m_logger.LogWarning(text);
				break;
			}
		}

		public void ValidateXmlDoc(XDocument xDoc)
		{
			MemoryStream memoryStream = new MemoryStream();
			xDoc.Save(memoryStream);
			memoryStream.Seek(0L, SeekOrigin.Begin);
			ValidateStream(memoryStream);
		}

		private void ValidateStream(Stream xmlStream)
		{
			XElement xElement = XElement.Load(xmlStream);
			m_externalXmlElements = new List<XElement>();
			getExternalElements(xElement, xElement.Name.Namespace);
			foreach (XElement externalXmlElement in m_externalXmlElements)
			{
				externalXmlElement.Remove();
			}
			XmlSchema schema = GetSchema(xElement.Name.Namespace);
			validateElement(xElement, schema);
			foreach (XElement externalXmlElement2 in m_externalXmlElements)
			{
				schema = GetSchema(externalXmlElement2.Name.Namespace);
				validateElement(externalXmlElement2, schema);
			}
		}

		public void LoadSchemasFromFiles(List<string> xsdFileList)
		{
			List<XSD> list = new List<XSD>();
			foreach (string xsdFile in xsdFileList)
			{
				XSD xSD = new XSD();
				xSD.file = xsdFile;
				xSD.ns = null;
				list.Add(xSD);
			}
			MergeSchema(list);
		}

		public List<XmlSchema> GetMergedSchemas()
		{
			return m_mergedSchemaList;
		}

		private void MergeSchema(List<XSD> schemaFileList)
		{
			List<string> list = new List<string>();
			XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
			List<XNamespace> list2 = new List<XNamespace>();
			m_mergedSchemaList = new List<XmlSchema>();
			foreach (XSD schemaFile in schemaFileList)
			{
				if (list.Contains(schemaFile.file))
				{
					continue;
				}
				if (!LongPathFile.Exists(schemaFile.file))
				{
					throw new PkgGenException("failed to load schema {0}", schemaFile.file);
				}
				try
				{
					Stream stream = null;
					if (schemaFile.ns != null)
					{
						XDocument xDocument = PkgBldrHelpers.XDocumentLoadFromLongPath(schemaFile.file);
						XNamespace xNamespace = schemaFile.ns;
						foreach (XAttribute item2 in xDocument.Root.Attributes())
						{
							if (item2.Name.LocalName.Equals("targetNamespace"))
							{
								item2.Value = xNamespace.NamespaceName;
							}
							else if (item2.IsNamespaceDeclaration && !item2.Name.LocalName.Equals("xsd") && !item2.Name.LocalName.Equals("xs"))
							{
								item2.Value = xNamespace.ToString();
							}
						}
						stream = new MemoryStream();
						xDocument.Save(stream);
						stream.Flush();
						stream.Position = 0L;
					}
					else
					{
						stream = new FileStream(schemaFile.file, FileMode.Open, FileAccess.Read);
					}
					XmlSchema xmlSchema = XmlSchema.Read(XmlReader.Create(stream), null);
					XNamespace item = xmlSchema.TargetNamespace;
					if (!list2.Contains(item))
					{
						list2.Add(item);
					}
					xmlSchemaSet.Add(xmlSchema);
					list.Add(schemaFile.file);
				}
				catch (XmlSchemaException ex)
				{
					throw new PkgGenException(ex, "schema errors: {0}, {1}", schemaFile, ex.Message);
				}
			}
			xmlSchemaSet.Compile();
			XmlSchemaSet xmlSchemaSet2 = new XmlSchemaSet();
			foreach (XNamespace item3 in list2)
			{
				ICollection collection = xmlSchemaSet.Schemas(item3.ToString());
				XmlSchema xmlSchema2 = null;
				XmlSerializerNamespaces xmlSerializerNamespaces = null;
				foreach (XmlSchema item4 in collection)
				{
					if (xmlSchema2 == null)
					{
						xmlSchema2 = item4;
						xmlSerializerNamespaces = item4.Namespaces;
						continue;
					}
					foreach (XmlSchemaObject item5 in item4.Items)
					{
						xmlSchema2.Items.Add(item5);
					}
					XmlQualifiedName[] array = item4.Namespaces.ToArray();
					XmlQualifiedName[] source = xmlSerializerNamespaces.ToArray();
					for (int i = 0; i < array.Count(); i++)
					{
						XmlQualifiedName xmlQualifiedName = array[i];
						if (!source.Contains(xmlQualifiedName))
						{
							xmlSerializerNamespaces.Add(xmlQualifiedName.Name, xmlQualifiedName.Namespace);
						}
					}
				}
				xmlSchemaSet2.Add(xmlSchema2);
				m_mergedSchemaList.Add(xmlSchema2);
			}
			xmlSchemaSet2.Compile();
		}

		public void LoadSchemasFromPlugins(IEnumerable<IPkgPlugin> plugins)
		{
			List<XSD> list = new List<XSD>();
			foreach (IPkgPlugin plugin in plugins)
			{
				string xsdPath = GetXsdPath(plugin.XmlSchemaPath.ToLowerInvariant());
				XSD xSD = new XSD();
				xSD.file = xsdPath;
				xSD.ns = plugin.XmlSchemaNameSpace;
				list.Add(xSD);
			}
			MergeSchema(list);
		}

		private string GetXsdPath(string schemaPath)
		{
			string fileName = LongPath.GetFileName(schemaPath);
			string path;
			switch (LongPath.GetDirectoryName(schemaPath))
			{
			case "pkgbldr.csi.xsd":
				path = Environment.ExpandEnvironmentVariables(m_pkgBldrArgs.csiXsdPath);
				break;
			case "pkgbldr.pkg.xsd":
				path = Environment.ExpandEnvironmentVariables(m_pkgBldrArgs.pkgXsdPath);
				break;
			case "pkgbldr.shared.xsd":
				path = Environment.ExpandEnvironmentVariables(m_pkgBldrArgs.sharedXsdPath);
				break;
			case "pkgbldr.wm.xsd":
				path = Environment.ExpandEnvironmentVariables(m_pkgBldrArgs.wmXsdPath);
				break;
			default:
				throw new Exception("Unrecognized xsd type " + schemaPath);
			}
			return LongPath.Combine(path, fileName);
		}
	}
}
