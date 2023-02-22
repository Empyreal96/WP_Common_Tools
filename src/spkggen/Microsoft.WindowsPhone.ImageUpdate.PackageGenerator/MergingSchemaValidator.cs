using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PackageGenerator
{
	internal class MergingSchemaValidator : XmlValidator
	{
		public MergingSchemaValidator(ValidationEventHandler eventHandler)
			: base(eventHandler)
		{
		}

		public XmlSchema AddSchemaWithPlugins(Stream baseSchemaStream, IEnumerable<IPkgPlugin> plugins)
		{
			return AddSchemaWithPlugins(XmlSchema.Read(baseSchemaStream, _validationEventHandler), plugins);
		}

		public XmlSchema AddSchemaWithPlugins(XmlSchema baseSchema, IEnumerable<IPkgPlugin> plugins)
		{
			List<KeyValuePair<Assembly, string>> list = new List<KeyValuePair<Assembly, string>>();
			XmlSchemaElement schemaComponentsType = GetSchemaComponentsType(baseSchema);
			if (schemaComponentsType == null)
			{
				throw new PkgGenException("Cannot load plugins into base schema, does not contain 'Components'.");
			}
			AddSchema(baseSchema);
			XmlSchemaChoice xmlSchemaChoice = (XmlSchemaChoice)((XmlSchemaComplexType)schemaComponentsType.SchemaType).Particle;
			foreach (IPkgPlugin plugin in plugins)
			{
				try
				{
					KeyValuePair<Assembly, string> item = new KeyValuePair<Assembly, string>(plugin.GetType().Assembly, plugin.XmlSchemaPath);
					if (!list.Contains(item))
					{
						XmlSchema xmlSchema = XmlSchema.Read(item.Key.GetManifestResourceStream(item.Value), null);
						if (!xmlSchema.TargetNamespace.Equals(baseSchema.TargetNamespace, StringComparison.InvariantCulture))
						{
							throw new PkgGenException("Plugin '{0}' returned a schema that does not target the '{1}' namespace. It must target this namespace.", plugin.Name, baseSchema.TargetNamespace);
						}
						foreach (XmlSchemaObject item2 in xmlSchema.Items)
						{
							baseSchema.Items.Add(item2);
						}
						list.Add(item);
					}
					if (!SchemaHasElementName(baseSchema, plugin.XmlElementName))
					{
						throw new PkgGenException("Plugin '{0}' does not seem to contain an element called '{1}' in the schema. Make sure to use <xs:element name=\"{1}\">", plugin.Name, plugin.XmlElementName);
					}
					XmlSchemaElement xmlSchemaElement = new XmlSchemaElement();
					xmlSchemaElement.RefName = new XmlQualifiedName(plugin.XmlElementName, baseSchema.TargetNamespace);
					xmlSchemaChoice.Items.Add(xmlSchemaElement);
					if (plugin.XmlElementUniqueXPath != null)
					{
						XmlSchemaUnique xmlSchemaUnique = new XmlSchemaUnique();
						xmlSchemaUnique.Name = plugin.XmlElementName;
						xmlSchemaUnique.Selector = new XmlSchemaXPath();
						xmlSchemaUnique.Selector.XPath = "ps:" + plugin.XmlElementName;
						XmlSchemaXPath xmlSchemaXPath = new XmlSchemaXPath();
						xmlSchemaXPath.XPath = plugin.XmlElementUniqueXPath;
						xmlSchemaUnique.Fields.Add(xmlSchemaXPath);
						schemaComponentsType.Constraints.Add(xmlSchemaUnique);
					}
					_xmlReaderSettings.Schemas.Reprocess(baseSchema);
				}
				catch (PkgGenException)
				{
					throw;
				}
				catch (XmlSchemaException ex2)
				{
					throw new PkgGenException(ex2, "Plugin '{0}' has schema errors: {1}", plugin.Name, ex2.Message);
				}
				catch (Exception innerException)
				{
					throw new PkgGenException(innerException, "Plugin '{0}' does not provide a schema snippet, or it could not be loaded.", plugin.Name);
				}
			}
			return baseSchema;
		}

		private bool SchemaHasElementName(XmlSchema schema, string xmlElementName)
		{
			foreach (XmlSchemaElement item in schema.Items.OfType<XmlSchemaElement>())
			{
				if (item.Name.Equals(xmlElementName, StringComparison.InvariantCulture))
				{
					return true;
				}
			}
			return false;
		}

		private XmlSchemaElement GetSchemaComponentsType(XmlSchema schema)
		{
			XmlSchemaElement result = null;
			foreach (XmlSchemaElement item in schema.Items.OfType<XmlSchemaElement>())
			{
				if (item.Name.Equals("Components", StringComparison.InvariantCulture))
				{
					return item;
				}
			}
			return result;
		}
	}
}
